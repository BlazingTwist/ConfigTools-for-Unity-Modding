using System.ComponentModel;
using UnityEngine;

namespace BlazingTwistConfigTools.configurables {
	[TypeConverter(typeof(KeyTypeConverter))]
	public readonly struct Key {
		internal readonly KeyCode keyCode;
		internal readonly bool negated;

		public Key(KeyCode keyCode, bool negated) {
			this.keyCode = keyCode;
			this.negated = negated;
		}

		public bool GetKeyState() {
			return negated ^ Input.GetKey(keyCode);
		}

		public bool GetKeyDown() {
			return negated ? Input.GetKeyUp(keyCode) : Input.GetKeyDown(keyCode);
		}

		public bool GetKeyUp() {
			return negated ? Input.GetKeyDown(keyCode) : Input.GetKeyUp(keyCode);
		}
	}
}