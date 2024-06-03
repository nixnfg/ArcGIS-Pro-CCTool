using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for ClipImage.xaml
    /// </summary>
    public partial class ClipImage : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ClipImage()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "批量裁剪影像";

        private void openImageFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textImagePath.Text = UITool.OpenDialogFolder();
        }

        private void openOutputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textOutputPath.Text = UITool.OpenDialogFolder();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string clipFc = combox_fc.ComboxText();
                string splitField = combox_field.ComboxText();
                string imagePath = textImagePath.Text;
                string outputPath = textOutputPath.Text;

                bool isArea = (bool)rb_area.IsChecked;
                bool isImage = (bool)rb_image.IsChecked;
                bool isAreaImage = (bool)rb_area_image.IsChecked;

                bool isCheckField = (bool)check_selectField.IsChecked;
                // 输出名称
                string outType = "";
                if (isArea) { outType = "area"; }
                else if (isImage) { outType = "image"; }
                else if (isAreaImage) { outType = "area_image"; }

                // 判断参数是否选择完全
                if (outputPath == "" || clipFc == "" || imagePath == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);

                Close();

                await QueuedTask.Run(() =>
                {

                    // 放到新建的数据库中
                    string filePath = @"D:\Program Files\临时文件";
                    string gdbName = "裁剪数据库";
                    Arcpy.CreateFileGDB(filePath, gdbName);
                    string new_gdb = $@"{filePath}\{gdbName}.gdb";
                    // 清理一下
                    new_gdb.ClearGDBItem();

                    // 先看看是否要按字段分地块
                    if (isCheckField)
                    {
                        Arcpy.SplitByAttributes(clipFc, new_gdb, splitField);
                    }
                    else
                    {
                        Arcpy.CopyFeatures(clipFc, $@"{new_gdb}\{clipFc}");
                    }

                    // 收集分割后的地块
                    List<string> list_area = new_gdb.GetFeatureClassAndTablePath();

                    // 获取所有shp文件
                    var files = imagePath.GetAllFiles(".tif");

                    foreach (var area in list_area)
                    {
                        string areaName = area[(area.LastIndexOf(".gdb") + 5)..];
                        foreach (var file in files)
                        {
                            string fileName = file[(file.LastIndexOf(@"\") + 1)..].Replace(".tif", "").Replace(".TIF", "");
                            string fullName = $"{areaName}_{fileName}".Replace(".tif", "").Replace(".TIF", "");
                            
                            // 裁剪栅格
                            pw.AddProcessMessage(10, time_base, fullName, Brushes.Gray);
                            Arcpy.RasterClip(file, @$"{outputPath}\{fullName}.tif", area);

                            // 命名修改
                            string na = outType switch
                            {
                                "area" => areaName,
                                "image" => fileName,
                                "area_image" => $"{areaName}_{fileName}",
                                _ => $"{areaName}_{fileName}",
                            };

                            var tifFiles = outputPath.GetAllFiles();
                            foreach (string tf in tifFiles)
                            {
                                if (tf.Contains(fullName))
                                {
                                    File.Move(tf, tf.Replace(fullName, na));
                                }
                            }
                        }
                    }


                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/139039298";
            UITool.Link2Web(url);
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_field);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }
    }
}
