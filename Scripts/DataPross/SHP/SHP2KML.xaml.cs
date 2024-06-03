using ArcGIS.Desktop.Framework.Threading.Tasks;
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
    /// Interaction logic for SHP2KML.xaml
    /// </summary>
    public partial class SHP2KML : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SHP2KML()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "要素按编号转KMZ";

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
                string shot_field = combox_field.ComboxText();

                // 判断参数是否选择完全
                if (folder_path == "" || fc == "" || shot_field == "")
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
                    // 创建一个空的GDB数据库，按属性分割
                    Arcpy.CreateFileGDB(folder_path, "newGDB");
                    string gdbPath = folder_path + @"\newGDB.gdb";
                    Arcpy.SplitByAttributes(fc, gdbPath, shot_field);

                    // 创建图层，转换成kml
                    pw.AddProcessMessage(40, time_base, "创建图层");
                    var fcs = gdbPath.GetFeatureClassPathFromGDB();

                    foreach (string fc in fcs)
                    {
                        string outLayer = fc[(fc.LastIndexOf(@"\") + 1)..];

                        pw.AddProcessMessage(5, time_base, $"处理{outLayer}");
                        // 创建图层
                        Arcpy.MakeFeatureLayer(fc, outLayer, true);
                        // 转换成KML
                        Arcpy.LayerToKML(outLayer, $@"{folder_path}\{outLayer}.kmz");
                        // 移除图层
                        MapCtlTool.RemoveLayer(outLayer);
                    }
                    pw.AddProcessMessage(10, time_base, "删除中间数据库");

                    try
                    {
                        Directory.Delete(gdbPath, true);
                    }
                    catch (Exception)
                    {

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
            string url = "https://blog.csdn.net/xcc34452366/article/details/135840446?spm=1001.2014.3001.5501";
            UITool.Link2Web(url);
        }
    }
}
