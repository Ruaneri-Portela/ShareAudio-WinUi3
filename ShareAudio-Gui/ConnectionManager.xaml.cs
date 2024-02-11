using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using libShareAudio;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareAudio_WinUi3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectionManager : Page
    {
        private App thisApp = App.thisApp;
        private bool isStarted = false;


        public ConnectionManager()
        {
            this.InitializeComponent();
            ChangeContext();
            MuteSwipe(false);
            ClientRadio.IsChecked = !thisApp.isServer;
            Port.Value = thisApp.port;
            Host.Text = thisApp.host;
            AudioDevices.SelectedIndex = thisApp.selectedDevice;
            VolumeSlider.Value = thisApp.volumeModifier;
            isStarted = true;
            if (thisApp.ctx != IntPtr.Zero)
            {
                UpdateStats();
            }
        }

        async private void UpdateStats()
        {
            while (thisApp.ctx != IntPtr.Zero)
            {
                List<string> Stats = SA.GetStats(thisApp.ctx);
                if (Stats.Count == 9)
                {
                    switch (Stats[8])
                    {
                        case "0":
                            Status.Text = "Status: Disconnected";
                            Status.Foreground = new SolidColorBrush(Colors.Red);
                            break;
                        case "1":
                            if (thisApp.isServer)
                            {
                                Status.Text = "Status: Waiting for client";
                            }
                            else
                            {
                                Status.Text = "Status: Connecting";
                            }
                            Status.Foreground = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "2":
                            int Chunks, Bandwidth, PacketsPerSecond, Kbps, Reviced, Sender, PacketLost, BandwidthElapsed;
                            if (int.TryParse(Stats[4], out Chunks) && Chunks != 0 &&
                                int.TryParse(Stats[2], out int Channels) &&
                                int.TryParse(Stats[3], out int SampleRate) &&
                                int.TryParse(Stats[0], out Sender) &&
                                int.TryParse(Stats[1], out Reviced))
                            {
                                Bandwidth = Chunks * Channels * 4;

                                if (Bandwidth != 0 && Chunks != 0)
                                {
                                    PacketsPerSecond = SampleRate / Chunks;
                                    Kbps = ((Bandwidth / 4) * PacketsPerSecond) / 3 / 10;
                                    Status.Text = "Status: Connected " + Kbps + " Kbps";
                                    Status.Foreground = new SolidColorBrush(Colors.GreenYellow);
                                }
                                if (!thisApp.isServer)
                                {
                                    ServerText.Text = "Packets Sender by server: " + Stats[0];
                                    StatusText.Text = "Packets Received: " + Stats[1];
                                    PacketLost = Sender - Reviced;
                                    if (PacketLost != 0 && Reviced != 0)
                                    {
                                        PercentageText.Text = "Lost Percentage: " + ((double)PacketLost / Reviced).ToString("P");
                                    }
                                    else
                                    {
                                        PercentageText.Text = "Packets Lost : 0";
                                    }
                                    LostText.Text = "Packets Lost: " + PacketLost.ToString();
                                    BandwidthElapsed = (Bandwidth * Sender) / 1024 / 1024;
                                    BandwidthText.Text = "Bandwidth: " + BandwidthElapsed.ToString() + " MB";
                                }
                                else
                                {
                                    PercentageText.Text = "";
                                    ServerText.Text = "";
                                    StatusText.Text = "";
                                    LostText.Text = "";
                                    BandwidthText.Text = "";
                                }
                                ChannelsText.Text = "Channels: " + Channels.ToString();
                                SampleRateText.Text = "Hz: " + SampleRate.ToString();
                            }
                            break;
                    }
                }
                await System.Threading.Tasks.Task.Delay(500);
            }
        }

        async private void WrongConfgAlert(string ContextError)
        {
            ContentDialog NonPathAlert = new ContentDialog()
            {
                Title = "Wrong Configuration",
                Content = ContextError,
                CloseButtonText = "Ok",
                XamlRoot = App.exportWindow.Content.XamlRoot
            };
            await NonPathAlert.ShowAsync();
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Host.Text == "" && Port.Text == "")
            {
                WrongConfgAlert("The host as empty and the port is empty");
            }
            else if (Host.Text == "")
            {
                WrongConfgAlert("The host as empty");
            }
            else if (Port.Text == "")
            {
                WrongConfgAlert("The port is empty");
            }
            else
            {
                if (thisApp.isStarted)
                {
                    SA.Close(thisApp.ctx);
                    thisApp.ctx = IntPtr.Zero;
                    thisApp.isStarted = !thisApp.isStarted;
                }
                else
                {
                    string selectedItem = AudioDevices.SelectedItem.ToString();
                    int index = selectedItem.IndexOf('@');
                    selectedItem = selectedItem.Substring(0, index - 1);
                    int device = 0;
                    foreach (List<string> Device in thisApp.Devices)
                    {
                        if (Device[0] == selectedItem)
                        {
                            device = int.Parse(Device[4]);
                            break;
                        }
                    }
                    thisApp.ctx = SA.Setup(device, Host.Text, thisApp.isServer ? 1 : 0, int.Parse(Port.Text), 0, 2, -1, 2048, -1);
                    SA.Init(thisApp.ctx);
                    if (thisApp.isServer)
                    {
                        SA.Server(thisApp.ctx);
                    }
                    else
                    {
                        SA.Client(thisApp.ctx);
                    }
                    thisApp.isStarted = !thisApp.isStarted;
                    thisApp.port = (int)Port.Value;
                    thisApp.host = Host.Text;
                    thisApp.selectedDevice = AudioDevices.SelectedIndex;
                    UpdateStats();
                }
                ChangeContext();
            }
        }

        private void ClientRadio_Checked(object sender, RoutedEventArgs e)
        {
            ServerRadio.IsChecked = false;
            thisApp.isServer = false;
            ChangeContext();
            UpdateComboBox();
        }

        private void ServerRadio_Checked(object sender, RoutedEventArgs e)
        {
            ClientRadio.IsChecked = false;
            thisApp.isServer = true;
            UpdateComboBox();
            ChangeContext();
        }

        private void ChangeContext()
        {
            RadioButton[] radioButtons = { ClientRadio, ServerRadio };
            if (thisApp.isStarted)
            {
                foreach (RadioButton radioButton in radioButtons)
                {
                    radioButton.IsEnabled = false;
                }
                Host.IsEnabled = false;
                Port.IsEnabled = false;
                AudioDevices.IsEnabled = false;
                ConnectionButton.Content = "Stop";
                Status.Text = "Status";
            }
            else
            {
                Status.Text = "";
                ServerText.Text = "";
                StatusText.Text = "";
                PercentageText.Text = "";
                LostText.Text = "";
                BandwidthText.Text = "";
                ChannelsText.Text = "";
                SampleRateText.Text = "";
                foreach (RadioButton radioButton in radioButtons)
                {
                    radioButton.IsEnabled = true;
                }
                Host.IsEnabled = true;
                Port.IsEnabled = true;
                AudioDevices.IsEnabled = true;
                if (ClientRadio.IsChecked == true)
                {
                    ConnectionButton.Content = "Connect";
                }
                else
                {
                    ConnectionButton.Content = "Start";
                }
            }
        }

        private void AudioMute_Click(object sender, RoutedEventArgs e)
        {
            MuteSwipe();
            if (thisApp.isMuted && thisApp.ctx != IntPtr.Zero)
            {
                SA.SetVolumeModifier((float)VolumeSlider.Value, thisApp.ctx);
            }
            else if (thisApp.ctx != IntPtr.Zero)
            {
                SA.SetVolumeModifier(0, thisApp.ctx);
            }

        }

        private void MuteSwipe(bool swipe = true)
        {
            if (swipe)
            {
                thisApp.isMuted = !thisApp.isMuted;
            }
            if (thisApp.isMuted)
            {
                AudioMuteIcon.Glyph = "\ue767";
            }
            else
            {
                AudioMuteIcon.Glyph = "\ue74f";
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isStarted)
            {
                if (VolumeSlider.Value == 0)
                {
                    thisApp.isMuted = true;
                }
                else
                {
                    thisApp.isMuted = false;
                }
                if (thisApp.ctx != IntPtr.Zero)
                    SA.SetVolumeModifier((float)VolumeSlider.Value, thisApp.ctx);
                thisApp.volumeModifier = (int)VolumeSlider.Value;
                MuteSwipe();
            }
        }

        private void UpdateComboBox()
        {
            thisApp.Devices = SA.ListAllAudioDevices(thisApp.ctx);
            AudioDevices.Items.Clear();
            foreach (List<string> Device in thisApp.Devices)
            {
                if (Device.Count == 5)
                    if (thisApp.isServer && Device[2] != "0")
                    {
                        AudioDevices.Items.Add(Device[0] + " @" + Device[1] + "Hz");
                    }
                    else if (!thisApp.isServer && Device[3] != "0")
                    {
                        AudioDevices.Items.Add(Device[0] + " @" + Device[1] + "Hz");
                    }
            }
            AudioDevices.SelectedIndex = 0;
        }
    }
}

