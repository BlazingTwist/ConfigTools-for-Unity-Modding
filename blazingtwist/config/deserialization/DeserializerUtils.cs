using System.Collections.Generic;
using System.IO;
using BlazingTwistConfigTools.config.types;

namespace BlazingTwistConfigTools.config.deserialization {
	public static class DeserializerUtils {
		public static IEnumerable<ConfigNode> Tokenize(IEnumerable<string> lines) {
			return Tokenize(new LineReader(lines));
		}

		public static IEnumerable<ConfigNode> Tokenize(LineReader lineReader) {
			TokenCollector collector = new TokenCollector();
			lineReader.Next(out char? character, out bool wasEscaped);
			while (character != null) {
				if (HandleCommonCharacters(lineReader, character, wasEscaped, collector)) {
					// already handled, do nothing
				} else if (character == '/') {
					lineReader.Next(out character, out wasEscaped);
					if (character == '/' && !wasEscaped) {
						ReadSingleLineComment(lineReader);
					} else if (character == '*' && !wasEscaped) {
						ReadRangedComment(lineReader);
					} else {
						collector.AddToken(ETokenType.StringValue, '/', lineReader.LineNumber);
						if (HandleCommonCharacters(lineReader, character, wasEscaped, collector)) {
							// already handled, do nothing
						} else {
							//Debug.Assert(character != null, nameof(character) + " != null");
							collector.AddToken(ETokenType.StringValue, character.Value, lineReader.LineNumber);
						}
					}
				} else {
					collector.AddToken(ETokenType.StringValue, character.Value, lineReader.LineNumber);
				}

				lineReader.Next(out character, out wasEscaped);
			}

			return collector.GetConfigNodes();
		}

		private static bool HandleCommonCharacters(LineReader reader, char? currentChar, bool wasEscaped, TokenCollector collector) {
			if (currentChar == null) {
				return true;
			}
			if (wasEscaped) {
				collector.AddToken(ETokenType.StringValue, currentChar.Value, reader.LineNumber);
				return true;
			}
			if (char.IsWhiteSpace(currentChar.Value)) {
				return true;
			}
			if (currentChar == SpecialCharacters.stringChar) {
				ReadString(reader, collector);
				return true;
			}
			if (currentChar == SpecialCharacters.objectDepth) {
				collector.AddToken(ETokenType.ObjectDepth, currentChar.Value, reader.LineNumber);
				return true;
			}
			if (currentChar == SpecialCharacters.valueAssignment || currentChar == SpecialCharacters.objectAssignment) {
				collector.AddToken(ETokenType.Assignment, currentChar.Value, reader.LineNumber);
				return true;
			}
			if (currentChar == SpecialCharacters.nullChar) {
				collector.AddNullToken(reader.LineNumber);
				return true;
			}
			return false;
		}

		private static void ReadString(LineReader reader, TokenCollector collector) {
			int stringStartLineNumber = reader.LineNumber;
			reader.Next(out char? character, out bool wasEscaped);
			while (character != null) {
				if (!wasEscaped && character == SpecialCharacters.stringChar) {
					return;
				}
				collector.AddToken(ETokenType.StringValue, character.Value, stringStartLineNumber);

				reader.Next(out character, out wasEscaped);
			}
			throw new InvalidDataException($"Quoted string opened on line {stringStartLineNumber} but was never closed!");
		}

		private static void ReadSingleLineComment(LineReader reader) {
			reader.PointerToNextLine();
		}

		private static void ReadRangedComment(LineReader reader) {
			int commentStartLineNumber = reader.LineNumber;
			reader.Next(out char? character, out bool wasEscaped);
			bool previousWasStar = false;
			while (character != null) {
				if (!wasEscaped) {
					if (previousWasStar && character == '/') {
						return;
					}
					previousWasStar = character == '*';
				}
				reader.Next(out character, out wasEscaped);
			}
			throw new InvalidDataException($"Ranged comment opened on line {commentStartLineNumber} but was never closed!");
		}
	}
}