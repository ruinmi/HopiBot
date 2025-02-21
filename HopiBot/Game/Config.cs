using System;
using System.Collections.Generic;
using System.IO;

namespace HopiBot.Game
{
    public class Config
    {
        private const string LeagueConfigPath = "E:\\Game\\WeGameApps\\英雄联盟\\Game\\Config\\game.cfg";
        private Dictionary<string, string> configData;
        public static Config Instance { get; } = new Config();

        public Config()
        {
            configData = new Dictionary<string, string>();
            ParseConfigFile();
        }

        private void ParseConfigFile()
        {
            try
            {
                string[] lines = File.ReadAllLines(LeagueConfigPath);

                foreach (string line in lines)
                {
                    // Skip empty lines and comments
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split('=');

                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        configData[key] = value;
                    }
                    else
                    {
                        // Handle improperly formatted lines
                        Console.WriteLine("Warning: Skipped improperly formatted line: " + line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while parsing config file: " + ex.Message);
            }
        }

        public string GetValue(string key)
        {
            string value = null;
            configData.TryGetValue(key, out value);
            return value;
        }
    }
}
