using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AndroidMove.R3.Extensions
{
    public static class Extension
    {
        public static void ToClipboard(this string imagePath, Orientation orientation,int pixelSize)
        {
            var img = new BitmapImage(new Uri(imagePath));
            var hoge = new BitmapImage();
            hoge.BeginInit();
            hoge.UriSource = new Uri(imagePath);

            hoge.DecodePixelHeight = GetDecodePixelHeight(img, orientation, pixelSize);
            hoge.DecodePixelWidth =GetDecodePixelWidth(img, orientation, pixelSize);
            hoge.EndInit();
            Clipboard.SetImage(hoge);
        }
        private static int GetDecodePixelWidth(this BitmapImage img, Orientation orientation, int pixelSize)
        {
            var ratio = (double)pixelSize / GetBasePixelSize(img, orientation);
            return orientation == Orientation.Horizontal ? pixelSize : (int)System.Math.Round(img.PixelWidth * ratio);
        }
        private static int GetDecodePixelHeight(this BitmapImage img, Orientation orientation, int pixelSize)
        {
            var ratio = (double)pixelSize / GetBasePixelSize(img, orientation);
            return orientation == Orientation.Vertical ? pixelSize : (int)System.Math.Round(img.PixelHeight * ratio);
        }
        private static double GetBasePixelSize(BitmapImage img,Orientation orientation)
        {
            if(orientation== Orientation.Horizontal)
            {
                return (double)img.PixelWidth;
            }
            else
            {
                return (double)img.PixelHeight;
            }
        }
        public static string CombinePath(this string path1, string path2)
        {
            if (path1.EndsWith("/"))
            {
                return $"{path1}{path2}";
            }
            else
            {
                return $"{path1}/{path2}";
            }
        }

        public static string? GetVersion(this Assembly asm)
        {
            var version = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (version != null)
            {
                int plusIndex = version.IndexOf('+');
                if (plusIndex != -1)
                {
                    return version.Substring(0, plusIndex);
                }
            }

            return version;
        }

        public static string? ToHtmlColor(this Color? value)
        {
            if (value is null)
            {
                return null;
            }

            static string lowerHexString(int i) => i.ToString("X2").ToLower();
            var hex = lowerHexString(value.Value.R) +
                      lowerHexString(value.Value.G) +
                      lowerHexString(value.Value.B);
            return "#" + hex;
        }

        public static Color? ToColor(this string code)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(code);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
