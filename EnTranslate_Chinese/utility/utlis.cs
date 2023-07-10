using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EnTranslate_Chinese.utility
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
