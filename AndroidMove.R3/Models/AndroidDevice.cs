using Cysharp.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace AndroidMove.R3.Models
{
    public class AndroidDevice
    {
        private AndroidDevice(string serial, string model)
        {
            Model = model;
            Serial = serial;
        }

        private static async Task<AndroidDevice>CreateAsync(string serial,string model)
        {
            var device = new AndroidDevice(serial, model);
            await device.InitializeAsync();
            return device;
        }
        public DeviceConfig? Config { get; private set; }
        private async Task InitializeAsync()
        {
            var conf = App.GetService<AppConfig>()!;
            try
            {
                if(!Directory.Exists(this.LocalDirectory))
                {
                    Directory.CreateDirectory(this.LocalDirectory);
                }
                await ProcessX.StartAsync(conf.AdbConfig.CreateScreenshotDirectoryCommand(this)).ToTask();
            }
            catch (Exception)
            {
            }

            var devConf = conf.GetDeviceConfig(this);
            if (devConf == null)
            {
                conf.Devices.Add(devConf!);
            }

            this.Config = devConf!;
        }

        public string Model { get; }
        public string Serial { get; }

        public string LocalDirectory => System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
            "Screenshots",
            this.Serial.Replace(':', '_'));

        public async Task<string> CaptureAsync()
        {
            var conf = App.GetService<AppConfig>()!.AdbConfig;
            await ProcessX.StartAsync(conf.GetScreenshotCommand(this, out var path)).ToTask();
            return path;
        }
        public async Task<string> PullAsync(string path)
        {
            var conf = App.GetService<AppConfig>()!.AdbConfig;
            await ProcessX.StartAsync(conf.GetPullCommand(this,path,this.LocalDirectory, out var localPath)).ToTask();
            return localPath;
        }

        public static async Task<List<AndroidDevice>> ListDevicesAsync()
        {
            var result = new List<AndroidDevice>();
            await foreach(var item in EnumerateDevicesAsync())
            {
                result.Add(item);
            }

            return result;
        }

        private static ProcessAsyncEnumerable ListDevicesCommandAsync()
        {
            var conf = App.GetService<AppConfig>()!.AdbConfig;
            return ProcessX.StartAsync(conf.GetListDevicesCommand());
        }
        public static async IAsyncEnumerable<AndroidDevice> EnumerateDevicesAsync()
        {
            await foreach (var line in ListDevicesCommandAsync())
            {
                var device = await AndroidDevice.CreateFromLineAsync(line);
                if (device != null)
                {
                    yield return device;
                }
            }
        }

        private static async Task<AndroidDevice?> CreateFromLineAsync(string line)
        {
            var conf = App.GetService<AppConfig>()!;
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("List of devices"))
            {
                return null;
            }

            var reg = new Regex(@"^(?<serial>\S+)\s+device product:(?<product>\S+)?(\s+model:(?<model>\S+))?(\s+device:(?<device>))", RegexOptions.Compiled);
            if(reg.IsMatch(line))
            {
                var match = reg.Match(line);
                var serial = match.Groups["serial"].Value;
                var model = match.Groups["model"].Success ? match.Groups["model"].Value : "UnknownModel";
                return await AndroidDevice.CreateAsync(serial, model);
            }
            else
            {
                return null;
            }
        }
    }
}
