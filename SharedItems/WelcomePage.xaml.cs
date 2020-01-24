using System;
using System.IO;

using PaatyDSM;

//using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using static MisHorarios.MainPage;

namespace MisHorarios
{
    public sealed partial class WelcomePage : Page
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage
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
            if (e.Content.ToString() == "BackButtonPressed")
            {
                // Clear errors
                rootPage.NotifyUser("", NotifyType.StatusMessage);
            }

            // Set title and colors
            if (GetCurrentProjectName() == "Mis Horarios SBX")
            {
                APP_TITLE.Text = GetCurrentProjectName();
            }
            else
            {
                APP_TITLE.Text = GetCurrentProjectName();
            }

            // Read last used legajo
            Read_legajo();

            // Check for updates
            //CheckUpdates();
        }

        // Read last used legajo
        private void Read_legajo()
        {
            // Read file
            try
            {
                var reader = new StreamReader(File.OpenRead(localfolder + "\\legajoLast.tmp"));
                while (!reader.EndOfStream)
                {
                    // Put legajo into TextBox
                    main_legajo_input.Text = reader.ReadLine();
                }
                reader.Dispose();
            }
            catch { }
        }

        // On click 'Consultar Horarios' validate Legajo and send it to the next page 'HorariosPage'
        private void Send_legajo_button(object sender, object e)
        {
            // Clear errors
            rootPage.NotifyUser("", NotifyType.StatusMessage);

            // Check if 'legajo' is valid
            var check = main_legajo_input.Text.FindFirstNotOf("0123456789");

            // Error handler and error messages
            // If no input
            if (main_legajo_input.Text.Length == 0)
            {
                rootPage.NotifyUser("Primero tenés que ingresar un legajo.", NotifyType.ErrorMessage);
            }
            // If illegal character
            else if (check != -1)
            {
                rootPage.NotifyUser("El legajo que ingresaste no es válido.", NotifyType.ErrorMessage);
            }
            // If valid
            else
            {
                // Start FadeOutAnimation
                Start_FadeOutAnimation(null, null);
            }
        }

        // Function start FadeOutAnimation
        private void Start_FadeOutAnimation(object sender, object e)
        {
            WelcomepPage_FadeOutAnimation.Begin();
        }

        // Navigate to HorariosPage
        private void NavigatetoHorariosPage(object sender, object e)
        {
            // Navigate to HorariosPage and send parameters
            Frame.Navigate(typeof(HorariosPage), main_legajo_input.Text);
        }
    }
}