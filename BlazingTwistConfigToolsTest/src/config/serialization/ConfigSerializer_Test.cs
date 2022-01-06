using System;
using System.Collections.Generic;
using System.Linq;
using BlazingTwistConfigTools.config;
using BlazingTwistConfigTools.config.serialization;
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
- intValue = 1
- stringValue = ~
- stringValue2 = ""~""
- intList :
-- 1
-- 1
- stringList :
-- ~
-- ""~""
- subClassList :
-- :
--- a = ~
--- b = 1
-- :
--- a = ""~""
--- b = 1
- intDict :
-- 1 = 1
-- 2 = 2
- stringDict :
-- str1 = ~
-- ""~"" = ""~""
- subClassDict :
-- str1 :
--- a = ~
--- b = 1
-- ""~"" :
--- a = ""~""
--- b = 1
";

			IEnumerable<string> lines = new ConfigSerializer().Serialize(exampleNullableConfig, EFormatOption.QuoteValue, EFormatOption.QuoteValue);
			Assert.AreEqual(expectedResult.Split(new []{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries), lines.ToArray());
		}

		[Test]
		public void Test_Serialize_SpecialCharOptions() {
			const string normalString = "asdf";
			string specialCharacterString = "" + SpecialCharacters.objectDepth + SpecialCharacters.valueAssignment + SpecialCharacters.objectAssignment
					+ SpecialCharacters.nullChar + SpecialCharacters.stringChar + SpecialCharacters.escapeChar;
			
			ExampleNullableConfig exampleNullableConfig = new ExampleNullableConfig {
					stringValue = normalString,
					stringValue2 = specialCharacterString,
					stringList = new List<string> { normalString, specialCharacterString },
					subClassList = new List<ExampleNullableConfig.ExampleNullableSubClass> {
							new ExampleNullableConfig.ExampleNullableSubClass { a = normalString },
							new ExampleNullableConfig.ExampleNullableSubClass { a = specialCharacterString }
					},
					stringDict = new Dictionary<string, string> {
							{ normalString, normalString },
							{ specialCharacterString, specialCharacterString }
					},
					subClassDict = new Dictionary<string, ExampleNullableConfig.ExampleNullableSubClass> {
							{ normalString, new ExampleNullableConfig.ExampleNullableSubClass { a = normalString } },
							{ specialCharacterString, new ExampleNullableConfig.ExampleNullableSubClass { a = specialCharacterString } }
					}
			};

			string[] ExpectedLines(EFormatOption keyOpt, EFormatOption valueOpt) {
				return new[] {
						$"- {SpecialCharacters.FormatStringValue("intValue", keyOpt)} = {SpecialCharacters.FormatStringValue("1", valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("stringValue", keyOpt)} = {SpecialCharacters.FormatStringValue(normalString, valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("stringValue2", keyOpt)} = {SpecialCharacters.FormatStringValue(specialCharacterString, valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("intList", keyOpt)} :",
						$"-- {SpecialCharacters.FormatStringValue("1", valueOpt)}",
						$"-- {SpecialCharacters.FormatStringValue("1", valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("stringList", keyOpt)} :",
						$"-- {SpecialCharacters.FormatStringValue(normalString, valueOpt)}",
						$"-- {SpecialCharacters.FormatStringValue(specialCharacterString, valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("subClassList", keyOpt)} :",
						"-- :",
						$"--- {SpecialCharacters.FormatStringValue("a", keyOpt)} = {SpecialCharacters.FormatStringValue(normalString, valueOpt)}",
						$"--- {SpecialCharacters.FormatStringValue("b", keyOpt)} = {SpecialCharacters.FormatStringValue("1", valueOpt)}",
						"-- :",
						$"--- {SpecialCharacters.FormatStringValue("a", keyOpt)} = {SpecialCharacters.FormatStringValue(specialCharacterString, valueOpt)}",
						$"--- {SpecialCharacters.FormatStringValue("b", keyOpt)} = {SpecialCharacters.FormatStringValue("1", valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("intDict", keyOpt)} :",
						$"-- {SpecialCharacters.FormatStringValue("1", keyOpt)} = {SpecialCharacters.FormatStringValue("1", valueOpt)}",
						$"-- {SpecialCharacters.FormatStringValue("2", keyOpt)} = {SpecialCharacters.FormatStringValue("2", valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("stringDict", keyOpt)} :",
						$"-- {SpecialCharacters.FormatStringValue(normalString, keyOpt)} = {SpecialCharacters.FormatStringValue(normalString, valueOpt)}",
						$"-- {SpecialCharacters.FormatStringValue(specialCharacterString, keyOpt)} = {SpecialCharacters.FormatStringValue(specialCharacterString, valueOpt)}",
						$"- {SpecialCharacters.FormatStringValue("subClassDict", keyOpt)} :",
						$"-- {SpecialCharacters.FormatStringValue(normalString, keyOpt)} :",
						$"--- {SpecialCharacters.FormatStringValue("a", keyOpt)} = {SpecialCharacters.FormatStringValue(normalString, valueOpt)}",
						$"--- {SpecialCharacters.FormatStringValue("b", keyOpt)} = {SpecialCharacters.FormatStringValue("1", valueOpt)}",
						$"-- {SpecialCharacters.FormatStringValue(specialCharacterString, keyOpt)} :",
						$"--- {SpecialCharacters.FormatStringValue("a", keyOpt)} = {SpecialCharacters.FormatStringValue(specialCharacterString, valueOpt)}",
						$"--- {SpecialCharacters.FormatStringValue("b", keyOpt)} = {SpecialCharacters.FormatStringValue("1", valueOpt)}",
				};
			}
			
			foreach (EFormatOption keyOpt in (EFormatOption[])Enum.GetValues(typeof(EFormatOption))) {
				foreach (EFormatOption valueOpt in (EFormatOption[])Enum.GetValues(typeof(EFormatOption))) {
					string[] resultLines = new ConfigSerializer().Serialize(exampleNullableConfig, keyOpt, valueOpt).ToArray();
					Assert.AreEqual(ExpectedLines(keyOpt, valueOpt), resultLines, "keyOpt: {0}, valueOpt: {1}", keyOpt, valueOpt);
				}				
			}
		}

		[Test]
		public void Test_Serialize_SingleFieldType() {
			ExampleSingleFieldConfig configInstance = new ExampleSingleFieldConfig
					{ firstField = new ExampleSingleFieldConfig.SingleFieldType1 { secondField = new ExampleSingleFieldConfig.SingleFieldType2 { a = 1, b = 2 } } };
			const string expectedResult = @"
- a = 1
- b = 2
";
			IEnumerable<string> lines = new ConfigSerializer().Serialize(configInstance, EFormatOption.QuoteValue, EFormatOption.QuoteValue);
			Assert.AreEqual(expectedResult.Split(new []{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries), lines.ToArray());
		}
	}
}