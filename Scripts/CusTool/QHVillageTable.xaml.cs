using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Vml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for QHVillageTable.xaml
    /// </summary>
    public partial class QHVillageTable : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public QHVillageTable()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "青海省村规结构调整表";

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
                string init_gdb = Project.Current.DefaultGeodatabasePath;
                string init_foder = Project.Current.HomeFolderPath;

                // 获取参数
                string fc_path = combox_fc.ComboxText();
                string field_bm = combox_bmField.ComboxText();
                string areaField = combox_areaField.ComboxText();

                string fc_path_gh = combox_fc_gh.ComboxText();
                string field_bm_gh = combox_bmField_gh.ComboxText();
                string areaField_gh = combox_areaField_gh.ComboxText();

                string excel_path = textExcelPath.Text;

                string output_table = init_gdb + @"\output_table";

                string czcField = "CZCSXM";   //  城镇村属性码

                await QueuedTask.Run(() =>
                {
                    if (!GisTool.IsHaveFieldInTarget(fc_path, czcField))
                    {
                        MessageBox.Show("现状图层缺少【CZCSXM】字段！");
                    }
                    if (!GisTool.IsHaveFieldInTarget(fc_path_gh, czcField))
                    {
                        MessageBox.Show("规划图层缺少【CZCSXM】字段！");
                    }
                });

                // 判断参数是否选择完全
                if (fc_path == "" || field_bm == "" || areaField == "" || fc_path_gh == "" || field_bm_gh == "" || areaField_gh == "" || excel_path == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);

                // 单位系数设置
                double unit_xs = 10000;

                Close();
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, time_base, "复制Excel文件");
                    // 汇总用地
                    string excelName = "【青海】空间功能结构调整表";
                    string excelMapper = "【青海】用地用海代码_村庄功能";

                    // 复制嵌入资源中的Excel文件
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", excel_path);
                    string excel_sheet = excel_path + @"\Sheet1$";
                    // 复制映射表
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelMapper}.xlsx", @$"{init_foder}\{excelMapper}.xlsx");

                    pw.AddProcessMessage(10, time_base, "村庄功能映射");
                    // 初步汇总
                    // 调用GP工具【汇总】
                    Arcpy.Statistics(fc_path, output_table, $"{areaField} SUM", $"{czcField};{field_bm}");
                    Arcpy.Statistics(fc_path_gh, output_table + "_gh", $"{areaField_gh} SUM", $"{czcField};{field_bm_gh}");

                    // 添加村庄功能字段
                    string gnField = "vgGN";
                    Arcpy.AddField(output_table, gnField, "TEXT");
                    Arcpy.AddField(output_table + "_gh", gnField, "TEXT");

                    // 村庄功能映射
                    string mapper = @$"{init_foder}\{excelMapper}.xlsx\sheet1$";
                    GisTool.AttributeMapper(output_table, field_bm, gnField, mapper);
                    GisTool.AttributeMapper(output_table + "_gh", field_bm_gh, gnField, mapper);
                    // 区分202、203等属性
                    Segment(output_table, field_bm, gnField);
                    Segment(output_table + "_gh", field_bm_gh, gnField);

                    pw.AddProcessMessage(10, time_base, $"汇总指标", Brushes.Gray);
                    // 汇总指标
                    List<string> bmList = new List<string>() { gnField };
                    string total = "国土面积";
                    Dictionary<string, double> dic = GisTool.MultiStatisticsToDic(output_table, $"Sum_{areaField}", bmList, total, unit_xs);
                    Dictionary<string, double> dic_gh = GisTool.MultiStatisticsToDic(output_table + "_gh", $"Sum_{areaField}", bmList, total, unit_xs);

                    pw.AddProcessMessage(10, time_base, $"写入Excel", Brushes.Gray);
                    // 属性映射现状用地
                    OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 10, 4, dic, 5);
                    // 属性映射规划用地
                    OfficeTool.ExcelAttributeMapperDouble(excel_sheet, 10, 6, dic_gh, 5);

                    // 删除0值行
                    OfficeTool.ExcelDeleteNullRow(excel_sheet, new List<int>() { 4, 6 }, 5);
                    // 删除指定列
                    OfficeTool.ExcelDeleteCol(excel_sheet, new List<int>() { 10 });

                    pw.AddProcessMessage(10, time_base, "删除中间数据");
                    Arcpy.Delect(output_table);
                    Arcpy.Delect(output_table + "_gh");

                    // 复制映射表
                    File.Delete(@$"{init_foder}\{excelMapper}.xlsx");
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        // 区分202、203等属性
        public void Segment(string tablePath, string bmField, string gnField)
        {
            Table table = tablePath.TargetTable();
            using RowCursor rowCursor = table.Search();
            while (rowCursor.MoveNext())
            {
                using Row row = rowCursor.Current;
                // 获取value
                string bm = row[bmField]?.ToString() ?? "";
                string gn = row[gnField]?.ToString() ?? "";
                string czcsxm = row["CZCSXM"]?.ToString() ?? "";
                // 村庄用地
                List<string> vgyd= new List<string>()
                {
                    "居住用地","公共管理与公共服务用地","商业服务业用地","工业用地","仓储用地","城镇村道路用地","交通场站用地","其他交通设施用地","公用设施用地","绿地与开敞空间用地","留白用地","空闲地",
                };
                // 城镇用地
                List<string> cityyd = new List<string>()
                {
                    "居住用地","公共管理与公共服务用地","商业服务业用地","工业用地","仓储用地","城镇村道路用地","交通场站用地","其他交通设施用地","公用设施用地","绿地与开敞空间用地","空闲地",
                };

                // 判断, 如果是城镇
                if (czcsxm == "201" || czcsxm == "202")
                {
                    if (cityyd.Contains(gn))
                    {
                        row[gnField] = "城镇" + gn;
                    }
                    else
                    {
                        row[gnField] = "城镇其他用地";
                    }
                }
                // 判断, 如果是村庄
                if (czcsxm == "203")
                {
                    if (vgyd.Contains(gn))
                    {
                        row[gnField] = "村庄" + gn;
                    }
                    else
                    {
                        row[gnField] = "村庄其他用地";
                    }
                }

                row.Store();
            }
        }



        private void combox_areaField_DropDown(object sender, EventArgs e)
        {
            string fc_path = combox_fc.ComboxText();
            UITool.AddAllFloatFieldsToComboxPlus(fc_path, combox_areaField);
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/138461488";
            UITool.Link2Web(url);
        }

        private void combox_fc_gh_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc_gh);
        }

        private void combox_bmField_gh_DropDown(object sender, EventArgs e)
        {
            // 将图层字段加入到Combox列表中
            UITool.AddTextFieldsToComboxPlus(combox_fc_gh.ComboxText(), combox_bmField_gh);
        }

        private void combox_areaField_gh_DropDown(object sender, EventArgs e)
        {
            string fc_path = combox_fc_gh.ComboxText();
            UITool.AddAllFloatFieldsToComboxPlus(fc_path, combox_areaField_gh);
        }
    }
}
