#region Imports

using System.Configuration;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yagasoft.Libraries.Common;
using Yagasoft.Tools.Common.Exceptions;

#endregion

namespace Yagasoft.Tools.Common.Helpers
{
	public static class ConfigHelpers
	{
		/// <summary>
		///     Gets the specified key from the config file.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <exception cref="ConfigurationErrorsException">Missing '{keyString}' key value from configuration.</exception>
		public static string Get(string key)
		{
			ValidateKeys(key);

			var value = ConfigurationManager.AppSettings[key];

			if (value.IsEmpty())
			{
				throw new ToolException($"Missing '{key}' key value in configuration.");
			}

			return value;
		}

		/// <summary>
		///     Validates the keys exit in the config file.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <exception cref="ConfigurationErrorsException">Missing '{missingKeys.StringAggregate(",")}' keys from configuration.</exception>
		public static void ValidateKeys(params string[] keys)
		{
			var missingKeys = keys.Except(ConfigurationManager.AppSettings.AllKeys).ToList();

			if (missingKeys.Any())
			{
				throw new ToolException(
					$"Missing '{missingKeys.StringAggregate(",")}' keys from configuration.");
			}
		}

		public static TConfig GetConfigurationParams<TConfig>(string configFilePath)
		{
			configFilePath.RequireFilled(nameof(configFilePath));

			if (!File.Exists(configFilePath))
			{
				throw new ToolException($"Couldn't find config file ({configFilePath}).");
			}

			var json = File.ReadAllText(configFilePath);

			return Deserialise<TConfig>(json);
		}

		public static TConfig Deserialise<TConfig>(string json)
		{
			json.RequireFilled(nameof(json));
			return JsonConvert.DeserializeObject<TConfig>(json,
				new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
		}

		public static string Serialise(object obj)
		{
			obj.Require(nameof(obj));
			return JsonConvert.SerializeObject(obj,
				new JsonSerializerSettings
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					Formatting = Formatting.Indented
				});
		}
	}
}
