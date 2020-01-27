using System;
using System.Threading;

using PaatyDSM;

using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

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

        // Path for local saving.
        public static readonly string localfolder = ApplicationData.Current.LocalFolder.Path;

        // We are now creating a HttpClient in the constructor and then storing it as a field so that we can reuse it.
        public static HttpBaseProtocolFilter filter;
        public static HttpClient httpClient;
        public static CancellationTokenSource cts;

        // ALSEA URL Server.
        public static string serverURL = "http://proveedores.alsea.com.ar";

        // string var for saving response as text from.
        public static string responseBodyAsText = "";

        // MainPage
        public MainPage()
        {
            InitializeComponent();

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            // Set global vars specific to the current running project.
            //SampleTitle.Text = FEATURE_NAME;

            // This are static public properties that allows downstream pages to ge handle to the MainPage instance
            // in order to get the elements of the UI Xaml and set it's properties.
            main_Grid = Main_Grid;
            frame = Page_Frame;
            footerPanel = FooterPanelV4;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set full-screen on mobile devices and tablets.
            Utils.SetFullScreenModeON(0);

            // Sets default windows size.
            ApplicationView view = ApplicationView.GetForCurrentView();
            view.TryResizeView(new Size(360, 576));
            view.SetPreferredMinSize(new Size(280, 420));

            // Set version number
            FP_VersionButton.Content = Utils.GetAppVersion();
        }

        private void OnPageLoaded(object sender, object e)
        {
            // Navigate to the WelcomePage
            Page_Frame.Navigate(typeof(WelcomePage), null);
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

            // Raise an event if necessary to enable a screen reader to announce the status update.
            var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock);
            peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
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

        // Start MainPage_to_ReleaseNotes_FadeOut animation.
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

        // On click 'Hyper-links'
        private async void Footer_Click(object sender, object e)
        {
            //123123123
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }
    }
}