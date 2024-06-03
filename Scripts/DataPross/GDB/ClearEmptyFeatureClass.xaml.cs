using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using CCTool.Scripts.DataPross.TXT;
using CCTool.Scripts.Manager;
using CCTool.Scripts.MiniTool.GetInfo;
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
using Table = ArcGIS.Core.Data.Table;

namespace CCTool.Scripts.DataPross.GDB
{
    /// <summary>
    /// Interaction logic for ClearEmptyFeatureClass.xaml
    /// </summary>
    public partial class ClearEmptyFeatureClass : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ClearEmptyFeatureClass()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "清理gdb里的空要素和表";


        // 定义一个空包
        List<FeatureClassAtt> fcAtt = new List<FeatureClassAtt>();


        private void openGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textGDBFolder.Text = UITool.OpenDialogGDB();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/137137515";
            UITool.Link2Web(url);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);
                // 定义一个空包
                List<FeatureClassAtt> fcAtt3 = new List<FeatureClassAtt>();
                // gdb路径
                string gdbPath = textGDBFolder.Text;

                if (!gdbPath.Contains(".gdb"))
                {
                    return;
                }

                int fcCount = 0;
                int tableCount = 0;

                await QueuedTask.Run(() =>
                {
                    // 打开gdb
                    using (Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath))))
                    {
                        // 删除空要素和表
                        foreach (var item in fcAtt)
                        {
                            if (item.IsEmpty == "为空")
                            {
                                if (item.Type == "FeatureClass")
                                {
                                    FeatureClass featureClass = gdb.OpenDataset<FeatureClass>(item.Name);
                                    // 删除
                                    Arcpy.Delect(featureClass);
                                    fcCount++;

                                    pw.AddProcessMessage(10, time_base, $"清理空要素：{item.Name}");
                                }
                                else if (item.Type == "Table")
                                {
                                    Table table = gdb.OpenDataset<Table>(item.Name);
                                    // 删除
                                    Arcpy.Delect(table);
                                    tableCount++;

                                    pw.AddProcessMessage(10, time_base, $"清理空表：{item.Name}");
                                }
                            }
                        }
                    };
                });

                // 更新表格
                UpdateTable();

                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void textGDBFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 更新表格
            UpdateTable();
        }

        public async void UpdateTable()
        {
            try
            {
                // 定义一个空包
                List<FeatureClassAtt> fcAtt2 = new List<FeatureClassAtt>();

                // gdb路径
                string gdbPath = textGDBFolder.Text;

                if (!gdbPath.Contains(".gdb"))
                {
                    return;
                }

                await QueuedTask.Run(() =>
                {
                    // 打开gdb
                    using (Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath))))
                    {
                        // 获取要素类和独立表
                        var fcDefinitions = gdb.GetDefinitions<FeatureClassDefinition>();
                        var tableDefinitions = gdb.GetDefinitions<TableDefinition>();
                        // 加入构造
                        int index = 1;
                        // 要素类
                        foreach (var fcDefinition in fcDefinitions)
                        {
                            string fcName = fcDefinition.GetName();
                            string fcAliasName = fcDefinition.GetAliasName();
                            Table table = gdb.OpenDataset<Table>(fcName);
                            long count = table.GetCount();

                            fcAtt2.Add(new FeatureClassAtt()
                            {
                                ID = index,
                                Name = fcName,
                                AliasName = (fcAliasName == "") ? fcName : fcAliasName,
                                IsEmpty = (count == 0) ? "为空" : "不为空",
                                BackgroudColor = (count == 0) ? "#FFF39292" : "White",
                                Type = "FeatureClass",
                            });

                            index++;
                        }

                        // 表
                        foreach (var tableDefinition in tableDefinitions)
                        {
                            string fcName = tableDefinition.GetName();
                            string fcAliasName = tableDefinition.GetAliasName();
                            Table table = gdb.OpenDataset<Table>(fcName);
                            long count = table.GetCount();

                            fcAtt2.Add(new FeatureClassAtt()
                            {
                                ID = index,
                                Name = fcName,
                                AliasName = (fcAliasName == "") ? fcName : fcAliasName,
                                IsEmpty = (count == 0) ? "为空" : "不为空",
                                BackgroudColor = (count == 0) ? "#FFF39292" : "White",
                                Type = "Table",
                            });

                            index++;
                        }
                    };
                });

                // 绑定
                dg.ItemsSource = fcAtt2;
                // 赋值
                fcAtt = fcAtt2;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }


    public class FeatureClassAtt
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string AliasName { get; set; }
        public string IsEmpty { get; set; }
        public string BackgroudColor { get; set; }
        public string Type { get; set; }
    }
}
