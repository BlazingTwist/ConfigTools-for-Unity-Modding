using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazingTwistConfigTools.blazingtwist.config {
	internal class ConfigNode {
		public string key;
		public string value;
		public List<ConfigNode> listValues;

		public override string ToString() {
			var builder = new StringBuilder();
			builder.Append("Key: ");
			builder.Append(key ?? "null");

			builder.Append(" | Value: ");
			builder.Append(value ?? "null");

			builder.Append(" | listValues: ");
			if (listValues == null) {
				builder.Append("null");
			} else {
				builder.Append("{Node: (");
				builder.Append(string.Join("), Node: (", listValues.Select(node => node.ToString())));
				builder.Append(")}");
			}

			return builder.ToString();
		}
	}
}