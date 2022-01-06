using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BlazingTwistConfigTools.config.types;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config.serialization {
	public class ConfigSerializer {
		private readonly Dictionary<Type, List<ConfigTypeInfo>> typeCache;
		private readonly ConfigOptions options;

		public ConfigSerializer(ConfigOptions options) {
			typeCache = new Dictionary<Type, List<ConfigTypeInfo>>();
			this.options = options;
		}

		[NotNull]
		public IEnumerable<string> Serialize(object data) {
			return Serialize(data, 0, 1, options.keyFormatOption, options.valueFormatOption);
		}

		[NotNull]
		private IEnumerable<string> Serialize(object data, int currentIndentation, int currentObjectDepth, EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			if (data == null) {
				return new string[] { };
			}
			Type dataType = data.GetType();
			EDataType eDataType = EDataTypes_Extensions.GetDataType(dataType);
			SerializationInfo serializationInfo = new SerializationInfo(data, dataType, eDataType);
			return Serialize(serializationInfo, currentIndentation, currentObjectDepth, keyFormatOption, valueFormatOption);
		}

		[NotNull]
		private IEnumerable<string> Serialize(SerializationInfo info, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			return info.eDataType.IsSingleValueType()
					? new[] { SerializeSingleValueType(info, valueFormatOption) }
					: SerializeMultiValueType(info, currentIndentation, currentObjectDepth, keyFormatOption, valueFormatOption);
		}

		[NotNull]
		private IEnumerable<string> SerializeListValue(SerializationInfo info, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			return info.eDataType.IsSingleValueType()
					? new[] { new string('\t', currentIndentation) + new string(SpecialCharacters.objectDepth, currentObjectDepth) + " " + SerializeSingleValueType(info, valueFormatOption) }
					: new[] { new string('\t', currentIndentation) + new string(SpecialCharacters.objectDepth, currentObjectDepth) + " " + SpecialCharacters.objectAssignment }
							.Concat(SerializeMultiValueType(info, currentIndentation, currentObjectDepth + 1, keyFormatOption, valueFormatOption));
		}

		[NotNull]
		private IEnumerable<string> SerializeDictionaryEntry(SerializationInfo keyInfo, SerializationInfo valueInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			string keyString = new string('\t', currentIndentation) + new string(SpecialCharacters.objectDepth, currentObjectDepth) + " " + SerializeSingleValueType(keyInfo, keyFormatOption);
			if (valueInfo.eDataType.IsSingleValueType()) {
				// can write entries as single line
				return new[] { keyString + $" {SpecialCharacters.valueAssignment} " + SerializeSingleValueType(valueInfo, valueFormatOption) };
			}

			// need to write entries as declaration : \n value
			return new[] { keyString + " " + SpecialCharacters.objectAssignment }
					.Concat(SerializeMultiValueType(valueInfo, currentIndentation, currentObjectDepth + 1, keyFormatOption, valueFormatOption));
		}

		[NotNull]
		internal static string SerializeSingleValueType(SerializationInfo serializationInfo, EFormatOption valueFormatOption) {
			if (serializationInfo.dataInstance == null) {
				return SpecialCharacters.FormatStringValue(null, valueFormatOption);
			}
			if (!serializationInfo.eDataType.IsSingleValueType()) {
				throw new ArgumentOutOfRangeException("eDataType: " + serializationInfo.eDataType + " is not a single-value Type!");
			}
			string result = TypeDescriptor.GetConverter(serializationInfo.dataType).ConvertToInvariantString(serializationInfo.dataInstance);
			return SpecialCharacters.FormatStringValue(result, valueFormatOption);
		}

		[NotNull]
		internal static string SerializeValueAssignmentDeclaration(string keyString, int indentation, int objectDepth, EFormatOption keyFormatOption) {
			return new string('\t', indentation)
					+ new string(SpecialCharacters.objectDepth, objectDepth) + " "
					+ SpecialCharacters.FormatStringValue(keyString, keyFormatOption)
					+ $" {SpecialCharacters.valueAssignment} ";
		}
		
		[NotNull]
		internal static string SerializeObjectAssignmentDeclaration(string keyString, int indentation, int objectDepth, EFormatOption keyFormatOption) {
			return new string('\t', indentation)
					+ new string(SpecialCharacters.objectDepth, objectDepth) + " "
					+ SpecialCharacters.FormatStringValue(keyString, keyFormatOption)
					+ " " + SpecialCharacters.objectAssignment;
		}

		[NotNull]
		internal IEnumerable<string> SerializeMultiValueType(SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
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
							select SerializeListValue(valueSerializationInfo, currentIndentation, currentObjectDepth, keyFormatOption, valueFormatOption)
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
						result.AddRange(SerializeDictionaryEntry(keyInfo, valueInfo, currentIndentation, currentObjectDepth, keyFormatOption, valueFormatOption));
					}
					return result;
				}
				case EDataType.NonGenericClass:
					return SerializeNonGenericClass(serializationInfo, currentIndentation, currentObjectDepth, keyFormatOption, valueFormatOption);

				case EDataType.TypeConvertibleClass:
				case EDataType.Enum:
				case EDataType.Primitive:
				case EDataType.String:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[NotNull]
		private IEnumerable<string> SerializeNonGenericClass(SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			List<ConfigTypeInfo> typeFieldInfo;
			if (!typeCache.ContainsKey(serializationInfo.dataType)) {
				typeFieldInfo = ConfigTypeInfo.GatherTypeInfo(serializationInfo.dataType, options).ToList();
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
					select configInfo.fieldSerializer.Serialize(this, configInfo.fieldInfo, info, currentIndentation, currentObjectDepth, keyFormatOption, valueFormatOption)) {
				result.AddRange(serializeResult);
			}
			return result;
		}
	}
}