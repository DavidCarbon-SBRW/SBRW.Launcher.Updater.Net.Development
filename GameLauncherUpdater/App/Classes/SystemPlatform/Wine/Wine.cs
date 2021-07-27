using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncherUpdater.App.Classes.SystemPlatform.Wine
{
    class Wine
    {
        public static bool Detected()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
