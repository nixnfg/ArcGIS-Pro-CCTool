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
    /// Interaction logic for SDStatisticCom.xaml
    /// </summary>
    public partial class SDStatisticCom : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SDStatisticCom()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "三调三大类面积汇总表";

        private void combox_xz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_xz);
        }

        private void combox_cz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_cz);
        }

        private void combox_group_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_group);
        }

        private void combox_qs_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_qs);
        }

        private void combox_tbh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_tbh);
        }

        private void combox_mj_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_mj);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void openTableButton_Click(object sender, RoutedEventArgs e)
        {
            textTablePath.Text = UITool.SaveDialogExcel();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/137836347";
            UITool.Link2Web(url);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 三调图层
                string fc_path = combox_fc.ComboxText();
                // 字段参数
                string xz_field = combox_xz.ComboxText();
                string cz_field = combox_cz.ComboxText();
                string group_field = combox_group.ComboxText();
                string qs_field = combox_qs.ComboxText();
                string tbh_field = combox_tbh.ComboxText();
                string mj_field = combox_mj.ComboxText();

                int digit = 4;

                // 保存excel路径
                string excel_path = textTablePath.Text;

                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (fc_path == "" || excel_path == "" || xz_field == "" || cz_field == "" || group_field == "" || qs_field == "" || tbh_field == "" || mj_field == "")
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
                    // 获取图层名
                    string single_layer_path = fc_path.GetLayerSingleName();

                    // 复制嵌入资源中的Excel文件
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调规程.土地利用现状三大类面积汇总表_一首歌.xlsx", excel_path);
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调用地自转换.xlsx", folder_path + @"\三调用地自转换.xlsx");

                    // 获取图斑号
                    List<string> valueList = fc_path.GetFieldValues(tbh_field);
                    // 写入表格
                    string txt = "";
                    foreach (string value in valueList)
                    {
                        txt += $"{value}，";
                    }
                    string txt2 = txt[..(txt.Length-1)];
                    OfficeTool.ExcelWriteCell(excel_path, 10, 6, txt2);

                    // 放到新建的数据库中
                    string filePath = @"D:\Program Files\临时文件";
                    string gdbName = "临时数据库";
                    Arcpy.CreateFileGDB(filePath, gdbName);
                    string new_gdb = $@"{filePath}\{gdbName}.gdb";
                    // 清理一下
                    new_gdb.ClearGDBItem();

                    // 复制要素
                    string adj_fc = $@"{gdb_path}\adj_fc";
                    Arcpy.CopyFeatures(fc_path, adj_fc);

                    pw.AddProcessMessage(10, time_base, "用地名称转为统计分类字段", Brushes.Gray);
                    // 添加一个类一、二级字段
                    Arcpy.AddField(adj_fc, "mc_1", "TEXT");
                    Arcpy.AddField(adj_fc, "mc_2", "TEXT");
                    Arcpy.AddField(adj_fc, "mc_3", "TEXT");

                    // 用地编码转换
                    GisTool.AttributeMapper(adj_fc, "DLMC", "mc_1", folder_path + @"\三调用地自转换.xlsx\三大类一级$");
                    GisTool.AttributeMapper(adj_fc, "DLMC", "mc_2", folder_path + @"\三调用地自转换.xlsx\二级$");
                    GisTool.AttributeMapper(adj_fc, "DLMC", "mc_3", folder_path + @"\三调用地自转换.xlsx\三大类$");

                    // 权属计算
                    string exp = "def ss(a):\r\n    if a == \"10\" or a == \"20\":\r\n        return \"国有\"\r\n    elif a == \"30\":\r\n        return \"集体\"\r\n    else:\r\n        return \"未知\"";
                    Arcpy.CalculateField(adj_fc, qs_field, $"ss(!{qs_field}!)", exp);

                    pw.AddProcessMessage(10, time_base, "分行统计要素生成", Brushes.Gray);

                    // 分村, 及总计，国有，集体，乡镇，村小计
                    Arcpy.SplitByAttributes(adj_fc, new_gdb, $"{xz_field};{cz_field};{group_field};{qs_field}");
                    Arcpy.CopyFeatures(adj_fc, @$"{new_gdb}\面积总合计");
                    Arcpy.Select(adj_fc, @$"{new_gdb}\国有部分合计", $"{qs_field} = '国有'");
                    Arcpy.Select(adj_fc, @$"{new_gdb}\集体部分合计", $"{qs_field} = '集体'");

                    // 收集分割后的地块
                    List<string> list_area_detail = new_gdb.GetFeatureClassAndTablePath();

                    // 新建sheet
                    OfficeTool.ExcelCopySheet(excel_path, "sheet1", fc_path);
                    string new_sheet_path = excel_path + @$"\{fc_path}$";

                    // 处理每个行政单位
                    int start_row = 7;
                    List<int> row_list = new List<int>();
                    foreach (var area_detail in list_area_detail)
                    {
                        // 要处理的要素名称
                        string area_name_detail = area_detail[(area_detail.LastIndexOf(@"\") + 1)..];
                        // 获取各个属性字段值
                        string xz = GisTool.GetCellFromPath(area_detail, xz_field);      // 乡镇名
                        string cz = GisTool.GetCellFromPath(area_detail, cz_field);      // 村庄名
                        string group = GisTool.GetCellFromPath(area_detail, group_field);      // 小组名
                        string qs = GisTool.GetCellFromPath(area_detail, qs_field);      // 权属性质

                        // 判断一下是分小组行还是合计行
                        bool isCZRow = true;
                        if (!area_name_detail.Contains('_') && area_name_detail.Contains("合计"))
                        {
                            isCZRow = false;
                        }

                        pw.AddProcessMessage(10, time_base, $"汇总_[{area_name_detail}]", Brushes.Gray);

                        // 收集未扣除田坎的面积
                        Arcpy.Statistics(area_detail, gdb_path + @"\前", mj_field + " SUM", "");
                        double zmj_old = double.Parse(GisTool.GetCellFromPath(gdb_path + @"\前", "SUM_" + mj_field));
                        // 计算扣除面积
                        Arcpy.CalculateField(area_detail, mj_field, $"round(!{mj_field}!* (1-!KCXS!),{digit})");

                        // 收集扣除田坎的面积
                        Arcpy.Statistics(area_detail, gdb_path + @"\后", mj_field + " SUM", "");
                        double zmj_new = double.Parse(GisTool.GetCellFromPath(gdb_path + @"\后", "SUM_" + mj_field));
                        double kcmj = Math.Round(zmj_old - zmj_new, digit);

                        if (isCZRow)
                        {
                            // 复制行
                            OfficeTool.ExcelCopyRow(new_sheet_path, 6, start_row);
                            // 记录写入行数的列表
                            row_list.Add(start_row);
                        }

                        // 汇总大、中类
                        List<string> list_bm = new List<string>() { "mc_1", "mc_2", "mc_3" };
                        string statistic_sd = gdb_path + @"\statistic_sd";
                        GisTool.MultiStatistics(area_detail, statistic_sd, mj_field + " SUM", list_bm, "总面积", 4);

                        // 统计田坎面积
                        if (kcmj != 0)
                        {
                            // 插入【田坎】行
                            GisTool.UpdataRowToTable(statistic_sd, $"分组,田坎;SUM_{mj_field},{kcmj}");
                            // 计算【其他土地、总面积】行
                            GisTool.IncreRowValueToTable(statistic_sd, $"分组,农用地其他土地;SUM_{mj_field},{kcmj}");
                            GisTool.IncreRowValueToTable(statistic_sd, $"分组,农用地;SUM_{mj_field},{kcmj}");
                            GisTool.IncreRowValueToTable(statistic_sd, $"分组,总面积;SUM_{mj_field},{kcmj}");
                        }
                        // 再次确认小数位数
                        Arcpy.CalculateField(statistic_sd, $"SUM_{mj_field}", $"round(!SUM_{mj_field}!, {digit})");
                        // 将映射属性表中获取字典Dictionary
                        Dictionary<string, double> dict = GisTool.GetDictFromPathDouble(gdb_path + @"\statistic_sd", @"分组", "SUM_" + mj_field);

                        Dictionary<string, string> dictString = new Dictionary<string, string>()
                        {
                            {"乡镇", xz },{"村庄", cz },{"小组", group },{"地类性质", qs }
                        };
                        // 如果不是合计行，新增行再赋值
                        if (isCZRow)
                        {
                            // 属性映射到Excel
                            OfficeTool.ExcelAttributeMapperCol(new_sheet_path, 1, start_row, dictString);
                            OfficeTool.ExcelAttributeMapperColDouble(new_sheet_path, 1, start_row, dict);
                            // 进入下一个行政单位
                            start_row++;
                        }
                        // 如果是合计行   直接赋值
                        else
                        {
                            int row = start_row;
                            if (area_name_detail == "面积总合计") { row = start_row; }
                            else if (area_name_detail == "国有部分合计") { row = start_row + 1; }
                            else if (area_name_detail == "集体部分合计") { row = start_row + 2; }

                            // 属性映射到Excel
                            OfficeTool.ExcelAttributeMapperColDouble(new_sheet_path, 1, row, dict);
                        }

                    }
                    pw.AddProcessMessage(20, time_base, "删除空列", Brushes.Gray);

                    // 删除空列
                    OfficeTool.ExcelDeleteNullCol(new_sheet_path, row_list, 6);
                    // 删除行
                    OfficeTool.ExcelDeleteRow(new_sheet_path, new List<int>() { 6, 1 });

                    // 合并单元格
                    List<int> cols = new List<int>() { 0, 1, 2, 3};
                    foreach (var col in cols)
                    {
                        OfficeTool.ExcelMergeSameCol(new_sheet_path, col, 5);
                    }

                    // 删除sheet1
                    OfficeTool.ExcelDeleteSheet(excel_path, "Sheet1");

                    // 删除中间数据
                    Arcpy.Delect(gdb_path + @"\statistic_sd");
                    Arcpy.Delect(gdb_path + @"\dissolveFC");
                    Arcpy.Delect(gdb_path + @"\前");
                    Arcpy.Delect(gdb_path + @"\后");
                    Arcpy.Delect(adj_fc);

                    File.Delete(folder_path + @"\三调用地自转换.xlsx");

                    // 收集分割后的地块
                    //List<string> list = new_gdb.GetFeatureClassAndTablePath();
                    //foreach (var item in list)
                    //{
                    //    Arcpy.Delect(item);
                    //}
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
