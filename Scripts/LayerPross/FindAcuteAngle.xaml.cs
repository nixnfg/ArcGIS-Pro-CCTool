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
using System.Security.Cryptography;
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
    /// Interaction logic for FindAcuteAngle.xaml
    /// </summary>
    public partial class FindAcuteAngle : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public FindAcuteAngle()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "查找锐角";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string txtAngle = txt_angle.Text;
                double acuteAngle = double.Parse(txtAngle);

                string gdbPath = Project.Current.DefaultGeodatabasePath;

                // 判断参数是否选择完全
                if (txtAngle == "")
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
                    pw.AddProcessMessage(10, time_base, "获取参数");
                    // 获取图层
                    FeatureLayer featurelayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                    string lyName = featurelayer.Name;
                    // 获取目标的坐标系
                    SpatialReference sr = featurelayer.GetSpatialReference();

                    // 如果结果要素存在，就先删除
                    if (GisTool.IsHaveFeaturClass(gdbPath, "锐角点"))
                    {
                        Arcpy.Delect(gdbPath + @"\锐角点");
                    }

                    pw.AddProcessMessage(20, time_base, "获取角度信息");

                    // 获取图层的要素类
                    FeatureClass featureClass = featurelayer.GetFeatureClass();

                    // 初始化小于目标值的点
                    List<MapPoint> resultMapPoints = new List<MapPoint>();

                    // 遍历面要素类中的所有要素(有选择就按选择)
                    RowCursor rowCursor = featurelayer.GetSelectCursor();

                    // 遍历图层的要素
                    while (rowCursor.MoveNext())
                    {
                        using (Feature feature = (Feature)rowCursor.Current)
                        {

                            // 获取要素的几何
                            ArcGIS.Core.Geometry.Polygon polygon = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;
                            // 检查要素的类型
                            if (polygon is null)
                            {
                                MessageBox.Show("所选图层不是面要素！");
                                return;
                            }

                            // 检查角度
                            // 获取几何的所有段
                            var parts = polygon.Parts.ToList();

                            foreach (ReadOnlySegmentCollection collection in parts)
                            {
                                // 初始化点集
                                List<MapPoint> points = new List<MapPoint>();
                                // 每个环进行处理（第一个为外环，其它为内环）
                                foreach (Segment segment in collection)
                                {
                                    MapPoint mapPoint = segment.StartPoint;     // 获取点
                                    points.Add(mapPoint);
                                }
                                // 补充首末点，方便下一步计算
                                points.Add(points[0]);
                                points.Insert(0, points[points.Count - 2]);

                                // 计算角度
                                for (int i = 1; i < points.Count - 1; i++)
                                {
                                    // 当前点及前后两个点
                                    MapPoint p1 = points[i - 1];
                                    MapPoint p2 = points[i];
                                    MapPoint p3 = points[i + 1];

                                    var angle1 = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
                                    var angle2 = Math.Atan2(p3.Y - p2.Y, p3.X - p2.X);

                                    var angle = Math.Abs((angle2 - angle1) * 180 / Math.PI);

                                    // 确保角度在0到180度之间
                                    if (angle > 180)
                                        angle = 360 - angle;

                                    // 符合条件的，加入到集合
                                    if (Math.Abs(180-angle) < acuteAngle)
                                    {
                                        resultMapPoints.Add(p2);
                                    }
                                }
                            }
                        }
                    }

                    // 删除重复点
                    List<MapPoint> finalMapPoints = resultMapPoints.Distinct().ToList();

                    pw.AddProcessMessage(40, time_base, "创建锐角点");

                    // 创建结果点要素
                    EditTool.CreatePointFromMapPoint(finalMapPoints, sr, gdbPath, "锐角点");
                    // 加载结果图层
                    MapCtlTool.AddFeatureLayerToMap(gdbPath + @"\锐角点");
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
            string url = "https://blog.csdn.net/xcc34452366/article/details/135748416?spm=1001.2014.3001.5501";
            UITool.Link2Web(url);
        }
    }
}
