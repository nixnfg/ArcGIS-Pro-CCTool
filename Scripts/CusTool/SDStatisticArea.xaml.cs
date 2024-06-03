using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
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
using Row = ArcGIS.Core.Data.Row;
using Table = ArcGIS.Core.Data.Table;

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for SDStatisticArea.xaml
    /// </summary>
    public partial class SDStatisticArea : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SDStatisticArea()
        {
            InitializeComponent();

            // 初始化combox
            combox_unit.Items.Add("平方米");
            combox_unit.Items.Add("公顷");
            combox_unit.Items.Add("平方公里");
            combox_unit.Items.Add("亩");
            combox_unit.SelectedIndex = 0;

            combox_area.Items.Add("投影面积");
            combox_area.Items.Add("图斑面积");
            combox_area.SelectedIndex = 0;

            combox_digit.Items.Add("1");
            combox_digit.Items.Add("2");
            combox_digit.Items.Add("3");
            combox_digit.Items.Add("4");
            combox_digit.Items.Add("5");
            combox_digit.Items.Add("6");
            combox_digit.SelectedIndex = 1;

            // 初始化测试
            //textTablePath.Text = @"C:\Users\Administrator\Desktop\FE.xlsx";

            //textGDBPath.Text = @"C:\Users\Administrator\Desktop\输出";

            //string lyName = "现状用地";
            //// 分地块参数
            //string fc_area_path = "用地红线";

            //UITool.InitFeatureLayerToComboxPlus(combox_fc, lyName);
            //UITool.InitFeatureLayerToComboxPlus(combox_fc_area, fc_area_path);

            UITool.InitFieldToComboxPlus(combox_xmmc, "xmmc", "string");
            UITool.InitFieldToComboxPlus(combox_dkh, "dkh", "string");
            //UITool.InitFieldToComboxPlus(lyName, combox_szxz, "ZLDWDM", "string");
            //UITool.InitFieldToComboxPlus(lyName, combox_qsdw, "ZLDWMC", "string");
            UITool.InitFieldToComboxPlus(combox_tbbh, "TBBH", "string");
            UITool.InitFieldToComboxPlus(combox_tdzh, "pwh", "string");
            UITool.InitFieldToComboxPlus(combox_qsxz, "QSXZ", "string");

        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "三调三大类面积汇总表(祥哥)";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void openTableButton_Click(object sender, RoutedEventArgs e)
        {
            textTablePath.Text = UITool.SaveDialogExcel();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 要素参数
                string fc_path = combox_fc.ComboxText();
                // 字段参数
                string xmmcField = combox_xmmc.ComboxText();
                string dkhField = combox_dkh.ComboxText();
                string szxzField = combox_szxz.ComboxText();
                string qsdwField = combox_qsdw.ComboxText();
                string tbbhField = combox_tbbh.ComboxText();
                string tdzhField = combox_tdzh.ComboxText();
                string qsxzField = combox_qsxz.ComboxText();
                // 面积参数
                string area_type = combox_area.Text[..2];
                string unit = combox_unit.Text;
                int digit = int.Parse(combox_digit.Text);
                // 分地块参数
                string fc_area_path = combox_fc_area.ComboxText();
                // 平差
                bool isAdj = (bool)checkBox_adj.IsChecked;

                string excel_path = textTablePath.Text;
                string outGDBPath = textGDBPath.Text;
                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (fc_path == "" || fc_area_path == "" || excel_path == "" || xmmcField == "" || dkhField == "" || szxzField == "" || qsdwField == "" || tbbhField == "" || tdzhField == "" || qsxzField == "")
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
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调规程.土地利用现状三大类面积汇总表_祥哥.xlsx", excel_path);
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调用地自转换.xlsx", folder_path + @"\三调用地自转换.xlsx");

                    pw.AddProcessMessage(10, time_base, $"初始化输出路径");
                    // 放到新建的数据库中
                    string gdbName = $"违法图斑数据库";
                    string new_gdb = $@"{outGDBPath}\{gdbName}.gdb";
                    if (Directory.Exists(new_gdb))    // 如果已经有gdb文件了，就清空里面的要素
                    {
                        new_gdb.ClearGDBItem();
                    }
                    Arcpy.CreateFileGDB(outGDBPath, gdbName);

                    // 获取oid字段
                    string oid = GisTool.GetIDFieldNameFromTarget(fc_area_path);

                    if (fc_area_path == "")  // 如果没有分地块统计
                    {
                        Arcpy.Dissolve(fc_path, $@"{new_gdb}\{single_layer_path}");
                    }
                    else   // 如果分地块统计
                    {
                        Arcpy.AddField(fc_area_path, "BJM", "TEXT");
                        Arcpy.CalculateField(fc_area_path, "BJM", $"!{oid}!");
                        // 排序一下
                        Arcpy.Sort(fc_area_path, $@"{gdb_path}\sort", $"{dkhField} DESCENDING", "UR");
                        // 按oid字段分割
                        Arcpy.SplitByAttributes($@"{gdb_path}\sort", new_gdb, "BJM");
                    }

                    // 收集分割后的地块
                    List<string> list_area = new_gdb.GetFeatureClassAndTablePath();

                    // 处理每个行政单位
                    int start_row = 7;
                    List<int> row_list = new List<int>();

                    // sheet
                    string new_sheet_path = excel_path + @$"\Sheet1$";

                    foreach (var area in list_area)
                    {

                        // 裁剪新用地
                        string area_name = area[(area.LastIndexOf(@"\") + 1)..];
                        string adj_fc_mid = $@"{new_gdb}\{area_name}_中间";
                        string adj_fc = $@"{new_gdb}\{area_name}_结果";
                        string dkName = GisTool.GetCellFromPath(area, dkhField);
                        pw.AddProcessMessage(20, time_base, $"处理分地块：{area_name}_{dkName}");

                        // 排除area图层中的【Join_Count】字段
                        if (GisTool.IsHaveFieldInTarget(area, "Join_Count"))
                        {
                            Arcpy.DeleteField(area, "Join_Count");
                        }

                        if (!isAdj)          // 如果不平差计算
                        {
                            GisTool.AdjustmentNot(fc_path, area, adj_fc_mid, area_type, unit, digit);
                        }
                        else          // 平差计算
                        {
                            pw.AddProcessMessage(10, time_base, $"平差计算", Brushes.Gray);
                            GisTool.Adjustment(fc_path, area, adj_fc_mid, area_type, unit, digit);
                        }
                        // 标记一下字段
                        Arcpy.Identity(adj_fc_mid, area, adj_fc);
                        Arcpy.DeleteField(adj_fc, $"{area_type}_1");

                        // 计算一下权属性质
                        string exp = "def ss(a):\r\n    if a == \"10\" or a == \"20\":\r\n        return \"国有\"\r\n    elif a == \"30\":\r\n        return \"集体\"\r\n    else:\r\n        return \"未知\"";
                        Arcpy.CalculateField(adj_fc, qsxzField, $"ss(!{qsxzField}!)", exp);

                        pw.AddProcessMessage(10, time_base, "用地名称转为统计分类字段", Brushes.Gray);
                        // 添加一个类一、二级字段
                        Arcpy.AddField(adj_fc, "mc_1", "TEXT");
                        Arcpy.AddField(adj_fc, "mc_2", "TEXT");
                        Arcpy.AddField(adj_fc, "mc_3", "TEXT");

                        // 用地编码转换
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_1", folder_path + @"\三调用地自转换.xlsx\三大类一级$");
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_2", folder_path + @"\三调用地自转换.xlsx\二级$");
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_3", folder_path + @"\三调用地自转换.xlsx\三大类$");

                        pw.AddProcessMessage(10, time_base, $"__汇总用地指标", Brushes.Gray);

                        // 遍历每个图斑，统计值
                        Table table = adj_fc.TargetTable();
                        using RowCursor rowCursor = table.Search();
                        while (rowCursor.MoveNext())
                        {
                            // 复制行
                            OfficeTool.ExcelCopyRow(new_sheet_path, 6, start_row);
                            // 记录写入行数的列表
                            row_list.Add(start_row);

                            using Row row = rowCursor.Current;
                            // 获取value
                            string zzsxmc = row["ZZSXMC"]?.ToString() ?? "";

                            string xmmc = row[xmmcField]?.ToString() ?? "";
                            string dkh = row[dkhField]?.ToString() ?? "";
                            string szxz = row[szxzField]?.ToString() ?? "";
                            string qsdw = row[qsdwField]?.ToString() ?? "";
                            string tbbh = row[tbbhField]?.ToString() ?? "";
                            string tdzh = row[tdzhField]?.ToString() ?? "";
                            string qsxz = row[qsxzField]?.ToString() ?? "";

                            string mc1 = row["mc_1"]?.ToString() ?? "";
                            string mc2 = row["mc_2"]?.ToString() ?? "";
                            string mc3 = row["mc_3"]?.ToString() ?? "";
                            double mj = Math.Round(double.Parse(row[area_type]?.ToString() ?? ""), digit);
                            double zmj = mj;   // 总面积
                            double kcxs = double.Parse(row["KCXS"]?.ToString() ?? "");
                            double tkmj = 0;
                            // 计算KCXS
                            if (kcxs != 0)
                            {
                                mj = Math.Round(mj * (1 - kcxs), digit);
                                tkmj = Math.Round(mj * kcxs, digit);
                            }

                            // 设置一个StringDict
                            Dictionary<string, string> dictString = new()
                            {
                                { "项目名称", xmmc}, { "地块号",dkh },{ "所在乡镇",szxz}, { "权属单位",qsdw },{ "图斑编号", tbbh }, { "土地证号",tdzh }, { "权属性质", qsxz}, { "备注", zzsxmc}
                            };
                            // 属性映射到Excel
                            OfficeTool.ExcelAttributeMapperCol(new_sheet_path, 1, start_row, dictString);

                            // 设置一个DoubleDict
                            Dictionary<string, double> dictDouble = new()
                            {
                                { mc1 ,mj },{ mc2 ,mj },{ mc3 ,mj },{ "总面积" ,zmj }
                            };
                            if (kcxs != 0)
                            {
                                dictDouble.Add("田坎", tkmj);
                            }
                            // 属性映射到Excel
                            OfficeTool.ExcelAttributeMapperColDouble(new_sheet_path, 1, start_row, dictDouble);


                            // 进入下一行
                            start_row++;
                        }

                    }

                    pw.AddProcessMessage(20, time_base, "删除空列", Brushes.Gray);

                    // 更新面积标识
                    OfficeTool.ExcelWriteCell(new_sheet_path, 2, 0, $"面积单位：{unit}");

                    // 删除空列    最后一列是备注列，不纳入计算
                    OfficeTool.ExcelDeleteNullCol(new_sheet_path, row_list, 7, 1);
                    // 删除行
                    OfficeTool.ExcelDeleteRow(new_sheet_path, new List<int>() { start_row, 6, 1 });

                    // 删除中间数据
                    Arcpy.Delect(gdb_path + @"\statistic_sd");
                    Arcpy.Delect(gdb_path + @"\前");
                    Arcpy.Delect(gdb_path + @"\后");
                    Arcpy.Delect($@"{gdb_path}\sort");
                    File.Delete(folder_path + @"\三调用地自转换.xlsx");

                    pw.AddProcessMessage(20, time_base, "分地块_分解Excel ", Brushes.Gray);
                    // 分解 Excel
                    // 获取所有地块号
                    List<string> dkhList = fc_area_path.GetFieldValues(dkhField);

                    foreach (string dkh in dkhList)
                    {
                        // 复制sheet
                        OfficeTool.ExcelCopySheet(excel_path, "Sheet1", dkh);
                        // 删除无关的行
                        string newSheet = $@"{excel_path}\{dkh}$";
                        // 获取工作薄、工作表
                        string excelFile = newSheet.GetExcelPath();
                        int sheetIndex = newSheet.GetExcelSheetIndex();
                        // 打开工作薄
                        Workbook wb = OfficeTool.OpenWorkbook(excelFile);
                        // 打开工作表
                        Worksheet sheet = wb.Worksheets[sheetIndex];
                        Cells cells = sheet.Cells;
                        // 逐行处理
                        for (int i = cells.MaxDataRow; i >= 5; i--)
                        {
                            //  获取目标cell
                            Cell inCell = cells[i, 1];
                            Cell totalCell = cells[i, 0];
                            // 属性映射
                            if (inCell is not null && totalCell is not null)
                            {
                                if (inCell.StringValue != dkh && totalCell.StringValue != "合计")
                                {
                                    cells.DeleteRow(i);
                                }
                            }
                            else
                            {
                                cells.DeleteRow(i);
                            }
                        }

                        // 删除空值列
                        for (int k = cells.MaxDataColumn - 1; k >= 8; k--)
                        {
                            bool isNull = true;
                            for (int i = cells.MaxDataRow; i >= 5; i--)
                            {
                                Cell totalCell = cells[i, 0];
                                //  获取目标cell
                                Cell inCell = cells[i, k];
                                // 属性映射
                                if (inCell is not null && totalCell is not null)
                                {
                                    if (inCell.StringValue != "")
                                    {
                                        if (inCell.DoubleValue != 0 && totalCell.StringValue != "合计")
                                        {
                                            isNull = false;
                                        }
                                    }
                                }
                            }
                            // 如果是全空的，就删除
                            if (isNull)
                            {
                                cells.DeleteColumn(k);
                            }
                        }
                        // 保存
                        wb.Save(excelFile);
                        wb.Dispose();
                    }

                    // 清理输出的gdb
                    GisTool.ClearGDBFiles(new_gdb, "_结果");

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
            string url = "https://blog.csdn.net/xcc34452366/article/details/138461810";
            UITool.Link2Web(url);
        }

        private void combox_xmmc_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc_area.ComboxText(), combox_xmmc);
        }

        private void combox_dkh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc_area.ComboxText(), combox_dkh);
        }

        private void combox_qsdw_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_qsdw);
        }

        private void combox_tbbh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_tbbh);
        }

        private void combox_tdzh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc_area.ComboxText(), combox_tdzh);
        }

        private void combox_qsxz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_qsxz);
        }

        private void combox_fc_area_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc_area);
        }

        private void openGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textGDBPath.Text = UITool.OpenDialogFolder();
        }

        private void combox_szxz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_szxz);
        }
    }
}
