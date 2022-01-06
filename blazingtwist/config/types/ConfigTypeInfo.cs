using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlazingTwistConfigTools.config.attributes;

namespace BlazingTwistConfigTools.config.types {
	public class ConfigTypeInfo {
		public readonly FieldInfo fieldInfo;
		public readonly SerializationAttribute serializationAttribute;

		private ConfigTypeInfo(FieldInfo fieldInfo, SerializationAttribute serializationAttribute) {
			this.fieldInfo = fieldInfo;
			this.serializationAttribute = serializationAttribute;
		}

		public static IEnumerable<ConfigTypeInfo> GatherTypeInfo<ConfigType>() {
			return GatherTypeInfo(typeof(ConfigType));
		}

		public static IEnumerable<ConfigTypeInfo> GatherTypeInfo(Type configType) {
			return configType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
					.Where(field => Attribute.IsDefined(field, typeof(SerializationAttribute)))
					.SelectMany(field
							=> field.GetCustomAttributes<SerializationAttribute>()
									.Select(attribute => new ConfigTypeInfo(field, attribute))
					).OrderBy(info => info.serializationAttribute.order);
		}
	}
}