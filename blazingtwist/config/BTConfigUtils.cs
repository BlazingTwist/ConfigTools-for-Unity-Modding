using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlazingTwistConfigTools.config.deserialization;
using BlazingTwistConfigTools.config.serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace BlazingTwistConfigTools.config {
	[PublicAPI]
	public static class BTConfigUtils {
		/// <summary>
		/// Loads a config from a file.
		/// If an instance is provided, the config will be loaded to that instance
		/// otherwise a new instance is created
		/// </summary>
		/// <param name="pathToFile">absolute path to the config file. '/' will be replaced with the DirectorySeparatorChar</param>
		/// <param name="instance">optional - instance of the existing config</param>
		/// <param name="verifyAllFieldsSet">if enabled, verifies that all Fields of TConfigType are set by the config</param>
		/// <param name="keyFormatOption">default format option for serializing keys</param>
		/// <param name="valueFormatOption">default format option for serializing values</param>
		/// <typeparam name="TConfigType">type of the config to load</typeparam>
		/// <returns>instance of the loaded config</returns>
		public static TConfigType LoadConfigFile<TConfigType>(string pathToFile, [CanBeNull] TConfigType instance, bool verifyAllFieldsSet,
				EFormatOption keyFormatOption = EFormatOption.UseDefault, EFormatOption valueFormatOption = EFormatOption.UseDefault) {
			pathToFile = pathToFile.Replace('/', Path.DirectorySeparatorChar);
			if (!File.Exists(pathToFile)) {
				if (instance == null) {
					instance = (TConfigType)Activator.CreateInstance(typeof(TConfigType));
				}
				IEnumerable<string> lines = new ConfigSerializer().Serialize(instance, keyFormatOption, valueFormatOption);

				using (StreamWriter writer = new StreamWriter(pathToFile, false)) {
					foreach (string line in lines) {
						writer.WriteLine(line);
					}
				}
				return instance;
			}

			try {
				using (StreamReader reader = File.OpenText(pathToFile)) {
					List<string> lines = new List<string>();
					{
						string line;
						while ((line = reader.ReadLine()) != null) {
							lines.Add(line);
						}
					}
					ConfigDeserializer<TConfigType> deserializer = new ConfigDeserializer<TConfigType>(DeserializerUtils.Tokenize(lines).ToList(), verifyAllFieldsSet);
					return instance == null ? deserializer.Deserialize() : deserializer.Deserialize(instance);
				}
			} catch (Exception e) {
				Debug.LogError("Failed to load config at path: '" + pathToFile + "', caught exception: " + e);
				return default;
			}
		}
	}
}