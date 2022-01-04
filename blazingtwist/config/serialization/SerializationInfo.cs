using System;
using BlazingTwistConfigTools.blazingtwist.config.types;

namespace BlazingTwistConfigTools.blazingtwist.config.serialization {
	public class SerializationInfo {
		public readonly object dataInstance;
		public readonly Type dataType;
		public readonly EDataType eDataType;

		public SerializationInfo(object dataInstance, Type dataType, EDataType eDataType) {
			this.dataInstance = dataInstance;
			this.dataType = dataType;
			this.eDataType = eDataType;
		}
	}
}