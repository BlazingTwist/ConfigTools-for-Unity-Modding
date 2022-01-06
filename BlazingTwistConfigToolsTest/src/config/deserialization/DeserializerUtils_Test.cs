using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlazingTwistConfigTools.config.deserialization;
using BlazingTwistConfigTools.config.types;
using NUnit.Framework;

namespace BlazingTwistConfigToolsTest.config.deserialization {
	[TestFixture]
	public class DeserializerUtils_Test {
		private class ConfigNodeVerifier {
			private readonly int lineNumber;
			private readonly int objectDepth;
			private readonly string key;
			private readonly string value;

			public ConfigNodeVerifier(int lineNumber, int objectDepth, string key, string value) {
				this.lineNumber = lineNumber;
				this.objectDepth = objectDepth;
				this.key = key;
				this.value = value;
			}

			public void Verify(ConfigNode instance) {
				Assert.AreEqual(lineNumber, instance.LineNumber, $"configNode had invalid line number | {instance}");
				Assert.AreEqual(objectDepth, instance.ObjectDepth, $"configNode had invalid objectDepth | {instance}");
				Assert.AreEqual(key, instance.Key, $"configNode had invalid key | {instance}");
				Assert.AreEqual(value, instance.Value, $"configNode had invalid value | {instance}");
			}
		}

		[Test]
		public void Test_Tokenize_Works() {
			List<ConfigNode> tokens = DeserializerUtils.Tokenize(new[] {
					"-key=123",
					"",
					"--deeperKey:\\4\\5\\6\\",
					"",
					"asd//comment",
					"fg/* ranged",
					"",
					"comment */",
					"/abortedComment/ 2/\"3 4\"",
					"--key:",
					"---=----",
					"/\\*escapedComment",
					"\" quoted",
					"",
					"string///*-With-Dash\"\\",
					"/"
			}).ToList();

			List<ConfigNodeVerifier> verifiers = new List<ConfigNodeVerifier> {
					new ConfigNodeVerifier(1, 1, "key", "123"),
					new ConfigNodeVerifier(3, 2, "deeperKey", "456\nasdfg/abortedComment/2/3 4"),
					new ConfigNodeVerifier(10, 2, "key", null),
					new ConfigNodeVerifier(11, 3, null, null),
					new ConfigNodeVerifier(11, 4, null, "/*escapedComment quoted\n\nstring///*-With-Dash\n/"),
			};

			Assert.AreEqual(tokens.Count, verifiers.Count);
			int tokenCount = tokens.Count;
			for (int i = 0; i < tokenCount; i++) {
				verifiers[i].Verify(tokens[i]);
			}
		}

		[Test]
		public void Test_Tokenize_SupportsNull() {
			List<ConfigNode> tokens = DeserializerUtils.Tokenize(new[] {
					"-value=~",
					"-value2=\\~",
					"-listValue:",
					"--~",
					"-", "-~",
					"--\\~",
					"-dictValue:",
					"--key1=~",
					"--~=~",
					"--\\~=\\~"
			}).ToList();

			List<ConfigNodeVerifier> verifiers = new List<ConfigNodeVerifier> {
					new ConfigNodeVerifier(1, 1, "value", null),
					new ConfigNodeVerifier(2, 1, "value2", "~"),
					new ConfigNodeVerifier(3, 1, "listValue", null),
					new ConfigNodeVerifier(4, 2, null, null),
					new ConfigNodeVerifier(5, 2, null, null),
					new ConfigNodeVerifier(7, 2, null, "~"),
					new ConfigNodeVerifier(8, 1, "dictValue", null),
					new ConfigNodeVerifier(9, 2, "key1", null),
					new ConfigNodeVerifier(10, 2, null, null),
					new ConfigNodeVerifier(11, 2, "~", "~")
			};

			Assert.AreEqual(tokens.Count, verifiers.Count);
			int tokenCount = tokens.Count;
			for (int i = 0; i < tokenCount; i++) {
				verifiers[i].Verify(tokens[i]);
			}
		}

		[Test]
		public void Test_Tokenize_ThrowsRangedCommentNeverClosed() {
			Assert.Throws(typeof(InvalidDataException), () => DeserializerUtils.Tokenize(new[] {
					"",
					"ranged/* comment",
					"that is never *\\/",
					"closed \\*/",
					""
			}));
		}

		[Test]
		public void Test_Tokenize_ThrowsStringNeverClosed() {
			Assert.Throws(typeof(InvalidDataException), () => DeserializerUtils.Tokenize(new[] {
					"",
					"\"string\\\"",
					"that \\\" is never",
					"\\\" closed.",
					""
			}));
		}

		[Test]
		public void Test_Tokenize_ThrowsInvalidFirstToken() {
			Assert.Throws(typeof(InvalidDataException), () => DeserializerUtils.Tokenize(new[] {
					"key=value",
					"-second = thing"
			}));
		}

		[Test]
		public void Test_Tokenize_ThrowsUnescapedNullCharInString() {
			Assert.Throws(typeof(InvalidDataException), () => DeserializerUtils.Tokenize(new[] {
					"-key=va~lue"
			}));
		}
	}
}