using libShareAudio;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareAudio_WinUi3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            MicaBackdrop micaBackdrop = new MicaBackdrop();
            micaBackdrop.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
            this.SystemBackdrop = micaBackdrop;
            ConnetionIcon.IsSelected = true;
            SA.SetLogFile("shareaudio.log", 1);
        }
        private void MyNavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ConnectionManager));
            MyNavigationView.IsPaneOpen = false;
        }
        private void MyNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedNavItem)
            {
                switch (selectedNavItem.Tag.ToString())
                {
                    case "ConnectionManager":
                        ContentFrame.Navigate(typeof(ConnectionManager));
                        break;
                    case "RecordManager":
                        ContentFrame.Navigate(typeof(Record));
                        break;
                    case "Info":
                        ContentFrame.Navigate(typeof(Info));
                        break;
                }
            }
        }
    }
}
