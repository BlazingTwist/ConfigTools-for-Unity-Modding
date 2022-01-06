using BlazingTwistConfigTools.config.attributes;

namespace BlazingTwistConfigToolsTest._dataClasses {
	
	[SingleFieldType(fieldName: nameof(firstField))]
	public class ExampleSingleFieldConfig {
		
		[SingleFieldType(fieldName: nameof(secondField))]
		public class SingleFieldType1 {
			public SingleFieldType2 secondField;
			
			public override bool Equals(object obj) {
				if (obj == this) {
					return true;
				}
				if (obj == null || obj.GetType() != GetType()) {
					return false;
				}
				SingleFieldType1 inObj = (SingleFieldType1)obj;
				return Equals(secondField, inObj.secondField);
			}
		}
		
		public class SingleFieldType2 {
			[ConfigValue] public int a;
			[ConfigValue] public int b;
			
			public override bool Equals(object obj) {
				if (obj == this) {
					return true;
				}
				if (obj == null || obj.GetType() != GetType()) {
					return false;
				}
				SingleFieldType2 inObj = (SingleFieldType2)obj;
				return a == inObj.a && b == inObj.b;
			}
		}

		public SingleFieldType1 firstField;

		public override bool Equals(object obj) {
			if (obj == this) {
				return true;
			}
			if (obj == null || obj.GetType() != GetType()) {
				return false;
			}
			ExampleSingleFieldConfig inObj = (ExampleSingleFieldConfig)obj;
			return Equals(firstField, inObj.firstField);
		}
	}
}