using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlazingTwistConfigTools.config.deserialization;
using NUnit.Framework;

namespace BlazingTwistConfigToolsTest.config.deserialization {
	[TestFixture]
	public class LineReader_Test {
		private static KeyValuePair<char, bool>[] GetTestLineCharacters() {
			return new[] {
					new KeyValuePair<char, bool>('1', false),
					new KeyValuePair<char, bool>('2', false),
					new KeyValuePair<char, bool>('3', false),
					new KeyValuePair<char, bool>('\n', false),
					new KeyValuePair<char, bool>('\n', false),
					new KeyValuePair<char, bool>('4', true),
					new KeyValuePair<char, bool>('5', true),
					new KeyValuePair<char, bool>('6', true),
					new KeyValuePair<char, bool>('\n', true)
			};
		}

		private static IEnumerable<string> GetTestLines() {
			return new[] {
					"123",
					"",
					"\\4\\5\\6\\",
					""
			};
		}

		private static string GetTestLinesAsString() {
			return string.Join("\n", GetTestLines());
		}

		[Test]
		public void Constructor_TextReader() {
			string testLinesAsString = GetTestLinesAsString();
			Test_Next_WhenReachedEnd_ReturnsNull(new LineReader(new StringReader(testLinesAsString)));
			Test_Next_ReturnsExpectedSequence(new LineReader(new StringReader(testLinesAsString)));
		}

		[Test]
		public void Constructor_IEnumerableOfString() {
			IEnumerable<string> testLines = GetTestLines().ToArray();
			Test_Next_WhenReachedEnd_ReturnsNull(new LineReader(testLines));
			Test_Next_ReturnsExpectedSequence(new LineReader(testLines));
		}

		private static void Test_Next_WhenReachedEnd_ReturnsNull(LineReader lineReader) {
			int expectedNonNullCharacters = GetTestLineCharacters().Length;
			for (int i = 1; i <= expectedNonNullCharacters; i++) {
				lineReader.Next(out char? consumedChar, out bool _);
				Assert.NotNull(consumedChar, $"consumed character number {i} was null, but expected {expectedNonNullCharacters} non-null characters!");
			}
			for (int i = 0; i < 5; i++) {
				lineReader.Next(out char? consumedChar, out bool _);
				Assert.Null(consumedChar, $"consumed character number {i + expectedNonNullCharacters} was not null, but expected all after {expectedNonNullCharacters - 1} to be null!");
			}
		}

		private static void Test_Next_ReturnsExpectedSequence(LineReader lineReader) {
			KeyValuePair<char, bool>[] characters = GetTestLineCharacters();
			int charCount = characters.Length;
			for (int i = 0; i < charCount; i++) {
				lineReader.Next(out char? consumedChar, out bool wasEscaped);
				Assert.NotNull(consumedChar);
				Assert.AreEqual(characters[i].Key, consumedChar.Value);
				Assert.AreEqual(characters[i].Value, wasEscaped);
			}
		}
	}
}