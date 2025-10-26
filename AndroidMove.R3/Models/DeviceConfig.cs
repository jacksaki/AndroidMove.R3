using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AndroidMove.R3.Models
{
    public class DeviceConfig
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;
        [JsonPropertyName("serial")]
        public string Serial { get; set; }= null!;
    }
}
