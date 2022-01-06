using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BlazingTwistConfigTools.config.attributes;
using BlazingTwistConfigTools.config.types;

namespace BlazingTwistConfigTools.config.deserialization {
	public class ConfigDeserializer<ConfigType> {
		private struct TokenNode {
			public readonly int lineNumber;
			public readonly string key;
			public readonly string simpleValue;
			public List<TokenNode> listValues;

			public TokenNode(ConfigNode node) : this() {
				lineNumber = node.LineNumber;
				key = node.Key;
				simpleValue = node.Value;
			}
		}

		private readonly bool verifyAllKeysSet;
		private readonly Dictionary<Type, Dictionary<string, FieldInfo>> typeCache;
		private readonly TokenNode rootNode;

		private static TokenNode BuildNode(IReadOnlyList<ConfigNode> nodes, int nodeIndex, ConfigNode node) {
			TokenNode currentNode = new TokenNode(node);
			if (currentNode.simpleValue != null) {
				return currentNode;
			}
			List<TokenNode> childNodes = new List<TokenNode>();
			currentNode.listValues = childNodes;
			int targetDepth = node.ObjectDepth + 1;

			int nodeCount = nodes.Count;
			for (int i = nodeIndex + 1; i < nodeCount; i++) {
				ConfigNode childNode = nodes[i];
				if (childNode.ObjectDepth < targetDepth) {
					return currentNode;
				}
				if (childNode.ObjectDepth > targetDepth) {
					continue;
				}

				childNodes.Add(BuildNode(nodes, i, childNode));
			}

			return currentNode;
		}

		private static List<TokenNode> GatherRootNodes(IReadOnlyList<ConfigNode> nodes) {
			List<TokenNode> rootNodes = new List<TokenNode>();
			int nodeCount = nodes.Count;
			for (int i = 0; i < nodeCount; i++) {
				ConfigNode node = nodes[i];
				if (node.ObjectDepth == 1) {
					rootNodes.Add(BuildNode(nodes, i, node));
				}
			}
			return rootNodes;
		}

		public ConfigDeserializer(IReadOnlyList<ConfigNode> nodes, bool verifyAllKeysSet) {
			this.verifyAllKeysSet = verifyAllKeysSet;
			typeCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
			rootNode = new TokenNode { listValues = GatherRootNodes(nodes) };
		}

		private Dictionary<string, FieldInfo> GetTypeInfo(Type type, EDataType eDataType) {
			if (eDataType != EDataType.NonGenericClass) {
				return null;
			}
			if (typeCache.ContainsKey(type)) {
				return typeCache[type];
			}
			
			Dictionary<string, FieldInfo> result = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
					.Where(field => Attribute.IsDefined(field, typeof(ConfigValueAttribute)))
					.ToDictionary(field => {
						string name = field.GetCustomAttribute<ConfigValueAttribute>().name ?? field.Name;
						Match match = Regex.Match(name, "^<(.+)>k__BackingField"); // TODO implement this
						return field.GetCustomAttribute<ConfigValueAttribute>().name ?? field.Name;
					}, field => field);
			typeCache[type] = result;
			return result;
		}

		private object DeserializeSimpleValue(TokenNode node, Type type) {
			//Debug.Assert(!eDataType.IsSingleValueType());
			if (node.simpleValue == null) {
				return type.IsValueType ? Activator.CreateInstance(type) : null;
			}
			object data = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(node.simpleValue);
			return data;
		}

		private void DeserializeListValue(TokenNode node, Type type, EDataType eDataType, IList resultList) {
			//Debug.Assert(node.listValues != null);
			foreach (TokenNode childNode in node.listValues) {
				if (childNode.key != null) {
					throw new InvalidDataException($"List-Entry on Line {childNode.lineNumber} contains a key: '{childNode.key}' (list values may not contain keys!)");
				}
				resultList.Add(DeserializeNodeValue(childNode, type, eDataType));
			}
		}

		private void DeserializeDictionary(TokenNode node, Type keyType, Type valueType, EDataType eValueType, IDictionary resultDictionary) {
			//Debug.Assert(node.listValues != null);
			//Debug.Assert(eKeyType.IsSingleValueType());
			foreach (TokenNode childNode in node.listValues) {
				if (childNode.key == null && !keyType.IsValueType) {
					throw new InvalidDataException($"Dictionary-Entry on Line {childNode.lineNumber} has no key! (key cannot be null)");
				}
				if (childNode.listValues == null && childNode.simpleValue == null) {
					throw new InvalidDataException($"Dictionary-Entry on Line {childNode.lineNumber} with key: '{childNode.key}' does not contain any values!");
				}
				object key = childNode.key == null ? Activator.CreateInstance(keyType) : TypeDescriptor.GetConverter(keyType).ConvertFromInvariantString(childNode.key);
				if (key == null) {
					throw new InvalidDataException($"Dictionary-Entry on Line {childNode.lineNumber}, key '{childNode.key}' parsed as 'null'!");
				}
				resultDictionary[key] = DeserializeNodeValue(childNode, valueType, eValueType);
			}
		}

