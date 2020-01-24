using System;
using System.Threading.Tasks;

using PaatyDSM;

using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using static MisHorarios.MainPage;

namespace MisHorarios
{
    public sealed partial class ReleaseNotesPage : Page
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage
        private readonly MainPage rootPage = Current;

        public ReleaseNotesPage()
        {
            InitializeComponent();
        }

        // Function start_fade-in_animation
        private void Start_FadeInAnimation(object sender, object e)
        {
            ReleaseNotes_FadeInAnimation.Begin();
        }

        // Function start_fade-out_animation
        private void Start_FadeOutAnimation(object sender, object e)
        {
            ReleaseNotes_FadeOutAnimation.Begin();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set Back Button on Desktop devices
            SetBackButton();

            // Se invoca cuando se presionan los botones de retroceso de hardware o software.
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

            // Restore Page_Frame Opacity
            rootPage.RestorePage_FrameOpacity();
        }

        // Se invoca cuando se presionan los botones de retroceso de hardware o software.
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Backbutton1(sender, null);
        }

        // Navigation: Back Button
        private void Backbutton1(object sender, object e)
        {
            Frame.Navigate(typeof(WelcomePage));
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

        private async void LoadReleaseNotes(object sender, object e)
        {
            Notes.Text = "";

            // Run in separate thread to not block the GUI while releaseNotes text is loading.
            await LoadReleaseNotesAsync().ConfigureAwait(false);

            //Notes.VerticalAlignment = ...;
        }

        private async Task LoadReleaseNotesAsync()
        {
            await Task.Run(() =>
            {
                // Load ReleaseNotes file
                LoadResource.ReadReleaseNotes();

                // Stop ProgressRing
                //loading_ring.IsActive = false;
            }).ConfigureAwait(false);
        }
    }
}