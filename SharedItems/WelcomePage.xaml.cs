using PaatyDSM;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using static MisHorarios.MainPage;

namespace MisHorarios
{
    public sealed partial class WelcomePage : Page
    {
        // A pointer back to the main page. This is needed if you want to call methods in MainPage
        private readonly MainPage rootPage = Current;

        public WelcomePage()
        {
            InitializeComponent();
        }

        // Function start_fade-in_animation
        private void Start_FadeInAnimation(object sender, object e)
        {
            WelcomepPage_FadeInAnimation.Begin();
        }

        // OnNavigatedTo function
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Clear status messages when BackButton is pressed.
            if (e.Content.ToString() == "BackButtonPressed") rootPage.NotifyUser("", NotifyType.StatusMessage);

            // Set title and colors.
            SetTitleAndColors();

            //123123123
            footerPanel.Visibility = Visibility.Visible;

            // Read last used legajo
            main_legajo_input.Text = Utils.TryReadFile(localfolder, "legajoLast.tmp");

            // Check for updates
            //CheckUpdates();

            ProgressRing_Animation1.IsActive = false;
        }

        public void SetTitleAndColors()
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

        // On click 'Consultar Horarios' validate Legajo and send it to the next page 'HorariosPage'
        private void Verify_and_SendLegajo(object sender, object e)
        {
            // Clear errors
            rootPage.NotifyUser("", NotifyType.StatusMessage);

            // Check if 'legajo' is valid
            if (ValidLegajo(main_legajo_input.Text))
            {
                // Start FadeOutAnimation
                Click_WelcomepPage_to_HorariosPage_FadeOut(null, null);
            }
        }

        private bool ValidLegajo(string legajo)
        {
            var check = legajo.FindFirstNotOf("0123456789");

            // If no input
            if (legajo.Length == 0)
            {
                rootPage.NotifyUser("Primero tenés que ingresar un legajo.", NotifyType.ErrorMessage);
                return false;
            }
            // If illegal character
            else if (check != -1)
            {
                rootPage.NotifyUser("El legajo que ingresaste no es válido.", NotifyType.ErrorMessage);
                return false;
            }
            else return true;
        }

        // Start FadeOutAnimation
        private void Click_WelcomepPage_to_HorariosPage_FadeOut(object sender, object e)
        {
            footerPanel.Visibility = Visibility.Collapsed;
            WelcomepPage_to_HorariosPage_FadeOut.Begin();
        }

        // Navigate to HorariosPage
        private void NavigatetoHorariosPage(object sender, object e)
        {
            // Navigate to HorariosPage and send parameters
            frame.Navigate(typeof(HorariosPage), main_legajo_input.Text);
        }
    }
}