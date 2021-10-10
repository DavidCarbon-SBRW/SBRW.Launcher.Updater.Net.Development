using System;

namespace GameLauncherUpdater.App.Classes.SystemPlatform.UnixOS
{
    class UnixOS
    {
        private static int CachedNumPlatform = 2020;

        public static int Platform()
        {
            try
            {
                if (CachedNumPlatform == 2020)
                {
                    CachedNumPlatform = (int)Environment.OSVersion.Platform;
                }

                return CachedNumPlatform;
            }
            catch
            {
                return CachedNumPlatform;
            }
        }

        public static PlatformIDPort ID(int Number)
        {
            switch (Number)
            {
                case 0:
                    return PlatformIDPort.Win32S;
                case 1:
                    return PlatformIDPort.Win32Windows;
                case 2:
                    return PlatformIDPort.Win32NT;
                case 3:
                    return PlatformIDPort.WinCE;
                case 4:
                    return PlatformIDPort.Unix;
                case 5:
                    return PlatformIDPort.Xbox;
                case 6:
                    return PlatformIDPort.MacOSX;
                case 128:
                    return PlatformIDPort.MonoLegacy;
                default:
                    return PlatformIDPort.Unknown;
            }
        }

        public static bool AmI()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                return true;
            }
            else
            {
                switch (ID(Platform()))
                {
                    case PlatformIDPort.Unix:
                    case PlatformIDPort.MonoLegacy:
                    case PlatformIDPort.MacOSX:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public static bool Detected() => AmI();
    }
    /// <summary>
    /// Identifies the operating system, or platform, supported by an assembly.
    /// </summary>
    enum PlatformIDPort
    {
        /// <summary>
        /// The operating system is Win32s. This value is no longer in use.
        /// </summary>
        Win32S = 0,
        /// <summary>
        /// The operating system is Windows 95 or Windows 98. This value is no longer in use.
        /// </summary>
        Win32Windows = 1,
        /// <summary>
        /// The operating system is Windows NT or later.
        /// </summary>
        Win32NT = 2,
        /// <summary>
        /// The operating system is Windows CE. This value is no longer in use.
        /// </summary>
        WinCE = 3,
        /// <summary>
        /// The operating system is Unix.
        /// </summary>
        Unix = 4,
        /// <summary>
        /// The development platform is Xbox 360. This value is no longer in use.
        /// </summary>
        Xbox = 5,
        /// <summary>
        /// The operating system is Macintosh. This value was returned by Silverlight. On .NET Core, its replacement is Unix.
        /// </summary>
        MacOSX = 6,
        /// <summary>
        /// The operating system is Unix. This value was returned by Mono CLR 1.x runtime.
        /// </summary>
        MonoLegacy = 128,
        /// <summary>
        /// Unable to detect operating system. This value is used as a fail safe.
        /// </summary>
        Unknown = 2017
    }
}
