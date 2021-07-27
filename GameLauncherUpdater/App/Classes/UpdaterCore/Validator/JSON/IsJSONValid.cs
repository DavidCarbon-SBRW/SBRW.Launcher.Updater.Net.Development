using System;

namespace GameLauncherUpdater.App.Classes.UpdaterCore.Validator.JSON
{
    class IsJSONValid
    {
        public static bool ValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput))
            {
                return false;
            }
            else
            {
                try
                {
                    strInput = strInput.Trim();
                    if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || /* For object */
                        (strInput.StartsWith("[") && strInput.EndsWith("]"))) /* For array */
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
