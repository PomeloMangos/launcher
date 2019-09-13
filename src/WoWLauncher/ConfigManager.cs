using System.Collections.Generic;
using System.IO;

namespace WoWLauncher
{
    public static class ConfigManager
    {
        public static readonly ConfigModel Config;

        public static void SaveConfig()
        {
            lock(Config)
            {
                File.WriteAllText("config.json", JsonUtil.SerializeObject(Config));
            }
        }

        static ConfigManager()
        {
            if (File.Exists("config.json"))
            {
                Config = JsonUtil.DeserializeObject<ConfigModel>(File.ReadAllText("config.json"));
            }
            else
            {
                Config = new ConfigModel();
            }
        }

        public class ConfigModel
        {
            public string server { get; set; }

            public string game_path { get; set; }

            public string announce { get; set; }

            public string register { get; set; }

            public List<Realm> realm_list { get; set; } = new List<Realm>();
        }

        public class Realm
        {
            public string name { get; set; }

            public string address { get; set; }
        }
    }
}
