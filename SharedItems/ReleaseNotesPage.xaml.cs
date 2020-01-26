using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using PaatyDSM;

using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using static MisHorarios.MainPage;

namespace MisHorarios
{
    public sealed partial class ReleaseNotesPage : Page
    {
        public ReleaseNotesPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set Back Button on Desktop devices
            SetBackButton();

            // Se invoca cuando se presionan los botones de retroceso de hardware o software.
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }

        private void OnPageLoaded(object sender, object e)
        {
            main_Grid.Opacity = 1;
            footerPanel.Visibility = Visibility.Collapsed;
            ReleaseNotes_FadeIn.Begin();
        }

        // Set Back Button on Desktop devices
        private void SetBackButton()
        {
            string platformFamily = AnalyticsInfo.VersionInfo.DeviceFamily;

            if (platformFamily.Equals("Windows.Mobile"))
            {
                BackButtonPC.Opacity = 0;
            }
        }

        // Se invoca cuando se presionan los botones de retroceso de hardware o software.
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            ReleaseNotes_to_MainPage_FadeOut.Begin();
        }

        // Navigation: Back Button
        private void BackButton(object sender, object e)
        {
            ReleaseNotes_to_MainPage_FadeOut.Begin();
        }

        // Navigation: Back Button
        private void BackToMainPage(object sender, object e)
        {
            MainPage.frame.Navigate(typeof(WelcomePage));
        }

        private async void LoadReleaseNotes(object sender, object e)
        {
            // Run in separate thread to not block the GUI while releaseNotes text is loading.
            await LoadReleaseNotesAsync().ConfigureAwait(false);

            //Notes.VerticalAlignment = ...;
        }

        private async Task LoadReleaseNotesAsync()
        {
            await Task.Run(() =>
            {
                // Load ReleaseNotes file
                Notes.Text = LoadResource.ReadReleaseNotes();

                // Stop ProgressRing
                //loading_ring.IsActive = false;

            }).ConfigureAwait(false);
        }
    }
}