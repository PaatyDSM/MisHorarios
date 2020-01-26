using System;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MisHorarios
{
    public sealed partial class MainPage : Page
    {
        // Referencia a MainPage
        public static MainPage Current;
        // Referencia al Frame en el cual todas las páginas son cargadas.
        public static Frame frame;
        // Referencia al Grid principal (para controlar efectos de animación).
        public static Grid main_Grid;
        // Referencia al FooterPanelV4.
        public static Grid footerPanel;

        // Path for local saving
        public static readonly string localfolder = ApplicationData.Current.LocalFolder.Path;

        // MainPage
        public MainPage()
        {
            InitializeComponent();

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            main_Grid = Main_Grid;
            frame = Page_Frame;
            footerPanel = FooterPanelV4;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set full-screen on mobile devices and tablets.
            SetFullScreenModeON(0);

            // Sets default windows size.
            ApplicationView view = ApplicationView.GetForCurrentView();
            view.TryResizeView(new Size(360, 576));
            view.SetPreferredMinSize(new Size(280, 420));

            // Set version number
            FP_VersionButton.Content = GetAppVersion();

            // Navigate to the WelcomePage
            Page_Frame.Navigate(typeof(WelcomePage), null);

        }

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
        private void SetFullScreenModeON(int device)
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

        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;

                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;

                case NotifyType.DebugMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Yellow);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            if (!string.IsNullOrEmpty(StatusBlock.Text))
            {
                StatusBorder.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Enumerates the different types of messages
        /// </summary>
        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage,
            DebugMessage
        };

        // On click 'Hyper-links'
        private async void Footer_Click(object sender, object e)
        {
            //123123123
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

        // Start _MainPage_to_ReleaseNotes_FadeOut animation.
        private void Click_MainPage_to_ReleaseNotes_FadeOut(object sender, RoutedEventArgs e)
        {
            MainPage_to_ReleaseNotes_FadeOut.Begin();
        }

        // When Click_MainPage_to_ReleaseNotes_FadeOut animation is completed, navigate to 'Release Notes' Page.
        private void MainPage_to_ReleaseNotes_FadeOut_Completed(object sender, object e)
        {
            // Clean error messages from previous page
            NotifyUser("", NotifyType.StatusMessage);

            Page_Frame.Navigate(typeof(ReleaseNotesPage), null);
        }

    }
}