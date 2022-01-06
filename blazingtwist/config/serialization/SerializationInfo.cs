using System;
using BlazingTwistConfigTools.config.attributes;
using BlazingTwistConfigTools.config.types;

namespace BlazingTwistConfigTools.config.serialization {
	public class SerializationInfo {
		public readonly object dataInstance;
		public readonly Type dataType;
		public readonly EDataType eDataType;

		public SerializationInfo(object dataInstance, Type dataType, EDataType eDataType) {
			SingleFieldTypeAttribute.ResolveForSerialization(ref dataType, ref eDataType, ref dataInstance);
			this.dataInstance = dataInstance;
			this.dataType = dataType;
			this.eDataType = eDataType;
		}
	}
}