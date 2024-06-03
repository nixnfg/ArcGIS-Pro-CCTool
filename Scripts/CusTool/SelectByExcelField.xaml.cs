using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using Microsoft.Office.Core;
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

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for SelectByExcelField.xaml
    /// </summary>
    public partial class SelectByExcelField : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SelectByExcelField()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "根据Excel特定值提取要素";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            textExcelPath.Text = UITool.OpenTable();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string in_fc = combox_fc.ComboxText();
                string in_field = combox_field.ComboxText();

                string map_tabel = textExcelPath.Text;
                string excel_field = combox_excelField.ComboxText();

                string out_fc = textFCPath.Text;

                // 判断参数是否选择完全
                if (map_tabel == "" || in_fc == "" || in_field == "" || in_field == "" || out_fc == "")
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
                    string defGDB = Project.Current.DefaultGeodatabasePath;
                    string fc_copy = $@"{defGDB}\fc_copy";
                    pw.AddProcessMessage(10, time_base, "复制数据");
                    Arcpy.CopyFeatures(in_fc, fc_copy);

                    pw.AddProcessMessage(10, time_base, "获取excel表中的值列表");
                    // 获取excel表中的值列表
                    List<string> fields = OfficeTool.GetColValueFromExcel(map_tabel, excel_field);

                    pw.AddProcessMessage(10, time_base, "选择符号要求的行并标记");
                    // 新建一个标记字段
                    string bz = "标记";
                    Arcpy.AddField(fc_copy, bz, "TEXT");
                    // 选择符号要求的行并标记
                    FeatureClass featureClass = fc_copy.TargetFeatureClass();
                    using RowCursor cursor = featureClass.Search();
                    // 遍历源图层的要素
                    while (cursor.MoveNext())
                    {
                        var row = cursor.Current;
                        var in_va = row[in_field];
                        if (in_va != null && fields.Contains(in_va))
                        {
                            row[bz] = "目标";
                            row.Store();
                        }
                    }

                    // 生成选中结果
                    string sql = "标记 = '目标'";
                    Arcpy.Select(fc_copy, out_fc, sql);

                    pw.AddProcessMessage(50, time_base, "删除中间数据");
                    Arcpy.DeleteField(out_fc, bz);
                    Arcpy.Delect(fc_copy);
                    // 加载
                    GisTool.AddLayer(out_fc);

                });

                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }


        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_field);
        }

        private void openFCButton_Click(object sender, RoutedEventArgs e)
        {
            textFCPath.Text = UITool.SaveDialogFeatureClass();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/139060536";
            UITool.Link2Web(url);
        }

        private void combox_excelField_DropDown(object sender, EventArgs e)
        {
            UITool.AddExcelFieldsToComboxPlus(textExcelPath.Text, combox_excelField);
        }
    }
}
