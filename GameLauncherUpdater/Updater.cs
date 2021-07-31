using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;
using System.Threading;
using System.Web.Script.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using GameLauncherUpdater.App.Classes.UpdaterCore.Json;
using GameLauncherUpdater.App.Classes.UpdaterCore.Time;
using GameLauncherUpdater.App.Classes.SystemPlatform.Wine;
using GameLauncherUpdater.App.Classes.UpdaterCore.Validator.JSON;
using GameLauncherUpdater.App.Classes.UpdaterCore.Support;

namespace GameLauncherUpdater
{
    public partial class Updater : Form
    {
        public static string LauncherFolder = Strings.Encode(AppDomain.CurrentDomain.BaseDirectory);
        public static string LauncherUpdaterFolder = Strings.Encode(Path.Combine(LauncherFolder, "Updater"));
        public static string TempNameZip = (!Wine.Detected()) ? Strings.Encode(Path.GetTempFileName()) : Path.Combine(LauncherUpdaterFolder, "Update.zip");
        public static string Version;

        public Updater()
        {
            InitializeComponent();
        }

        public void DisplayError(string Message, int Timer)
        {
            Information.Text = Message.ToString();
            Time.WaitSeconds(Timer);
            Application.Exit();
        }

        public void success(string success)
        {
            Information.Text = success.ToString();
        }

        public void DoUpdate()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length == 2)
            {
                Process.GetProcessById(Convert.ToInt32(args[1])).Kill();
            }

            if (File.Exists("GameLauncher.exe"))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo("GameLauncher.exe");
                Version = versionInfo.ProductVersion;
            }
            else
            {
                Version = "0.0.0.0";
            }

            ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
            ServicePointManager.Expect100Continue = true;
            try { /* TLS 1.3 */ ServicePointManager.SecurityProtocol |= (SecurityProtocolType)12288 | SecurityProtocolType.Tls12 | 
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; }
            catch (NotSupportedException)
            {
                try { /* TLS 1.2 */ ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | 
                        SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; }
                catch (NotSupportedException)
                {
                    try { /* TLS 1.1 */ ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; }
                    catch (NotSupportedException)
                    {
                        try { /* TLS 1.0 */ ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls; }
                        catch { }
                    }
                }
            }
            ServicePointManager.ServerCertificateValidationCallback = 
                (Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                bool isOk = true;
                if (sslPolicyErrors != SslPolicyErrors.None)
                {
                    for (int i = 0; i < chain.ChainStatus.Length; i++)
                    {
                        if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                        {
                            continue;
                        }
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 15);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                            break;
                        }
                    }
                }
                return isOk;
            };

            try
            {
                WebClient client = new WebClient();
                Uri StringToUri = new Uri("https://api.github.com/repos/SoapboxRaceWorld/GameLauncher_NFSW/releases/latest");
                client.Headers.Add("user-agent", "GameLauncherUpdater " + Application.ProductVersion +
                    " (+https://github.com/SoapBoxRaceWorld/GameLauncher_NFSW)");
                client.CancelAsync();
                client.DownloadStringAsync(StringToUri);
                client.DownloadStringCompleted += (sender3, e3) =>
                {
                    string JSONFile = e3.Result;

                    if (IsJSONValid.ValidJson(JSONFile))
                    {
                        try
                        {
                            if (Wine.Detected() && !Directory.Exists(LauncherUpdaterFolder))
                            {
                                Directory.CreateDirectory(LauncherUpdaterFolder);
                            }

                            GitHubReleaseSchema json = new JavaScriptSerializer().Deserialize<GitHubReleaseSchema>(JSONFile);

                            if (Version != json.tag_name)
                            {
                                Thread thread = new Thread(() =>
                                {
                                    WebClient client2 = new WebClient();
                                    client2.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Client_DownloadProgressChanged);
                                    client2.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);
                                    client2.DownloadFileAsync(new Uri("http://github.com/SoapboxRaceWorld/GameLauncher_NFSW/releases/download/" +
                                        json.tag_name + "/Release_" + json.tag_name + ".zip"), TempNameZip);
                                });
                                thread.Start();
                            }
                            else
                            {
                                Process.Start(@"GameLauncher.exe");
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
            this.BeginInvoke((MethodInvoker)delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                Information.Text = "Downloaded " + FormatFileSize(e.BytesReceived) + " of " + FormatFileSize(e.TotalBytesToReceive);
                DownloadProgress.Style = ProgressBarStyle.Blocks;
                DownloadProgress.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }

        void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                DownloadProgress.Style = ProgressBarStyle.Marquee;

                using (ZipArchive archive = ZipFile.OpenRead(TempNameZip))
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
                            if (fullName != "GameLauncherUpdater.exe" || (Wine.Detected() 
                            && (fullName != "PresentationCore.dll" || fullName != "PresentationFramework.dll")))
                            {
                                if (File.Exists(fullName))
                                {
                                    File.Delete(fullName);
                                }

                                Information.Text = "Extracting: " + fullName;
                                try { entry.ExtractToFile(Path.Combine(LauncherFolder, fullName)); } catch { }
                                Time.WaitMSeconds(200);
                            }
                        }

                        DownloadProgress.Value = (int)((long)100 * current / numFiles);
                        current++;
                    }
                }

                try
                {
                    if (File.Exists(TempNameZip))
                    {
                        File.Delete(TempNameZip);
                    }
                }
                catch { }

                Process.Start(@"GameLauncher.exe");
                DisplayError("Sucessfully Updated. Starting SBRW Launcher", 2);
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                DoUpdate();
            });
        }
    }
}
