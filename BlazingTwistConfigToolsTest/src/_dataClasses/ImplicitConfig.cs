using BlazingTwistConfigTools.config.attributes;
using JetBrains.Annotations;

namespace BlazingTwistConfigToolsTest._dataClasses {
	public class ImplicitConfig {
		public class SubClass {
			[UsedImplicitly] public int a;
			[ConfigValue(name: "b")] public int c;

			public SubClass() { }

			public SubClass(int a, int c) {
				this.a = a;
				this.c = c;
			}
		}

		[UsedImplicitly] public int a;
		[ConfigValue(name: "b")] public int c;
		[UsedImplicitly] public SubClass subClass;

		public ImplicitConfig() { }

		public ImplicitConfig(int a, int c, SubClass subClass) {
			this.a = a;
			this.c = c;
			this.subClass = subClass;
		}
	}
}