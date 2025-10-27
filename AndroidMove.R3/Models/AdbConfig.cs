using System.Diagnostics;
using System.Text.Json.Serialization;
using AndroidMove.R3.Extensions;

namespace AndroidMove.R3.Models
{
    public class AdbConfig
    {

        [JsonPropertyName("adb_path")]
        public string? AdbPath { get; set; }

        [JsonPropertyName("interval_seconds")]
        public int IntervalSeconds { get; set; }
        [JsonIgnore]
        public string RootDirectory => "/sdcard/";
        [JsonIgnore]
        public string ScreenshotDirectoryName => "AndroidMove";
        [JsonIgnore]
        public string ScreenshotDirectory => $"{Extension.CombinePath(this.RootDirectory, this.ScreenshotDirectoryName)}";

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(AdbPath);
        }

        public ProcessStartInfo GetListFilesCommand(AndroidDevice device)
        {
            var conf=App.GetService<AppConfig>()!;
            return new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = $"-s {device.Serial} shell find {conf.AdbConfig.ScreenshotDirectory} -type f",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        public ProcessStartInfo GetTimestampCommand(AndroidDevice device,string path)
        {
            var conf = App.GetService<AppConfig>()!;
            return new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = $"-s {device.Serial} shell stat -c \"%y\" {path}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }


        public ProcessStartInfo DirectoryExistsCommand(AndroidDevice device)
        {
            var conf = App.GetService<AppConfig>()!;
            return new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = $"-s {device.Serial} shell ls {conf.AdbConfig.ScreenshotDirectory}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        public ProcessStartInfo CreateScreenshotDirectoryCommand(AndroidDevice device)
        {
            var conf= App.GetService<AppConfig>()!;
            return new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = $"-s {device.Serial} shell mkdir -p {conf.AdbConfig.ScreenshotDirectory}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }
        public ProcessStartInfo GetScreenshotCommand(AndroidDevice device,out string path)
        {
            var conf = App.GetService<AppConfig>()!;
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".png"; 
            path=Extension.CombinePath(conf.AdbConfig.ScreenshotDirectory,fileName);
            return new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = $"-s {device.Serial} shell screencap -p {path}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        [JsonIgnore]
        public string ListDevicesCommandArgument => "devices -l";

        public ProcessStartInfo GetListDevicesCommand()
        {
            return new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = $"{this.ListDevicesCommandArgument}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        public ProcessStartInfo GetPullCommand(AndroidDevice device, string src,out string destPath)
        {
            return GetPullCommand(device, src, device.LocalDirectory,out destPath);
        }

        public ProcessStartInfo GetPullCommand(AndroidDevice device, string src, string dest,out string destPath)
        {
            destPath = System.IO.Path.Combine(device.LocalDirectory, src.Split('/').Last());
            return new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = $"-s {device.Serial} pull \"{src}\" \"{destPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }
    }
}
