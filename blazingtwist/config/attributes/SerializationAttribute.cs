using System;
using System.Collections.Generic;
using System.Reflection;
using BlazingTwistConfigTools.config.serialization;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config.attributes {
	[MeansImplicitUse]
	public abstract class SerializationAttribute : Attribute {
		public int order { get; }

		protected SerializationAttribute(int order) {
			this.order = order;
		}

		[NotNull]
		public abstract IEnumerable<string> Serialize(ConfigSerializer serializer, FieldInfo fieldInfo, SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption);
	}
}