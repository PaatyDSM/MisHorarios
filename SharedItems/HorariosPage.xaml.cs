using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using HttpClientSample;

using JSonDbUtilities;

using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

using static MisHorarios.MainPage;

namespace MisHorarios
{
    public sealed partial class HorariosPage : Page
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        private readonly MainPage rootPage = Current;

        // Path for local saving
        public readonly string localfolder = ApplicationData.Current.LocalFolder.Path;

        // We are now creating a HttpClient in the constructor and then storing it as a field so that we can reuse it.
        private HttpClient httpClient;

        private HttpBaseProtocolFilter filter;
        private CancellationTokenSource cts;

        // Function start_fade-in_animation
        private void Start_FadeInAnimation()
        {
            HorariosPage_FadeInAnimation.Begin();
        }

        // OnNavigatedTo function
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set Back Button on Desktop devices
            SetBackButton();

            // Se invoca cuando se presionan los botones de retroceso de hardware o software.
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

            // CacheControl
            filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

            // Initialize HTTP client
            httpClient = new HttpClient(filter);
            cts = new CancellationTokenSource();

            // Create URL with 'legajo' string
            string url = "http://proveedores.alsea.com.ar:25080/asignaciones-server/mobile/main/asignaciones/legajos/" + e.Parameter.ToString();

            // Save last used legajo
            Save_last_legajo(e.Parameter.ToString());

