using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareAudio_WinUi3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        static public App thisApp;
        public App()
        {
            this.InitializeComponent();
            thisApp = this;
        }
        public static Window exportWindow;
        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            exportWindow = m_window;
            m_window.Activate();
            m_window.ExtendsContentIntoTitleBar = true;
        }

        private Window m_window;

        public bool isStarted = false;
        public bool isMuted = true;
        public bool isServer = false;
        public IntPtr ctx = IntPtr.Zero;
        public string host = "";
        public int port = 9950;
        public int selectedDevice = 0;
        public int volumeModifier = 100;
        public List<List<string>> Devices;

        public string filename = "MyRecord";
        public string filepath = "";
        public bool isRecording = false;
        public long timestampTicks = 0;
    }
}
