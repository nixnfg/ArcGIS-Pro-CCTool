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

namespace CCTool.Scripts.MessageWindow
{
    /// <summary>
    /// Interaction logic for XYInfoWindow.xaml
    /// </summary>
    public partial class XYInfoWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public XYInfoWindow()
        {
            InitializeComponent();
        }

        // 添加XY信息
        public void AddXYInfo(double x,double y)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                textBox_x.Text = x.ToString();
                textBox_y.Text = y.ToString();
            });
        }
    }
}
