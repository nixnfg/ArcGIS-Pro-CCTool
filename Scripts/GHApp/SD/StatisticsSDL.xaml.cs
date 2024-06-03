using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells.Drawing;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using CCTool.Scripts.UI.ProWindow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using CheckBox = System.Windows.Controls.CheckBox;
using Polygon = ArcGIS.Core.Geometry.Polygon;
using Row = ArcGIS.Core.Data.Row;
using Table = ArcGIS.Core.Data.Table;

namespace CCTool.Scripts.GHApp.SD
{
    /// <summary>
    /// Interaction logic for StatisticsSDL.xaml
    /// </summary>
    public partial class StatisticsSDL : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public StatisticsSDL()
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

            // 初始化listbox_zb
            List<string> zbList = new List<string>()
            {
                "GDMJ(耕地面积)","YDMJ(园地面积)","LDMJ(林地面积)","CDMJ(草地面积)","QTYDMJ(其它用地面积)","JSYDMJ(建设用地面积)","WLYDMJ(未利用地面积)","NYDMJ(农用地面积)"
            };
            foreach (var zb in zbList)
            {
                // 将txt文件做成checkbox放入列表中
                CheckBox cb = new()
                {
                    Content = zb,
                    IsChecked = true
                };
                listbox_zb.Items.Add(cb);
            }
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "三调_统计三大类";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void combox_zone_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_zone);
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/137136703";
            UITool.Link2Web(url);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.ComboxText();
                string area_type = combox_area.Text[..2];
                string unit = combox_unit.Text;
                int digit = int.Parse(combox_digit.Text);
                bool isAdj = (bool)checkBox_adj.IsChecked;

                string zone_path = combox_zone.ComboxText();

                // 列表框
                List<string> zbList = listbox_zb.GetCheckListBoxText();

                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;


                // 判断参数是否选择完全
                if (fc_path == "" || zone_path == "" || zbList.Count == 0)
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
                    string gdbName = "临时数据库";
                    Arcpy.CreateFileGDB(filePath, gdbName);
                    string new_gdb = $@"{filePath}\{gdbName}.gdb";
                    // 清理一下
                    new_gdb.ClearGDBItem();

                    pw.AddProcessMessage(10, time_base, "分割分地块", Brushes.Gray);

                    // 添加一个字段等oid
                    Arcpy.CopyFeatures(zone_path, gdb_path + @"\copyZone");
                    Arcpy.AddField(gdb_path + @"\copyZone", "SF", "LONG");

                    string oidField = GisTool.GetIDFieldNameFromTarget(gdb_path + @"\copyZone");
                    Arcpy.CalculateField(gdb_path + @"\copyZone", "SF", $"!{oidField}!");

                    // 按属性分割
                    Arcpy.SplitByAttributes(gdb_path + @"\copyZone", new_gdb, "SF");
                    // 收集分割后的地块
                    List<string> list_area = new_gdb.GetFeatureClassAndTablePath();

                    Dictionary<string, double> dict = new Dictionary<string, double>();

                    foreach (var area in list_area)
                    {
                        // 裁剪新用地
                        string area_name = area[(area.LastIndexOf(@"\") + 1)..];
                        string adj_fc = $@"{new_gdb}\{area_name}_结果";

                        pw.AddProcessMessage(20, time_base, $"处理分地块：{area_name}");

                        if (!isAdj)          // 如果不平差计算
                        {
                            GisTool.AdjustmentNot(fc_path, area, adj_fc, area_type, unit, digit);
                        }
                        else          // 平差计算
                        {
                            pw.AddProcessMessage(10, time_base, $"平差计算", Brushes.Gray);
                            GisTool.Adjustment(fc_path, area, adj_fc, area_type, unit, digit);
                        }

                        pw.AddProcessMessage(10, time_base, "分类统计", Brushes.Gray);

                        List<string> GDMJ = new List<string>() { "0101", "0102", "0103" };
                        List<string> LDMJ = new List<string>() { "0301", "0302", "0303", "0304", "0305", "0306", "0307" };
                        List<string> CDMJ = new List<string>() { "0401", "0403" };
                        List<string> YDMJ = new List<string>() { "0201", "0202", "0203", "0204" };
                        List<string> QTYDMJ = new List<string>() { "1006", "1103", "1104", "1107", "1202", "1203", "0402" };
                        List<string> JSYDMJ = new List<string>() { "05H1", "0508", "0601", "0602", "0603", "0701", "0702", "08H1", "08H2", "0809", "0810", "09", "1001", "1002", "1003", "1004", "1005", "1007", "1008", "1009", "1109", "1201", "0810A" };
                        List<string> WLYDMJ = new List<string>() { "0404", "1101", "1102", "1105", "1106", "1108", "1110", "1204", "1205", "1206", "1207" };
                        List<string> NYDMJ = new List<string>() { "0101", "0102", "0103", "0201", "0202", "0203", "0204", "0301", "0302", "0303", "0304", "0305", "0306", "0307", "0401", "0402", "0403", "1006", "1103", "1104", "1107", "1202", "1203", "1107A" };

                        Dictionary<string, List<string>> strings = new Dictionary<string, List<string>>()
                        {
                            {"GDMJ", GDMJ },{"LDMJ", LDMJ },{"CDMJ", CDMJ },{"YDMJ", YDMJ },{"QTYDMJ", QTYDMJ },{"JSYDMJ", JSYDMJ },{"WLYDMJ", WLYDMJ },{"NYDMJ", NYDMJ}
                        };

                        // 汇总统计
                        Table table = adj_fc.TargetTable();

                        using (RowCursor rowCursor = table.Search())
                        {
                            while (rowCursor.MoveNext())
                            {
                                using Row row = rowCursor.Current;
                                string bm = row["DLBM"].ToString();         // 地类编码
                                double mj = double.Parse(row[area_type].ToString());         // 面积
                                                                                             // 分类
                                foreach (var fl in strings)
                                {
                                    if (fl.Value.Contains(bm))
                                    {
                                        dict.Accumulation($"{area_name}_{fl.Key}", mj);
                                    }
                                }
                            }
                        }
                    }

                    pw.AddProcessMessage(10, time_base, "新建统计字段", Brushes.Gray);
                    foreach (var zb in zbList)
                    {
                        string name = zb[..zb.IndexOf("(")];
                        string aliasName = zb[(zb.IndexOf("(") + 1)..zb.IndexOf(")")];

                        Arcpy.AddField(zone_path, name, "Double", aliasName);
                    }

                    pw.AddProcessMessage(20, time_base, "把指标赋值给分地块", Brushes.Gray);
                    // 把指标赋值给分地块
                    string oidField2 = GisTool.GetIDFieldNameFromTarget(zone_path);

                    Table table2 = zone_path.TargetTable();
                    using (RowCursor rowCursor = table2.Search())
                    {
                        while (rowCursor.MoveNext())
                        {
                            using Row row = rowCursor.Current;
                            string oidValue = row[oidField2].ToString();         // OID
                            // 分类
                            foreach (var zb in zbList)
                            {
                                string name = zb[..zb.IndexOf("(")];
                                string dicKey = $"T{oidValue}_{name}";
                                if (dict.ContainsKey(dicKey))
                                {
                                    row[name] = dict[$"T{oidValue}_{name}"];
                                }
                                else
                                {
                                    row[name] = 0;
                                }
                            }

                            row.Store();
                        }
                    }

                    // 收集分割后的地块
                    List<string> list = new_gdb.GetFeatureClassAndTablePath();
                    foreach (var item in list)
                    {
                        Arcpy.Delect(item);
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


    }
}
