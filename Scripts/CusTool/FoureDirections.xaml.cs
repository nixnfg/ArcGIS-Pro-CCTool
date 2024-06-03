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
    /// Interaction logic for FoureDirections.xaml
    /// </summary>
    public partial class FoureDirections : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public FoureDirections()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "获取四至信息";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.ComboxText();
                string fc_field_from = combox_field_from.ComboxText();
                double dic = double.Parse(textDis.Text);

                bool isBuffer = (bool)checkBuffer.IsChecked;

                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (fc_path == "" || fc_field_from == "")
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
                    pw.AddProcessMessage(10, time_base, $"新建中心点坐标字段");
                    string xField = "X中心";
                    string yField = "Y中心";
                    // 计算中心点坐标
                    if (!GisTool.IsHaveFieldInTarget(fc_path, xField))
                    {
                        Arcpy.AddField(fc_path, xField, "DOUBLE");
                    }
                    if (!GisTool.IsHaveFieldInTarget(fc_path, yField))
                    {
                        Arcpy.AddField(fc_path, yField, "DOUBLE");
                    }

                    pw.AddProcessMessage(10, time_base, $"计算中心点坐标");
                    Arcpy.CalculateGeometryAttributes(fc_path, $"{xField} INSIDE_X");
                    Arcpy.CalculateGeometryAttributes(fc_path, $"{yField} INSIDE_Y");

                    // 创建四个字段
                    List<string> fieldNames = new List<string>() { "东至", "西至", "南至", "北至" };
                    foreach (string field in fieldNames)
                    {
                        if (!GisTool.IsHaveFieldInTarget(fc_path, field))
                        {
                            Arcpy.AddField(fc_path, field, "TEXT");
                        }
                    }

                    FeatureLayer originFeatureLayer = fc_path.TargetFeatureLayer();
                    // 获取原始图层和标识图层的要素类
                    FeatureClass originFeatureClass = fc_path.TargetFeatureClass();

                    pw.AddProcessMessage(10, time_base, $"遍历要素");
                    if (isBuffer) { pw.AddProcessMessage(0, time_base, $"启用要素缓冲，缓冲距离{dic}米", Brushes.Green); }
                    // 获取目标图层和源图层的要素游标
                    using RowCursor originCursor = originFeatureClass.Search();

                    long index = 1;   // 计数器
                    // 遍历源图层的要素
                    while (originCursor.MoveNext())
                    {
                        // 计数标志
                        if (index % 500 == 0)
                        {
                            pw.AddProcessMessage(0, time_base, $"累计图斑数量：{index}", Brushes.Gray);
                        }
                        
                        // 定义四至
                        Dir east = new() { Name = "东至", Angle = -1, Lable = "" };
                        Dir west = new() { Name = "西至", Angle = -1, Lable = "" };
                        Dir north = new() { Name = "北至", Angle = -1, Lable = "" };
                        Dir south = new() { Name = "南至", Angle = -1, Lable = "" };

                        using Feature originFeature = (Feature)originCursor.Current;
                        // 获取源要素的几何
                        ArcGIS.Core.Geometry.Geometry originGeometry = originFeature.GetShape();
                        // 用来空间分析的Geometry，主要看是否要buffer
                        ArcGIS.Core.Geometry.Geometry originGeometryBuffer = originGeometry;
                        if (isBuffer)
                        {
                            originGeometryBuffer = GeometryEngine.Instance.Buffer(originGeometry, dic);
                        }

                        // 获取标记的字段值
                        var originFrom = originFeature[fc_field_from];
                        if (originFrom is null) { continue; }
                        string originFromValue = originFrom.ToString();

                        // 获取xy
                        var xx = originFeature[xField];
                        if (xx is null) { continue; }
                        double xxValue = double.Parse(xx.ToString());
                        var yy = originFeature[yField];
                        if (yy is null) { continue; }
                        double yyValue = double.Parse(yy.ToString());

                        // 获取目标要素的内部质心点
                        List<double> originPoint = new List<double>() { xxValue, yyValue };

                        // 创建空间查询过滤器，以获取与源要素有重叠的目标要素
                        SpatialQueryFilter spatialFilter = new SpatialQueryFilter
                        {
                            FilterGeometry = originGeometryBuffer,
                            SpatialRelationship = SpatialRelationship.Intersects
                        };

                        // 在目标图层中查询与源要素重叠的要素
                        using RowCursor identityCursor = originFeatureClass.Search(spatialFilter);
                        while (identityCursor.MoveNext())
                        {
                            using Feature identityFeature = (Feature)identityCursor.Current;
                            // 获取目标要素的几何
                            ArcGIS.Core.Geometry.Geometry identityGeometry = identityFeature.GetShape();

                            // 计算源要素与目标要素的重叠面积
                            ArcGIS.Core.Geometry.Geometry intersection = GeometryEngine.Instance.Intersection(identityGeometry, originGeometryBuffer);
                            // 如果存在相交，则进行下一步处理
                            if (intersection != null)
                            {
                                // 获取标记的字段值
                                var identityFrom = identityFeature[fc_field_from];
                                if (identityFrom is null) { continue; }
                                string identityFromValue = identityFrom.ToString();

                                // 获取xy
                                var x = identityFeature[xField];
                                if (x is null) { continue; }
                                double xValue = double.Parse(x.ToString());
                                var y = identityFeature[yField];
                                if (y is null) { continue; }
                                double yValue = double.Parse(y.ToString());

                                if (identityFromValue != originFromValue)   // 不是自身相交的情况
                                {
                                    // 获取相邻地块的内部质心点位置
                                    List<double> identyPoint = new List<double>() { xValue, yValue };
                                    // 获取两点之间的角度
                                    double angle = CalculateAngle(identyPoint, originPoint);

                                    // 根据角度判断方向
                                    if (angle >= -45 && angle < 45)
                                    {
                                        // direction = "东方向";
                                        bool isCover = CalAngle(east.Angle, angle);
                                        if (isCover)
                                        {
                                            east.Angle = angle;
                                            east.Lable = identityFromValue;
                                        }
                                    }
                                    else if (angle >= 45 && angle < 135)
                                    {
                                        // direction = "北方向";
                                        bool isCover = CalAngle(north.Angle, angle);
                                        if (isCover)
                                        {
                                            north.Angle = angle;
                                            north.Lable = identityFromValue;
                                        }
                                    }
                                    else if (angle >= 135 || angle < -135)
                                    {
                                        // direction = "西方向";
                                        bool isCover = CalAngle(west.Angle, angle);
                                        if (isCover)
                                        {
                                            west.Angle = angle;
                                            west.Lable = identityFromValue;
                                        }
                                    }
                                    else
                                    {
                                        // direction = "南方向";
                                        bool isCover = CalAngle(south.Angle, angle);
                                        if (isCover)
                                        {
                                            south.Angle = angle;
                                            south.Lable = identityFromValue;
                                        }
                                    }
                                }

                            }
                        }

                        // 先清空再赋值
                        originFeature["东至"] = "";
                        originFeature["西至"] = "";
                        originFeature["南至"] = "";
                        originFeature["北至"] = "";

                        originFeature["东至"] = west.Lable;
                        originFeature["西至"] = east.Lable;
                        originFeature["南至"] = north.Lable;
                        originFeature["北至"] = south.Lable;

                        originFeature.Store();

                        index++;    // 计数加1
                    }
                    pw.AddProcessMessage(30, time_base, $"删除中间字段");
                    // 删除中间字段
                    Arcpy.DeleteField(fc_path, xField);
                    Arcpy.DeleteField(fc_path, yField);
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        // 计算点2相对于点1的角度
        private double CalculateAngle(List<double> point1, List<double> point2)
        {
            double deltaX = point2[0] - point1[0];
            double deltaY = point2[1] - point1[1];
            double radians = Math.Atan2(deltaY, deltaX);
            double angle = radians * (180 / Math.PI);
            return angle;
        }

        // 筛选最正确角度的图斑
        private bool CalAngle(double angle1, double angle2)
        {
            bool isCover = false;

            if (angle1 == -1)   // 初始阶段
            {
                isCover = true;
            }
            else
            {
                if (angle2 >= -45 && angle2 < 45)    // 东面
                {
                    if (Math.Abs(angle2) < Math.Abs(angle1))    // 如果新的点更接近东面
                    {
                        isCover = true;
                    }
                    else
                    {
                        isCover = false;
                    }
                }
                else if (angle2 >= 45 && angle2 < 135)    // 北面
                {
                    double a1 = angle1 - 90;
                    double a2 = angle2 - 90;
                    if (Math.Abs(a2) < Math.Abs(a1))    // 如果新的点更接近北面
                    {
                        isCover = true;
                    }
                    else
                    {
                        isCover = false;
                    }
                }
                else if (angle2 >= 135 && angle2 < -135)    // 西面
                {
                    if (Math.Abs(angle2) > Math.Abs(angle1))    // 如果新的点更接近西面
                    {
                        isCover = true;
                    }
                    else
                    {
                        isCover = false;
                    }
                }
                else    // 南面
                {
                    double a1 = angle1 +90;
                    double a2 = angle2 +90;
                    if (Math.Abs(a2) < Math.Abs(a1))    // 如果新的点更接近南面
                    {
                        isCover = true;
                    }
                    else
                    {
                        isCover = false;
                    }
                }
            }

            return isCover;
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/137835463";
            UITool.Link2Web(url);
        }

        private void combox_field_from_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_field_from);
        }
    }

    public class Dir
    {
        public string Name { get; set; }
        public double Angle { get; set; }
        public string Lable { get; set; }
    }

}
