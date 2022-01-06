using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlazingTwistConfigTools.config;
using BlazingTwistConfigTools.config.deserialization;
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
					intList = new List<int> { 0, 1 },
					stringList = new List<string> { null, "~" },
					subClassList = new List<ExampleNullableConfig.ExampleNullableSubClass> {
							new ExampleNullableConfig.ExampleNullableSubClass { a = null, b = 0 },
							new ExampleNullableConfig.ExampleNullableSubClass { a = "~", b = 1 }
					},
					intDict = new Dictionary<int, int> {
							{ 0, 0 },
							{ 1, 1 }
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
					new ConfigOptions()
			);
			ExampleNullableConfig deserializeResult = configDeserializer.Deserialize();

			Assert.AreEqual(expectedConfig, deserializeResult);
		}

		[Test]
		public void Test_Deserialize_SingleFieldType() {
			ExampleSingleFieldConfig expectedConfig = new ExampleSingleFieldConfig
					{ firstField = new ExampleSingleFieldConfig.SingleFieldType1 { secondField = new ExampleSingleFieldConfig.SingleFieldType2 { a = 1, b = 2 } } };
			const string configString = @"
- a = 1
- b = 2
";

			ConfigDeserializer<ExampleSingleFieldConfig> configDeserializer = new ConfigDeserializer<ExampleSingleFieldConfig>(
					DeserializerUtils.Tokenize(new LineReader(new StringReader(configString))).ToList(),
					new ConfigOptions()
			);
			ExampleSingleFieldConfig deserializeResult = configDeserializer.Deserialize();

			Assert.AreEqual(expectedConfig, deserializeResult);
		}

		[Test]
		public void Test_Deserialize_ExistingSingleFieldType() {
			ExampleSingleFieldConfig targetInstance = new ExampleSingleFieldConfig
					{ firstField = new ExampleSingleFieldConfig.SingleFieldType1 { secondField = new ExampleSingleFieldConfig.SingleFieldType2 { a = 1, b = 2 } } };
			const string configString = @"
- a = 3
- b = 4
";
			ConfigDeserializer<ExampleSingleFieldConfig> configDeserializer = new ConfigDeserializer<ExampleSingleFieldConfig>(
					DeserializerUtils.Tokenize(new LineReader(new StringReader(configString))).ToList(),
					new ConfigOptions()
			);
			configDeserializer.Deserialize(targetInstance);

			Assert.AreEqual(3, targetInstance.firstField.secondField.a);
			Assert.AreEqual(4, targetInstance.firstField.secondField.b);
		}

		[Test]
		public void Test_Deserialize_Implicitly() {
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

			void RunTest(bool outerIsExplicit, bool innerIsExplicit, ConfigOptions configOptions) {
				ConfigDeserializer<ImplicitConfig> configDeserializer = new ConfigDeserializer<ImplicitConfig>(
						DeserializerUtils.Tokenize(new LineReader(GetLines(outerIsExplicit, innerIsExplicit))).ToList(),
						configOptions);
				ImplicitConfig result = configDeserializer.Deserialize();
				if (outerIsExplicit) {
					Assert.AreEqual(0, result.a);
					Assert.AreEqual(4, result.c);
					Assert.AreEqual(null, result.subClass);
				} else {
					Assert.AreEqual(3, result.a);
					Assert.AreEqual(4, result.c);
					Assert.NotNull(result.subClass);
					Assert.AreEqual(innerIsExplicit ? 0 : 5, result.subClass.a);
					Assert.AreEqual(6, result.subClass.c);
				}
			}

			Type outerType = typeof(ImplicitConfig);
			Type innerType = typeof(ImplicitConfig.SubClass);

			RunTest(true, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit, verifyAllKeysSet = true });
			RunTest(true, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit, explicitTypes = new List<Type> { outerType, innerType }, verifyAllKeysSet = true });
			RunTest(false, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit, implicitTypes = new List<Type> { outerType }, verifyAllKeysSet = true });
			RunTest(false, true, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit, explicitTypes = new List<Type> { innerType }, verifyAllKeysSet = true });
			RunTest(true, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit, implicitTypes = new List<Type> { innerType }, verifyAllKeysSet = true });
			RunTest(true, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit, explicitTypes = new List<Type> { outerType }, verifyAllKeysSet = true });
			RunTest(false, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Explicit, implicitTypes = new List<Type> { outerType, innerType }, verifyAllKeysSet = true });
			RunTest(false, false, new ConfigOptions { fieldSelectorOption = EFieldSelectorOption.Implicit, verifyAllKeysSet = true });
		}
	}
}