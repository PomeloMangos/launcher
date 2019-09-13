using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace WoWLauncher
{
    public class JsonUtil
    {
        public static T DeserializeObject<T>(string json)
        {
            DataContractJsonSerializer jsonFormator = new DataContractJsonSerializer(typeof(T));
            using (Stream readStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)jsonFormator.ReadObject(readStream);
            }
        }

        public static string SerializeObject(object obj)
        {
            DataContractJsonSerializer jsonFormator = new DataContractJsonSerializer(obj.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                jsonFormator.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
