using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using EnTranslate.Model;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace vsix_EnTranslate.utility
{
    public static class MakeInfoItem
    {
        public static UIElement TranslateInfo(this ITrackingSpan applicationSpan, Dictionarys val)
        {
            // 创建 StackPanel，并设置布局方式为垂直
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;

            // 创建标题文本块
            var title = new TextBlock();
            var information = new Run
            {
                Text = "翻译：",
                FontWeight = FontWeights.Bold,
                Foreground = utility.utlis.StringGetColor("#33CEA2")
            };
            var Key = new Run
            {
                Text = val.key,
                Foreground = utility.utlis.StringGetColor("#29AB87")
            };
            title.Inlines.Add(information);
            title.Inlines.Add(Key);
            // 判断音标是否为空
            if (!string.IsNullOrEmpty(val.p))
            {
                var annotate = new Run()
                {
                    Text = $"   音标注解：",
                    Foreground = utility.utlis.StringGetColor("#33CEA2")
                };
                var gloss = new Run()
                {
                    Text = val.p,
                    Foreground = utility.utlis.StringGetColor("#29AB87")
                };
                title.Inlines.Add(annotate);
                title.Inlines.Add(gloss);
            }
            stackPanel.Children.Add(title);

            // 创建内容文本块
            var SplitWord = val.t.Replace(@"\n", "@@$$$@@").Split(new string[] { "@@$$$@@" }, StringSplitOptions.None);
            foreach (var item in SplitWord)
            {
                var contentTextBlock = new TextBlock();
                contentTextBlock.Text = item;
                contentTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8e8e8"));
                contentTextBlock.Margin = new Thickness(10, 0 , 0, 0);
                stackPanel.Children.Add(contentTextBlock);
            }

            return stackPanel;
        }
    }
}
