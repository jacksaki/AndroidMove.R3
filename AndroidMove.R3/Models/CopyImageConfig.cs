using System.Text.Json.Serialization;
using System.Windows.Controls;

namespace AndroidMove.R3.Models
{
    public class CopyImageConfig
    {
        [JsonPropertyName("pixel_size")]
        public int PixelSize { get; set; } = 400;

        [JsonPropertyName("orientation")]
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        [JsonPropertyName("with_clipboard")]
        public bool WithClipboard { get; set; } = true;
    }
}
