using System;
using System.Linq;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.config {
	public static class SpecialCharacters {
		public const char objectDepth = '-';
		public const char valueAssignment = '=';
		public const char objectAssignment = ':';
		public const char nullChar = '~';
		public const char stringChar = '"';
		public const string singleLineComment = "//";
		public const string multiLineComment = "/*";
		public const string multiLineCommentEnd = "*/";
		public const char escapeChar = '\\';

		private static bool ContainsSpecialChar([NotNull] string value) {
			return value.Contains(objectDepth.ToString())
					|| value.Contains(valueAssignment.ToString())
					|| value.Contains(objectAssignment.ToString())
					|| value.Contains(nullChar.ToString())
					|| value.Contains(stringChar.ToString())
					|| value.Contains(singleLineComment)
					|| value.Contains(multiLineComment)
					|| value.Contains(escapeChar.ToString());
		}

		private static bool ContainsWhiteSpace([NotNull] string value) {
			return value.ToCharArray().Any(char.IsWhiteSpace);
		}

		private static string EscapeCharacter(this string value, [NotNull] string character) {
			return value.Replace(character, escapeChar + character);
		}

		private static string QuoteCharacter(this string value, [NotNull] string character) {
			return value.Replace(character, stringChar + character + stringChar);
		}

		[NotNull]
		public static string FormatStringValue([CanBeNull] string value, EFormatOption option) {
			if (value == null) {
				return "~";
			}
			bool containsWhiteSpace = ContainsWhiteSpace(value);
			bool containsSpecialChar = ContainsSpecialChar(value);
			if (option != EFormatOption.AlwaysQuote && !containsWhiteSpace && !containsSpecialChar) {
				return value;
			}

			switch (option) {
				case EFormatOption.Escape:
					value = value.EscapeCharacter(escapeChar.ToString());
					if (containsWhiteSpace) {
						char[] whiteSpaceEscapedChars = value.ToCharArray().SelectMany(c => char.IsWhiteSpace(c) ? new[] { escapeChar, c } : new[] { c }).ToArray();
						value = new string(whiteSpaceEscapedChars);
					}
					if (containsSpecialChar) {
						value = value.EscapeCharacter(objectDepth.ToString())
								.EscapeCharacter(valueAssignment.ToString())
								.EscapeCharacter(objectAssignment.ToString())
								.EscapeCharacter(nullChar.ToString())
								.EscapeCharacter(stringChar.ToString())
								.EscapeCharacter(singleLineComment)
								.EscapeCharacter(multiLineComment);
					}
					return value;
				case EFormatOption.QuoteCharacter:
					value = value.EscapeCharacter(escapeChar.ToString())
							.EscapeCharacter(stringChar.ToString());
					if (containsWhiteSpace) {
						char[] whiteSpaceQuotedChars = value.ToCharArray().SelectMany(c => char.IsWhiteSpace(c) ? new[] { stringChar, c, stringChar } : new[] { c }).ToArray();
						value = new string(whiteSpaceQuotedChars);
					}
					if (containsSpecialChar) {
						value = value.QuoteCharacter(objectDepth.ToString())
								.QuoteCharacter(valueAssignment.ToString())
								.QuoteCharacter(objectAssignment.ToString())
								.QuoteCharacter(nullChar.ToString())
								.QuoteCharacter(singleLineComment)
								.QuoteCharacter(multiLineComment);
					}
					return value;
				case EFormatOption.AlwaysQuote:
				case EFormatOption.UseDefault:
				case EFormatOption.QuoteValue:
					return stringChar
							+ value.EscapeCharacter(escapeChar.ToString())
									.EscapeCharacter(stringChar.ToString())
							+ stringChar;
				default:
					throw new ArgumentOutOfRangeException(nameof(option), option, null);
			}
		}
	}
}