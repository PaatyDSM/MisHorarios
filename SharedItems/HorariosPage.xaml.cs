﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using HttpClientSample;

using JSonDbUtilities;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using static MisHorarios.MainPage;

namespace MisHorarios
{
    public sealed partial class HorariosPage : Page
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        private readonly MainPage rootPage = Current;

        // We are now creating a HttpClient in the constructor and then storing it as a field so that we can reuse it.
        private HttpBaseProtocolFilter filter;
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        //
        private string OutputText = string.Empty;

        // Function start_fade-in_animation
        private void Start_FadeInAnimation()
        {
            HorariosPage_FadeInAnimation.Begin();
        }

        // OnNavigatedTo function
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Un-hide Back Button on Desktop devices.
            SetBackButton();

            // Se invoca cuando se presionan los botones de retroceso de hardware o software.
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

            // Create an HttpClient instance with custom cache settings and a CancellationToken.
            filter = new HttpBaseProtocolFilter();
            httpClient = new HttpClient(filter);
            cts = new CancellationTokenSource();

            // Create URL with 'legajo' string.
            string URL = "http://proveedores.alsea.com.ar:25080/asignaciones-server/mobile/main/asignaciones/legajos/" + e.Parameter.ToString();

            // Save last used legajo.
            Save_last_legajo(e.Parameter.ToString());

            // Start Connection Async.
            await StartConnectionAsync(URL, e.Parameter.ToString(), 0).ConfigureAwait(false);
        }

        // Start Connection Async
        private async Task StartConnectionAsync(string URL, string legajo, int retry)
        {
            // Show Message.
            rootPage.NotifyUser("Consultando horarios...", NotifyType.StatusMessage);

            // Do an asynchronous GET.
            try
            {
                // Create a new httpClient.
                //httpClient?.Dispose();
                // Cache control.
                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
                filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                // Ignore SSL errors.
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
                // Default headers will be sent with every request sent from this HttpClient instance.
                httpClient.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.19035", "18.19035"));

                // Connect to server and get text.
                HttpResponseMessage response = await httpClient.GetAsync(new Uri(URL, UriKind.Absolute)).AsTask(cts.Token).ConfigureAwait(false);

                string responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token).ConfigureAwait(false);

                // Save received text to a variable
                OutputText = responseBodyAsText.Replace("<br>", Environment.NewLine);

                // Find ':[{' string to check if the data contains a valid legajo info
                if (OutputText.Contains(":[{"))
                {
                    try
                    {
                        // Parse JSON
                        DataContext = new User(OutputText);

                        // Show successful
                        rootPage.NotifyUser("Horarios leídos!", NotifyType.StatusMessage);

                        // Save cache
                        Save_cache(OutputText, legajo);

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

            OutputText = "{\"asignaciones\":[],\"fechaConsulta\":\"\",\"legajo\":\"\"}";
            DataContext = new User(OutputText);

            // Go to page
            Frame.Navigate(typeof(WelcomePage));
        }

        // Navigation: Back Button
        private void BackButton1(object sender, object e)
        {
            // Cancel current HTTP connection
            CancelHttpTask();

            // Clear List of Horarios
            OutputText = "{\"asignaciones\":[],\"fechaConsulta\":\"\",\"legajo\":\"\"}";

            DataContext = new User(OutputText);

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
            OutputText = "{\"asignaciones\":[],\"fechaConsulta\":\"\",\"legajo\":\"\"}";

            DataContext = new User(OutputText);

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

        // Read from cache
        private void Read_cache(string legajo, int database_error)
        {
            // Read file
            string data = "";

            try
            {
                var reader = new StreamReader(File.OpenRead(localfolder + "\\hoariosLast" + legajo + ".tmp"));
                while (!reader.EndOfStream)
                {
                    // Put legajo into TextBox
                    data = reader.ReadLine();
                }
                reader.Dispose();
            }
            catch { }

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