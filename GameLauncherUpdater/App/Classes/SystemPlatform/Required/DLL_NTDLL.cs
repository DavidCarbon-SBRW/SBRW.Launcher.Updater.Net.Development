using System;
using System.Runtime.InteropServices;

namespace GameLauncherUpdater.App.Classes.SystemPlatform.Required
{
    /// <summary>
    /// User-mode face of the Windows kernel to support any number of application-level subsystems
    /// </summary>
    public class DLL_NTDLL
    {
        /// <summary>
		/// Returns used <see href="https://wiki.winehq.org/Developer_FAQ#How_can_I_detect_Wine.3F">Wine version</see>
		/// </summary>
		/// <returns>Used Wine version</returns>
        [DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern string GetWineVersion();
        /// <summary>
		/// Returns used <see href="https://wiki.winehq.org/Developer_FAQ#How_can_I_detect_Wine.3F">Wine version</see>
		/// </summary>
		/// <returns>Wine version if user has not disabled the function, otherwise string.Empty</returns>
        public static string WineVersion()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(GetWineVersion()))
                {
                    return GetWineVersion();
                }
            }
            catch (Exception)
            {
                
            }

            return default;
        }
        /// <summary>
        /// Returns used <see href="https://source.winehq.org/git/wine.git/blob/HEAD:/include/wine/library.h">Wine build</see>
        /// </summary>
        /// <returns>Wine build being used</returns>
        [DllImport("ntdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "wine_get_build_id")]
        private static extern string GetWineBuildId();
        /// <summary>
        /// Returns used <see href="https://source.winehq.org/git/wine.git/blob/HEAD:/include/wine/library.h">Wine build</see>
        /// </summary>
        /// <returns>Wine build if user has not disabled the function, otherwise string.Empty</returns>
        public static string WineBuildId()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(GetWineBuildId()))
                {
                    return GetWineBuildId();
                }
            }
            catch (Exception)
            {
                
            }

            return default;
        }
        /// <summary>
        /// Returns <see href="https://source.winehq.org/git/wine.git/blob/HEAD:/include/wine/library.h">Wine host</see>
        /// </summary>
        /// <returns>Wine host being used</returns>
        [DllImport("ntdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "wine_get_host_version")]
        private static extern void GetWineHostVersion(out string sysname, out string release);
        /// <summary>
        /// Returns <see href="https://source.winehq.org/git/wine.git/blob/HEAD:/include/wine/library.h">Wine host</see>
        /// </summary>
        /// <returns>Wine host being used</returns>
        public static string WineHostVersion()
        {
            string Compiled_Host_Version = default;

            try
            {
                GetWineHostVersion(out string sysname, out string release);

                if (!string.IsNullOrWhiteSpace(sysname))
                {
                    Compiled_Host_Version += $" {sysname}";
                }

                if (!string.IsNullOrWhiteSpace(release))
                {
                    Compiled_Host_Version += $" {release}";
                }
            }
            catch (Exception)
            {
                
            }

            return Compiled_Host_Version;
        }
        /// <summary>
        /// Checks <see cref="WineVersion()">Version</see>, 
        /// <see cref="WineBuildId()">Build ID</see>, and <see cref="WineHostVersion()">Host Version</see> found in Wine.
        /// </summary>
        /// <returns>True if Wine was Detected, otherwise False</returns>
        public static bool WineDetected()
        {
            return !string.IsNullOrWhiteSpace(WineVersion()) ||
                !string.IsNullOrWhiteSpace(WineBuildId()) ||
                !string.IsNullOrWhiteSpace(WineHostVersion());
        }
        /// <summary>
        /// Checks if Mono RunTime is being Used
        /// </summary>
        /// <returns>True if Mono RunTime was Detected, otherwise False</returns>
        public static bool MonoDetected()
        {
            try
            {
                return Type.GetType("Mono.Runtime") != null;
            }
            catch (Exception)
            {
                
            }

            return false;
        }
    }
}
