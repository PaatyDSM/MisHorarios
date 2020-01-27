using System;
using System.IO;
using System.Reflection;

using Windows.ApplicationModel;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace PaatyDSM
{
    /// <summary>
    /// Load embedded resources.
    /// </summary>
    public static class LoadResource
    {
        /// <summary>
        /// Load the ReleaseNotes file.
        /// </summary>
        public static string ReadReleaseNotes()
        {
            try
            {
                var assembly = typeof(LoadResource).GetTypeInfo().Assembly;
                Stream stream = assembly.GetManifestResourceStream("MisHorarios.ReleaseNotes.txt");

                string text = "";
                using (var reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                stream.Dispose();
                return text;
            }
            catch
            {
                return "";
            }
        }
    }

    /// <summary>
    /// PaatyDSM Utilities
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Returns current app version like v1.5.
        /// </summary>
        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("v{0}.{1}", version.Major, version.Minor);
        }

        /// <summary>
        /// Returns current project name.
        /// </summary>
        public static string GetCurrentProjectName()
        {
            Package package = Package.Current;
            return package.DisplayName;
        }

        /// <summary>
        /// Launch UWP apps in full-screen mode.
        /// </summary>
        /// <param name="device">0 for Mobile and Tablets, 1 for PC and 2 for both platforms</param>
        public static void SetFullScreenModeON(int device)
        {
            string platformFamily = AnalyticsInfo.VersionInfo.DeviceFamily;

            if (device == 0)
            {
                if (platformFamily == "Windows.Mobile")
                {
                    ApplicationView view = ApplicationView.GetForCurrentView();
                    view.TryEnterFullScreenMode();
                }
            }
            else if (device == 1)
            {
                if (platformFamily == "Windows.Desktop")
                {
                    ApplicationView view = ApplicationView.GetForCurrentView();
                    view.TryEnterFullScreenMode();
                }
            }
            else if (device == 2)
            {
                ApplicationView view = ApplicationView.GetForCurrentView();
                view.TryEnterFullScreenMode();
            }
        }

        public static string TryReadFile(string folder, string filename)
        {
            Stream stream = null;
            try
            {
                stream = new FileStream(folder + "\\" + filename, FileMode.Open);
                using (StreamReader reader = new StreamReader(stream))
                {
                    string text = "";
                    while (!reader.EndOfStream)
                    {
                        text = reader.ReadLine();
                    }
                    reader.Dispose();
                    return text;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                return "";
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                stream?.Dispose();
            }
        }

        public static void TryWriteFile(string folder, string filename, string data)
        {
            Stream stream = null;
            try
            {
                stream = new FileStream(folder + "\\" + filename, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    if (data != null)
                        writer.WriteLine(data);
                    else writer.WriteLine("");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                throw new Exception("Can't access/write the file. Details:" + e);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                stream?.Dispose();
            }
        }

        /// <summary>
        /// PaatyDSM FooterPanel Actions
        /// </summary>
        public static class FooterPanelActions
        {
            /// <summary>
            /// Hyper-link Action.
            /// </summary>
            public static async void Footer_Cli2ck(object sender, object e)
            {
                //123123123
                await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
            }
        }
    }
}