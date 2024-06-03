using ArcGIS.Desktop.Framework.Threading.Tasks;
using Aspose.Cells.Drawing;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace CCTool.Scripts.DataPross.SHP
{
    /// <summary>
    /// Interaction logic for Splite2SHP.xaml
    /// </summary>
    public partial class Splite2SHP : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public Splite2SHP()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "按属性分割成SHP";

        private void openForderButton_Click(object sender, RoutedEventArgs e)
        {
            folderPath.Text = UITool.OpenDialogFolder();
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            string fc = combox_fc.ComboxText();
            UITool.AddFieldsToComboxPlus(fc, combox_field);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string folder_path = folderPath.Text;
                string fc = combox_fc.ComboxText();
                string field = combox_field.ComboxText();
                bool isHaveFolder = (bool)check_folder.IsChecked;

                // 判断参数是否选择完全
                if (folder_path == "" || fc == "" || field == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();
                // 异步执行
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, "按属性分割");
                    // 按属性分割
                    Arcpy.SplitByAttributes(fc, folder_path, field);

                    // 如果要放在单独文件夹下
                    if (isHaveFolder)
                    {
                        pw.AddProcessMessage(40, time_base, "给每个shp文件创建一个单独的文件夹");
                        // 获取所有shp文件
                        List<string> files = folder_path.GetAllFiles(".shp");
                        foreach (string file in files)
                        {
                            // 获取名称和路径
                            string short_path = file.Replace(folder_path + @"\", "");
                            int index1 = short_path.LastIndexOf(@"\");      // 最后一个【"\"】的位置
                            int index2 = short_path.LastIndexOf(@".shp");  // 最后一个【".shp"】的位置
                            string name = short_path.Substring(index1 + 1, index2 - index1 - 1);
                            // 创建文件夹
                            Directory.CreateDirectory($@"{folder_path}\{name}");
                            // 移动shp文件
                            Arcpy.CopyFeatures(file, $@"{folder_path}\{name}\{short_path}");
                            Arcpy.Delect(file);
                            
                        }

                    }

                    pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
                });

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/138462781";
            UITool.Link2Web(url);
        }
    }
}
