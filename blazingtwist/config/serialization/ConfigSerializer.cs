using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BlazingTwistConfigTools.blazingtwist.config.types;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.blazingtwist.config.serialization {
	public class ConfigSerializer {
		private readonly Dictionary<Type, List<ConfigTypeInfo>> typeCache;

		public ConfigSerializer() {
			typeCache = new Dictionary<Type, List<ConfigTypeInfo>>();
		}

		[NotNull]
		public IEnumerable<string> Serialize(object data) {
			return Serialize(data, 0, 1);
		}

		[NotNull]
		private IEnumerable<string> Serialize(object data, int currentIndentation, int currentObjectDepth) {
			if (data == null) {
				return new string[] { };
			}
			Type dataType = data.GetType();
			EDataType eDataType = EDataTypes_Extensions.GetDataType(dataType);
			SerializationInfo serializationInfo = new SerializationInfo(data, dataType, eDataType);
			return Serialize(serializationInfo, currentIndentation, currentObjectDepth);
		}

		[NotNull]
		private IEnumerable<string> Serialize(SerializationInfo info, int currentIndentation, int currentObjectDepth) {
			return info.eDataType.IsSingleValueType()
					? new[] { SerializeSingleValueType(info) }
					: SerializeMultiValueType(info, currentIndentation, currentObjectDepth);
		}

		[NotNull]
		private IEnumerable<string> SerializeListValue(SerializationInfo info, int currentIndentation, int currentObjectDepth) {
			return info.eDataType.IsSingleValueType()
					? new[] { new string('\t', currentIndentation) + new string('-', currentObjectDepth) + " " + SerializeSingleValueType(info) }
					: new[] { new string('\t', currentIndentation) + new string('-', currentObjectDepth) + " :" }
							.Concat(SerializeMultiValueType(info, currentIndentation, currentObjectDepth + 1));
		}

		[NotNull]
		private IEnumerable<string> SerializeDictionaryEntry(SerializationInfo keyInfo, SerializationInfo valueInfo, int currentIndentation, int currentObjectDepth) {
			string keyString = new string('\t', currentIndentation) + new string('-', currentObjectDepth) + " " + SerializeSingleValueType(keyInfo);
			if (valueInfo.eDataType.IsSingleValueType()) {
				// can write entries as single line
				return new[] { keyString + " = " + SerializeSingleValueType(valueInfo) };
			}

			// need to write entries as declaration : \n value
			return new[] { keyString + " :" }
					.Concat(SerializeMultiValueType(valueInfo, currentIndentation, currentObjectDepth + 1));
		}

		[NotNull]
		public static string SerializeSingleValueType(SerializationInfo serializationInfo) {
			if (serializationInfo.dataInstance == null) {
				return FormatStringValue(null);
			}
			if (!serializationInfo.eDataType.IsSingleValueType()) {
				throw new ArgumentOutOfRangeException("eDataType: " + serializationInfo.eDataType + " is not a single-value Type!");
			}
			string result = TypeDescriptor.GetConverter(serializationInfo.dataType).ConvertToInvariantString(serializationInfo.dataInstance);
			return FormatStringValue(result);
		}

		[NotNull]
		public static string FormatStringValue(string value) {
			if (value == null) {
				return "~";
			}
			string formatValue = value.Replace("\\", "\\\\")
					.Replace("\"", "\\\"");
			return "\"" + formatValue + "\"";
		}

		[NotNull]
		public IEnumerable<string> SerializeMultiValueType(SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth) {
			if (serializationInfo.dataInstance == null) {
				return new string[] { };
			}
			if (!serializationInfo.eDataType.IsMultiValueType()) {
				throw new ArgumentOutOfRangeException("eDataType: " + serializationInfo.eDataType + " is not a multi-value Type!");
			}
			switch (serializationInfo.eDataType) {
				case EDataType.GenericList: {
					IList list = (IList)serializationInfo.dataInstance;
					Type valueType = serializationInfo.dataType.GetGenericArguments()[0];
					EDataType valueEDataType = EDataTypes_Extensions.GetDataType(valueType);
					return (
							from object value in list
							select new SerializationInfo(value, valueType, valueEDataType)
							into valueSerializationInfo
							select SerializeListValue(valueSerializationInfo, currentIndentation, currentObjectDepth)
					).SelectMany(sList => sList);
				}
				case EDataType.GenericDictionary: {
					IDictionary dictionary = (IDictionary)serializationInfo.dataInstance;
					Type keyType = serializationInfo.dataType.GetGenericArguments()[0];
					EDataType eKeyType = EDataTypes_Extensions.GetDataType(keyType);
					Type valueType = serializationInfo.dataType.GetGenericArguments()[1];
					EDataType eValueType = EDataTypes_Extensions.GetDataType(valueType);

					if (!eKeyType.IsSingleValueType()) {
						throw new InvalidDataException("Key of Config Dictionary must be Single-Value Type!");
					}

					List<string> result = new List<string>();
					foreach (DictionaryEntry entry in dictionary) {
						SerializationInfo keyInfo = new SerializationInfo(entry.Key, keyType, eKeyType);
						SerializationInfo valueInfo = new SerializationInfo(entry.Value, valueType, eValueType);
						result.AddRange(SerializeDictionaryEntry(keyInfo, valueInfo, currentIndentation, currentObjectDepth));
					}
					return result;
				}
				case EDataType.NonGenericClass:
					return SerializeNonGenericClass(serializationInfo, currentIndentation, currentObjectDepth);

				case EDataType.TypeConvertibleClass:
				case EDataType.Enum:
				case EDataType.Primitive:
				case EDataType.String:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[NotNull]
		private IEnumerable<string> SerializeNonGenericClass(SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth) {
			List<ConfigTypeInfo> typeFieldInfo;
			if (!typeCache.ContainsKey(serializationInfo.dataType)) {
				typeFieldInfo = ConfigTypeInfo.GatherTypeInfo(serializationInfo.dataType).ToList();
				typeCache[serializationInfo.dataType] = typeFieldInfo;
			} else {
				typeFieldInfo = typeCache[serializationInfo.dataType];
			}

			List<string> result = new List<string>();
			foreach (IEnumerable<string> serializeResult in
					from configInfo in typeFieldInfo
					let fieldValue = configInfo.fieldInfo.GetValue(serializationInfo.dataInstance)
					let fieldType = configInfo.fieldInfo.FieldType
					let info = new SerializationInfo(fieldValue, fieldType, EDataTypes_Extensions.GetDataType(fieldType))
					select configInfo.serializationAttribute.Serialize(this, configInfo.fieldInfo, info, currentIndentation, currentObjectDepth)) {
				result.AddRange(serializeResult);
			}
			return result;
		}
	}
}