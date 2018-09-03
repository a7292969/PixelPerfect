using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PixelPerfect
{
    class Utils
    {
        public static byte[] hexBytes = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f' };

        public static string GetResourcePath(string path)
        {
            return @"pack://application:,,,/" + path;
        }

        public static BitmapImage ImageFromResource(string path)
        {
            Console.WriteLine(GetResourcePath(path));
            return new BitmapImage(new Uri(GetResourcePath(path), UriKind.Absolute));
        }

        public static async Task<Dictionary<string, string>> GetMojangStatus()
        {
            string response = await Connector.GetAsync("https://status.mojang.com/check");

            if (response == "-1")
                return null;

            JArray servers = JArray.Parse(response);
            Dictionary<string, string> map = new Dictionary<string, string>();

            map.Add("minecraft.net", (string)servers[0]["minecraft.net"]);
            map.Add("session.minecraft.net", (string)servers[1]["session.minecraft.net"]);
            map.Add("account.mojang.com", (string)servers[2]["account.mojang.com"]);
            map.Add("authserver.mojang.com", (string)servers[3]["authserver.mojang.com"]);
            map.Add("sessionserver.mojang.com", (string)servers[4]["sessionserver.mojang.com"]);
            map.Add("api.mojang.com", (string)servers[5]["api.mojang.com"]);
            map.Add("textures.minecraft.net", (string)servers[6]["textures.minecraft.net"]);
            map.Add("mojang.com", (string)servers[7]["mojang.com"]);

            return map;
        }

        public static async Task<string> GetPlayerUUID(string username)
        {
            string response = await Connector.GetAsync("https://api.mojang.com/users/profiles/minecraft/" + username);
            JObject o = JObject.Parse(response);
            return (string)o["id"];
        }

        public static string GenerateClientToken()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");

            Random random = new Random();

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == '-')
                    continue;

                if (random.Next(100) >= 50)
                    bytes[i] = Encoding.UTF8.GetBytes(random.Next(10).ToString())[0];
                else
                    bytes[i] = hexBytes[random.Next(6)];
            }

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
