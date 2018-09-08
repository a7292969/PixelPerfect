using Ionic.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
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

        public static string ComputeFileHash(string path)
        {
            FileStream file = File.OpenRead(path);
            string hash = BitConverter.ToString(SHA1.Create().ComputeHash(file));
            hash = hash.Replace("-", string.Empty).ToLower();
            return hash;
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

        public static async Task<string> GetAccessData(string clientToken, string username, string password)
        {
            JObject payload = new JObject();
            payload["username"] = username;
            payload["password"] = password;
            payload["clientToken"] = clientToken;

            JObject agent = new JObject();
            agent["name"] = "Minecraft";
            agent["version"] = 1;

            payload["agent"] = agent;

            return await Connector.PostAsync("https://authserver.mojang.com/authenticate", payload.ToString());
        }

        public static string ValidateAccessData(string accessToken, string clientToken)
        {
            JObject payload = new JObject();
            payload["accessToken"] = accessToken;
            payload["clientToken"] = clientToken;

            return Connector.Post("https://authserver.mojang.com/validate", payload.ToString());
        }

        public static string RefreshAccessData(string accessToken, string clientToken)
        {
            JObject payload = new JObject();
            payload["accessToken"] = accessToken;
            payload["clientToken"] = clientToken;

            return Connector.Post("https://authserver.mojang.com/refresh", payload.ToString());
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

                if (type != "old_alpha" && type != "old_beta")
                {
                    MCVersion version = new MCVersion(type, url);
                    versions.Add(id, version);
                }

                if (id == "1.6.4")
                    break;
            }

            JObject latest = (JObject)o["latest"];
            return new VersionManifest(versions, latest["release"].ToString(), latest["snapshot"].ToString());
        }

        public static async Task<List<FileToDownload>> GetFilesForDownload(string version, string gamePath, VersionManifest manifest)
        {
            string assetsPath = gamePath + "\\assets\\";
            string legacyAssetsPath = gamePath + "\\assets\\virtual\\legacy\\";
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


            List<FileToDownload> files = new List<FileToDownload>();

            // Version jar file
            files.Add(new FileToDownload(version + ".jar", versionsPath + version + "\\" + version + ".jar", (string)verData["downloads"]["client"]["url"], (string)verData["downloads"]["client"]["sha1"], (long)verData["downloads"]["client"]["size"]));

            // Logging
            if (verData.ContainsKey("logging"))
            {
                JObject file = (JObject)verData["logging"]["client"]["file"];

                string name = (string)file["id"];
                string url = (string)file["url"];

                string loggingpath = assetsPath + "log_configs\\" + name;

                if (!File.Exists(loggingpath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(loggingpath));
                    File.AppendAllText(loggingpath, await Connector.GetAsync(url));
                }
            }

            // Libraries
            JArray libraries = (JArray)verData["libraries"];
            foreach (JObject o in libraries)
            {
                if (o.ContainsKey("rules"))
                {
                    JArray rules = (JArray)o["rules"];
                    JObject rule0 = (JObject)rules[0];

                    if (rule0.ContainsKey("action") && rule0.ContainsKey("os") && (string)rule0["action"] == "allow" && (string)rule0["os"]["name"] == "osx")
                        continue;
                }

                if (o.ContainsKey("downloads") && ((JObject)o["downloads"]).ContainsKey("artifact"))
                {
                    string path = librariesPath + (string)o["downloads"]["artifact"]["path"];
                    string name = Path.GetFileName(path);
                    string url = (string)o["downloads"]["artifact"]["url"];
                    string sha1 = (string)o["downloads"]["artifact"]["sha1"];
                    long size = (long)o["downloads"]["artifact"]["size"];

                    FileToDownload file = new FileToDownload(name, path, url, sha1, size);
                    if (!files.Contains(file))
                        files.Add(file);
                }

                if (o.ContainsKey("natives") && ((JObject)o["natives"]).ContainsKey("windows"))
                {
                    string keyname = "natives-windows";

                    if (!((JObject)o["downloads"]["classifiers"]).ContainsKey(keyname))
                        keyname = "natives-windows-64";

                    string path = librariesPath + (string)o["downloads"]["classifiers"][keyname]["path"];
                    string name = Path.GetFileName(path);
                    string url = (string)o["downloads"]["classifiers"][keyname]["url"];
                    string sha1 = (string)o["downloads"]["classifiers"][keyname]["sha1"];
                    long size = (long)o["downloads"]["classifiers"][keyname]["size"];

                    files.Add(new FileToDownload(name, path, url, sha1, size));

                }
            }

            // Assets
            JObject assets = (JObject)JObject.Parse(assetIndexData)["objects"];
            foreach (JProperty prop in assets.Properties())
            {
                JObject o = (JObject)prop.Value;

                string hash = (string)o["hash"];
                long size = (long)o["size"];
                string subHash = hash.Substring(0, 2);
                string path = assetsPath + "objects\\" + subHash + "\\" + hash;


                FileToDownload file;

                if ((string)verData["assets"] == "legacy")
                {
                    file = new FileToDownload(prop.Name, path, legacyAssetsPath + prop.Name, "http://resources.download.minecraft.net/" + subHash + "/" + hash, hash, size);
                }
                else
                {
                    file = new FileToDownload(prop.Name, path, "http://resources.download.minecraft.net/" + subHash + "/" + hash, hash, size);
                }

                if (!files.Contains(file))
                    files.Add(file);
            }

            return files;
        }

        public static async Task<string> CreateMinecraftStartArgs(string version, string javaArgs, string gamePath, string profilePath, string username, string uuid, string accessToken, int width, int height)
        {
            return await Task.Run(() =>
            {
                string nativesPath = Path.GetTempPath() + "\\PixelPerfectMCNatives";
                string assetsPath = gamePath + "\\assets\\";
                string legacyAssetsPath = gamePath + "\\assets\\virtual\\legacy\\";
                string librariesPath = gamePath + "\\libraries\\";
                string versionsPath = gamePath + "\\versions\\";

                string versionJsonPath = versionsPath + version + "\\" + version + ".json";
                string versionJsonData = File.ReadAllText(versionJsonPath);
                string jarPath = versionsPath + version + "\\" + version + ".jar";

                List<string> librariesPaths = new List<string>();
                List<string> nativeJarsPaths = new List<string>();


                JObject verData = JObject.Parse(versionJsonData);

                // Libraries
                JArray libraries = (JArray)verData["libraries"];
                foreach (JObject o in libraries)
                {
                    if (o.ContainsKey("rules"))
                    {
                        JArray rules = (JArray)o["rules"];
                        JObject rule0 = (JObject)rules[0];

                        if (rule0.ContainsKey("action") && rule0.ContainsKey("os") && (string)rule0["action"] == "allow" && (string)rule0["os"]["name"] == "osx")
                            continue;
                    }

                    if (o.ContainsKey("downloads") && ((JObject)o["downloads"]).ContainsKey("artifact"))
                    {
                        string path = ((string)o["name"]).Replace(":", "\\");
                        string jarVersion = Path.GetFileName(path);
                        string jarName = Directory.GetParent(path).Name;

                        path = librariesPath + path.Replace(jarName + "\\" + jarVersion, string.Empty).Replace(".", "\\") + jarName + "\\" + jarVersion + "\\" + jarName + "-" + jarVersion + ".jar";

                        if (!librariesPaths.Contains(path))
                            librariesPaths.Add(path);
                    }

                    if (o.ContainsKey("natives") && ((JObject)o["natives"]).ContainsKey("windows"))
                    {
                        string keyname = "natives-windows";

                        if (!((JObject)o["downloads"]["classifiers"]).ContainsKey(keyname))
                            keyname = "natives-windows-64";

                        string path = librariesPath + (string)o["downloads"]["classifiers"][keyname]["path"];

                        if (!nativeJarsPaths.Contains(path))
                            nativeJarsPaths.Add(path);
                    }
                }

                // Natives
                foreach (string path in nativeJarsPaths)
                {
                    using (ZipFile zip1 = ZipFile.Read(path))
                    {
                        foreach (ZipEntry e in zip1)
                        {
                            if (!e.FileName.Contains("META-INF"))
                                e.Extract(nativesPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }


                // Other Data
                string mainClass = (string)verData["mainClass"];
                string assetIndex = (string)verData["assetIndex"]["id"];
                string versionType = (string)verData["type"];

                string cAssetsPath = (string)verData["assets"] == "legacy" ? legacyAssetsPath : assetsPath;

                string librariesStr = "";
                foreach (string path in librariesPaths)
                    librariesStr += path + ";";
                librariesStr += jarPath;

                string loggingPath = "LOG_CONFIG";
                if (verData.ContainsKey("logging"))
                {
                    JObject file = (JObject)verData["logging"]["client"]["file"];
                    string name = (string)file["id"];
                    loggingPath = assetsPath + "log_configs\\" + name;
                }

                // Args pattern
                string args = "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump " +
                    "-Xss1M -Djava.library.path=NATIVES_PATH -Dminecraft.launcher.brand=minecraft-launcher -Dminecraft.launcher.version=3.0.0000 -cp DATA_LIBRARIES " +
                    "JAVA_ARGS -Dlog4j.configurationFile=LOG_CONFIG MAIN_CLASS " +
                    "--username DATA_USERNAME --version DATA_VERSION --gameDir DATA_GAMEDIR --assetsDir DATA_ASSETSDIR --assetIndex DATA_ASSETINDEX " +
                    "--uuid DATA_UUID --accessToken DATA_ACCESSTOKEN --userProperties {} --userType DATA_USERTYPE --versionType DATA_TYPE --width DATA_WIDTH --height DATA_HEIGHT";

                args = args.Replace("NATIVES_PATH", nativesPath);
                args = args.Replace("DATA_LIBRARIES", librariesStr);
                args = args.Replace("JAVA_ARGS", javaArgs);
                args = args.Replace("LOG_CONFIG", loggingPath);
                args = args.Replace("MAIN_CLASS", mainClass);
                args = args.Replace("DATA_USERNAME", username);
                args = args.Replace("DATA_VERSION", version);
                args = args.Replace("DATA_GAMEDIR", profilePath);
                args = args.Replace("DATA_ASSETSDIR", cAssetsPath);
                args = args.Replace("DATA_ASSETINDEX", assetIndex);
                args = args.Replace("DATA_UUID", uuid);
                args = args.Replace("DATA_ACCESSTOKEN", accessToken);
                args = args.Replace("DATA_USERTYPE", "mojang");
                args = args.Replace("DATA_TYPE", versionType);
                args = args.Replace("DATA_WIDTH", width.ToString());
                args = args.Replace("DATA_HEIGHT", height.ToString());

                return args;
            });
        }
    }
}
