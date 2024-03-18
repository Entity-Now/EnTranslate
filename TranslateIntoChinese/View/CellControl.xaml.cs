using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TranslateIntoChinese.View
{
    /// <summary>
    /// CellControl.xaml 的交互逻辑
    /// </summary>
    public partial class CellControl : UserControl
    {
        public string O_Title { get; set; }
        public string O_Content { get; set; }
        public object O_Handle { get; set; }
        public CellControl()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            v_Title.Text = O_Title;
            v_Content.Text = O_Content;
            v_Handle.Content = O_Handle;
        }
    }
}
