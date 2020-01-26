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
    /// Load a text file embedded resource.
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
                return text;
            }
            catch
            {
                return "";
            }
        }
    }

    public static class Utils
    {
        /// <summary>
        /// Returns version like v1.5
        /// </summary>
        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("v{0}.{1}", version.Major, version.Minor);
        }

        /// <summary>
        /// Returns current project name
        /// </summary>
        public static string GetCurrentProjectName()
        {
            Package package = Package.Current;
            return package.DisplayName;
        }

        /// <summary>
        /// Launch UWP apps in full-screen mode on mobile devices and tablets, desktop or both.
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
    }

    public static class FooterPanelActions
    {
        // On click 'Hyper-links'
        public static async void Footer_Click(object sender, object e)
        {
            //123123123
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }
    }
}
