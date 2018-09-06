using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        public static byte[] ImageToBytes(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                PngBitmapEncoder enc = new PngBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);

                Bitmap bitmap = new Bitmap(outStream);

                return (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[]));
            }
        }

        public static BitmapImage BytesToImage(byte[] array)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                memory.Write(array, 0, array.Length);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
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

        public static VersionManifest GetMCVersions()
        {
            string response = Connector.Get("https://launchermeta.mojang.com/mc/game/version_manifest.json");

            if (response == "-1")
                return null;

            Dictionary<string, MCVersion> versions = new Dictionary<string, MCVersion>();

            JObject o = JObject.Parse(response);
            JArray vers = (JArray)o["versions"];

            foreach (JObject obj in vers)
            {
                string id = obj["id"].ToString();
                string type = obj["type"].ToString();
                string url = obj["url"].ToString();

                MCVersion version = new MCVersion(type, url);
                versions.Add(id, version);
            }

            JObject latest = (JObject)o["latest"];
            return new VersionManifest(versions, latest["release"].ToString(), latest["snapshot"].ToString());
        }

        public static async Task<Dictionary<string, FileToDownload>> GetFilesForDownload(string version, string gamePath, VersionManifest manifest)
        {
            string assetsPath = gamePath + "\\assets\\";
            string librariesPath = gamePath + "\\libraries\\";
            string versionsPath = gamePath + "\\versions\\";


            // Main version file
            string versionJsonPath = versionsPath + version + "\\" + version + ".json";
            string versionJsonData;

            Directory.CreateDirectory(Path.GetDirectoryName(versionJsonPath));
            if (File.Exists(versionJsonPath))
                versionJsonData = File.ReadAllText(versionJsonPath);
            else
            {
                versionJsonData = await Connector.GetAsync(manifest.versions[version].resourcesURL);
                File.AppendAllText(versionJsonPath, versionJsonData);
            }


            JObject verData = JObject.Parse(versionJsonData);


            // Version index file
            string assetIndexPath = assetsPath + "indexes\\" + (string)verData["assetIndex"]["id"] + ".json";
            string assetIndexData;

            Directory.CreateDirectory(Path.GetDirectoryName(assetIndexPath));
            if (File.Exists(assetIndexPath))
                assetIndexData = File.ReadAllText(assetIndexPath);
            else
            {
                assetIndexData = await Connector.GetAsync((string)verData["assetIndex"]["url"]);
                File.AppendAllText(assetIndexPath, assetIndexData);
            }


            Dictionary<string, FileToDownload> files = new Dictionary<string, FileToDownload>();

            // Version jar file
            files.Add(version + ".jar", new FileToDownload(versionsPath + version + "\\" + version + ".jar", (string)verData["downloads"]["client"]["url"]));

            // Libraries
            JArray libraries = (JArray)verData["libraries"];
            foreach (JObject o in libraries)
            {
                if (o.ContainsKey("downloads"))
                {
                    string path = librariesPath + (string)o["downloads"]["artifact"]["path"];
                    string name = Path.GetFileName(path);
                    string url = (string)o["downloads"]["artifact"]["url"];

                    files.Add(name, new FileToDownload(path, url));
                }
            }

            return files;
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
