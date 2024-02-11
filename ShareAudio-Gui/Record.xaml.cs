using libShareAudio;
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
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareAudio_WinUi3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Record : Page
    {
        App thisApp = App.thisApp;
        public Record()
        {
            this.InitializeComponent();
            switchEnable();
            RecordFilePath.Text = thisApp.filepath;
            RecordFileName.Text = thisApp.filename;
            updateFileName();
            updateButton();
            if (thisApp.isRecording)
            {
                UpdateTime();
            }
        }

        private async void UpdateTime()
        {
            long localTimeStamp = 0;
            while (true)
            {
                localTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                DateTime dateTime1 = DateTimeOffset.FromUnixTimeSeconds(thisApp.timestampTicks).UtcDateTime;
                DateTime dateTime2 = DateTimeOffset.FromUnixTimeSeconds(localTimeStamp).UtcDateTime;
                TimeSpan diff = dateTime2 - dateTime1;
                RecordTime.Text = $"{(int)diff.TotalHours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
                if (thisApp.isRecording)
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                }
                else
                {
                    RecordTime.Text = "00:00:00";
                    break;
                }
            }
        }

        private void switchEnable()
        {
            if (App.thisApp.ctx == IntPtr.Zero)
            {
                thisApp.isRecording = false;
            }
            if (App.thisApp.ctx != IntPtr.Zero && !thisApp.isRecording)
            {
                RecordButton.IsEnabled = true;
                RecordFilePath.IsEnabled = true;
                RecordFileName.IsEnabled = true;
                RecordFilePicker.IsEnabled = true;
            }
            else
            {
                if (App.thisApp.ctx == IntPtr.Zero)
                {
                    RecordButton.IsEnabled = false;
                }
                RecordFilePath.IsEnabled = false;
                RecordFileName.IsEnabled = false;
                RecordFilePicker.IsEnabled = false;
            }
        }

        private async void RecordFilePicker_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            var window = App.exportWindow;
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(folderPicker, hWnd);
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = null;
            while (folder == null)
            {
                folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    RecordFilePath.Text = (folder.Path);
                }
                else
                {
                    ContentDialog NonPathAlert = new ContentDialog()
                    {
                        Title = "No path selected",
                        Content = "Please select a file diretory to save as record audio",
                        CloseButtonText = "Ok",
                        XamlRoot = window.Content.XamlRoot
                    };
                    await NonPathAlert.ShowAsync();
                }
            }
            thisApp.filepath = folder.Path;
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            thisApp.isRecording = !thisApp.isRecording;
            if (thisApp.isRecording)
            {
                thisApp.timestampTicks = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                UpdateTime();
                SA.InitWavRecord(thisApp.ctx, RecordFileFullPath.Text);
            }
            else
            {
                thisApp.timestampTicks = 0;
                SA.CloseWavRecord();
                updateFileName();
            }
            switchEnable();
            updateButton();
        }

        private void RecordFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateFileName();
        }

        private void RecordFilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateFileName();
        }

        private string GetValidFileName(string path)
        {
            string TryPath = path + ".wav";
            if (File.Exists(TryPath))
            {
                for (int i = 0; ; i++)
                {
                    TryPath = path + "(" + (i+1).ToString() + ")" + ".wav";
                    if (!File.Exists(TryPath))
                    {
                        return TryPath;
                    }
                }
            }
            else
            {
                return TryPath;
            }

        }

        private void updateFileName()
        {
            if (RecordFilePath.Text == "")
            {
                RecordFileFullPath.Text = GetValidFileName(RecordFileName.Text);
            }
            else
            {
                RecordFileFullPath.Text = GetValidFileName(RecordFilePath.Text + "\\" + RecordFileName.Text);
            }
        }

        private void updateButton()
        {
            if (thisApp.isRecording)
            {
                RecordButton.Content = "Stop";
            }
            else
            {
                RecordButton.Content = "Record";
            }
        }
    }
}
