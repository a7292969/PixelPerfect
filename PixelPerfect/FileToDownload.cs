using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfect
{
    public class FileToDownload
    {
        public string path;
        public string url;

        public FileToDownload(string path, string url)
        {
            this.path = path;
            this.url = url;
        }
    }
}
