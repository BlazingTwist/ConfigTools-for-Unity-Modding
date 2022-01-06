using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BlazingTwistConfigTools.config.types;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config.attributes {
	[AttributeUsage(AttributeTargets.Class)]
	[MeansImplicitUse]
	[PublicAPI]
	public class SingleFieldTypeAttribute : Attribute {
		private string fieldName { get; }

		public SingleFieldTypeAttribute(string fieldName) {
			this.fieldName = fieldName;
		}

		internal static void ResolveForSerialization(ref Type type, ref EDataType eType, ref object instance) {
			SingleFieldTypeAttribute attribute = type.GetCustomAttribute<SingleFieldTypeAttribute>();
			if (attribute == null) {
				return;
			}
			while (true) {
				FieldInfo targetField = type.GetField(attribute.fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				if (targetField == null) {
					throw new InvalidDataException($"Type '{type.FullName}' has 'SingleFieldType'-Attribute, but does not contain specified field: '{attribute.fieldName}'");
				}
				type = targetField.FieldType;
				instance = instance == null ? null : targetField.GetValue(instance);
				attribute = type.GetCustomAttribute<SingleFieldTypeAttribute>();
				if (attribute != null) {
					continue;
				}
				eType = EDataTypes_Extensions.GetDataType(type);
				break;
			}
		}

		internal static bool IsSingleFieldType(Type type) {
			return type.GetCustomAttribute<SingleFieldTypeAttribute>() != null;
		}

		internal static List<FieldInfo> ResolveFieldChain(Type startingType) {
			List<FieldInfo> result = new List<FieldInfo>();
			Type type = startingType;
			while (true) {
				SingleFieldTypeAttribute attribute = type.GetCustomAttribute<SingleFieldTypeAttribute>();
				if (attribute == null) {
					break;
				}
				FieldInfo targetField = type.GetField(attribute.fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				if (targetField == null) {
					throw new InvalidDataException($"Type '{type.FullName}' has 'SingleFieldType'-Attribute, but does not contain specified field: '{attribute.fieldName}'");
				}
				result.Add(targetField);
				type = targetField.FieldType;
			}
			return result;
		}
	}
}