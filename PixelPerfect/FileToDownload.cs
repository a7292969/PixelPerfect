using System;

namespace PixelPerfect
{
    public class FileToDownload : IEquatable<FileToDownload>
    {
        public string name;
        public string path;
        public string url;
        public string hash;
        public long size;

        public FileToDownload(string name, string path, string url, string hash, long size)
        {
            this.name = name;
            this.path = path;
            this.url = url;
            this.hash = hash;
            this.size = size;
        }

        public bool Equals(FileToDownload other)
        {
            return path == other.path;
        }
    }
}
