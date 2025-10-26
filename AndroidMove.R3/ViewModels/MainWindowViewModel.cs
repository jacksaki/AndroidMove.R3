using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.Xml;
using System.Windows.Controls;
using System.Windows.Data;
using AndroidMove.R3.Extensions;
using AndroidMove.R3.Models;
using AndroidMove.R3.Views;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using ObservableCollections;
using R3;

namespace AndroidMove.R3.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public delegate void SnackbarMessageEventHandler(object sender, SnackbarMessageEventArgs e);
        public event SnackbarMessageEventHandler SnackbarMessage = delegate { };
        public ObservableCollection<AndroidDeviceBoxViewModel> DeviceViewModels { get; }
        public SnackbarMessageQueue SnackbarMessageQueue { get; }
        public IDialogCoordinator? DialogCoordinator { get; set; }
        public BindableReactiveProperty<bool> WithClipboard { get; }
        public BindableReactiveProperty<bool> WidthSelected { get; }
        public BindableReactiveProperty<Orientation> Orientation { get; } 
        public BindableReactiveProperty<int> PixelSize { get; }

        public string AppTitle { get; }
        public string AppFullTitle { get; }
        public string AppVersion { get; }
        public ReactiveCommand ConfigCommand { get; }

        public MainWindowViewModel()
        {
            var conf = App.GetService<AppConfig>()!;
            if (!conf.AdbConfig.IsValid())
            {
                var window = App.GetService<ConfigWindow>()!;
                if (window.ShowDialog() != true)
                {
                    App.Current.Shutdown();
                }
            }
            this.DeviceViewModels = new ObservableCollection<AndroidDeviceBoxViewModel>();
            this.DialogCoordinator = MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetVersion();
            var fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.AppTitle = $"{fv.ProductName}";
            this.AppVersion = $"ver {version}";
            this.AppFullTitle = $"{AppTitle} {AppVersion}";
            this.SnackbarMessageQueue = new SnackbarMessageQueue();
            this.DeviceViewModels.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(DeviceViewModels));

            this.Orientation = new BindableReactiveProperty<Orientation>(conf.CopyImageConfig.Orientation);
            this.WidthSelected = new BindableReactiveProperty<bool>(conf.CopyImageConfig.Orientation == System.Windows.Controls.Orientation.Horizontal);
            this.WidthSelected.Subscribe(x =>
            {
                this.Orientation.Value = x ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
            });
            this.PixelSize = new BindableReactiveProperty<int>(conf.CopyImageConfig.PixelSize);
            this.WithClipboard = new BindableReactiveProperty<bool>(conf.CopyImageConfig.WithClipboard);

            var d = Observable.Interval(TimeSpan.FromSeconds(conf.AdbConfig.IntervalSeconds)).SubscribeAwait(async (x, ct) =>
            {
                var list = await AndroidDevice.ListDevicesAsync();
                foreach (var device in list)
                {
                    if (!this.DeviceViewModels.Any(vm => vm.Device.Serial == device.Serial))
                    {
                        this.DeviceViewModels.Add(new AndroidDeviceBoxViewModel(device));
                    }
                }

                var removeDevices = new List<AndroidDeviceBoxViewModel>();
                foreach (var vm in this.DeviceViewModels)
                {
                    if (!list.Where(x => x.Serial == vm.Device.Serial).Any())
                    {
                        removeDevices.Add(vm);
                    }
                }
                foreach (var device in removeDevices)
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
                        await DialogCoordinator.ShowMessageAsync(this,"設定","設定を保存しました。アプリを再起動します");
                        App.Current.Shutdown();
                    }
                });
            });
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