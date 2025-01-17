﻿using System;

namespace GameLauncherUpdater.App.Classes.UpdaterCore.Support
{
    /// <summary>
    /// Version Extensions
    /// </summary>
    public static class Versions
    {
        /// <summary>
        /// Checks if Version is Current
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns><b>True</b> or <b>False</b></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static bool Current(this Version v1, Version v2)
        {
            if (v1.Equals(default) || v2.Equals(default))
            {
                throw new ArgumentNullException();
            }
            else
            {
                return v1.CompareVersions(v2) == 0;
            }
        }
        /// <summary>
        /// Checks if Version is Current
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns><b>True</b> or <b>False</b></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static bool Current(this string v1, string v2)
        {
            return v1.CompareVersions(v2) == 0;
        }
        /// <summary>
        /// Checks if Version is Outdated
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns><b>True</b> or <b>False</b></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static bool Outdated(this Version v1, Version v2)
        {
            if (v1.Equals(default) || v2.Equals(default))
            {
                throw new ArgumentNullException();
            }
            else
            {
                return v1.CompareVersions(v2) == -1;
            }
        }
        /// <summary>
        /// Checks if Version is Outdated
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns><b>True</b> or <b>False</b></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static bool Outdated(this string v1, string v2)
        {
            return v1.CompareVersions(v2) == -1;
        }
        /// <summary>
        /// Checks if Version is New
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns><b>True</b> or <b>False</b></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static bool Preview(this Version v1, Version v2)
        {
            if (v1.Equals(default) || v2.Equals(default))
            {
                throw new ArgumentNullException();
            }
            else
            {
                return v1.CompareVersions(v2) == 1;
            }
        }
        /// <summary>
        /// Checks if Version is New
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns><b>True</b> or <b>False</b></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static bool Preview(this string v1, string v2)
        {
            return v1.CompareVersions(v2) == 1;
        }
        /// <summary>
        /// Compare two version strings.
        /// V1 and V2 can have different number of components.
        /// Components must be delimited by dot.
        /// </summary>
        /// <remarks>
        /// Modified Version based off <see href="https://stackoverflow.com/a/68595578/17539426">Stack Overflow</see>
        /// </remarks>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>
        /// <b>-1</b> if v1 is lower version number than v2<br></br>
        /// <b>0</b> if v1 == v2<br></br>
        /// <b>1</b> if v1 is higher version number than v2<br></br>
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static int CompareVersions(this Version v1, Version v2)
        {
            if (v1.Equals(default) || v2.Equals(default))
            {
                throw new ArgumentNullException();
            }
            else
            {
                return v1.ToString().CompareVersions(v2.ToString());
            }
        }
        /// <summary>
        /// Compare two version strings, e.g.  "3.2.1.0.b40" and "3.10.1.a".
        /// V1 and V2 can have different number of components.
        /// Components must be delimited by dot.
        /// </summary>
        /// <remarks>
        /// Modified Version based off <see href="https://stackoverflow.com/a/68595578/17539426">Stack Overflow</see>
        /// </remarks>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>
        /// <b>-1</b> if v1 is lower version number than v2<br></br>
        /// <b>0</b> if v1 == v2<br></br>
        /// <b>1</b> if v1 is higher version number than v2<br></br>
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static int CompareVersions(this string v1, string v2)
        {
            if (string.IsNullOrWhiteSpace(v1) || string.IsNullOrWhiteSpace(v2))
            {
                throw new ArgumentNullException();
            }
            else
            {
                v1 = v1.ToLowerInvariant();
                v2 = v2.ToLowerInvariant();

                if (v1 == v2)
                {
                    return 0;
                }
                else
                {
                    int rc = -1000;
                    string[] v1_parts = v1.Split('.');
                    string[] v2_parts = v2.Split('.');

                    for (int i = 0; i < v1_parts.Length; i++)
                    {
                        if (v2_parts.Length < i + 1)
                        {
                            /* we're done here */
                            break;
                        }
                        else
                        {
                            string v1_Token = v1_parts[i];
                            string v2_Token = v2_parts[i];

                            bool v1_Numeric = int.TryParse(v1_Token, out int oh);
                            bool v2_Numeric = int.TryParse(v2_Token, out int hi);

                            /* handle scenario {"2" versus "20"} by prepending zeroes, e.g. it would become {"02" versus "20"} */
                            if (v1_Numeric && v2_Numeric)
                            {
                                while (v1_Token.Length < v2_Token.Length)
                                {
                                    v1_Token = "0" + v1_Token;
                                }

                                while (v2_Token.Length < v1_Token.Length)
                                {
                                    v2_Token = "0" + v2_Token;
                                }
                            }

                            rc = string.Compare(v1_Token, v2_Token, StringComparison.Ordinal);

                            if (rc != 0)
                            {
                                break;
                            }
                        }
                    }

                    if (rc == 0)
                    {
                        /* catch this scenario: v1="1.0.1" v2="1.0" */
                        if (v1_parts.Length > v2_parts.Length)
                        {
                            /* v1 is higher version than v2 */
                            rc = 1;
                        }
                        /* catch this scenario: v1="1.0" v2="1.0.1" */
                        else if (v2_parts.Length > v1_parts.Length)
                        {
                            /* v1 is lower version than v2 */
                            rc = -1;
                        }
                    }

                    if (rc == -1000)
                    {
                        throw new AggregateException();
                    }
                    else if (rc == 0)
                    {
                        return rc;
                    }
                    else
                    {
                        return rc < 0 ? -1 : 1;
                    }
                }
            }
        }
    }
}
