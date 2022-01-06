using System;
using System.Collections.Generic;
using System.Linq;
using BlazingTwistConfigTools.config.attributes;

namespace BlazingTwistConfigToolsTest._dataClasses {
	public class ExampleConfig {
		public class ExampleSubClass {
			[ConfigValue] public int a = 0;
			[ConfigValue] public int b = 1;
		}
		
		[ConfigValue(name: "testName", comments: "this used to be called 'intValue'")]
		public int intValue = 1;
		
		[ConfigComment(emptyLinesAbove: 1, comments: "This is a region comment!")]
		[ConfigValue(indentation: 1, comments: new[] { "first comment", "second comment" })]
		public float floatValue = 2.5f;

		[ConfigValue(emptyLinesAbove: 2, comments: "integer-List")]
		public List<int> intList = new List<int> { 3, 4, 5 };

		[ConfigValue]
		public Dictionary<string, float> stringFloatDict = new Dictionary<string, float> {
				{ "key1", 6.5f },
				{ "key2", 7.5f },
				{ "key3", 8.5f }
		};

		[ConfigComment(emptyLinesAbove: 1, comments: "Recursive Config!")]
		[ConfigValue(indentation: 1)]
		public ExampleConfig nestedConfig = null;

		[ConfigValue(emptyLinesAbove: 1)]
		public List<ExampleSubClass> subClassList = new List<ExampleSubClass> {
				new ExampleSubClass { a = 1, b = 2 },
				new ExampleSubClass { a = 3, b = 4 }
		};
		
		[ConfigValue(emptyLinesAbove: 1)]
		public Dictionary<string, ExampleSubClass> subClassDict = new Dictionary<string, ExampleSubClass> {
				{"first", new ExampleSubClass{a = 5, b = 6}},
				{"second", new ExampleSubClass{a = 7, b = 8}}
		};

		public override string ToString() {
			return "ExampleConfig{"
					+ "intValue=" + intValue + ", "
					+ "floatValue=" + floatValue + ", "
					+ "intList=[" + string.Join("; ", intList) + "], "
					+ "stringFloatDict={" + string.Join(", ", stringFloatDict.Select(kvp => "" + kvp.Key + "=" + kvp.Value)) + "}, "
					+ "nestedConfig=" + nestedConfig
					+ "}";
		}

		public override bool Equals(object obj) {
			if (obj == this) {
				return true;
			}
			if (obj == null || obj.GetType() != GetType()) {
				return false;
			}
			ExampleConfig inConfig = (ExampleConfig)obj;
			return intValue == inConfig.intValue
					&& Math.Abs(floatValue - inConfig.floatValue) < float.Epsilon
					&& ListEquality(intList, inConfig.intList)
					&& DictionaryEquality(stringFloatDict, inConfig.stringFloatDict)
					&& Equals(nestedConfig, inConfig.nestedConfig);
		}

		private static bool ListEquality<TValue>(IReadOnlyCollection<TValue> a, IReadOnlyCollection<TValue> b) {
			return a.Count == b.Count && !a.Except(b).Any();
		}

		private static bool DictionaryEquality<TKey, TValue>(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b) {
			return a.Count == b.Count && !a.Except(b).Any();
		}
	}
}