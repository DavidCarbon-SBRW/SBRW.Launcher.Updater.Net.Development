using System.Text;

namespace GameLauncherUpdater.App.Classes.UpdaterCore.Support
{
    class Strings
    {
        public static string Encode(string Value)
        {
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Value));
        }
    }
}
