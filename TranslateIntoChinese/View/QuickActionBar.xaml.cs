using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TranslateIntoChinese.Model;
using TranslateIntoChinese.Utility;

namespace TranslateIntoChinese.View
{
    public partial class QuickActionBar : UserControl
    {
        public event EventHandler<QuickActionItem> ActionRequested;

        public QuickActionBar()
        {
            InitializeComponent();
        }

        public void SetActions(IList<QuickActionItem> actions)
        {
            ButtonsPanel.Children.Clear();
            if (actions == null) return;

            foreach (var item in actions)
            {
                if (item == null || !item.Enabled) continue;
                var button = new Button
                {
                    Content = item.Title,
                    Tag = item,
                    Margin = new Thickness(0, 0, 4, 2),
                    Padding = new Thickness(8, 3, 8, 3),
                    FontSize = 11,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Background = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x40)),
                    Foreground = ColorHelper.Text,
                    BorderThickness = new Thickness(0),
                    ToolTip = item.IsBuiltIn ? "内置指令" : "自定义指令"
                };
                button.Click += Button_Click;
                ButtonsPanel.Children.Add(button);
            }
        }

        public void SetBusy(bool busy, string message = null)
        {
            BusyText.Text = string.IsNullOrWhiteSpace(message) ? "AI 处理中…" : message;
            BusyText.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
            ButtonsPanel.IsEnabled = !busy;
            ButtonsPanel.Opacity = busy ? 0.45 : 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is QuickActionItem item)
                ActionRequested?.Invoke(this, item);
        }
    }
}
