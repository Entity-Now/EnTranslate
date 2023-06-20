using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace vsix_EnTranslate.utility
{
    public static class utlis
    {
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
