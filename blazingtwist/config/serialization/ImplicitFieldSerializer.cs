using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlazingTwistConfigTools.config.types;

namespace BlazingTwistConfigTools.config.serialization {
	public class ImplicitFieldSerializer : IFieldSerializer {
		public static ImplicitFieldSerializer Instance() => instance ?? (instance = new ImplicitFieldSerializer());
		private static ImplicitFieldSerializer instance;

		private ImplicitFieldSerializer() { }

		public int GetOrder() {
			return int.MaxValue;
		}

		public IEnumerable<string> Serialize(ConfigSerializer serializer, FieldInfo fieldInfo, SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			if (serializationInfo.eDataType.IsSingleValueType()) {
				string assignmentString = ConfigSerializer.SerializeValueAssignmentDeclaration(fieldInfo.Name, currentIndentation, currentObjectDepth, keyFormatOption);
				string dataString = ConfigSerializer.SerializeSingleValueType(serializationInfo, valueFormatOption);
				return new[] { assignmentString + dataString };
			}
			string declarationString = ConfigSerializer.SerializeObjectAssignmentDeclaration(fieldInfo.Name, currentIndentation, currentObjectDepth, keyFormatOption);
			return new[] { declarationString }
					.Concat(serializer.SerializeMultiValueType(serializationInfo, currentIndentation, currentObjectDepth + 1, keyFormatOption, valueFormatOption));
		}
	}
}