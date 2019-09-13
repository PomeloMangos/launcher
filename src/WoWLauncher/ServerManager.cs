using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;

namespace WoWLauncher
{
    public static class ServerManager
    {
        public static bool IsSpecifiedServer() => string.IsNullOrEmpty(ConfigManager.Config.server);

        public static string GetAnnounce()
        {
            var announce = HttpGet("/launcher/announce");
            ConfigManager.Config.announce = announce;
            ConfigManager.SaveConfig();
            return announce;
        }
        public static string GetRegisterAddress()
        {
            var register = HttpGet("/launcher/register");
            ConfigManager.Config.register = register;
            ConfigManager.SaveConfig();
            return register;
        }

        public static List<ConfigManager.Realm> GetRealmList()
        {
            var list = HttpGetJson<List<ConfigManager.Realm>>("/launcher/realm");
            ConfigManager.Config.realm_list.Clear();
            ConfigManager.Config.realm_list.AddRange(list);
            return ConfigManager.Config.realm_list;
        }

        public static string HttpGet(string endpoint)
        {
            var req = HttpWebRequest.Create($"{ConfigManager.Config.server}{endpoint}");
            req.Method = "GET";
            var res = req.GetResponse();
            using (var stream = res.GetResponseStream())
            using (var sw = new StreamReader(stream))
            {
                return sw.ReadToEnd();
            }
        }

        public static T HttpGetJson<T>(string endpoint)
        {
            var json = HttpGet(endpoint);
            return JsonUtil.DeserializeObject<T>(json);
        }

        private static string ByteArrayToHexString(byte[] buf)
        {
            string returnStr = "";
            if (buf != null)
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    returnStr += buf[i].ToString("X2");
                }
            }
            return returnStr;
        }

        private static void DownloadFile(string filename, Action<long, long> onBytesReceived)
        {
            var req = HttpWebRequest.Create($"{ConfigManager.Config.server}/launcher/download?filename={Uri.EscapeDataString(filename)}");
            req.Method = "GET";
            var res = req.GetResponse();
            var bytes = new byte[res.ContentLength];
            using (var stream = res.GetResponseStream())
            {
                var read = 0L;
                while (read < res.ContentLength)
                {
                    read += stream.Read(bytes, (int)read, (int)res.ContentLength - (int)read);
                    onBytesReceived(res.ContentLength, read);
                }
            }
            var folder = Path.GetDirectoryName(Path.Combine(ConfigManager.Config.game_path, filename));
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllBytes(Path.Combine(ConfigManager.Config.game_path, filename), bytes);
        }
    }
}
