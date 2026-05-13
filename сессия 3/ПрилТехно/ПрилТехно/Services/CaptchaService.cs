using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;

namespace ПрилТехно.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly Random _random = new Random();

        public (ImageSource CaptchaImage, string ExpectedAnswer) GenerateCaptcha()
        {
            int a = _random.Next(10, 99);
            int b = _random.Next(10, 99);
            string expression = $"{a} + {b} = ?";
            string answer = (a + b).ToString();

            using (var bitmap = new Bitmap(200, 60))
            using (var g = Graphics.FromImage(bitmap))
            {
                // Явно указываем System.Drawing.Color
                g.Clear(System.Drawing.Color.LightGray);
                using (var font = new Font("Arial", 16, FontStyle.Bold))
                {
                    // Явно указываем System.Drawing.Brushes
                    g.DrawString(expression, font, System.Drawing.Brushes.Black, 10, 20);
                }
                // Шум
                for (int i = 0; i < 30; i++)
                {
                    int x = _random.Next(bitmap.Width);
                    int y = _random.Next(bitmap.Height);
                    bitmap.SetPixel(x, y, System.Drawing.Color.Gray);
                }

                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    return (image, answer);
                }
            }
        }
    }
}