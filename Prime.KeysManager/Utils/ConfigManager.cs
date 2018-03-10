﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Prime.KeysManager.Utils
{
    public static class ConfigManager
    {
        private const string ConfigFileName = "config.json";

        private static AppConfig LoadAppConfig()
        {
            try
            {
                if (!File.Exists(ConfigFileName))
                    return null;

                var configContents = File.ReadAllText(ConfigFileName);
                var config = JsonConvert.DeserializeObject<AppConfig>(configContents);

                return config;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to read app config: {e.Message}");
                return null;
            }
        }

        public static AppConfig AppConfig { get; } = LoadAppConfig();
    }
}
