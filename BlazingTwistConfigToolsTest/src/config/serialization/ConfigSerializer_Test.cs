using System;
using System.Collections.Generic;
using System.Linq;
using BlazingTwistConfigTools.blazingtwist.config.serialization;
using BlazingTwistConfigToolsTest._dataClasses;
using NUnit.Framework;

namespace BlazingTwistConfigToolsTest.config.serialization {
	[TestFixture]
	public class ConfigSerializer_Test {
		[Test]
		public void Test_SerializeNullValues() {
			ExampleNullableConfig exampleNullableConfig = new ExampleNullableConfig {
					stringValue = null,
					stringValue2 = "~",
					stringList = new List<string> { null, "~" },
					subClassList = new List<ExampleNullableConfig.ExampleNullableSubClass> {
							new ExampleNullableConfig.ExampleNullableSubClass { a = null },
							new ExampleNullableConfig.ExampleNullableSubClass { a = "~" }
					},
					stringDict = new Dictionary<string, string> {
							{ "str1", null },
							{ "~", "~" }
					},
					subClassDict = new Dictionary<string, ExampleNullableConfig.ExampleNullableSubClass> {
							{ "str1", new ExampleNullableConfig.ExampleNullableSubClass { a = null } },
							{ "~", new ExampleNullableConfig.ExampleNullableSubClass { a = "~" } }
					}
			};

			const string expectedResult = @"
- ""intValue"" = ""1""
- ""stringValue"" = ~
- ""stringValue2"" = ""~""
- ""intList"" :
-- ""1""
-- ""1""
- ""stringList"" :
-- ~
-- ""~""
- ""subClassList"" :
-- :
--- ""a"" = ~
--- ""b"" = ""1""
-- :
--- ""a"" = ""~""
--- ""b"" = ""1""
- ""intDict"" :
-- ""1"" = ""1""
-- ""2"" = ""2""
- ""stringDict"" :
-- ""str1"" = ~
-- ""~"" = ""~""
- ""subClassDict"" :
-- ""str1"" :
--- ""a"" = ~
--- ""b"" = ""1""
-- ""~"" :
--- ""a"" = ""~""
--- ""b"" = ""1""
";

			IEnumerable<string> lines = new ConfigSerializer().Serialize(exampleNullableConfig);
			Assert.AreEqual(expectedResult.Split(new []{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries), lines.ToArray());
		}
	}
}