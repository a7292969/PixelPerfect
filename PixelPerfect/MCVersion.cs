using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfect
{
    public class MCVersion
    {
        public string type;
        public string resourcesURL;

        public MCVersion(string type, string resourcesURL)
        {
            this.type = type;
            this.resourcesURL = resourcesURL;
        }
    }
}
