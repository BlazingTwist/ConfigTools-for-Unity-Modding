using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BlazingTwistConfigTools.blazingtwist.config.types;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.blazingtwist.config.serialization {
	[AttributeUsage(AttributeTargets.Field)]
	[PublicAPI]
	public class ConfigValueAttribute : SerializationAttribute {
		public string name { get; }
		private int indentation { get; }
		private int emptyLinesAbove { get; }
		private string[] comments { get; }

		public ConfigValueAttribute([CallerLineNumber] int order = 0) : base(order) {
			comments = new string[] { };
		}

		public ConfigValueAttribute([CallerLineNumber] int order = 0,
				string name = null, int indentation = 0, int emptyLinesAbove = 0, params string[] comments) : base(order) {
			this.name = name;
			this.indentation = indentation;
			this.emptyLinesAbove = emptyLinesAbove;
			this.comments = comments ?? new string[] { };
		}

		public override IEnumerable<string> Serialize(ConfigSerializer serializer, FieldInfo fieldInfo, SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth) {
			IEnumerable<string> dataLines;
			if (serializationInfo.eDataType.IsSingleValueType()) {
				string dataString = ConfigSerializer.SerializeSingleValueType(serializationInfo);
				dataLines = new[] { SerializeOneliner(fieldInfo, serializationInfo, dataString, currentIndentation, currentObjectDepth) };
			} else {
				string declarationString = SerializeMultilineFieldDeclaration(fieldInfo, serializationInfo, currentIndentation, currentObjectDepth);
				dataLines = new[] { declarationString }
						.Concat(serializer.SerializeMultiValueType(serializationInfo, currentIndentation + indentation, currentObjectDepth + 1));

			}
			return Enumerable.Repeat("", emptyLinesAbove)
					.Concat(comments.Select(comment => new string('\t', currentIndentation + indentation) + "// " + comment))
					.Concat(dataLines);
		}

		private string SerializeOneliner(FieldInfo fieldInfo, SerializationInfo info, string dataString, int currentIndentation, int currentObjectDepth) {
			return new string('\t', currentIndentation + indentation)
					+ new string('-', currentObjectDepth)
					+ " "
					+ ConfigSerializer.FormatStringValue(name ?? fieldInfo.Name)
					+ " = "
					+ dataString;
		}

		private string SerializeMultilineFieldDeclaration(FieldInfo fieldInfo, SerializationInfo info, int currentIndentation, int currentObjectDepth) {
			return new string('\t', currentIndentation + indentation)
					+ new string('-', currentObjectDepth)
					+ " "
					+ ConfigSerializer.FormatStringValue(name ?? fieldInfo.Name)
					+ " :";
		}
	}
}