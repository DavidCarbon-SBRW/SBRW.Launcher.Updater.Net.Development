using System.Collections.Generic;

namespace GameLauncherUpdater.App.Classes.UpdaterCore.Json
{
    class GitHubReleaseSchema
    {
        public string tag_name { get; set; }

        public string name { get; set; }

        public string prerelease { get; set; }
        
        public List<AssetModel> assets { get; set; }

        public class AssetModel
        {
            public string name { get; set; }

            public string browser_download_url { get; set; }
        }
    }
}
