using System;
using System.Threading;
using System.Threading.Tasks;

using PaatyDSM;
using PaatyDSM.Json;

using Windows.ApplicationModel.Core;
using Windows.Security.Cryptography.Certificates;
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
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such as NotifyUser()
        private readonly MainPage rootPage = Current;

        public HorariosPage()
        {
            InitializeComponent();
        }

        // OnNavigatedTo function
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set title.
            SetTitle();

            HorariosPage_FadeIn.Begin();

            // Save legajo contained in NavigatinoEventArgs to a string.
            string legajo = e.Parameter.ToString();

            // Un-hide Back Button on Desktop devices.
            await SetBackButtonAsync();

            // Se invoca cuando se presionan los botones de retroceso de hardware o software.
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

            // Crear una nueva instancia del HttpClient con parámetros personalizados de caché y un token de cancelación.
            httpClient?.Dispose();
            filter = new HttpBaseProtocolFilter();
            httpClient = new HttpClient(filter);
            cts = new CancellationTokenSource();

            // Set app URL server.
            string URL = SetAppURLServer();

            // Guardar el último legajo utilizado.
            TrySaveCache(legajo, false);

            // Establecer la conexión al servidor y obtener los datos.
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StartConnectionAsync(URL + legajo, legajo, 0).ConfigureAwait(false));
        }

        public void SetTitle()
        {
            if (Utils.GetCurrentProjectName() == "Mis Horarios SBX")
            {
                APP_TITLE.Text = Utils.GetCurrentProjectName();
            }
            else
            {
                APP_TITLE.Text = Utils.GetCurrentProjectName();
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

        // Establece el Back Button e la plataforma de Escritorio.
        private async Task SetBackButtonAsync()
        {
            string platformFamily = AnalyticsInfo.VersionInfo.DeviceFamily;

            if (platformFamily.Equals("Windows.Mobile"))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => BackButtonPC.Opacity = 0);
            }
        }

        // Establece la dirección del servidor según la aplicación en ejecución.
        private string SetAppURLServer()
        {
            if (Utils.GetCurrentProjectName() == "Mis Horarios SBX")
            {
                return serverURL + ":25080/asignaciones-server/mobile/main/asignaciones/legajos/";
            }
            else
            {
                return serverURL + ":48080/asignaciones-server/mobile/main/asignaciones/legajos/";
            }
        }

        // Start Connection Async
        private async Task StartConnectionAsync(string URL, string legajo, int retry)
        {
            if (retry == 0)
            {
                // Mostrar mensaje de consulta.
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Consultando horarios...", NotifyType.StatusMessage));

                // Cache control.
                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
                filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                // Ignore SSL errors.
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
                // Default headers will be sent with every request sent from this HttpClient instance.
                httpClient.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.19035", "18.19035"));

                // Do an asynchronous GET in separate thread to not block the GUI while receiving the data from Internet.
                // and save it to a variable
                try
                {
                    // Obtener los datos del servidor y almacenarlos
                    HttpResponseMessage response = await httpClient.GetAsync(new Uri(URL, UriKind.Absolute)).AsTask(cts.Token).ConfigureAwait(false);
                    responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token).ConfigureAwait(false);

                    // Try parse JSON
                    TryParseJson(responseBodyAsText, false);

                    // Guardar caché
                    TrySaveCache(legajo, true);
                }
                catch (TaskCanceledException)
                {
                    // Mostrar mensaje de conexión cancelada.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Conexión cancelada", NotifyType.DebugMessage));

                    // Detener animación de carga.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressRing_Animation3.IsActive = false);

                    // Start HorariosPage FadeOut animation
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HorariosPage_FadeOut.Begin());
                }
                catch (Exception e)
                {
#if DEBUG
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Excepción en GetAsync. Detalles:" + e, NotifyType.DebugMessage));
#endif
                    // Mostrar mensaje de reintento.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Sin Internet. Reintentando...", NotifyType.ErrorMessage));

                    // Si no hay conexión a Internet disponible, reintentar la conexión una vez y mostrar un mensaje.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StartConnectionAsync(URL, legajo, 1).ConfigureAwait(false));
                }
            }
            // Reintento 1
            else
            {
                // Cache control.
                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
                filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                // Ignore SSL errors.
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
                // Default headers will be sent with every request sent from this HttpClient instance.
                httpClient.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.19035", "18.19035"));

                // Do an asynchronous GET in separate thread to not block the GUI while receiving the data from Internet.
                // and save it to a variable
                try
                {
                    // Obtener los datos del servidor y almacenarlos
                    HttpResponseMessage response = await httpClient.GetAsync(new Uri(URL, UriKind.Absolute)).AsTask(cts.Token).ConfigureAwait(false);
                    responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(cts.Token).ConfigureAwait(false);

                    // Try parse JSON
                    TryParseJson(responseBodyAsText, false);

                    // Guardar caché
                    TrySaveCache(legajo, true);
                }
                catch (TaskCanceledException)
                {
                    // Mostrar mensaje de conexión cancelada.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Conexión cancelada", NotifyType.DebugMessage));

                    // Detener animación de carga.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressRing_Animation3.IsActive = false);

                    // Start HorariosPage FadeOut animation
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HorariosPage_FadeOut.Begin());
                }
                catch (Exception e)
                {
#if DEBUG
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Excepción en GetAsync. Detalles:" + e, NotifyType.DebugMessage));
#endif

                    // Si en el reintento sigue sin haber conexión a Internet disponible, intentar leer los horarios en la caché y mostrar un mensaje.
                    string cache = Utils.TryReadFile(localfolder, "horarios" + legajo + ".tmp");

                    // Chequear si existe un archivo en caché que corresponda a los horarios que se quieren leer.
                    if (!string.IsNullOrEmpty(cache))
                    {
                        TryParseJson(cache, true);
                    }
                    else
                    {
                        // Mostrar el mensaje: No hay conexión a Internet.
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("No hay conexión a Internet.", NotifyType.ErrorMessage));

                        // Detener animación de carga.
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressRing_Animation3.IsActive = false);

                        // Start HorariosPage FadeOut animation
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HorariosPage_FadeOut.Begin());
                    }
                }
            }
        }

        // Try parse JSON
        private async void TryParseJson(string data, bool IsFromCache)
        {
            // Chequear si el archivo en caché contiene datos válidos.
            if (data.Contains(":[{"))
            {
                try
                {
                    // Parse JSON
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.DataContext = new AlseaJson(data));

                    // Mostrar mensaje exitoso.
                    if (IsFromCache) await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Sin Internet. Los horarios pueden estar desactualizados!", NotifyType.ErrorMessage));
                    else await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("Horarios leídos!", NotifyType.StatusMessage));

                    // Show ContentPanelInfo
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ContentPanelInfo.Visibility = Visibility.Visible);

                    // Show list
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ScrollList.Visibility = Visibility.Visible);

                    // Detener animación de carga.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressRing_Animation3.IsActive = false);
                }
                catch (Exception)
                {
                    // Mensaje de error de base de datos.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("La base de datos está dañada", NotifyType.ErrorMessage));

                    // Detener animación de carga.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressRing_Animation3.IsActive = false);

                    // Start HorariosPage FadeOut animation
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HorariosPage_FadeOut.Begin());
                }
            }
            else
            {
                // Mensaje de legajo inexistente o sin horarios asignados.
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("El legajo no existe o no tiene los horarios asignados.", NotifyType.ErrorMessage));

                // Detener animación de carga.
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressRing_Animation3.IsActive = false);

                // Start HorariosPage FadeOut animation
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HorariosPage_FadeOut.Begin());
            }
        }

        // Guardar caché
        private void TrySaveCache(string legajo, bool cache)
        {
            // Guardar los últimos horarios leídos.
            Utils.TryWriteFile(localfolder, "horariosLast.tmp", responseBodyAsText);
            if (cache)
            {
                // Guardar una caché de todos los horarios leídos.
                Utils.TryWriteFile(localfolder, "horarios" + legajo + ".tmp", responseBodyAsText);
            }
        }

        // Back Button Navigation from App_BackRequested()
        private async void BackButton(object sender, RoutedEventArgs e)
        {
            // Clear StatusBlock
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => rootPage.NotifyUser("", NotifyType.StatusMessage));

            // Start HorariosPage FadeOut animation
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HorariosPage_FadeOut.Begin());
        }

        // Back Button Navigation from UI()
        private void BackButton2(object sender, object e)
        {
            // Clear StatusBlock
            rootPage.NotifyUser("", NotifyType.StatusMessage);

            // Start HorariosPage FadeOut animation
            HorariosPage_FadeOut.Begin();
        }

        // Start GoPageBack when HorariosPage_FadeOut animation is completed.
        private void HorariosPage_FadeOutCompleted(object sender, object e)
        {
            // Go page back.
            GoPageBack();
        }

        // Go back to MainPage with uncleared errors
        private void GoPageBack()
        {
            // Cancel current HTTP connection
            CancelHttpTask();

            responseBodyAsText = "{\"asignaciones\":[],\"fechaConsulta\":\"\",\"legajo\":\"\"}";

            // Parse JSON
            DataContext = new AlseaJson(responseBodyAsText);

            // Go to page
            Frame.Navigate(typeof(WelcomePage));
        }

        private void CancelHttpTask()
        {
            cts.Cancel();
            cts.Dispose();

            // Re-create the CancellationTokenSource.
            cts = new CancellationTokenSource();
        }

        public static void Dispose()
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