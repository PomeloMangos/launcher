using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace WoWLauncher
{
    public static class ServerManager
    {
        public class ClientMetadata
        {
            public string version { get; set; }

            public long timestamp { get; set; }

            public IEnumerable<ClientFile> files { get; set; }
        }

        public class ClientFile
        {
            public string filename { get; set; }

            public string hash { get; set; }
        }

        public static bool IsSpecifiedServer() => string.IsNullOrEmpty(ConfigManager.Config.server);

        public static void HandleUpdate(Action<long, long> onTotalProgressChanged, Action<string, long, long> onCurrentProgressChanged)
        {
            var remote = GetClientMetadata();
            var local = LoadLocalClientMetadata();
            if (local == null || remote.version != local.version || remote.timestamp != local.timestamp)
            {
                using (var sha256 = SHA256.Create())
                {
                    var i = 0;
                    foreach (var x in remote.files)
                    {
                        onCurrentProgressChanged?.Invoke(Path.GetFileName(x.filename), 0, 1);
                        var path = Path.Combine(ConfigManager.Config.game_path, x.filename);
                        if (File.Exists(path))
                        {
                            var hash = sha256.ComputeHash(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                            if (ByteArrayToHexString(hash) == x.hash)
                            {
                                onTotalProgressChanged?.Invoke(++i, remote.files.Count());
                                onCurrentProgressChanged?.Invoke(Path.GetFileName(x.filename), 1, 1);
                                continue;
                            }
                        }
                        DownloadFile(x.filename, (recv, total) =>
                        {
                            onCurrentProgressChanged?.Invoke(Path.GetFileName(x.filename), recv, total);
                        }, -1);
                    }
                }
                SaveClientMetadata(remote);
            }
        }

        public static ClientMetadata GetClientMetadata()
        {
            return HttpGetJson<ClientMetadata>("/launcher/client.json");
        }

        public static void SaveClientMetadata(ClientMetadata meta)
        {
            File.WriteAllText("client.json", JsonUtil.SerializeObject(meta));
        }

        public static ClientMetadata LoadLocalClientMetadata()
        {
            if (!File.Exists("client.json"))
            {
                return null;
            }

            return JsonUtil.DeserializeObject<ClientMetadata>(File.ReadAllText("client.json"));
        }

        public static string GetAnnounce()
        {
            var announce = HttpGet("/launcher/announce.html");
            ConfigManager.Config.announce = announce;
            ConfigManager.SaveConfig();
            return announce;
        }

        public static string GetRegisterAddress()
        {
            var register = HttpGet("/launcher/register.url");
            ConfigManager.Config.register = register;
            ConfigManager.SaveConfig();
            return register;
        }

        public static List<ConfigManager.Realm> GetRealmList()
        {
            var list = HttpGetJson<List<ConfigManager.Realm>>("/launcher/realm.json");
            ConfigManager.Config.realm_list.Clear();
            ConfigManager.Config.realm_list.AddRange(list);
            return ConfigManager.Config.realm_list;
        }

        public static string HttpGet(string endpoint)
        {
            var req = HttpWebRequest.Create($"{ConfigManager.Config.server}{endpoint}");
            req.Method = "GET";
            req.Timeout = 10000;
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

        private static void DownloadFile(string filename, Action<long, long> onBytesReceived, int retry = 0)
        {
            try
            {
                var folder = Path.GetDirectoryName(Path.Combine(ConfigManager.Config.game_path, filename));
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var req = HttpWebRequest.Create($"{ConfigManager.Config.server}/launcher/download?filename={Uri.EscapeDataString(filename)}");
                req.Method = "GET";
                var res = req.GetResponse();
                var total = res.ContentLength;
                onBytesReceived?.Invoke(0, res.ContentLength);
                using (var stream = res.GetResponseStream())
                using (var fs = new FileStream(Path.Combine(ConfigManager.Config.game_path, filename), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    var read = 0L;
                    while (read < res.ContentLength)
                    {
                        var buffer = new byte[4096];
                        var count = 4096;
                        if (res.ContentLength - read < 4096)
                        {
                            count = (int)(res.ContentLength - read);
                        }
                        read += stream.Read(buffer, 0, count);
                        fs.Write(buffer, 0, count);
                        onBytesReceived?.Invoke(read, res.ContentLength);
                    }
                }
            }
            catch
            {
                if (retry == 0) return;
                Thread.Sleep(5000);

                if (retry == -1)
                {
                    DownloadFile(filename, onBytesReceived, retry);
                }
                else
                {
                    DownloadFile(filename, onBytesReceived, retry - 1);
                }
            }
        }
    }
}
