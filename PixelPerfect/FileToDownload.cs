using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfect
{
    public class FileToDownload
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
    }
}
