using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace ПрилТехно.Services
{
    public interface ICaptchaService
    {
        (ImageSource CaptchaImage, string ExpectedAnswer) GenerateCaptcha();
    }
}
