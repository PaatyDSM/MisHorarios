﻿using System;

using PaatyDSM;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MisHorarios
{
    /// <summary>
    /// Proporciona un comportamiento específico de la aplicación para complementar la clase Application predeterminada.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Inicializa el objeto de aplicación Singleton. Esta es la primera línea de código creado
        /// ejecutado y, como tal, es el equivalente lógico de main() o WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Se invoca cuando la aplicación la inicia normalmente el usuario final. Se usarán otros puntos
        /// de entrada cuando la aplicación se inicie para abrir un archivo específico, por ejemplo.
        /// </summary>
        /// <param name="e">Información detallada acerca de la solicitud y el proceso de inicio.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            /* START:DEBUG SECTION */
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Debug Options:
                //this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            /* END:DEBUG SECTION */

            // No repetir la inicialización de la aplicación si la ventana tiene contenido todavía,
            // solo asegurarse de que la ventana está activa.
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Crear un marco para que actúe como contexto de navegación y navegar a la primera página.
                rootFrame = new Frame
                {
                    // Establecer el lenguaje predeterminado.
                    Language = ApplicationLanguages.Languages[0]
                };

                // Configurar el evento que se lanzará cuando ocurra un error de navegación de página.
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TO DO: Cargar el estado de la aplicación suspendida previamente
                }

                // Poner el marco en la ventana actual.
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Set resource dictionary
                SetResourceDictionary();

                // Cuando no se restaura la pila de navegación, navegar a la primera página,
                // configurando la nueva página pasándole la información requerida como
                // parámetro de navegación
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            // Asegurarse de que la ventana actual está activa.
            Window.Current.Activate();
        }

        private void SetResourceDictionary()
        {
            ResourceDictionary resourceDictionary = new ResourceDictionary();

            if (Utils.GetCurrentProjectName() == "Mis Horarios SBX")
                resourceDictionary.Source = new Uri("ms-appx:///Styles/SBXDictionary.xaml", UriKind.Absolute);
            else resourceDictionary.Source = new Uri("ms-appx:///Styles/BKDictionary.xaml", UriKind.Absolute);
            Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        /// <summary>
        /// Se invoca cuando la aplicación la inicia normalmente el usuario final. Se usarán otros puntos
        /// </summary>
        /// <param name="sender">Marco que produjo el error de navegación</param>
        /// <param name="e">Detalles sobre el error de navegación</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Error al cargar la siguiente página" + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Se invoca al suspender la ejecución de la aplicación. El estado de la aplicación se guarda
        /// sin saber si la aplicación se terminará o se reanudará con el contenido
        /// de la memoria aún intacto.
        /// </summary>
        /// <param name="sender">Origen de la solicitud de suspensión.</param>
        /// <param name="e">Detalles sobre la solicitud de suspensión.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            // TO DO: Guardar el estado de la aplicación y detener toda actividad en segundo plano
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}