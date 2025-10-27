using AndroidMove.R3.Extensions;
using AndroidMove.R3.Models;
using AndroidMove.R3.Views;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using ObservableCollections;
using R3;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.Xml;
using System.Windows.Controls;
using System.Windows.Data;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace AndroidMove.R3.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public delegate void SnackbarMessageEventHandler(object sender, SnackbarMessageEventArgs e);
        public event SnackbarMessageEventHandler SnackbarMessage = delegate { };
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        public ObservableCollection<AndroidDeviceBoxViewModel> DeviceViewModels { get; }
        public SnackbarMessageQueue SnackbarMessageQueue { get; }
        public IDialogCoordinator? DialogCoordinator { get; set; }
        public BindableReactiveProperty<bool> WithClipboard { get; }
        public BindableReactiveProperty<bool> WidthSelected { get; }
        public BindableReactiveProperty<Orientation> Orientation { get; } 
        public BindableReactiveProperty<int> PixelSize { get; }
        public ReactiveCommand ShowThemeSettingsCommand { get; }
        public BindableReactiveProperty<bool> IsTopMost { get; }
        public string AppTitle { get; }
        public string AppFullTitle { get; }
        public string AppVersion { get; }
        public ReactiveCommand ConfigCommand { get; }

        public BindableReactiveProperty<bool> IsDarkMode { get; }

        public MainWindowViewModel()
        {
            this.DialogCoordinator = MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;

            var conf = App.GetService<AppConfig>()!;
            if (!conf.AdbConfig.IsValid())
            {
                var window = App.GetService<ConfigWindow>()!;
                if (window.ShowDialog() != true)
                {
                    App.Current.Shutdown();
                }
            }
            this.IsDarkMode = new BindableReactiveProperty<bool>(conf.Theme!.IsDarkTheme);
            this.IsDarkMode.Subscribe(x =>
            {
                conf.Theme!.IsDarkTheme = x;
                Theme theme = _paletteHelper.GetTheme();
                theme.SetBaseTheme(x ? BaseTheme.Dark : BaseTheme.Light);
                _paletteHelper.SetTheme(theme);
            });

            this.IsTopMost = new BindableReactiveProperty<bool>(conf.IsTopMost);
            this.IsTopMost.Subscribe(x => conf.IsTopMost = x);

            this.DeviceViewModels = new ObservableCollection<AndroidDeviceBoxViewModel>();
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetVersion();
            var fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.AppTitle = $"{fv.ProductName}";
            this.AppVersion = $"ver {version}";
            this.AppFullTitle = $"{AppTitle} {AppVersion}";
            this.SnackbarMessageQueue = new SnackbarMessageQueue();
            this.DeviceViewModels.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(DeviceViewModels));

            this.Orientation = new BindableReactiveProperty<Orientation>(conf.CopyImageConfig.Orientation);
            this.Orientation.Subscribe(x =>
            {
                conf.CopyImageConfig.Orientation = x;
            });

            this.WidthSelected = new BindableReactiveProperty<bool>(conf.CopyImageConfig.Orientation == System.Windows.Controls.Orientation.Horizontal);
            this.WidthSelected.Subscribe(x =>
            {
                this.Orientation.Value = x ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
            });

            this.PixelSize = new BindableReactiveProperty<int>(conf.CopyImageConfig.PixelSize);
            this.PixelSize.Subscribe(x =>
            {
                conf.CopyImageConfig.PixelSize = x;
            });

            this.WithClipboard = new BindableReactiveProperty<bool>(conf.CopyImageConfig.WithClipboard);
            this.WithClipboard.Subscribe(x =>
            {
                conf.CopyImageConfig.WithClipboard = x;
            });

            var d = Observable.Interval(TimeSpan.FromSeconds(conf.AdbConfig.IntervalSeconds)).SubscribeAwait(async (x, ct) =>
            {
                var list = await AndroidDevice.ListDevicesAsync();
                foreach(var device in GetAddedDevices(list))
                {
                    this.DeviceViewModels.Add(new AndroidDeviceBoxViewModel(device));
                }

                foreach (var device in GetRemovedDevices(list))
                {
                    this.DeviceViewModels.Remove(device);
                }
            });

            this.ConfigCommand = new ReactiveCommand();
            this.ConfigCommand.Subscribe(async _ =>
            {
                await App.Current.Dispatcher.InvokeAsync(async () =>
                {
                    var window = App.GetService<ConfigWindow>()!;
                    if (window.ShowDialog() == true)
                    {
                        await DialogCoordinator.ShowMessageAsync(this,"設定","設定を保存しました。アプリを再起動してください");
                        App.Current.Shutdown();
                    }
                });
            });

            this.ShowThemeSettingsCommand = new ReactiveCommand();
            this.ShowThemeSettingsCommand.Subscribe(_ =>
            {
                var window= App.GetService<ThemeSettingsWindow>()!;
                window.ShowDialog();
            });
        }

        private IEnumerable<AndroidDeviceBoxViewModel> GetRemovedDevices(List<AndroidDevice> listedDevices)
        {
            return this.DeviceViewModels.Where(x => 
                !listedDevices.Where(y => y.Serial == x.Device.Serial).Any());
        }
        
        private IEnumerable<AndroidDevice> GetAddedDevices(List<AndroidDevice>listedDevices)
        {
            return listedDevices.Where(x => !this.DeviceViewModels.Any(vm => vm.Device.Serial == x.Serial));
        }

        public void OnErrorOccurred(object sender, ErrorOccurredEventArgs e)
        {
            DialogCoordinator?.ShowMessageAsync(this, "エラー", e.Message, MessageDialogStyle.Affirmative);
        }

        public void OnMessage(object sender, MessageEventArgs e)
        {
            DialogCoordinator?.ShowMessageAsync(this, e.Title, e.Message, MessageDialogStyle.Affirmative);
        }

        public void OnSnackBarMessage(object sender, SnackBarMessageEventArgs e)
        {
            this.SnackbarMessageQueue.Enqueue(e.Message);
        }
    }
}