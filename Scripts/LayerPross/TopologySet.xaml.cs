using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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

namespace CCTool.Scripts.LayerPross
{
    /// <summary>
    /// Interaction logic for TopologySet.xaml
    /// </summary>
    public partial class TopologySet : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public TopologySet()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "要素拓扑_自由选择规则";


        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取要素图层
                FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                // 获取参数
                List<string> rules = listBox_rule.ListboxText();

                // 判断参数是否选择完全
                if (rules.Count==0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 获取默认数据库
                var gdb = Project.Current.DefaultGeodatabasePath;

                string db_name = "Top2Check";    // 要素数据集名
                string fc_name = "top_fc";        // 要素名
                string top_name = "Topology";       // TOP名
                string db_path = gdb + "\\" + db_name;    // 要素数据集路径
                string fc_path = db_path + "\\" + fc_name;         // 要素路径
                string top_path = db_path + "\\" + top_name;         // TOP路径

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                 await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, "创建检查用的数据库");

                    //获取图层的坐标系
                    var sr = ly.GetSpatialReference();
                    //在数据库中创建要素数据集
                    Arcpy.CreateFeatureDataset(gdb, db_name, sr);
                    // 将所选要素复制到创建的要素数据集中
                    Arcpy.CopyFeatures(ly, fc_path);

                    pw.AddProcessMessage(10, time_base, "创建拓扑");
                    // 新建拓扑
                    Arcpy.CreateTopology(db_path, top_name);
                    // 向拓扑中添加要素
                    Arcpy.AddFeatureClassToTopology(top_path, fc_path);
                    // 添加拓扑规则【重叠】
                    foreach (var rule in rules)
                    {
                        pw.AddProcessMessage(10, time_base, $"添加规则：{rule}");
                        Arcpy.AddRuleToTopology(top_path, ChangeRule(rule), fc_path);
                    }
                    pw.AddProcessMessage(20, time_base, "验证拓扑并输出错误要素");
                    // 验证拓扑
                    Arcpy.ValidateTopology(top_path);
                    // 输出TOP错误
                    Arcpy.ExportTopologyErrors(top_path, gdb, "TopErr", true);

                    pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }


        public string ChangeRule(string rule)
        {
            string result = rule switch
            {
                "不能重叠(面)" => "Must Not Overlap (Area)",
                "不能有空隙" => "Must Not Have Gaps (Area)",
                "不能重叠(线)" => "Must Not Overlap (Line)",
                "不能相交" => "Must Not Intersect (Line)",
                "不能有悬挂点" => "Must Not Have Dangles (Line)",
                "不能有伪结点" => "Must Not Have Pseudo-Nodes (Line)",
                _ => "",
            };

            return result;
        }



        private async void fm_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                // 加载规则
                GeometryType geometryType = await QueuedTask.Run(() =>
                {
                    // 获取要素图层
                    FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                    FeatureClass featureClass = featureLayer.GetFeatureClass();
                    // 获取要素类型
                    GeometryType geometryType = featureClass.GetDefinition().GetShapeType();
                    return geometryType;
                });

                // 图层图片
                string imagetop1 = "/CCTool;component/Data/Icons/top1.png";
                string imagetop2 = "/CCTool;component/Data/Icons/top2.png";
                string imagetop3 = "/CCTool;component/Data/Icons/top3.png";
                string imagetop4 = "/CCTool;component/Data/Icons/top4.png";
                string imagetop5 = "/CCTool;component/Data/Icons/top5.png";
                string imagetop6 = "/CCTool;component/Data/Icons/top6.png";

                // 规则
                Dictionary<string, string> rules = new Dictionary<string, string>();
                if (geometryType == GeometryType.Polygon)
                {
                    rules.Add("不能重叠(面)", imagetop1);
                    rules.Add("不能有空隙", imagetop2);
                }
                else if (geometryType == GeometryType.Polyline)
                {
                    rules.Add("不能重叠(线)", imagetop3);
                    rules.Add("不能相交", imagetop4);
                    rules.Add("不能有悬挂点", imagetop5);
                    rules.Add("不能有伪结点", imagetop6);
                }

                List<ListBoxContent> lbc = new List<ListBoxContent>();


                // 加到combox中
                foreach (var rule in rules)
                {
                    lbc.Add(new ListBoxContent() { IsSelect = false, Path = rule.Value, Name = rule.Key });
                }
                listBox_rule.ItemsSource = lbc;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                throw;
            }
        }
    }


    // 图层
    public class ListBoxContent
    {
        public bool IsSelect { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
    }
}
