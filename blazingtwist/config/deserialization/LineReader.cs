using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlazingTwistConfigTools.config.deserialization {
	public class LineReader {
		private readonly List<char[]> lines;
		private readonly int lineCount;

		private int lineIndex;
		private int charIndex;
		private char[] currentLine;
		private int currentLineLength;

		private bool ReachedEnd =>
				lineIndex >= lineCount
				|| lineIndex == lineCount - 1 && charIndex >= currentLineLength;

		public int LineNumber => lineIndex + 1;

		private static IEnumerable<string> GetLines(TextReader reader) {
			string line;
			while ((line = reader.ReadLine()) != null) {
				yield return line;
			}
		}

		public LineReader(TextReader reader) : this(GetLines(reader)) { }

		public LineReader(IEnumerable<string> lines) {
			this.lines = lines.Select(str => str.ToCharArray()).ToList();
			lineCount = this.lines.Count;

			currentLine = lineCount == 0 ? new char[] { } : this.lines[charIndex];
			currentLineLength = currentLine.Length;
		}

		public void Next(out char? consumedChar, out bool wasEscaped) {
			if (ReachedEnd) {
				consumedChar = null;
				wasEscaped = false;
				return;
			}

			if (charIndex >= currentLineLength) {
				consumedChar = '\n';
				wasEscaped = false;
				PointerToNextLine();
				return;
			}

			char currentChar = currentLine[charIndex];
			wasEscaped = currentChar == SpecialCharacters.escapeChar;
			if (wasEscaped) {
				char? nextChar = PeekNextCharOnLine();
				if (nextChar == null) {
					PointerToNextLine();
				} else {
					charIndex += 2;
				}
				consumedChar = nextChar ?? '\n';
			} else {
				charIndex++;
				consumedChar = currentChar;
			}
		}

		private char? PeekNextCharOnLine() {
			if (charIndex + 1 >= currentLineLength) {
				return null;
			}
			return currentLine[charIndex + 1];
		}

		public void PointerToNextLine() {
			lineIndex++;
			charIndex = 0;
			currentLine = lineIndex >= lineCount ? new char[] { } : lines[lineIndex];
			currentLineLength = currentLine.Length;
		}
	}
}