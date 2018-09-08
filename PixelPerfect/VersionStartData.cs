using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfect
{
    class VersionStartData
    {
        public List<string> libraryPaths;
        public List<string> nativeJarsPaths;

        public VersionStartData()
        {
            libraryPaths = new List<string>();
            nativeJarsPaths = new List<string>();
        }

        public void Add(VersionStartData data)
        {
            libraryPaths.AddRange(data.libraryPaths);
            nativeJarsPaths.AddRange(data.nativeJarsPaths);
        }
    }
}
