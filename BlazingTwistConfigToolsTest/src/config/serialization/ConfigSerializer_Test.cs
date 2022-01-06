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

			IEnumerable<string> lines = new ConfigSerializer(
					new ConfigOptions {
							keyFormatOption = EFormatOption.QuoteValue,
							valueFormatOption = EFormatOption.QuoteValue
					}).Serialize(exampleNullableConfig);
			Assert.AreEqual(expectedResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), lines.ToArray());
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
					string[] resultLines = new ConfigSerializer(
							new ConfigOptions {
									keyFormatOption = keyOpt,
									valueFormatOption = valueOpt
							}).Serialize(exampleNullableConfig).ToArray();
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
			IEnumerable<string> lines = new ConfigSerializer(
					new ConfigOptions {
							keyFormatOption = EFormatOption.QuoteValue,
							valueFormatOption = EFormatOption.QuoteValue
					}).Serialize(configInstance);
			Assert.AreEqual(expectedResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), lines.ToArray());
		}

		[Test]
		public void Test_Serialize_Implicitly() {
			/* Scenarios to check:
			 *  - explicit by default, no override
			 *    + implicit by default, override both types
			 *  - explicit by default, override outer type
			 *    + implicit by default, override inner type
			 *  - explicit by default, override inner type
			 *    + implicit by default, override outer type
			 *  - explicit by default, override both types
			 *    + implicit by default, no override
			 */

			IEnumerable<string> GetLines(bool outerIsExplicit, bool innerIsExplicit) {
				if (!outerIsExplicit) {
					yield return "- a = 3";
				}
				yield return "- b = 4";
				if (!outerIsExplicit) {
					yield return "- subClass :";
					if (!innerIsExplicit) {
						yield return "-- a = 5";
					}
					yield return "-- b = 6";
				}
			}

			ImplicitConfig configToSerialize = new ImplicitConfig(3, 4, new ImplicitConfig.SubClass(5, 6));

			void RunTest(bool outerIsExplicit, bool innerIsExplicit, ConfigOptions configOptions) {
				ConfigSerializer configDeserializer = new ConfigSerializer(configOptions);
				string[] resultLines = configDeserializer.Serialize(configToSerialize).ToArray();
				string[] expectedLines = GetLines(outerIsExplicit, innerIsExplicit).ToArray();
				bool areEqual = resultLines.Length == expectedLines.Length && !resultLines.Except(expectedLines).Any();
				Assert.True(areEqual, "Expected lines: '{0}', Received Lines: '{1}'", string.Join("', '", expectedLines), string.Join("', '", resultLines));
			}

			Type outerType = typeof(ImplicitConfig);
			Type innerType = typeof(ImplicitConfig.SubClass);

			RunTest(true, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit });
			RunTest(true, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit, explicitTypes = new List<Type> { outerType, innerType } });
			RunTest(false, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit, implicitTypes = new List<Type> { outerType } });
			RunTest(false, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit, explicitTypes = new List<Type> { innerType } });
			RunTest(true, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit, implicitTypes = new List<Type> { innerType } });
			RunTest(true, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit, explicitTypes = new List<Type> { outerType } });
			RunTest(false, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit, implicitTypes = new List<Type> { outerType, innerType } });
			RunTest(false, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit });
		}
	}
}