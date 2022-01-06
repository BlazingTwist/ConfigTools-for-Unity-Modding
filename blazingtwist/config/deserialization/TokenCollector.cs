using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BlazingTwistConfigTools.config.types;

namespace BlazingTwistConfigTools.config.deserialization {
	public class TokenCollector {
		public struct TokenInfo {
			public ETokenType tokenType;
			public string value;
			public int lineNumber;

			public override string ToString() {
				return $"TokenInfo({tokenType}='{value}' | line#={lineNumber})";
			}
		}

		private readonly List<List<TokenInfo>> tokenMap;
		private List<TokenInfo> activeTokens;
		private ETokenType? currentTokenType;
		private StringBuilder currentTokenBuilder;
		private int currentTokenLineNumber = -1;

		public TokenCollector() {
			tokenMap = new List<List<TokenInfo>>();
		}

		private void CollectCurrentToken() {
			if (currentTokenBuilder != null && currentTokenType != null) {
				if (activeTokens == null && currentTokenType != ETokenType.ObjectDepth) {
					throw new InvalidDataException($"Invalid first token: {currentTokenType}={currentTokenBuilder} line#={currentTokenLineNumber} | expected ObjectDepth-Token!");
				}
				if (currentTokenType == ETokenType.ObjectDepth) {
					if (activeTokens != null) {
						tokenMap.Add(activeTokens);
					}
					activeTokens = new List<TokenInfo>();
				}
				//Debug.Assert(activeTokens != null, nameof(activeTokens) + " != null");
				activeTokens.Add(new TokenInfo {
						tokenType = currentTokenType.Value,
						value = currentTokenBuilder.ToString(),
						lineNumber = currentTokenLineNumber
				});
				currentTokenType = null;
				currentTokenBuilder = null;
				currentTokenLineNumber = -1;
			}
		}

		public void AddToken(ETokenType type, char value, int lineNumber) {
			if (currentTokenType != type) {
				CollectCurrentToken();
				currentTokenType = type;
				currentTokenBuilder = new StringBuilder();
				currentTokenLineNumber = lineNumber;
			}
			currentTokenBuilder.Append(value);
		}

		public void AddNullToken(int lineNumber) {
			if (currentTokenType == ETokenType.StringValue) {
				throw new InvalidDataException($"Unescaped null character in string '{currentTokenBuilder}' on line#={currentTokenLineNumber}");
			}
			CollectCurrentToken();
			activeTokens.Add(new TokenInfo {
					tokenType = ETokenType.StringValue,
					value = null,
					lineNumber = lineNumber
			});
			currentTokenType = null;
			currentTokenBuilder = null;
			currentTokenLineNumber = -1;
		}

		public IEnumerable<ConfigNode> GetConfigNodes() {
			CollectCurrentToken();
			if (activeTokens != null) {
				tokenMap.Add(activeTokens);
				activeTokens = null;
			}
			return tokenMap.Select(tokens => new ConfigNode(tokens));
		}
	}
}