using System.Text.Json.Serialization;
using System.Windows.Media;
using AndroidMove.R3.Models;

namespace AndroidMove.R3.Services
{
    public class ColorConfig
    {
        [JsonPropertyName("scheme")]
        public ColorScheme Scheme { get; set; }
        [JsonPropertyName("color")]
        public Color? Color { get; set; }
    }
}
