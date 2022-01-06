using System.Collections.Generic;
using System.Linq;
using BlazingTwistConfigTools.config.attributes;
using JetBrains.Annotations;

namespace BlazingTwistConfigTools.configurables {
	[PublicAPI]
	[SingleFieldType(nameof(keys))]
	public class KeyBind {
		private List<Key> keys;

		/// <summary>
		/// Creates a KeyBind.
		/// </summary>
		/// <param name="keys">Keys assigned to this KeyBind</param>
		[PublicAPI]
		public KeyBind(List<Key> keys) {
			this.keys = keys;
		}

		/// <summary>
		/// Behaves like Unity Input.GetKey()
		/// </summary>
		/// <returns>true if this keyBind is currently pressed, false otherwise</returns>
		[PublicAPI]
		public bool GetKey() {
			if (keys == null || keys.Count == 0) {
				return false;
			}
			return keys.All(key => key.GetKeyState());
		}

		/// <summary>
		/// Behaves like Unity Input.GetKeyDown()
		/// </summary>
		/// <returns>true if this keyBind was pressed this frame, false otherwise</returns>
		[PublicAPI]
		public bool GetKeyDown() {
			return GetKey() && keys.Any(key => key.GetKeyDown());
		}

		/// <summary>
		/// Behaves like Unity Input.GetKeyUp()
		/// </summary>
		/// <returns>true if this keyBind was released this frame, false otherwise</returns>
		[PublicAPI]
		public bool GetKeyUp() {
			if (keys == null || keys.Count == 0) {
				return false;
			}
			int keysReleased = 0;
			foreach (Key releasedKey in keys.Where(key => !key.GetKeyState())) {
				keysReleased++;
				if (!releasedKey.GetKeyUp()) {
					return false;
				}
			}
			return keysReleased >= 1;
		}
	}
}