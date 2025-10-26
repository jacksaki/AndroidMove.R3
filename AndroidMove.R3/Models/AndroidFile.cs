using AndroidMove.R3.Extensions;
using Cysharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidMove.R3.Models
{
    public class AndroidFile
    {
        public AndroidDevice Device { get; }
        public string Path { get; }
        public string FileName => this.Path.Split('/').Last();
        public DateTime LastUpdateTime { get; }

        private AndroidFile(AndroidDevice device,string path, DateTime lastUpdateTime)
        {
            Device = device;
            Path = path;
            LastUpdateTime = lastUpdateTime;
        }

        private static async Task<DateTime>GetDateTimeCommand(AndroidDevice device,string path)
        {
            var conf = App.GetService<AppConfig>()!;
            var s =await ProcessX.StartAsync(conf.AdbConfig.GetTimestampCommand(device, path)).FirstOrDefaultAsync();
            return s.ToDateTime() ?? DateTime.Now;
        }

        internal static async Task<AndroidFile?> CreateFromLineAsync(AndroidDevice device, string line)
        {
            if(string.IsNullOrWhiteSpace(line))
            {
                return null!;
            }
            var path = line.Trim();
            var d= await GetDateTimeCommand(device, path);
            return new AndroidFile(device, path, d);
        }
    }
}
