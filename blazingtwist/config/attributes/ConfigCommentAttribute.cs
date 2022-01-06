using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BlazingTwistConfigTools.config.serialization;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config.attributes {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	[PublicAPI]
	public class ConfigCommentAttribute : SerializationAttribute {
		private int indentation { get; }
		private int emptyLinesAbove { get; }
		private string[] comments { get; }

		public ConfigCommentAttribute([CallerLineNumber] int order = 0,
				int indentation = 0, int emptyLinesAbove = 0, params string[] comments) : base(order) {
			this.indentation = indentation;
			this.emptyLinesAbove = emptyLinesAbove;
			this.comments = comments ?? new string[] { };
		}

		public override IEnumerable<string> Serialize(ConfigSerializer serializer, FieldInfo fieldInfo, SerializationInfo serializationInfo, int currentIndentation, int currentObjectDepth,
				EFormatOption keyFormatOption, EFormatOption valueFormatOption) {
			return Enumerable.Repeat("", emptyLinesAbove)
					.Concat(comments.Select(comment => new string('\t', currentIndentation + indentation)
							+ SpecialCharacters.multiLineComment + " " + comment + " " + SpecialCharacters.multiLineCommentEnd));
		}
	}
}