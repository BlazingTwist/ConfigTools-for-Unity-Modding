using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BlazingTwistConfigTools.config.serialization;
using BlazingTwistConfigTools.config.types;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config.attributes {
	[AttributeUsage(AttributeTargets.Field)]
	[PublicAPI]
	public class ConfigValueAttribute : SerializationAttribute {
		public string name { get; }
		private int indentation { get; }
		private int emptyLinesAbove { get; }
		private EFormatOption _keyFormatOption { get; }
		private EFormatOption _valueFormatOption { get; }
		private string[] comments { get; }

		public ConfigValueAttribute([CallerLineNumber] int order = 0) : base(order) {
			comments = new string[] { };
		}

		public ConfigValueAttribute([CallerLineNumber] int order = 0,
				string name = null, int indentation = 0, int emptyLinesAbove = 0,
				EFormatOption keyFormatOption = EFormatOption.UseDefault, EFormatOption valueFormatOption = EFormatOption.UseDefault,
				params string[] comments) : base(order) {
			this.name = name;
			this.indentation = indentation;
			this.emptyLinesAbove = emptyLinesAbove;
			_keyFormatOption = keyFormatOption;
			_valueFormatOption = valueFormatOption;
			this.comments = comments ?? new string[] { };
		}

		private EFormatOption GetKeyFormatOption(EFormatOption fallback) {
			return _keyFormatOption == EFormatOption.UseDefault ? fallback : _keyFormatOption;
		}
		
		private EFormatOption GetValueFormatOption(EFormatOption fallback) {
			return _valueFormatOption == EFormatOption.UseDefault ? fallback : _valueFormatOption;
		}

		public override IEnumerable<string> Serialize(ConfigSerializer serializer, FieldInfo fieldInfo, SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			IEnumerable<string> dataLines;
			keyFormatOption = GetKeyFormatOption(keyFormatOption);
			valueFormatOption = GetValueFormatOption(valueFormatOption);
			
			if (serializationInfo.eDataType.IsSingleValueType()) {
				string dataString = ConfigSerializer.SerializeSingleValueType(serializationInfo, valueFormatOption);
				dataLines = new[] { SerializeOneliner(fieldInfo, dataString, currentIndentation, currentObjectDepth, keyFormatOption) };
			} else {
				string declarationString = SerializeMultilineFieldDeclaration(fieldInfo, currentIndentation, currentObjectDepth, keyFormatOption);
				dataLines = new[] { declarationString }
						.Concat(serializer.SerializeMultiValueType(serializationInfo, currentIndentation + indentation, currentObjectDepth + 1, keyFormatOption, valueFormatOption));
			}
			return Enumerable.Repeat("", emptyLinesAbove)
					.Concat(comments.Select(comment => new string('\t', currentIndentation + indentation) + SpecialCharacters.singleLineComment + " " + comment))
					.Concat(dataLines);
		}

		private string SerializeOneliner(FieldInfo fieldInfo, string dataString, int currentIndentation, int currentObjectDepth, EFormatOption keyFormatOption) {
			return new string('\t', currentIndentation + indentation)
					+ new string(SpecialCharacters.objectDepth, currentObjectDepth)
					+ " "
					+ SpecialCharacters.FormatStringValue(name ?? fieldInfo.Name, keyFormatOption)
					+ $" {SpecialCharacters.valueAssignment} "
					+ dataString;
		}

		private string SerializeMultilineFieldDeclaration(FieldInfo fieldInfo, int currentIndentation, int currentObjectDepth, EFormatOption keyFormatOption) {
			return new string('\t', currentIndentation + indentation)
					+ new string(SpecialCharacters.objectDepth, currentObjectDepth)
					+ " "
					+ SpecialCharacters.FormatStringValue(name ?? fieldInfo.Name, keyFormatOption)
					+ " " + SpecialCharacters.objectAssignment;
		}
	}
}