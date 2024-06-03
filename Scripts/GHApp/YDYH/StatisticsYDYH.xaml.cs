using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Vml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;

namespace CCTool.Scripts
{
    /// <summary>
    /// Interaction logic for StatisticsYDYH.xaml
    /// </summary>
    public partial class StatisticsYDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public StatisticsYDYH()
        {
            InitializeComponent();
            Init();       // 初始化
        }
        // 初始化
        private void Init()
        {
            // combox_model框中添加3种转换模式，默认【大类】
            combox_model.Items.Add("大类");
            combox_model.Items.Add("中类");
            combox_model.Items.Add("小类");
            combox_model.SelectedIndex = 0;

            combox_version.Items.Add("旧版");
            combox_version.Items.Add("新版");
            combox_version.SelectedIndex = 0;

            combox_unit.Items.Add("平方米");
            combox_unit.Items.Add("公顷");
            combox_unit.Items.Add("平方公里");
            combox_unit.Items.Add("亩");
            combox_unit.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "用地用海指标汇总";

        // 点击打开按钮，选择输出的Excel文件位置
        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开Excel文件
            string path = UITool.SaveDialogExcel();
            // 将Excel文件的路径置入【textExcelPath】
            textExcelPath.Text = path;
        }

        // 添加要素图层的所有字符串字段到combox中
        private void combox_field_DropDown(object sender, EventArgs e)
        {
            // 将图层字段加入到Combox列表中
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_bmField);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        // 运行
        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取默认数据库
                var init_gdb = Project.Current.DefaultGeodatabasePath;
                // 获取参数
                string fc_path = combox_fc.ComboxText();
                string field_bm = combox_bmField.ComboxText();
                string output_table = init_gdb + @"\output_table";
                string excel_path = textExcelPath.Text;

                string unit = combox_unit.Text;

                string version = combox_version.Text;

                string areaField = combox_areaField.ComboxText();

                List<string> bmList = new List<string>() { field_bm };

                // 判断参数是否选择完全
                if (fc_path == "" || field_bm == "" || output_table == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);

                // 模式转换
                int model = 1;
                if (combox_model.Text == "中类") { model = 2; }
                else if (combox_model.Text == "小类") { model = 3; }

                // 单位系数设置
                double unit_xs = unit switch
                {
                    "平方米" => 1,
                    "公顷" => 10000,
                    "平方公里" => 1000000,
                    "亩" => 666.66667,
                    _ => 1,
                };

                Close();
                await QueuedTask.Run(() =>
                {

                     // 汇总用地
                    if (model == 1)         // 大类
                    {
                        // 复制嵌入资源中的Excel文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】用地用海_大类.xlsx", excel_path);
                        string excel_sheet = excel_path + @"\用地用海$";
                        pw.AddProcessMessage(20, time_base, "汇总大类指标");
                        // 汇总大、中、小类
                        Dictionary<string, double> dic = GisTool.MultiStatisticsToDic(fc_path, areaField, bmList, "合计", unit_xs);

                        pw.AddProcessMessage(10, time_base, "分割指标");
                        // 指标分割
                        Dictionary<string, double> dict = GisTool.DecomposeSummary(dic);

                        pw.AddProcessMessage(20, time_base, "指标写入Excel");
                        // 属性映射大类
                        OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 1, 3, dict, 3);
                        // 删除0值行
                        OfficeTool.ExcelDeleteNullRow(excel_sheet, 3, 3);
                        // 改Excel中的单位
                        OfficeTool.ExcelWriteCell(excel_path, 2, 3, $"用地面积({unit})");
                    }
                    else if (model == 2)       // 中类
                    {
                        string excelName = "";
                        if (version == "旧版")
                        {
                            excelName = "【模板】用地用海_中类";
                        }
                        else
                        {
                            excelName = "【新模板】用地用海_中类";
                        }

                        // 复制嵌入资源中的Excel文件
                        BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", excel_path);
                        string excel_sheet = excel_path + @"\用地用海$";
                        pw.AddProcessMessage(20, time_base, "汇总大、中类指标");
                        // 汇总大、中、小类
                        Dictionary<string, double> dic = GisTool.MultiStatisticsToDic(fc_path, areaField, bmList, "合计", unit_xs);

                        pw.AddProcessMessage(10, time_base, "分割指标");
                        // 指标分割
                        Dictionary<string, double> dict = GisTool.DecomposeSummary(dic);

                        pw.AddProcessMessage(20, time_base, "指标写入Excel");
                        // 属性映射大类
                        OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 7, 4, dict, 4);
                        // 属性映射中类
                        OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 8, 4, dict, 4);
                        // 删除0值行
                        OfficeTool.ExcelDeleteNullRow(excel_sheet, 4, 4);
                        // 删除指定列
                        OfficeTool.ExcelDeleteCol(excel_path + @"\用地用海$", new List<int>() { 8, 7 });
                        // 改Excel中的单位
                        OfficeTool.ExcelWriteCell(excel_path, 2, 4, $"用地面积({unit})");
                    }
                    if (model == 3)       // 小类
                    {
                        string excelName = "";
                        if (version == "旧版")
                        {
                            excelName = "【模板】用地用海_小类";
                        }
                        else
                        {
                            excelName = "【新模板】用地用海_小类";
                        }

                        // 复制嵌入资源中的Excel文件
                        BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", excel_path);
                        string excel_sheet = excel_path + @"\用地用海$";
                        pw.AddProcessMessage(20, time_base, "汇总大、中、小类指标");
                        // 汇总大、中、小类
                        Dictionary<string, double> dic =  GisTool.MultiStatisticsToDic(fc_path, areaField, bmList, "合计", unit_xs);

                        pw.AddProcessMessage(10, time_base, "分割指标");
                        // 指标分割
                        Dictionary<string, double> dict = GisTool.DecomposeSummary(dic);

                        pw.AddProcessMessage(20, time_base, "指标写入Excel");
                        // 属性映射大类
                        OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 7, 5, dict, 4);
                        // 属性映射中类
                        OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 8, 5, dict, 4);
                        // 属性映射小类
                        OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 9, 5, dict, 4);
                        // 删除0值行
                        OfficeTool.ExcelDeleteNullRow(excel_sheet, 5, 4);
                        // 删除指定列
                        OfficeTool.ExcelDeleteCol(excel_sheet, new List<int>() { 9, 8, 7 });
                        // 改Excel中的单位
                        OfficeTool.ExcelWriteCell(excel_path, 2, 5, $"用地面积({unit})");
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

        private void combox_areaField_DropDown(object sender, EventArgs e)
        {
            string fc_path = combox_fc.ComboxText();
            UITool.AddAllFloatFieldsToComboxPlus(fc_path, combox_areaField);
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/135694013?spm=1001.2014.3001.5501";
            UITool.Link2Web(url);
        }
    }
}
