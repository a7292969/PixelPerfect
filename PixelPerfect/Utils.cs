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

        public static JObject getVersionData(string version, string gamePath)
        {
            string path = gamePath + "\\versions\\" + version + "\\" + version + ".json";
            return JObject.Parse(File.ReadAllText(path));
        }

        public static string getOriginalVersionPath(string checkVersion, string gamePath)
        {
            try
            {
                JObject ver = JObject.Parse(File.ReadAllText(gamePath + "\\versions\\" + checkVersion + "\\" + checkVersion + ".json"));

                if (ver.ContainsKey("inheritsFrom"))
                {
                    string version = (string)ver["inheritsFrom"];
                    return getOriginalVersionPath(version, gamePath);
                }
                else
                {
                    string version = (string)ver["id"];
                    return gamePath + "\\versions\\" + version + "\\" + version + ".json";
                }
            } catch { return gamePath + "\\versions\\" + checkVersion + "\\" + checkVersion + ".json"; }
        }

        public static List<FileToDownload> getAllLibrariesToDownload(JObject verData, string gamePath)
        {
            string librariesPath = gamePath + "\\libraries\\";
            List<FileToDownload> files = new List<FileToDownload>();

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

                if (o.ContainsKey("name"))
                {
                    string path = ((string)o["name"]).Replace(":", "\\");
                    string jarVersion = Path.GetFileName(path);
                    string jarName = Directory.GetParent(path).Name;

                    path = librariesPath + path.Replace(jarName + "\\" + jarVersion, string.Empty).Replace(".", "\\") + jarName + "\\" + jarVersion + "\\" + jarName + "-" + jarVersion + ".jar";

                    string urlPath = ((string)o["name"]).Replace(":", "/");
                    urlPath = urlPath.Replace(jarName + "/" + jarVersion, string.Empty).Replace(".", "/") + jarName + "/" + jarVersion + "/" + jarName + "-" + jarVersion + ".jar";
                    string url;
                    
                    if (o.ContainsKey("url"))
                        url = (string)o["url"] + "/" + urlPath;
                    else
                        url = "https://libraries.minecraft.net/" + urlPath;

                    string fileName = jarName + "-" + jarVersion + ".jar";

                    FileToDownload file = new FileToDownload(fileName, path, url, "0", 0);
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

            return files;
        }

        public static List<FileToDownload> ParseInheritsDownloads(JObject json, string gamePath)
        {
            List<FileToDownload> files = new List<FileToDownload>();

            files.AddRange(getAllLibrariesToDownload(json, gamePath));

            if (json.ContainsKey("inheritsFrom"))
            {
                JObject inher = getVersionData((string)json["inheritsFrom"], gamePath);
                files.AddRange(ParseInheritsDownloads(inher, gamePath));
            }

            return files;
        }

        public static VersionStartData getAllLibraries(JObject json, string gamePath)
        {
            string librariesPath = gamePath + "\\libraries\\";

            VersionStartData data = new VersionStartData();

            JArray libraries = (JArray)json["libraries"];
            foreach (JObject o in libraries)
            {
                if (o.ContainsKey("rules"))
                {
                    JArray rules = (JArray)o["rules"];
                    JObject rule0 = (JObject)rules[0];

                    if (rule0.ContainsKey("action") && rule0.ContainsKey("os") && (string)rule0["action"] == "allow" && (string)rule0["os"]["name"] == "osx")
                        continue;
                }


                if (o.ContainsKey("name"))
                {
                    string path = ((string)o["name"]).Replace(":", "\\");
                    string jarVersion = Path.GetFileName(path);
                    string jarName = Directory.GetParent(path).Name;

                    path = librariesPath + path.Replace(jarName + "\\" + jarVersion, string.Empty).Replace(".", "\\") + jarName + "\\" + jarVersion + "\\" + jarName + "-" + jarVersion + ".jar";

                    if (!data.libraryPaths.Contains(path))
                        data.libraryPaths.Add(path);
                }


                if (o.ContainsKey("natives") && ((JObject)o["natives"]).ContainsKey("windows"))
                {
                    string keyname = "natives-windows";

                    if (!((JObject)o["downloads"]["classifiers"]).ContainsKey(keyname))
                        keyname = "natives-windows-64";

                    string path = librariesPath + (string)o["downloads"]["classifiers"][keyname]["path"];

                    if (!data.nativeJarsPaths.Contains(path))
                        data.nativeJarsPaths.Add(path);
                }
            }

            return data;
        }

        public static VersionStartData ParseInherits(JObject json, string gamePath)
        {
            VersionStartData data = new VersionStartData();

            data.Add(getAllLibraries(json, gamePath));

            if (json.ContainsKey("inheritsFrom"))
            {
                JObject inher = getVersionData((string)json["inheritsFrom"], gamePath);
                data.Add(ParseInherits(inher, gamePath));
            }
            else
            {
                string version = (string)json["id"];
                string jarPath = gamePath + "\\versions\\" + version + "\\" + version + ".jar";
                data.libraryPaths.Add(jarPath);
            }

            return data;
        }

        public static async Task<List<FileToDownload>> GetFilesForDownload(string version, string gamePath, VersionManifest manifest)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string assetsPath = gamePath + "\\assets\\";
                    string legacyAssetsPath = gamePath + "\\assets\\virtual\\legacy\\";
                    string librariesPath = gamePath + "\\libraries\\";
                    string versionsPath = gamePath + "\\versions\\";


                    // Main version file
                    string originalVersionJsonPath = getOriginalVersionPath(version, gamePath);
                    string originalVersionJsonData;

                    string versionJsonPath = versionsPath + version + "\\" + version + ".json";
                    string versionJsonData;

                    if (originalVersionJsonPath == versionJsonPath)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(versionJsonPath));
                        if (File.Exists(versionJsonPath))
                            versionJsonData = File.ReadAllText(versionJsonPath);
                        else
                        {
                            versionJsonData = Connector.Get(manifest.versions[version].resourcesURL);
                            File.AppendAllText(versionJsonPath, versionJsonData);
                        }

                        originalVersionJsonData = versionJsonData;
                    }
                    else
                    {
                        originalVersionJsonData = File.ReadAllText(originalVersionJsonPath);
                        versionJsonData = File.ReadAllText(versionJsonPath);
                    }



                    JObject originalVerData = JObject.Parse(originalVersionJsonData);
                    JObject verData = JObject.Parse(versionJsonData);


                    // Version index file
                    string assetIndexPath = assetsPath + "indexes\\" + (string)originalVerData["assetIndex"]["id"] + ".json";
                    string assetIndexData;

                    Directory.CreateDirectory(Path.GetDirectoryName(assetIndexPath));
                    if (File.Exists(assetIndexPath))
                        assetIndexData = File.ReadAllText(assetIndexPath);
                    else
                    {
                        assetIndexData = Connector.Get((string)verData["assetIndex"]["url"]);
                        File.AppendAllText(assetIndexPath, assetIndexData);
                    }


                    List<FileToDownload> files = new List<FileToDownload>();

                    // Version jar file
                    files.Add(new FileToDownload(version + ".jar", versionsPath + version + "\\" + version + ".jar", (string)originalVerData["downloads"]["client"]["url"], (string)originalVerData["downloads"]["client"]["sha1"], (long)originalVerData["downloads"]["client"]["size"]));

                    // Logging
                    if (verData.ContainsKey("logging"))
                    {
                        JObject file = (JObject)originalVerData["logging"]["client"]["file"];

                        string name = (string)file["id"];
                        string url = (string)file["url"];

                        string loggingpath = assetsPath + "log_configs\\" + name;

                        if (!File.Exists(loggingpath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(loggingpath));
                            File.AppendAllText(loggingpath, Connector.Get(url));
                        }
                    }

                    // Libraries
                    files.AddRange(ParseInheritsDownloads(verData, gamePath));
                    //JArray libraries = (JArray)verData["libraries"];
                    //foreach (JObject o in libraries)
                    //{
                    //    if (o.ContainsKey("rules"))
                    //    {
                    //        JArray rules = (JArray)o["rules"];
                    //        JObject rule0 = (JObject)rules[0];

                    //        if (rule0.ContainsKey("action") && rule0.ContainsKey("os") && (string)rule0["action"] == "allow" && (string)rule0["os"]["name"] == "osx")
                    //            continue;
                    //    }

                    //    if (o.ContainsKey("downloads") && ((JObject)o["downloads"]).ContainsKey("artifact"))
                    //    {
                    //        string path = librariesPath + (string)o["downloads"]["artifact"]["path"];
                    //        string name = Path.GetFileName(path);
                    //        string url = (string)o["downloads"]["artifact"]["url"];
                    //        string sha1 = (string)o["downloads"]["artifact"]["sha1"];
                    //        long size = (long)o["downloads"]["artifact"]["size"];

                    //        FileToDownload file = new FileToDownload(name, path, url, sha1, size);
                    //        if (!files.Contains(file))
                    //            files.Add(file);
                    //    }

                    //    if (o.ContainsKey("natives") && ((JObject)o["natives"]).ContainsKey("windows"))
                    //    {
                    //        string keyname = "natives-windows";

                    //        if (!((JObject)o["downloads"]["classifiers"]).ContainsKey(keyname))
                    //            keyname = "natives-windows-64";

                    //        string path = librariesPath + (string)o["downloads"]["classifiers"][keyname]["path"];
                    //        string name = Path.GetFileName(path);
                    //        string url = (string)o["downloads"]["classifiers"][keyname]["url"];
                    //        string sha1 = (string)o["downloads"]["classifiers"][keyname]["sha1"];
                    //        long size = (long)o["downloads"]["classifiers"][keyname]["size"];

                    //        files.Add(new FileToDownload(name, path, url, sha1, size));

                    //    }
                    //}

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
                            file = new FileToDownload(prop.Name, path, legacyAssetsPath + prop.Name, "http://resources.download.minecraft.net/" + subHash + "/" + hash, hash, size);
                        else
                            file = new FileToDownload(prop.Name, path, "http://resources.download.minecraft.net/" + subHash + "/" + hash, hash, size);

                        if (!files.Contains(file))
                            files.Add(file);
                    }

                    return files;
                } catch { return null; }
            });
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


                string originalVersionJsonPath = getOriginalVersionPath(version, gamePath);
                string originalVersionJsonData = File.ReadAllText(originalVersionJsonPath);

                string versionJsonPath = versionsPath + version + "\\" + version + ".json";
                string versionJsonData = File.ReadAllText(versionJsonPath);



                string jarPath = versionsPath + version + "\\" + version + ".jar";

                //List<string> librariesPaths = new List<string>();
                //List<string> nativeJarsPaths = new List<string>();

                JObject originalVerData = JObject.Parse(originalVersionJsonData);
                JObject verData = JObject.Parse(versionJsonData);

                VersionStartData versionStartData = ParseInherits(verData, gamePath);

                // Libraries
                //JArray libraries = (JArray)verData["libraries"];
                //foreach (JObject o in libraries)
                //{
                //    if (o.ContainsKey("rules"))
                //    {
                //        JArray rules = (JArray)o["rules"];
                //        JObject rule0 = (JObject)rules[0];

                //        if (rule0.ContainsKey("action") && rule0.ContainsKey("os") && (string)rule0["action"] == "allow" && (string)rule0["os"]["name"] == "osx")
                //            continue;
                //    }

                //    if (o.ContainsKey("downloads") && ((JObject)o["downloads"]).ContainsKey("artifact"))
                //    {
                //        string path = ((string)o["name"]).Replace(":", "\\");
                //        string jarVersion = Path.GetFileName(path);
                //        string jarName = Directory.GetParent(path).Name;

                //        path = librariesPath + path.Replace(jarName + "\\" + jarVersion, string.Empty).Replace(".", "\\") + jarName + "\\" + jarVersion + "\\" + jarName + "-" + jarVersion + ".jar";

                //        if (!librariesPaths.Contains(path))
                //            librariesPaths.Add(path);
                //    }

                //    if (o.ContainsKey("natives") && ((JObject)o["natives"]).ContainsKey("windows"))
                //    {
                //        string keyname = "natives-windows";

                //        if (!((JObject)o["downloads"]["classifiers"]).ContainsKey(keyname))
                //            keyname = "natives-windows-64";

                //        string path = librariesPath + (string)o["downloads"]["classifiers"][keyname]["path"];

                //        if (!nativeJarsPaths.Contains(path))
                //            nativeJarsPaths.Add(path);
                //    }
                //}

                // Natives
                foreach (string path in versionStartData.nativeJarsPaths)
                {
                    using (ZipFile zip1 = ZipFile.Read(path))
                    {
                        foreach (ZipEntry e in zip1)
                        {
                            try
                            {
                                if (!e.FileName.Contains("META-INF"))
                                    e.Extract(nativesPath, ExtractExistingFileAction.OverwriteSilently);
                            } catch { }
                        }
                    }
                }


                // Other Data
                string mainClass = (string)verData["mainClass"];
                string assetIndex = (string)originalVerData["assetIndex"]["id"];
                string versionType = (string)verData["type"];

                string cAssetsPath = (string)verData["assets"] == "legacy" ? legacyAssetsPath : assetsPath;

                string librariesStr = "";
                foreach (string path in versionStartData.libraryPaths)
                    librariesStr += path + ";";
                //librariesStr += jarPath;

                string loggingPath = "LOG_CONFIG";
                if (verData.ContainsKey("logging"))
                {
                    JObject file = (JObject)originalVerData["logging"]["client"]["file"];
                    string name = (string)file["id"];
                    loggingPath = assetsPath + "log_configs\\" + name;
                }


                string mcArgs = "--username ${auth_player_name} --version ${version_name} --gameDir ${game_directory} --assetsDir ${assets_root} --assetIndex ${assets_index_name} --uuid ${auth_uuid} --accessToken ${auth_access_token} --userType ${user_type} --versionType ${version_type}";
                string jvmArgs = "-Djava.library.path=${natives_directory} -cp ${classpath} -Xss1M ${custom_jvm_args} -Dlog4j.configurationFile=${log_config_path} ${main_class}";

                if (verData.ContainsKey("minecraftArguments")) // Old arguments system
                {
                    mcArgs = (string)verData["minecraftArguments"] + " --width ${resolution_width} --height ${resolution_height}";
                }
                else if (originalVerData.ContainsKey("arguments")) // New arguments system
                {
                    JObject o = (JObject)originalVerData["arguments"];
                    JArray game = (JArray)o["game"];

                    mcArgs = "--width ${resolution_width} --height ${resolution_height} ";

                    for (int i = 0; i < game.Count; i++)
                    {
                        try
                        {
                            string str = (string)game[i];
                            mcArgs += str + " ";
                        } catch { }
                    }

                    if (verData.ContainsKey("arguments"))
                    {
                        o = (JObject)verData["arguments"];
                        game = (JArray)o["game"];

                        for (int i = 0; i < game.Count; i++)
                        {
                            try
                            {
                                string str = (string)game[i];

                                if (!mcArgs.Contains(str))
                                    mcArgs += str + " ";
                            }
                            catch { }
                        }
                    }
                }


                string args = "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump " +
                    "-Dminecraft.launcher.brand=minecraft-launcher -Dminecraft.launcher.version=3.0.0000 " +
                    jvmArgs + " " + mcArgs;

                args = args.Replace("${natives_directory}", "\"" + nativesPath + "\"");
                args = args.Replace("${classpath}", "\"" + librariesStr + "\"");
                args = args.Replace("${custom_jvm_args}", javaArgs);
                args = args.Replace("${log_config_path}", "\"" + loggingPath + "\"");
                args = args.Replace("${main_class}", mainClass);

                args = args.Replace("${auth_player_name}", username);
                args = args.Replace("${version_name}", version);
                args = args.Replace("${game_directory}", "\"" + profilePath + "\"");
                args = args.Replace("${assets_root}", "\"" + cAssetsPath.TrimEnd(Path.DirectorySeparatorChar) + "\"");
                args = args.Replace("${assets_index_name}", assetIndex);
                args = args.Replace("${auth_uuid}", uuid);
                args = args.Replace("${auth_access_token}", accessToken);
                args = args.Replace("${version_type}", versionType);
                args = args.Replace("${user_type}", "mojang");
                args = args.Replace("${resolution_width}", width.ToString());
                args = args.Replace("${resolution_height}", height.ToString());
                args = args.Replace("${user_properties}", "{}");

                Console.WriteLine(args);
                return args;
            });
        }
    }
}
