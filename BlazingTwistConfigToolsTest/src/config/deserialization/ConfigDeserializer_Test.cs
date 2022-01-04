using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlazingTwistConfigTools.blazingtwist.config.deserialization;
using BlazingTwistConfigToolsTest._dataClasses;
using NUnit.Framework;

namespace BlazingTwistConfigToolsTest.config.deserialization {
	[TestFixture]
	public class ConfigDeserializer_Test {
		[Test]
		public void Test_Deserialize_NullDeserializeToDefault() {
			ExampleNullableConfig expectedConfig = new ExampleNullableConfig {
					intValue = 0,
					stringValue = null,
					stringValue2 = "~",
					intList = new List<int>{0, 1},
					stringList = new List<string> { null, "~" },
					subClassList = new List<ExampleNullableConfig.ExampleNullableSubClass> {
							new ExampleNullableConfig.ExampleNullableSubClass { a = null, b = 0 },
							new ExampleNullableConfig.ExampleNullableSubClass { a = "~", b = 1 }
					},
					intDict = new Dictionary<int, int> {
							{0, 0},
							{1, 1}
					},
					stringDict = new Dictionary<string, string> {
							{ "str1", null },
							{ "~", "~" }
					},
					subClassDict = new Dictionary<string, ExampleNullableConfig.ExampleNullableSubClass> {
							{ "str1", new ExampleNullableConfig.ExampleNullableSubClass { a = null, b = 0 } },
							{ "~", new ExampleNullableConfig.ExampleNullableSubClass { a = "~", b = 1 } }
					}
			};
			
			const string configString = @"
- intValue = ~
- stringValue = ~
- stringValue2 = ""~""
- intList :
-- ~
-- 1
- stringList :
-- ~
-- ""~""
- subClassList :
-- :
--- a = ~
--- b = ~
-- :
--- a = ""~""
--- b = 1
- intDict :
-- ~ = ~
-- 1 = 1
- stringDict :
-- str1 = ~
-- ""~"" = ""~""
- subClassDict :
-- str1 :
--- a = ~
--- b = ~
-- ""~"" :
--- a = ""~""
--- b = 1
";

			ConfigDeserializer<ExampleNullableConfig> configDeserializer = new ConfigDeserializer<ExampleNullableConfig>(
					DeserializerUtils.Tokenize(new LineReader(new StringReader(configString))).ToList(),
					false
			);
			ExampleNullableConfig deserializeResult = configDeserializer.Deserialize();
			
			Assert.AreEqual(expectedConfig, deserializeResult);
		}
	}
}