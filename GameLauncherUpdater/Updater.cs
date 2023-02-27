using GameLauncherUpdater.App.Classes.SystemPlatform.UnixOS;
using GameLauncherUpdater.App.Classes.UpdaterCore.Json;
using GameLauncherUpdater.App.Classes.UpdaterCore.Support;
using GameLauncherUpdater.App.Classes.UpdaterCore.Time;
using GameLauncherUpdater.App.Classes.UpdaterCore.Validator.JSON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace GameLauncherUpdater
{
    public partial class Updater : Form
    {
        private static string GitHub_Launcher_Developer { get; set; } = "https://api.github.com/repos/DavidCarbon-SBRW/SBRW.Launcher.Releases/releases/latest";
        private static string GitHub_Launcher_Stable { get; set; } = "https://api.github.com/repos/SoapboxRaceWorld/GameLauncher_NFSW/releases/latest";
        private static string GitHub_Launcher_Beta { get; set; } = "https://api.github.com/repos/SoapboxRaceWorld/GameLauncher_NFSW/releases";
        private static string LauncherFolder { get { return Strings.Encode(AppDomain.CurrentDomain.BaseDirectory); } }
        private static string LauncherUpdaterFolder { get { return Strings.Encode(Path.Combine(LauncherFolder, "Updater")); } }
        private static string SET_TempLauncherNameZip { get; set; }
        private static string TempLauncherNameZip
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SET_TempLauncherNameZip))
                {
                    SET_TempLauncherNameZip = !UnixOS.Detected() ? Strings.Encode(Path.GetTempFileName()) : Path.Combine(LauncherUpdaterFolder, "Launcher_Update.zip");
                }

                return SET_TempLauncherNameZip;
            }
        }
        private static bool UsingDevelopment { get; set; }
        private static bool UsingPreview { get; set; }
        private static string Version { get; set; }
        private static string Version_Build { get; set; }

        public Updater()
        {
            InitializeComponent();
            VersionLabel.Text = "v: " + Application.ProductVersion;
            Shown += (x, y) =>
            {
                DoUpdate();
            };
        }

        public void DisplayError(string Message, int Timer, bool Exit_Application = true)
        {
            Information.Text = Message.ToString();
            Time.WaitSeconds(Timer);

            if (Exit_Application)
            {
                Application.Exit();
            }
        }

        private static GitHubReleaseSchema Insider_Release_Tag(string JSON_Data, string Current_Launcher_Build)
        {
            GitHubReleaseSchema Temp_Latest_Launcher_Build = new GitHubReleaseSchema();

            if (IsJSONValid.ValidJson(JSON_Data) && !string.IsNullOrWhiteSpace(Current_Launcher_Build))
            {
                int Top_Ten = 0;
                bool Latest_Found_Build = false;

                List<GitHubReleaseSchema> Scrollable_List = new List<GitHubReleaseSchema>();
                Scrollable_List.AddRange(new JavaScriptSerializer().Deserialize<List<GitHubReleaseSchema>>(JSON_Data));

                if (Scrollable_List.Count > 0)
                {
                    foreach (GitHubReleaseSchema GH_Releases in Scrollable_List)
                    {
                        if (!string.IsNullOrWhiteSpace(GH_Releases.tag_name))
                        {
                            if (!GH_Releases.prerelease && !Latest_Found_Build)
                            {
                                Latest_Found_Build = true;
                                Temp_Latest_Launcher_Build = GH_Releases;
                            }

                            if (Current_Launcher_Build.Comparisons(GH_Releases.tag_name) < 0)
                            {
                                return GH_Releases;
                            }
                            else if (Top_Ten >= 10)
                            {
                                break;
                            }
                            else
                            {
                                Top_Ten++;
                            }
                        }
                    }
                }
            }

            return Temp_Latest_Launcher_Build;
        }

        public void DoUpdate()
        {
            string[] args = Environment.GetCommandLineArgs();
            bool Disable_Popup = false;

            if (args.Length >= 2)
            {
                int Process_Live_ID = 0;
                bool Launcher_Process_ID_Terminated = false;

                try
                {
                    if (int.TryParse(args[1], out int Converted_Process_ID))
                    {
                        Process_Live_ID = Converted_Process_ID;

                        if (Process_Live_ID > 0)
                        {
                            Process Launcher_Process_ID = Process.GetProcessById(Process_Live_ID);
                            if (!Launcher_Process_ID.HasExited)
                            {
                                Launcher_Process_ID_Terminated = Launcher_Process_ID.CloseMainWindow();

                                if (!Launcher_Process_ID_Terminated)
                                {
                                    Launcher_Process_ID.Kill();
                                }
                            }

                            Disable_Popup = true;
                        }
                        else if (Process_Live_ID == -1)
                        {
                            Disable_Popup = true;
                        }
                        else if (Process_Live_ID == -2)
                        {
                            BranchStatus.Text = "Preview Branch";
                            Disable_Popup = UsingPreview = true;
                        }
                        else if (Process_Live_ID == -3)
                        {
                            BranchStatus.Text = "Developer Branch";
                            Disable_Popup = UsingDevelopment = true;
                        }
                        else if (MessageBox.Show(null, "Which Launcher Build Would you Opt Into?" + 
                            "\nClick Yes to Download the Stable Build" +
                            "\nClick No to Download the Beta Build", "GameLauncherUpdater", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            BranchStatus.Text = "Preview Branch";
                            Disable_Popup = UsingPreview = true;
                        }
                    }
                }
                catch
                {
                    try
                    {
                        if (!Launcher_Process_ID_Terminated)
                        {
                            Process.GetProcessById(Convert.ToInt32(Process_Live_ID)).Kill();
                        }
                    }
                    catch 
                    { 

                    }

                    Disable_Popup = true;
                }
            }

            if (args.Length >= 3)
            {
                if (args[2].ToString() == "Preview")
                {
                    BranchStatus.Text = "Preview Branch";
                    Disable_Popup = UsingPreview = true;
                }
                else if (args[2].ToString() == "Developer")
                {
                    BranchStatus.Text = "Developer Branch";
                    Disable_Popup = UsingDevelopment = true;
                }
                else if (args[2].ToString() == "Stable")
                {
                    Disable_Popup = true;
                }
            }

            if (args.Length >= 4)
            {
                Version_Build = args[3].ToString();
            }

            if (!Disable_Popup)
            {
                if (MessageBox.Show(null, "Which Launcher Build Would you Opt Into?" +
                            "\n\nClick Yes to Download the Stable Build" +
                            "\n\nClick No to Download the Beta Build", "GameLauncherUpdater", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    BranchStatus.Text = "Preview Branch";
                    UsingPreview = true;
                }
            }

            if (File.Exists("SBRW.Launcher.exe"))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo("SBRW.Launcher.exe");
                Version = versionInfo.ProductVersion;
            }
            else if (File.Exists("GameLauncher.exe"))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo("GameLauncher.exe");
                Version = versionInfo.ProductVersion;
            }
            else
            {
                Version = "0.0.0.0";
            }

            try
            {
                WebClient client = new WebClient();
                Uri StringToUri = new Uri(UsingDevelopment ? GitHub_Launcher_Developer : UsingPreview ? GitHub_Launcher_Beta : GitHub_Launcher_Stable);
                client.Headers.Add("user-agent", "GameLauncherUpdater " + Application.ProductVersion +
                    " (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");
                client.DownloadStringCompleted += (sender3, e3) =>
                {
                    string JSONFile = e3.Result;

                    if (IsJSONValid.ValidJson(JSONFile))
                    {
                        try
                        {
                            if (UnixOS.Detected() && !Directory.Exists(LauncherUpdaterFolder))
                            {
                                Directory.CreateDirectory(LauncherUpdaterFolder);
                            }

                            if (UnixOS.Detected() && !File.Exists(TempLauncherNameZip))
                            {
                                File.Create(TempLauncherNameZip).Close();
                            }

                            GitHubReleaseSchema LatestLauncherBuild = (UsingPreview) ?
                            Insider_Release_Tag(JSONFile, Version) :
                            new JavaScriptSerializer().Deserialize<GitHubReleaseSchema>(JSONFile);
                            int Revision = UsingDevelopment ? 
                            Version_Build.CompareTo(LatestLauncherBuild.tag_name) : Version.Comparisons(LatestLauncherBuild.tag_name);
                            if (Revision < 0)
                            {
                                WebClient client2 = new WebClient();
                                client2.Headers.Add("user-agent", "GameLauncherUpdater " + Application.ProductVersion +
                                    " (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");
                                client2.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Client_DownloadProgressChanged);
                                client2.DownloadDataCompleted += new DownloadDataCompletedEventHandler(Launcher_Client_DownloadFileCompleted);
                                string Url_Download_Path = (UsingDevelopment ? "http://github.com/DavidCarbon-SBRW/SBRW.Launcher.Releases/releases/download/" :
                                    "http://github.com/SoapboxRaceWorld/GameLauncher_NFSW/releases/download/") +
                                    LatestLauncherBuild.tag_name + "/" + (UnixOS.Detected() ? "Unix." : "") + (UsingDevelopment ? "" : "Release_") +
                                    LatestLauncherBuild.tag_name + ".zip";
                                client2.DownloadDataAsync(new Uri(Url_Download_Path));
                            }
                            else
                            {
                                if (File.Exists("SBRW.Launcher.exe"))
                                {
                                    Process.Start(@"SBRW.Launcher.exe");
                                }
                                else if (File.Exists("GameLauncher.exe"))
                                {
                                    Process.Start(@"GameLauncher.exe");
                                }

                                DisplayError("Up To Date. Starting SBRW Launcher", 2);
                            }
                        }
                        catch (Exception Error)
                        {
                            DisplayError("Failed to Update.\n" + Error.Message, 10);
                        }
                    }
                    else
                    {
                        DisplayError("Failed to Update.\nRetrived Invalid JSON", 10);
                    }
                };
                client.DownloadStringAsync(StringToUri);
            }
            catch (Exception Error)
            {
                DisplayError(Error.Message, 10);
            }
        }

        private string FormatFileSize(long byteCount)
        {
            double[] numArray = new double[] { 1073741824, 1048576, 1024, 0 };
            string[] strArrays = new string[] { "GB", "MB", "KB", "Bytes" };
            for (int i = 0; i < (int)numArray.Length; i++)
            {
                if ((double)byteCount >= numArray[i])
                {
                    return string.Concat(string.Format("{0:0.00}", (double)byteCount / numArray[i]), strArrays[i]);
                }
            }

            return "0 Bytes";
        }

        void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            Information.Text = "Downloaded " + FormatFileSize(e.BytesReceived) + " of " + FormatFileSize(e.TotalBytesToReceive);
            DownloadProgress.Style = ProgressBarStyle.Blocks;
            DownloadProgress.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        void Launcher_Client_DownloadFileCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                DisplayError(e.Error.Message, 10);
            }
            else
            {
                try
                {
                    using (FileStream fileStream = new FileStream(TempLauncherNameZip, FileMode.Create))
                    {
                        fileStream.Write(e.Result, 0, e.Result.Length);
                    }

                    if (File.Exists(TempLauncherNameZip))
                    {
                        if (new FileInfo(TempLauncherNameZip).Length > 0)
                        {
                            DownloadProgress.Style = ProgressBarStyle.Marquee;

                            using (ZipArchive archive = ZipFile.OpenRead(TempLauncherNameZip))
                            {
                                int numFiles = archive.Entries.Count;
                                int current = 1;

                                DownloadProgress.Style = ProgressBarStyle.Blocks;

                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    string fullName = entry.FullName;

                                    if (fullName.Substring(fullName.Length - 1) == "/")
                                    {
                                        string folderName = fullName.Remove(fullName.Length - 1);
                                        if (Directory.Exists(folderName))
                                        {
                                            Directory.Delete(folderName, true);
                                        }

                                        Directory.CreateDirectory(folderName);
                                    }
                                    else
                                    {
                                        if (fullName != "GameLauncherUpdater.exe")
                                        {
                                            if (File.Exists(fullName))
                                            {
                                                File.Delete(fullName);
                                            }

                                            Information.Text = "Extracting: " + fullName;
                                            try
                                            {
                                                entry.ExtractToFile(Path.Combine(LauncherFolder, fullName));
                                            }
                                            catch { }
                                            Time.WaitMSeconds(200);
                                        }
                                    }

                                    DownloadProgress.Value = (int)((long)100 * current / numFiles);
                                    current++;
                                }
                            }

                            try
                            {
                                if (File.Exists(TempLauncherNameZip))
                                {
                                    File.Delete(TempLauncherNameZip);
                                }
                            }
                            catch { }

                            if (File.Exists("SBRW.Launcher.exe"))
                            {
                                Process.Start(@"SBRW.Launcher.exe");
                            }
                            else if (File.Exists("GameLauncher.exe"))
                            {
                                Process.Start(@"GameLauncher.exe");
                            }

                            DisplayError("Sucessfully Updated. Starting SBRW Launcher", 2);
                        }
                        else
                        {
                            DownloadProgress.Style = ProgressBarStyle.Marquee;
                            DisplayError("Corrupt Update File. Closing Updater", 10);
                        }
                    }
                    else
                    {
                        DownloadProgress.Style = ProgressBarStyle.Marquee;
                        DisplayError("Update File Does not Exist. Closing Updater", 10);
                    }
                }
                catch (Exception Error)
                {
                    DisplayError(Error.Message, 10);
                }
            }
        }
    }
}
