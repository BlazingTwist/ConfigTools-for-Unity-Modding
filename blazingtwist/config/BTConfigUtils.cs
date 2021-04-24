using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace BlazingTwistConfigTools.blazingtwist.config {
	[PublicAPI]
	public static class BTConfigUtils {
		/// <summary>
		/// Loads a config from a file.
		/// If an instance is provided, the config will be loaded to that instance
		/// otherwise a new instance is created
		/// </summary>
		/// <param name="pathToFile">absolute path to the config file. '/' will be replaced with the DirectorySeparatorChar</param>
		/// <param name="instance">optional - instance of the existing config</param>
		/// <typeparam name="TConfigType">type of the config to load</typeparam>
		/// <returns>instance of the loaded config</returns>
		public static TConfigType LoadConfigFile<TConfigType>(string pathToFile, [CanBeNull] TConfigType instance) {
			pathToFile = pathToFile.Replace('/', Path.DirectorySeparatorChar);
			if (!File.Exists(pathToFile)) {
				Debug.LogError("Config file not found at path: '" + pathToFile + "'");
				return default;
			}

			try {
				using (StreamReader reader = File.OpenText(pathToFile)) {
					if (instance == null) {
						return BTConfigTools.LoadConfig<TConfigType>(reader);
					}

					BTConfigTools.LoadConfig(reader, instance);
					return instance;
				}
			} catch (Exception e) {
				Debug.LogError("Failed to load config at path: '" + pathToFile + "', caught exception: " + e);
				return default;
			}
		}
	}
}