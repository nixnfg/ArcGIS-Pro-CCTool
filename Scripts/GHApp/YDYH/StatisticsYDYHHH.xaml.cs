using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
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

namespace CCTool.Scripts.GHApp.YDYH
{
    /// <summary>
    /// Interaction logic for StatisticsYDYHHH.xaml
    /// </summary>
    public partial class StatisticsYDYHHH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public StatisticsYDYHHH()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "用地用海指标汇总(含混合用地)";

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
                string field_mj = combox_mjField.ComboxText();

                string excel_path = textExcelPath.Text;
                bool isBymj = (bool)checkBox_mj.IsChecked;

                double totalMJ = double.Parse(textMJ.Text);

                // 判断参数是否选择完全
                if (fc_path == "" || field_bm == "" || excel_path == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                Close();
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, "复制Excel表格");

                    /// 汇总用地
                    // 复制嵌入资源中的Excel文件
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【混合用地】用地汇总表-新国空代码板式.xlsx", excel_path);
                    string excel_sheet = excel_path + @"\用地用海汇总表$";

                    pw.AddProcessMessage(10, time_base, "初步汇总");

                    // 面积取值
                    string mjField = "shape_area";
                    if (isBymj)
                    {
                        mjField = field_mj;
                    }

                    // 汇总指标
                    List<string> bm1 = new List<string>() { field_bm};
                    string totalField = "合计";
                    Dictionary<string, double> dict = GisTool.MultiStatisticsToDic(fc_path, mjField, bm1, totalField, 10000);

                    pw.AddProcessMessage(20, time_base, "指标分割");
                    // 指标分割
                    Dictionary<string, double> dict2 = GisTool.DecomposeSummary(dict);

                    pw.AddProcessMessage(10, time_base, "统计道路/建设用地/其他用地/总用地");

                    // 统计道路用地
                    dict2.Accumulation("1207", totalMJ - dict2[totalField]);
                    dict2.Accumulation("12", totalMJ - dict2[totalField]);
                    // 统计总面积
                    dict2.Accumulation("总用地面积", totalMJ);

                    // 统计建设用地\其他用地
                    List<string> jsydList = new List<string>() {"07", "08", "HH", "09", "10", "11", "12", "13", "14", "15", "16" };
                    List<string> qtydList = new List<string>() { "01", "02", "03", "04", "05", "06", "17", "18", "19", "20", "21", "22", "23", "24" };

                    Dictionary<string, List<string>> pairs = new Dictionary<string, List<string>>
                    {
                        { "建设用地小计", jsydList }, { "其他用地小计", qtydList }
                    };
                    // 局部汇总
                    Dictionary<string, double> result = GisTool.PartSummary(dict2, pairs);

                    pw.AddProcessMessage(10, time_base, "计算面积占比");
                    // 计算面积占比
                    Dictionary<string, double> dic_pes= new Dictionary<string, double>();
                    List<string> bmOnes= new List<string>() { "07", "08", "HH", "09", "10", "11", "12", "13", "14", "15", "16", "建设用地小计" };
                    double jsmj = result["建设用地小计"];
                    foreach (var bm in bmOnes)
                    {
                        if (result.ContainsKey(bm))
                        {
                            dic_pes.Add(bm, result[bm] / jsmj);
                        }
                    }

                    pw.AddProcessMessage(10, time_base, "指标写入Excel");
                    // 属性映射
                    OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 8, 5, result, 1);
                    // 属性映射-百分比
                    OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 8, 6, dic_pes, 1);
                    // 删除0值行
                    OfficeTool.ExcelDeleteNullRow(excel_sheet, 5, 1);

                    // 删除参照列
                    OfficeTool.ExcelDeleteCol(excel_sheet, 8);
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
            string url = "";
            UITool.Link2Web(url);
        }

        private void combox_mjField_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_mjField);
        }
    }
}
