using EnTranslate.Model;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.PlatformUI;

namespace EnTranslate_Chinese.utility
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
                Foreground = utlis.StringGetColor("#33CEA2")
            };
            var Key = new Run
            {
                Text = val.key,
                Foreground = utlis.StringGetColor("#29AB87")
            };
            title.Inlines.Add(information);
            title.Inlines.Add(Key);
            // 判断音标是否为空
            if (!string.IsNullOrEmpty(val.p))
            {
                var annotate = new Run()
                {
                    Text = $"   音标注解：",
                    Foreground = utlis.StringGetColor("#33CEA2")
                };
                var gloss = new Run()
                {
                    Text = val.p,
                    Foreground = utlis.StringGetColor("#29AB87")
                };
                title.Inlines.Add(annotate);
                title.Inlines.Add(gloss);
            }
            stackPanel.Children.Add(title);

            // 创建内容文本块
            var SplitWord = val.t.Split(new string[] { @"\n" }, StringSplitOptions.None);
            foreach (var item in SplitWord)
            {
                var contentTextBlock = new TextBlock();
                contentTextBlock.Text = item;
                contentTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8e8e8"));
                contentTextBlock.Margin = new Thickness(10, 0, 0, 0);
                stackPanel.Children.Add(contentTextBlock);
            }
            // 使用 ScrollViewer 包裹 StackPanel
            return new ScrollViewer()
            {
                Content = stackPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            }; ;
        }
        /// <summary>
        /// 获取vs 主题颜色
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Drawing.Color GetThemeColor(ThemeResourceKey themeResourceKey)
        {
            // 获取字体和颜色中，文本编辑器中的高亮深色
            var color = VSColorTheme.GetThemedColor(themeResourceKey);
            return color;
        }
    }
}
