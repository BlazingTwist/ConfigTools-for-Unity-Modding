using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config.serialization {
	public interface IFieldSerializer {
		
		int GetOrder();
		
		[NotNull]
		IEnumerable<string> Serialize(ConfigSerializer serializer, FieldInfo fieldInfo, SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption);
	}
}