            // Start Connection async
            await Task.Factory.StartNew(async () => await StartConnectionAsync(url, e.Parameter.ToString(), 0).ConfigureAwait(false), CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, TaskScheduler.Default).ConfigureAwait(false);
        }

        // Se invoca cuando se presionan los botones de retroceso de hardware o software.
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Cancel current HTTP connection
            CancelHttpTask();

            e.Handled = true;
            BackButton(null, null);
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

        // Save last used legajo
        private void Save_last_legajo(string legajo)
        {
            // Save last used legajo to a file
            var writer = new StreamWriter(File.OpenWrite(localfolder + "\\legajoLast.tmp"));
            writer.WriteLine(legajo);
            writer.Dispose();
        }

        public HorariosPage()
        {
            InitializeComponent();
        }

        // Function start_fadeout_animation
        private void Start_FadeOutAnimation()
        {
            HorariosPage_FadeOutAnimation.Begin();
        }

        // Function start_fadeout_animation2
        private void Start_FadeOutAnimation2()
        {
            HorariosPage_FadeOutAnimation2.Begin();
        }

        // On click 'Hyper-links'
        private async void Footer_Click(object sender, EventArgs e)
        {
            //123123123
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

        // Go back to MainPage with uncleared errors
        private void GoPageBack(object sender, object e)
        {
            // Cancel current HTTP connection
            CancelHttpTask();

            HiddenOutputField.Text = "{\"asignaciones\":[],\"fechaConsulta\":\"\",\"legajo\":\"\"}";
            DataContext = new User(HiddenOutputField.Text);

            // Go to page
            Frame.Navigate(typeof(WelcomePage));
        }

        // Navigation: Back Button
        private void BackButton1(object sender, object e)
        {
            // Cancel current HTTP connection
            CancelHttpTask();

            // Clear List of Horarios
            HiddenOutputField.Text = "{\"asignaciones\":[],\"fechaConsulta\":\"\",\"legajo\":\"\"}";

            DataContext = new User(HiddenOutputField.Text);

            // Clear StatusBlock
            rootPage.NotifyUser("", NotifyType.StatusMessage);

            // Go to page
            Frame.Navigate(typeof(WelcomePage));
        }

        private void BackButton(object sender, RoutedEventArgs e)
        {
            // Cancel current HTTP connection
            CancelHttpTask();

            // Clear List of Horarios
            HiddenOutputField.Text = "{\"asignaciones\":[],\"fechaConsulta\":\"\",\"legajo\":\"\"}";

            DataContext = new User(HiddenOutputField.Text);

            // Clear StatusBlock
            rootPage.NotifyUser("", NotifyType.StatusMessage);

            // Go to page
            Frame.Navigate(typeof(WelcomePage));
        }

        // Save cache to a file
        private void Save_cache(string cache, string legajo)
        {
            // Save last readed horarios from last used legajo to a file
            var writer = new StreamWriter(File.OpenWrite(localfolder + "\\horariosLast.tmp"));
            writer.WriteLine(cache);
            writer.Dispose();

            // Cache horarios
            var writer2 = new StreamWriter(File.OpenWrite(localfolder + "\\horariosLast" + legajo + ".tmp"));
            writer.WriteLine(cache);
            writer.Dispose();
        }

        // Start Connection Async
        private async Task StartConnectionAsync(string URL, string legajo, int retry)
        {
            // Show Message
            rootPage.NotifyUser("Consultando horarios...", NotifyType.StatusMessage);

            // Do an asynchronous GET.
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(new Uri(URL, UriKind.Absolute)).AsTask().ConfigureAwait(false);

                await Helpers.DisplayTextResultAsync(response, HiddenOutputField, cts.Token).ConfigureAwait(false);

                // Find ':[{' string to check if the data contains a valid legajo info
                if (HiddenOutputField.Text.Contains(":[{"))
                {
                    try
                    {
                        // Parse JSON
                        DataContext = new User(HiddenOutputField.Text);

                        // Show successful
                        rootPage.NotifyUser("Horarios leídos!", NotifyType.StatusMessage);

                        // Save cache
                        Save_cache(HiddenOutputField.Text, legajo);

                        // Show ContentPanelInfo
                        ContentPanelInfo.Visibility = Visibility.Visible;

                        // Show list
                        List.Visibility = Visibility.Visible;
                    }
                    catch (Exception)
                    {
                        //123123123
                        // Database parsing error
                        rootPage.NotifyUser("Conexión a Internet muy débil.\nReintentando...", NotifyType.ErrorMessage);
                        if (retry == 0)
                        {
                            await StartConnectionAsync(URL, legajo, 1).ConfigureAwait(false);
                        }
                        else
                        {
                            rootPage.NotifyUser("Sin conexión a Internet.", NotifyType.ErrorMessage);

                            // Try to read from cache
                            Read_cache(legajo, 1);
                        }
                    }
                }
                else
                {
                    // Legajo NOT FOUND error
                    rootPage.NotifyUser("Legajo inexistente o sin horarios asignados.", NotifyType.ErrorMessage);
                    GoPageBack(null, null);
                }
            }
            catch (TaskCanceledException)
            {
                rootPage.NotifyUser("Conexión cancelada", NotifyType.DebugMessage);
            }
            catch (Exception)
            {
                // If no Internet connection is available, check if the last legajo obtained is equal to
                // the actual legajo and read it from the cache and show a message.

                rootPage.NotifyUser("Error. No hay conexión a Internet.", NotifyType.ErrorMessage);

                // Try to read from cache
                Read_cache(legajo, 0);
            }
            finally
            {
                // Stop ProgressRing
                loading_ring.IsActive = false;
            }
        }

        // Read from cache
        private void Read_cache(string legajo, int database_error)
        {
            // Read file
            string data = "";

            var reader = new StreamReader(File.OpenRead(localfolder + "\\hoariosLast" + legajo + ".tmp"));
            while (!reader.EndOfStream)
            {
                // Put legajo into TextBox
                data = reader.ReadLine();
            }

            // legajo item format
            string query = "\"legajo\":\"" + legajo + "\"}";

            // Check if legajo is valid and then parse JSon
            if (data.Contains(query))
            {
                // Parse JSon
                DataContext = new User(data);

                // Show ContentPanelInfo
                ContentPanelInfo.Visibility = Visibility.Visible;

                // Show ErrorMessage
                if (database_error == 1)
                {
                    rootPage.NotifyUser("Sin conexión.\nMostrando los últimos horarios descargados", NotifyType.ErrorMessage);
                }
                else
                {
                    rootPage.NotifyUser("Sin conexión.\nMostrando los últimos horarios descargados", NotifyType.ErrorMessage);
                }

                // Show list
                List.Visibility = Visibility.Visible;

                // Stop ProgressRing
                loading_ring.IsActive = false;
            }
            else
            {
                GoPageBack(null, null);
            }
        }

        private void CancelHttpTask()
        {
            cts.Cancel();
            cts.Dispose();

            // Re-create the CancellationTokenSource.
            cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            if (filter != null)
            {
                filter.Dispose();
                filter = null;
            }

            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }

            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
        }
    }
}