using System.Collections.Generic;
using System.Linq;
using BlazingTwistConfigTools.blazingtwist.config.serialization;

namespace BlazingTwistConfigToolsTest._dataClasses {
	public class ExampleNullableConfig {
		public class ExampleNullableSubClass {
			[ConfigValue] public string a = "string";
			[ConfigValue] public int b = 1;

			public override string ToString() {
				return "ExampleNullableSubClass{"
						+ "a=" + a + ", "
						+ "b=" + b + ", "
						+ "}";
			}

			public override bool Equals(object obj) {
				if (obj == this) {
					return true;
				}
				if (obj == null || obj.GetType() != GetType()) {
					return false;
				}
				ExampleNullableSubClass inSubClass = (ExampleNullableSubClass)obj;
				return a == inSubClass.a && b == inSubClass.b;
			}

			public override int GetHashCode() {
				return a?.GetHashCode() ?? 0 + b.GetHashCode();
			}
		}

		[ConfigValue] public int intValue = 1;
		[ConfigValue] public string stringValue = "string";
		[ConfigValue] public string stringValue2 = "string";

		[ConfigValue] public List<int> intList = new List<int> { 1, 1 };
		[ConfigValue] public List<string> stringList = new List<string> { "string", "string" };
		[ConfigValue] public List<ExampleNullableSubClass> subClassList = new List<ExampleNullableSubClass> { new ExampleNullableSubClass(), new ExampleNullableSubClass() };

		[ConfigValue] public Dictionary<int, int> intDict = new Dictionary<int, int> { { 1, 1 }, { 2, 2 } };
		[ConfigValue] public Dictionary<string, string> stringDict = new Dictionary<string, string> { { "str1", "str1" }, { "str2", "str2" } };
		[ConfigValue] public Dictionary<string, ExampleNullableSubClass> subClassDict = new Dictionary<string, ExampleNullableSubClass> {
				{ "str1", new ExampleNullableSubClass() },
				{ "str2", new ExampleNullableSubClass() }
		};

		public override string ToString() {
			return "ExampleConfig{"
					+ "intValue=" + intValue + ", "
					+ "stringValue=" + stringValue + ", "
					+ "stringValue2=" + stringValue2 + ", "
					+ "intList=[" + string.Join("; ", intList) + "], "
					+ "stringList=[" + string.Join("; ", stringList) + "], "
					+ "subClassList=[" + string.Join("; ", subClassList) + "], "
					+ "intDict={" + string.Join(", ", intDict.Select(kvp => "" + kvp.Key + "=" + kvp.Value)) + "}, "
					+ "stringDict={" + string.Join(", ", stringDict.Select(kvp => "" + kvp.Key + "=" + kvp.Value)) + "}, "
					+ "subClassDict={" + string.Join(", ", subClassDict.Select(kvp => "" + kvp.Key + "=" + kvp.Value)) + "}, "
					+ "}";
		}

		public override bool Equals(object obj) {
			if (obj == this) {
				return true;
			}
			if (obj == null || obj.GetType() != GetType()) {
				return false;
			}
			ExampleNullableConfig inConfig = (ExampleNullableConfig)obj;
			return intValue == inConfig.intValue
					&& stringValue == inConfig.stringValue
					&& stringValue2 == inConfig.stringValue2
					&& ListEquality(intList, inConfig.intList)
					&& ListEquality(stringList, inConfig.stringList)
					&& ListEquality(subClassList, inConfig.subClassList)
					&& DictionaryEquality(intDict, inConfig.intDict)
					&& DictionaryEquality(stringDict, inConfig.stringDict)
					&& DictionaryEquality(subClassDict, inConfig.subClassDict);
		}

		private static bool ListEquality<TValue>(IReadOnlyCollection<TValue> a, IReadOnlyCollection<TValue> b) {
			return a.Count == b.Count && !a.Except(b).Any();
		}

		private static bool DictionaryEquality<TKey, TValue>(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b) {
			return a.Count == b.Count && !a.Except(b).Any();
		}
	}
}