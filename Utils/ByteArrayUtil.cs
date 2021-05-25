using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RFGarage.Utils
{
    public static class ByteArrayUtil
    {
        public static byte[] Serialize<T>(this T m)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, m);
                return ms.ToArray();
            }
        }
        public static T Deserialize<T>(this byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return (T)new BinaryFormatter().Deserialize(ms);
            }
        }
    }
}