using APKInstaller.Models;
using CommunityToolkit.WinUI.Connectivity;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Net.Http;
using Windows.System;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace APKInstaller.Controls.Dialogs
{
    public sealed partial class MarkdownDialog : ContentDialog, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ContentInfoProperty = DependencyProperty.Register(
           "ContentInfo",
           typeof(GitInfo),
           typeof(MarkdownDialog),
           new PropertyMetadata(default(GitInfo), OnContentUrlChanged));

        public GitInfo ContentInfo
        {
            get => (GitInfo)GetValue(ContentInfoProperty);
            set => SetValue(ContentInfoProperty, value);
        }

        private static void OnContentUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as MarkdownDialog).UpdateContent();

        private bool isInitialized;
        internal bool IsInitialized
        {
            get => isInitialized;
            set
            {
                isInitialized = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MarkdownDialog() => InitializeComponent();

        private async void UpdateContent()
        {
            if (ContentInfo == default(GitInfo)) { return; }
            IsInitialized = false;
            string value = ContentInfo.FormatURL(GitInfo.GITHUB_API);
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                MarkdownText.Text = value;
                return;
            }
            using (HttpClient client = new())
            {
                try
                {
                    string text = await client.GetStringAsync(value);
                    if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentNullException(nameof(text)); }
                    MarkdownText.Text = text;
                    Title = string.Empty;
                }
                catch
                {
                    try
                    {
                        string text = await client.GetStringAsync(ContentInfo.FormatURL(GitInfo.JSDELIVR_API));
                        if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentNullException(nameof(text)); }
                        MarkdownText.Text = text;
                        Title = string.Empty;
                    }
                    catch
                    {
                        MarkdownText.Text = value;
                    }
                }
            }
            IsInitialized = true;
        }

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(e.Link));
    }
}
