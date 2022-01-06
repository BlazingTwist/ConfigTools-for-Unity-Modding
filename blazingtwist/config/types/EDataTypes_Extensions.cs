using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace BlazingTwistConfigTools.config.types {
	public static class EDataTypes_Extensions {
		public static EDataType GetDataType(Type type) {
			if (Attribute.IsDefined(type, typeof(TypeConverterAttribute))) {
				return EDataType.TypeConvertibleClass;
			}
			if (type.IsEnum) {
				return EDataType.Enum;
			}
			if (type.IsPrimitive) {
				return EDataType.Primitive;
			}
			if (type == typeof(string)) {
				return EDataType.String;
			}
			if (type.IsGenericType) {
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				if (genericTypeDefinition == typeof(List<>)) {
					return EDataType.GenericList;
				}
				if (genericTypeDefinition == typeof(Dictionary<,>)) {
					return EDataType.GenericDictionary;
				}
				throw new InvalidDataException($"Unsupported generic type: {type}");
			}
			return EDataType.NonGenericClass;
		}

		public static bool IsSingleValueType(this EDataType eDataType) {
			return eDataType == EDataType.Enum
					|| eDataType == EDataType.Primitive
					|| eDataType == EDataType.String
					|| eDataType == EDataType.TypeConvertibleClass;
		}

		public static bool IsMultiValueType(this EDataType eDataType) {
			return !eDataType.IsSingleValueType();
		}
	}
}