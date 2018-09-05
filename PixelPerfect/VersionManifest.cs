using System.Collections.Generic;

namespace PixelPerfect
{
    public class VersionManifest
    {
        public Dictionary<string, MCVersion> versions;
        public string latestVersion;
        public string latestSnapshot;

        public VersionManifest(Dictionary<string, MCVersion> versions, string latestVersion, string latestSnapshot)
        {
            this.versions = versions;
            this.latestVersion = latestVersion;
            this.latestSnapshot = latestSnapshot;
        }
    }
}
