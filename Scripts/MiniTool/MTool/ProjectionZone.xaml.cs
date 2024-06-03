using CCTool.Scripts.Manager;
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

namespace CCTool.Scripts.MiniTool.MTool
{
    /// <summary>
    /// Interaction logic for ProjectionZone.xaml
    /// </summary>
    public partial class ProjectionZone : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ProjectionZone()
        {
            InitializeComponent();
        }

        private void txt_lng_Changed(object sender, TextChangedEventArgs e)
        {
            
            try
            {
                // 获取汉字字符
                double lng = double.Parse(txt_lng.Text);

                if (lng !=0)
                {
                    // 3度带
                    zone3.Text = Math.Floor((lng+1.5)/3).ToString();
                    zone3_center.Text = (Math.Floor((lng + 1.5) / 3) * 3).ToString();
                    // 6度带
                    zone6.Text = Math.Floor((lng + 6) / 6).ToString();
                    zone6_center.Text = (Math.Floor((lng + 6) / 6) * 6-3).ToString();
                }
                
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
            }
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/137135921";
            UITool.Link2Web(url);
        }
    }
}
