using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Reflection;
using BlazingTwistConfigTools.blazingtwist.config.serialization;
using BlazingTwistConfigTools.blazingtwist.config.types;

namespace BlazingTwistConfigTools.blazingtwist.config.deserialization {
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

		private static Dictionary<Type, Dictionary<string, FieldInfo>> GatherAllTypes(Dictionary<Type, Dictionary<string, FieldInfo>> cache, Type type, EDataType eDataType) {
			if (eDataType != EDataType.NonGenericClass) {
				return cache;
			}

			if (!cache.ContainsKey(type)) {
				Dictionary<string, FieldInfo> fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
						.Where(field => Attribute.IsDefined(field, typeof(ConfigValueAttribute)))
						.ToDictionary(field => field.GetCustomAttribute<ConfigValueAttribute>().name ?? field.Name, field => field);
				cache[type] = fieldInfos;

				foreach (Type fieldType in fieldInfos.Values.Select(field => field.FieldType)) {
					GatherAllTypes(cache, fieldType, EDataTypes_Extensions.GetDataType(fieldType));
					if (fieldType.IsGenericType) {
						foreach (Type genericArgument in fieldType.GetGenericArguments()) {
							GatherAllTypes(cache, genericArgument, EDataTypes_Extensions.GetDataType(genericArgument));
						}
					}
				}
			}
			return cache;
		}

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
			typeCache = GatherAllTypes(new Dictionary<Type, Dictionary<string, FieldInfo>>(), typeof(ConfigType), EDataTypes_Extensions.GetDataType(typeof(ConfigType)));
			rootNode = new TokenNode { listValues = GatherRootNodes(nodes) };
		}

		private object DeserializeSimpleValue(TokenNode node, Type type, EDataType eDataType) {
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

		private void DeserializeDictionary(TokenNode node, Type keyType, EDataType eKeyType, Type valueType, EDataType eValueType, IDictionary resultDictionary) {
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

		private void DeserializeNonGenericObject(TokenNode node, Type objectType, object objectInstance) {
			//Debug.Assert(node.listValues != null);
			Dictionary<string, FieldInfo> fieldInfos = typeCache[objectType];
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

		private object DeserializeNodeValue(TokenNode node, Type type, EDataType eType) {
			switch (eType) {
				case EDataType.TypeConvertibleClass:
				case EDataType.Enum:
				case EDataType.Primitive:
				case EDataType.String: {
					return DeserializeSimpleValue(node, type, eType);
				}
				case EDataType.GenericList: {
					IList list = (IList)Activator.CreateInstance(type);
					Type listValueType = type.GetGenericArguments()[0];
					DeserializeListValue(node, listValueType, EDataTypes_Extensions.GetDataType(listValueType), list);
					return list;
				}
				case EDataType.GenericDictionary: {
					IDictionary dictionary = (IDictionary)Activator.CreateInstance(type);
					Type[] genericArguments = type.GetGenericArguments();
					Type dictKeyType = genericArguments[0];
					Type dictValueType = genericArguments[1];
					DeserializeDictionary(node, dictKeyType, EDataTypes_Extensions.GetDataType(dictKeyType), dictValueType, EDataTypes_Extensions.GetDataType(dictValueType), dictionary);
					return dictionary;
				}
				case EDataType.NonGenericClass: {
					if (node.listValues.Count == 0) {
						return null;
					}
					object objectInstance = Activator.CreateInstance(type);
					DeserializeNonGenericObject(node, type, objectInstance);
					return objectInstance;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(eType), eType, null);
			}
		}

		/**
		 * TODO serialize to existing file
		 * 
		 * TODO versioning system
		 * 
		 * TODO
		 *  optional implicit deserialization (without requiring ConfigValue attributes)
		 *  - for all types
		 *  - for a set of types
		 */
		public ConfigType Deserialize() {
			Type configType = typeof(ConfigType);
			ConfigType configInstance = (ConfigType)Activator.CreateInstance(configType);
			DeserializeNonGenericObject(rootNode, configType, configInstance);
			return configInstance;
		}

		public ConfigType Deserialize(ConfigType instance) {
			Type configType = typeof(ConfigType);
			DeserializeNonGenericObject(rootNode, configType, instance);
			return instance;
		}
	}
}