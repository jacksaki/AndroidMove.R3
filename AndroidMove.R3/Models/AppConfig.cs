using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Windows.Controls;
using AndroidMove.R3.Services;

namespace AndroidMove.R3.Models
{
    public class AppConfig
    {
        public static string Path => System.IO.Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, ".conf");

        [JsonPropertyName("adb_config")]
        [JsonInclude]
        public AdbConfig AdbConfig { get; private set; } = new AdbConfig();

        [JsonPropertyName("copy_image_config")]
        [JsonInclude]
        public CopyImageConfig CopyImageConfig { get; private set; } = new CopyImageConfig();

        [JsonPropertyName("devices")]
        [JsonInclude]
        public List<DeviceConfig> Devices { get; private set; } = new List<DeviceConfig>();

        [JsonPropertyName("theme")]
        [JsonInclude]
        public ThemeConfig? Theme { get; private set; }

        public void Reload()
        {
            if (!System.IO.File.Exists(Path))
            {
                return;
            }

            var json = System.IO.File.ReadAllText(Path);
            var conf = JsonSerializer.Deserialize<AppConfig>(json)!;
            this.AdbConfig = conf.AdbConfig;
            this.CopyImageConfig = conf.CopyImageConfig;
        }

        public void SetTheme(ThemeConfig conf)
        {
            this.Theme = conf;
        }

        public void Save()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            };
            options.MakeReadOnly();
            System.IO.File.WriteAllText(Path, JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
        }

        public static AppConfig Load()
        {
            if (!System.IO.File.Exists(Path))
            {
                return new AppConfig();
            }
            var json = System.IO.File.ReadAllText(Path);
            var conf = JsonSerializer.Deserialize<AppConfig>(json)!;
            if (conf.Theme == null)
            {
                conf.Theme = new ThemeConfig()
                {
                    IsDarkTheme = false,
                };
            }
            return conf;
        }

        internal DeviceConfig? GetDeviceConfig(AndroidDevice device)
        {
            var conf = this.Devices.Where(x=>x.Serial == device.Serial).FirstOrDefault();
            if(conf == null)
            {
                var newConf = new DeviceConfig()
                {
                    Model = device.Model,
                    Serial = device.Serial,
                };
                this.Devices.Add(newConf);
                return newConf;
            }
            else
            {
                return conf;
            }
        }
    }
}
