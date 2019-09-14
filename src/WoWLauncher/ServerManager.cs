using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Reflection;
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

        private static MethodInfo methodAddWithoutValidate = typeof(WebHeaderCollection).GetMethod
                        ("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);

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
                            byte[] hash = null;
                            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                hash = sha256.ComputeHash(fs);
                            }
                            if (ByteArrayToHexString(hash) == x.hash)
                            {
                                onTotalProgressChanged?.Invoke(++i, remote.files.Count());
                                onCurrentProgressChanged?.Invoke(Path.GetFileName(x.filename), 1, 1);
                                continue;
                            }
                            else
                            {
                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                }
                            }
                        }
                        DownloadFile(x.filename, (recv, total) =>
                        {
                            onCurrentProgressChanged?.Invoke(Path.GetFileName(x.filename), recv, total);
                        }, 0, -1);
                    }
                }
                SaveClientMetadata(remote);
            }
        }

        public static ClientMetadata GetClientMetadata()
        {
            var meta = HttpGetJson<ClientMetadata>("/launcher/client.json");
            if (!ConfigManager.Config.install_addons)
            {
                meta.files = meta.files.Where(x => !x.filename.ToLower().StartsWith("interface\\"));
            }
            return meta;
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

        private static void DownloadFile(string filename, Action<long, long> onBytesReceived, long pos = 0, int retry = 0)
        {
            try
            {
                // Create folder if non eixsts.
                var folder = Path.GetDirectoryName(Path.Combine(ConfigManager.Config.game_path, filename));
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Start http request
                var req = HttpWebRequest.Create($"{ConfigManager.Config.server}/launcher/download?filename={Uri.EscapeDataString(filename)}");
                req.Method = "GET";

                // Set position
                methodAddWithoutValidate.Invoke(req.Headers, new object[] { "Range", $"bytes={pos}-" });
                var res = req.GetResponse();
                var total = res.ContentLength + pos;
                onBytesReceived?.Invoke(pos, total);

                // Start download
                using (var stream = res.GetResponseStream())
                using (var fs = new FileStream(Path.Combine(ConfigManager.Config.game_path, filename), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    var read = pos;
                    while (read < total)
                    {
                        var buffer = new byte[4096];
                        fs.Seek(0, SeekOrigin.End);
                        var count = stream.Read(buffer, 0, 4096);
                        read += count;
                        fs.Write(buffer, 0, count);
                        onBytesReceived?.Invoke(read, total);
                    }
                }
            }
            catch
            {
                if (retry == 0) return;
                Thread.Sleep(5000);
                var fi = new FileInfo(Path.Combine(ConfigManager.Config.game_path, filename));
                if (retry == -1)
                {
                    DownloadFile(filename, onBytesReceived, !fi.Exists ? 0 : fi.Length, retry);
                }
                else
                {
                    DownloadFile(filename, onBytesReceived, !fi.Exists ? 0 : fi.Length, retry - 1);
                }
            }
        }
    }
}
