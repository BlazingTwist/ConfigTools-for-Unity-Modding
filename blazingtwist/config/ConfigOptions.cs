using System;
using System.Collections.Generic;
using System.Reflection;
using BlazingTwistConfigTools.config.attributes;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config {
	/// <summary>
	/// Configure how the De/serializer behaves
	/// </summary>
	[PublicAPI]
	public class ConfigOptions {
		/// <summary>
		/// If enabled, deserializer throws an Exception if the config file is missing a key.
		/// </summary>
		public bool verifyAllKeysSet;

		/// <summary>
		/// Default behaviour for serializing Keys
		/// Can be overwritten by Attributes on Serialized Fields
		/// </summary>
		public EFormatOption keyFormatOption = EFormatOption.UseDefault;

		/// <summary>
		/// Default behaviour for serializing values
		/// Can be overwritten by Attributes on Serialized Fields
		/// </summary>
		public EFormatOption valueFormatOption = EFormatOption.UseDefault;

		/// <summary>
		/// Default behaviour for finding Fields that can be de/serialized.
		/// Can be overwritten by specifying <see cref="explicitTypes"/> or <see cref="implicitTypes"/>
		/// </summary>
		public EFieldSelectorOption fieldSelectorOption = EFieldSelectorOption.Explicit;

		/// <summary>
		/// These Types will always use explicit Field search (field with <see cref="BlazingTwistConfigTools.config.attributes.ConfigValueAttribute"/> attribute)
		/// </summary>
		public List<Type> explicitTypes;

		/// <summary>
		/// These Types will always use implicit Field search (all fields in the class will be de/serialized)
		/// </summary>
		public List<Type> implicitTypes;

		internal bool IsFieldRelevant(Type containingType, FieldInfo field) {
			switch (GetSelectorOption(containingType)) {
				case EFieldSelectorOption.Implicit:
					return true;
				case EFieldSelectorOption.Explicit:
					return Attribute.IsDefined(field, typeof(ConfigValueAttribute));
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal EFieldSelectorOption GetSelectorOption(Type type) {
			EFieldSelectorOption selectorOption = fieldSelectorOption;
			if (explicitTypes != null && explicitTypes.Contains(type)) {
				selectorOption = EFieldSelectorOption.Explicit;
			} else if (implicitTypes != null && implicitTypes.Contains(type)) {
				selectorOption = EFieldSelectorOption.Implicit;
			}
			return selectorOption;
		}
	}
}