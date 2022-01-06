using System.Collections.Generic;
using System.IO;
using BlazingTwistConfigTools.config.deserialization;

namespace BlazingTwistConfigTools.config.types {
	public class ConfigNode {
		public int LineNumber { get; }
		public int ObjectDepth { get; }
		public string Key { get; private set; }
		public string Value { get; private set; }

		private void InitializeTwoTokens(IReadOnlyList<TokenCollector.TokenInfo> tokens) {
			//Debug.Assert(tokens.Count == 2);
			// raw value '- value'
			// list mapping '- :'
			TokenCollector.TokenInfo secondToken = tokens[1];
			switch (secondToken.tokenType) {
				case ETokenType.StringValue:
					// value only
					Value = secondToken.value;
					break;
				case ETokenType.Assignment:
					// no key, no value
					break;
				case ETokenType.ObjectDepth:
				default:
					throw new InvalidDataException($"Unexpected Token: {secondToken}");
			}
		}

		private void InitializeThreeTokens(IReadOnlyList<TokenCollector.TokenInfo> tokens) {
			//Debug.Assert(tokens.Count == 3);
			// subMapping '- key :'
			TokenCollector.TokenInfo secondToken = tokens[1];
			TokenCollector.TokenInfo thirdToken = tokens[2];
			if (secondToken.tokenType != ETokenType.StringValue) {
				throw new InvalidDataException($"Unexpected Token: {secondToken}");
			}
			if (thirdToken.tokenType != ETokenType.Assignment) {
				throw new InvalidDataException($"Unexpected Token: {thirdToken}");
			}
			Key = secondToken.value;
		}

		private void InitializeFourTokens(IReadOnlyList<TokenCollector.TokenInfo> tokens) {
			//Debug.Assert(tokens.Count == 4);
			// keyValuePair '- key = value'
			TokenCollector.TokenInfo secondToken = tokens[1];
			TokenCollector.TokenInfo thirdToken = tokens[2];
			TokenCollector.TokenInfo fourthToken = tokens[3];
			if (secondToken.tokenType != ETokenType.StringValue) {
				throw new InvalidDataException($"Unexpected Token: {secondToken}");
			}
			if (thirdToken.tokenType != ETokenType.Assignment) {
				throw new InvalidDataException($"Unexpected Token: {thirdToken}");
			}
			if (fourthToken.tokenType != ETokenType.StringValue) {
				throw new InvalidDataException($"Unexpected Token: {thirdToken}");
			}
			Key = secondToken.value;
			Value = fourthToken.value;
		}

		public ConfigNode(IReadOnlyList<TokenCollector.TokenInfo> tokens) {
			int tokenCount = tokens.Count;
			if (tokenCount > 0) {
				TokenCollector.TokenInfo firstToken = tokens[0];
				if (firstToken.tokenType == ETokenType.ObjectDepth) {
					LineNumber = firstToken.lineNumber;
					ObjectDepth = firstToken.value.Length;
				} else {
					throw new InvalidDataException($"Expected DepthToken, but got {firstToken}");
				}
			}

			if (tokenCount == 2) {
				InitializeTwoTokens(tokens);
			} else if (tokenCount == 3) {
				InitializeThreeTokens(tokens);
			} else if (tokenCount == 4) {
				InitializeFourTokens(tokens);
			} else {
				int lineNumber2 = tokenCount > 0 ? tokens[0].lineNumber : -1;
				throw new InvalidDataException($"Config Node at line {lineNumber2} had unexpected amount of tokens: {tokenCount} ({string.Join(", ", tokens)})");
			}
		}
	}
}