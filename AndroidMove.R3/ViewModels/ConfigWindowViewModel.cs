using AndroidMove.R3.Models;
using MahApps.Metro.Controls.Dialogs;
using R3;
using System.Text;

namespace AndroidMove.R3.ViewModels
{
    public class ConfigWindowViewModel : ViewModelBase
    {
        public IDialogCoordinator? DialogCoordinator { get; set; }
        public BindableReactiveProperty<string> AdbPath { get; }
        public BindableReactiveProperty<int> IntervalSeconds { get; }
        public BindableReactiveProperty<int> PixelSize { get; }
        public BindableReactiveProperty<bool> WidthSelected { get; }
        public bool? DialogResult { get; set; }
        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        public ConfigWindowViewModel() : base()
        {
            this.DialogCoordinator = MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;
            var conf = App.GetService<AppConfig>()!;
            var adbConfig = conf.AdbConfig;
            var copyImageConfig = conf.CopyImageConfig;
            this.AdbPath = new BindableReactiveProperty<string>(adbConfig.AdbPath ?? "adb-path");
            this.IntervalSeconds = new BindableReactiveProperty<int>(adbConfig.IntervalSeconds <= 0 ? 3 : adbConfig.IntervalSeconds);
            this.PixelSize = new BindableReactiveProperty<int>(copyImageConfig.PixelSize < 400 ? 400 : copyImageConfig.PixelSize);
            this.WidthSelected = new BindableReactiveProperty<bool>(copyImageConfig.Orientation == System.Windows.Controls.Orientation.Horizontal);
            this.SaveCommand = new ReactiveCommand();
            this.SaveCommand.Subscribe(async _ =>
            {
                try
                {
                    this.Validate();
                    this.Save();
                    this.DialogResult = true;
                    RaisePropertyChanged(nameof(DialogResult));
                }
                catch (Exception ex)
                {
                    await this.DialogCoordinator.ShowMessageAsync(this, "Error", ex.Message);
                }
            });
            this.CancelCommand = new ReactiveCommand();
            this.CancelCommand.Subscribe(_ =>
            {
                this.DialogResult = false;
                RaisePropertyChanged(nameof(DialogResult));
            });
        }

        private void Validate()
        {
            var sb=new StringBuilder();
            if (string.IsNullOrWhiteSpace(this.AdbPath.Value))
            {
                sb.AppendLine("ADBパスを入力してください");
            }
            if(System.IO.Path.GetFileName(this.AdbPath.Value)?.ToLower() != "adb.exe")
            {
                sb.AppendLine("ADBパスはADB.exeを指定してください");
            }
            if (!System.IO.File.Exists(this.AdbPath.Value))
            {
                sb.AppendLine("ADB.exeが存在しません");
            }
            if (this.IntervalSeconds.Value <= 0)
            {
                sb.AppendLine("監視間隔は1秒以上にしてください");
            }
            if(this.PixelSize.Value < 200)
            {
                sb.AppendLine("ピクセルサイズは200以上にしてください");
            }
            if(sb.Length > 0)
            {
                throw new InvalidOperationException(sb.ToString());
            }
        }
        private void Save()
        {
            var conf = App.GetService<AppConfig>()!;
            conf.AdbConfig.AdbPath = this.AdbPath.Value;
            conf.AdbConfig.IntervalSeconds = this.IntervalSeconds.Value;
            conf.CopyImageConfig.PixelSize = this.PixelSize.Value;  
            conf.CopyImageConfig.Orientation = this.WidthSelected.Value ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
            conf.Save();
        }
    }
}
