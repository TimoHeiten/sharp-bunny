using System.Text;
using Newtonsoft.Json;

namespace SharpBunny
{
    public static class Config
    {
        public static string ContentEncoding => "utf-8";
        public static string ContentType  => "application/json";

        public static byte[] Serialize<T>(T msg)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
        }
    }
}