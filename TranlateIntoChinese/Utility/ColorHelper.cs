using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace TranlateIntoChinese.Utility
{
    public static class ColorHelper
    {
        public static SolidColorBrush Primary = StringGetColor("#33CEA2");
        public static SolidColorBrush Second = StringGetColor("#29AB87");
        public static SolidColorBrush Text = StringGetColor("#f5f6fa");
        public static SolidColorBrush Background = StringGetColor("#353b48");

        public static void LightColor()
        {
            Background = StringGetColor("#ffffff");
            Text = StringGetColor("#2f3542");
            Primary = StringGetColor("#2ed573");
            Second = StringGetColor("#7bed9f");
        }
        /// <summary>
        /// 使用16进制颜色代码获取画笔
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static SolidColorBrush StringGetColor(string color)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
    }
}
