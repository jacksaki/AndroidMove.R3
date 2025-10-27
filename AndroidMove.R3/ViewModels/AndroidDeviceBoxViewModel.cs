using AndroidMove.R3.Models;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AndroidMove.R3.Extensions;

namespace AndroidMove.R3.ViewModels
{
    public class AndroidDeviceBoxViewModel : BoxViewModelBase
    {
        public AndroidDevice Device { get; }

        public ReactiveCommand CaptureAndPullCommand { get; }
        public ReactiveCommand CaptureCommand { get; }
        public ReactiveCommand FileListCommand { get; }
        public ReactiveCommand SettingsCommand { get; }
        public BindableReactiveProperty<bool> WithClipboard =>this.Parent.WithClipboard;
        public BindableReactiveProperty<int> PixelSize => this.Parent.PixelSize;
        public BindableReactiveProperty<Orientation> Orientation => this.Parent.Orientation;
        public AndroidDeviceBoxViewModel(AndroidDevice device)
            :base()
        {
            var conf=App.GetService<AppConfig>()!;
            this.Device = device;
            var deviceConfig = conf.GetDeviceConfig(device)!;

            this.CaptureAndPullCommand = new ReactiveCommand();
            this.CaptureAndPullCommand.SubscribeAwait(async(x,ct) =>
            {
                try
                {
                    var path = await this.Device.CaptureAsync();
                    var localPath = await this.Device.PullAsync(path);
                    if (this.WithClipboard.Value)
                    {
                        localPath.ToClipboard(this.Orientation.Value, this.PixelSize.Value);
                        OnSnackBarMessage(new SnackBarMessageEventArgs("コピーしました"));
                    }
                    else
                    {
                        OnSnackBarMessage(new SnackBarMessageEventArgs("スクリーンショットを取得しました"));
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(new ErrorOccurredEventArgs("エラー", ex.ToString()));
                }
            },  AwaitOperation.Sequential);
            this.CaptureCommand = new ReactiveCommand();
            this.CaptureCommand.SubscribeAwait(async(x,ct) =>
            {
                try
                {
                    var path = await this.Device.CaptureAsync();
                    OnSnackBarMessage(new SnackBarMessageEventArgs($"スクリーンショットを取得しました {path}"));
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(new ErrorOccurredEventArgs("エラー", ex.ToString()));
                }
            }, AwaitOperation.Sequential);
            this.FileListCommand = new ReactiveCommand();
            this.FileListCommand.Subscribe(_ =>
            {
                var window = App.GetService<Views.FileListWindow>()!;
                var vm =  new FileListWindowViewModel(this.Device);
                window.DataContext = vm;
                window.ShowDialog();
            });
            this.SettingsCommand = new ReactiveCommand();
            this.SettingsCommand.Subscribe(_ =>
            {
                var window = App.GetService<Views.DeviceConfigWindow>()!;
                var vm = new DeviceConfigWindowViewModel(this.Device);
                window.DataContext = vm;
                window.ShowDialog();
            });
        }   
    }
}
