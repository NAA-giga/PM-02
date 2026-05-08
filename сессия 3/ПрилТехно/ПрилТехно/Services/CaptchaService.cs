using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace ПрилТехно.Services
{
    [SupportedOSPlatform("windows")]
    public static class CaptchaService
    {
        private static readonly Random _rand = Random.Shared;

        public static (string Text, byte[] ImageBytes) GenerateCaptcha()
        {
            var text = _rand.Next(1000, 9999).ToString();
            using var bmp = new Bitmap(150, 50);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            using var font = new Font("Arial", 18, FontStyle.Bold);
            g.DrawString(text, font, Brushes.Blue, 10, 10);
            for (int i = 0; i < 50; i++)
                g.DrawLine(Pens.Gray, _rand.Next(bmp.Width), _rand.Next(bmp.Height),
                                          _rand.Next(bmp.Width), _rand.Next(bmp.Height));

            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return (text, ms.ToArray());
        }
    }

}
