using System;
using System.Text;

namespace GameLauncherUpdater.App.Classes.UpdaterCore.Support
{
    public static class Strings
    {
        public static string Encode(this string Value)
        {
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Value));
        }
    }
}