		private object DeserializeSingleFieldType(TokenNode node, Type objectType, object instanceToUse, IReadOnlyList<FieldInfo> fieldChain) {
			int fieldCount = fieldChain.Count;
			if (fieldCount <= 0) {
				throw new InvalidDataException($"Tried to Deserialize SingleFieldType '{objectType.FullName}' but did not find any Fields!");
			}
			FieldInfo lastField = fieldChain[fieldCount - 1];
			EDataType lastEType = EDataTypes_Extensions.GetDataType(lastField.FieldType);
			bool isSingleValueType = lastEType.IsSingleValueType();
			if (isSingleValueType && node.simpleValue == null
					|| !isSingleValueType && node.listValues.Count == 0) {
				return null;
			}

			object lastFieldValue = DeserializeNodeValue(node, lastField.FieldType, lastEType);
			if (lastFieldValue == null) {
				return null;
			}

			object resultInstance = instanceToUse ?? Activator.CreateInstance(objectType);
			object lastInstance = resultInstance;
			for (int i = 0; i < fieldCount - 1; i++) {
				FieldInfo fieldInfo = fieldChain[i];
				object fieldValue = Activator.CreateInstance(fieldInfo.FieldType);
				fieldInfo.SetValue(lastInstance, fieldValue);
				lastInstance = fieldValue;
			}
			lastField.SetValue(lastInstance, lastFieldValue);
			return resultInstance;
		}

		private void DeserializeNonGenericObject(TokenNode node, Type objectType, EDataType eType, object objectInstance) {
			//Debug.Assert(node.listValues != null);
			Dictionary<string, FieldInfo> fieldInfos = GetTypeInfo(objectType, eType);
			List<string> keysToVerify = verifyAllKeysSet ? fieldInfos.Keys.ToList() : null;
			foreach (TokenNode childNode in node.listValues) {
				if (childNode.key == null) {
					throw new InvalidDataException($"Object-Entry on Line {childNode.lineNumber} has no key!");
				}
				if (!fieldInfos.ContainsKey(childNode.key)) {
					throw new InvalidDataException($"Object-Entry on Line {childNode.lineNumber} has invalid key: '{childNode.key}' possible keys are: ('{string.Join("', '", fieldInfos.Keys)}')");
				}
				FieldInfo fieldInfo = fieldInfos[childNode.key];
				if (childNode.listValues == null && childNode.simpleValue == null) {
					throw new InvalidDataException($"Object-Entry on Line {childNode.lineNumber} with key: '{childNode.key}' does not contain any values!");
				}
				Type fieldType = fieldInfo.FieldType;
				fieldInfo.SetValue(objectInstance, DeserializeNodeValue(childNode, fieldType, EDataTypes_Extensions.GetDataType(fieldType)));
				if (verifyAllKeysSet) {
					keysToVerify?.Remove(childNode.key);
				}
			}
			if (verifyAllKeysSet) {
				if (keysToVerify != null && keysToVerify.Count > 0) {
					throw new InvalidDataException($"Object on Line {node.lineNumber} was missing keys: ('{string.Join("', '", keysToVerify)}')");
				}
			}
		}

		private object DeserializeNodeValue(TokenNode node, Type type, EDataType eType, object instanceToUse = null) {
			switch (eType) {
				case EDataType.TypeConvertibleClass:
				case EDataType.Enum:
				case EDataType.Primitive:
				case EDataType.String: {
					return DeserializeSimpleValue(node, type);
				}
				case EDataType.GenericList: {
					IList list = (IList)(instanceToUse ?? Activator.CreateInstance(type));
					Type listValueType = type.GetGenericArguments()[0];
					DeserializeListValue(node, listValueType, EDataTypes_Extensions.GetDataType(listValueType), list);
					return list;
				}
				case EDataType.GenericDictionary: {
					IDictionary dictionary = (IDictionary)(instanceToUse ?? (IDictionary)Activator.CreateInstance(type));
					Type[] genericArguments = type.GetGenericArguments();
					Type dictKeyType = genericArguments[0];
					Type dictValueType = genericArguments[1];
					DeserializeDictionary(node, dictKeyType, dictValueType, EDataTypes_Extensions.GetDataType(dictValueType), dictionary);
					return dictionary;
				}
				case EDataType.NonGenericClass: {
					if (SingleFieldTypeAttribute.IsSingleFieldType(type)) {
						List<FieldInfo> resolveFieldChain = SingleFieldTypeAttribute.ResolveFieldChain(type);
						return DeserializeSingleFieldType(node, type, instanceToUse, resolveFieldChain);
					}
					if (node.listValues.Count == 0) {
						return null;
					}
					object objectInstance = instanceToUse ?? Activator.CreateInstance(type);
					DeserializeNonGenericObject(node, type, eType, objectInstance);
					return objectInstance;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(eType), eType, null);
			}
		}

		public ConfigType Deserialize() {
			Type configType = typeof(ConfigType);
			ConfigType instance = (ConfigType)DeserializeNodeValue(rootNode, configType, EDataTypes_Extensions.GetDataType(configType));
			return instance;
		}

		public ConfigType Deserialize(ConfigType instance) {
			Type configType = typeof(ConfigType);
			DeserializeNodeValue(rootNode, configType, EDataTypes_Extensions.GetDataType(configType), instance);
			return instance;
		}
	}
}