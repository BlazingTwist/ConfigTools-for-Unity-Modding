using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BlazingTwistConfigTools.configurables {
	public class KeyTypeConverter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if (value is string stringValue) {
				bool negated = stringValue.StartsWith("!");
				string keyString = negated ? stringValue.Substring(1) : stringValue;
				object keyCode = TypeDescriptor.GetConverter(typeof(KeyCode)).ConvertFromString(context, culture, keyString);
				if (keyCode == null) {
					throw new InvalidDataException($"Failed to deserialize KeyCode: '{keyString}', refer to https://docs.unity3d.com/ScriptReference/KeyCode.html for a list of KeyCodes");
				}
				return new Key((KeyCode)keyCode, negated);
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string) && value is Key key) {
				return (key.negated ? "!" : "") + TypeDescriptor.GetConverter(typeof(KeyCode)).ConvertToString(context, culture, key.keyCode);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}