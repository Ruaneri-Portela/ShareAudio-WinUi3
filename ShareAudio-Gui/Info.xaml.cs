using libShareAudio;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareAudio_WinUi3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Info : Page
    {
        public Info()
        {
            this.InitializeComponent();
            List<string> Versions = SA.GetVersion();
            WrapperVer.Text = Versions[4];
            LibVer.Text = Versions[0] + " Build on " + Versions[1];
            PaVer.Text = Versions[2];
            GuiVer.Text = "Alpha";
        }
    }
}
