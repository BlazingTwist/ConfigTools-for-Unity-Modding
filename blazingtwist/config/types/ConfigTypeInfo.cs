using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlazingTwistConfigTools.config.attributes;
using BlazingTwistConfigTools.config.serialization;

namespace BlazingTwistConfigTools.config.types {
	public class ConfigTypeInfo {
		public readonly FieldInfo fieldInfo;
		public readonly IFieldSerializer fieldSerializer;

		private ConfigTypeInfo(FieldInfo fieldInfo, IFieldSerializer fieldSerializer) {
			this.fieldInfo = fieldInfo;
			this.fieldSerializer = fieldSerializer;
		}

		public static IEnumerable<ConfigTypeInfo> GatherTypeInfo(Type configType, ConfigOptions options) {
			FieldInfo[] fieldInfos = configType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			
			IEnumerable<ConfigTypeInfo> result = fieldInfos
					.SelectMany(field => field.GetCustomAttributes<SerializationAttribute>()
							.Select(attribute => new ConfigTypeInfo(field, attribute))
					).OrderBy(info => info.fieldSerializer.GetOrder());

			EFieldSelectorOption selectorOption = options.GetSelectorOption(configType);
			if (selectorOption == EFieldSelectorOption.Implicit) {
				result = result.Concat(
						fieldInfos.Where(field => !Attribute.IsDefined(field, typeof(SerializationAttribute)))
								.Select(field => new ConfigTypeInfo(field, ImplicitFieldSerializer.Instance()))
				);
			}
			
			return result;
		}
	}
}