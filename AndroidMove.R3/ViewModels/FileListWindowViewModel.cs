using AndroidMove.R3.Models;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AndroidMove.R3.ViewModels
{
    public class FileListWindowViewModel : ViewModelBase
    {
        public ReactiveCommand RefreshFilesCommand { get; }
        public string ScreenshotDirectory { get; }
        public ReactiveCommand SelectLocalDirectoryCommand { get; }
        public BindableReactiveProperty<string> LocalDirectory { get; }
        public ObservableCollection<BindableAndroidFile> Files { get; }
        public ReactiveCommand PullCommand { get; }
        public IDialogCoordinator DialogCoordinator { get; set; }
        public SnackbarMessageQueue MessageQueue { get; }
        public BindableReactiveProperty<bool> FileSelected { get; }
        public ReactiveCommand ShowLocalDirectoryCommand { get; }
        public AndroidDevice Device { get; }
        public FileListWindowViewModel(AndroidDevice device) : base()
        {
            this.DialogCoordinator = MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;
            this.MessageQueue = new SnackbarMessageQueue();
            var conf = App.GetService<AppConfig>()!;
            Device = device;
            this.FileSelected = new BindableReactiveProperty<bool>();

            this.ScreenshotDirectory= conf.AdbConfig.ScreenshotDirectory;
            this.LocalDirectory= new BindableReactiveProperty<string>(device.LocalDirectory);
            this.Files = new ObservableCollection<BindableAndroidFile>();

            this.RefreshFilesCommand = new ReactiveCommand();
            this.RefreshFilesCommand.SubscribeAwait(async (x, ct) =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                this.Files.Clear();
                try
                {
                    await foreach (var file in device.EnumerateFilesAsync())
                    {
                        this.Files.Add(new BindableAndroidFile(file));
                    }
                    this.MessageQueue.Enqueue($"{this.Files.Count}件");
                }
                catch (Exception ex)
                {
                    await DialogCoordinator.ShowMessageAsync(this, "エラー", ex.Message, MessageDialogStyle.Affirmative);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                    RaisePropertyChanged(nameof(Files));
                }
            });


            this.PullCommand = new ReactiveCommand();
            this.PullCommand.Subscribe(async _ =>
            {
                try
                {
                    foreach(var file in this.Files.Where(f => f.IsSelected.Value))
                    {
                        await device.PullAsync(file.File.Path);
                    }
                    this.MessageQueue.Enqueue("ダウンロード完了しました");
                }
                catch (Exception ex)
                {
                    await DialogCoordinator.ShowMessageAsync(this, "エラー", ex.Message, MessageDialogStyle.Affirmative);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            });

            this.SelectLocalDirectoryCommand = new ReactiveCommand();
            this.SelectLocalDirectoryCommand.Subscribe(_ =>
            {
                var dlg = new CommonOpenFileDialog();
                dlg.IsFolderPicker = true;
                dlg.InitialDirectory = this.LocalDirectory.Value;
                if(dlg.ShowDialog()== CommonFileDialogResult.Ok)
                {
                    this.LocalDirectory.Value=dlg.FileName;
                }
            });
            this.ShowLocalDirectoryCommand=new ReactiveCommand();
            this.ShowLocalDirectoryCommand.Subscribe(_ =>
            {
                Process.Start($"explorer", $"\"{this.LocalDirectory.Value}\"");
            });
        }

        public void OnMessage(object sender, MessageEventArgs e)
        {
            DialogCoordinator?.ShowMessageAsync(this, e.Title, e.Message, MessageDialogStyle.Affirmative);
        }
    }
}
