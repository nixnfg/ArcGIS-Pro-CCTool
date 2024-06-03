using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Spreadsheet;
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
    /// Interaction logic for ContentLine.xaml
    /// </summary>
    public partial class ContentLine : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ContentLine()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "连接道路线";


        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void combox_field1_DropDown(object sender, EventArgs e)
        {
            UITool.AddFieldsToComboxPlus(combox_fc.ComboxText(), combox_field1);
        }

        private void combox_field2_DropDown(object sender, EventArgs e)
        {
            UITool.AddFieldsToComboxPlus(combox_fc.ComboxText(), combox_field2);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.ComboxText();
                string fc_field1 = combox_field1.ComboxText();
                string fc_field2 = combox_field2.ComboxText();

                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (fc_path == "" || fc_field1 == "" || fc_field2 == "")
                {
                    MessageBox.Show("有必选参数为空或输入错误！！！");
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
                    FeatureLayer originFeatureLayer = fc_path.TargetFeatureLayer();
                    // 获取原始图层和标识图层的要素类
                    FeatureClass originFeatureClass = fc_path.TargetFeatureClass();

                    string OID = GisTool.GetIDFieldNameFromTarget(fc_path);

                    // 初始化要合并的要素OID
                    List<List<long>> objectIDList = new();

                    pw.AddProcessMessage(10, time_base, $"遍历要素");
                    // 获取目标图层和源图层的要素游标
                    using (RowCursor originCursor = originFeatureClass.Search())
                    {
                        // 遍历源图层的要素
                        while (originCursor.MoveNext())
                        {
                            using Feature originFeature = (Feature)originCursor.Current;

                            // 用来标识的字段值
                            var field1 = originFeature[fc_field1];
                            if (field1 is null) { continue; }
                            string fieldValue1 = field1.ToString();

                            var field2 = originFeature[fc_field2];
                            if (field2 is null) { continue; }
                            string fieldValue2 = field2.ToString();

                            // ID值
                            long originID = long.Parse(originFeature[OID].ToString());

                            // 在目标图层中查询与源要素重叠的要素
                            using RowCursor identityCursor = originFeatureClass.Search();
                            while (identityCursor.MoveNext())
                            {
                                using Feature identityFeature = (Feature)identityCursor.Current;

                                // 用来标识的字段值
                                var targetField1 = identityFeature[fc_field1];
                                if (targetField1 is null) { continue; }
                                string targetFieldValue1 = targetField1.ToString();

                                var targetField2 = identityFeature[fc_field2];
                                if (targetField2 is null) { continue; }
                                string targetFieldValue2 = targetField2.ToString();

                                // objectID值
                                long identityID = long.Parse(identityFeature[OID].ToString());
                                // 字段交错相等的情况下
                                if (fieldValue1 == targetFieldValue2 && fieldValue2 == targetFieldValue1)
                                {
                                    // 预判相反顺序，但是是同一组线段的情况
                                    List<long> li = new() { originID, identityID};

                                    bool containsTargetList = objectIDList.Any(list => list.SequenceEqual(li));

                                    if (!containsTargetList)
                                    {
                                        objectIDList.Add(new List<long>() { originID, identityID });
                                    }
                                    
                                }
                            }
                        }
                    }
                    pw.AddProcessMessage(30, time_base, "合并要素");
                    // 合并
                    foreach (var item in objectIDList)
                    {
                        EditTool.MergeFeatures(originFeatureLayer, item);
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
            string url = "https://blog.csdn.net/xcc34452366/article/details/137835012";
            UITool.Link2Web(url);
        }
    }
}
