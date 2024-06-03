using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.Formula.PTG;
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
using Polygon = ArcGIS.Core.Geometry.Polygon;
using Polyline = ArcGIS.Core.Geometry.Polyline;
using SpatialReference = ArcGIS.Core.Geometry.SpatialReference;

namespace CCTool.Scripts.DataPross.Excel
{
    /// <summary>
    /// Interaction logic for Polygon2BoundaryPolyline.xaml
    /// </summary>
    public partial class Polygon2BoundaryPolyline : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public Polygon2BoundaryPolyline()
        {
            InitializeComponent();

            combox_ptDigit.Items.Add("1");
            combox_ptDigit.Items.Add("2");
            combox_ptDigit.Items.Add("3");
            combox_ptDigit.Items.Add("4");
            combox_ptDigit.SelectedIndex = 2;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "面要素导出边界线";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void openFeatureClassButton_Click(object sender, RoutedEventArgs e)
        {
            textFeatureClassPath.Text = UITool.SaveDialogFeatureClass();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string in_fc = combox_fc.ComboxText();
                string out_point = textFeatureClassPath.Text;

                string zdh_field = combox_field.ComboxText();

                int ptDigit = int.Parse(combox_ptDigit.Text);

                // 判断参数是否选择完全
                if (in_fc == "" || out_point == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 获取目标数据库和点要素名
                string gdbPath = out_point[..(out_point.IndexOf(".gdb") + 4)];
                string fcName = out_point[(out_point.LastIndexOf(@"\") + 1)..];

                // 判断要素名是不是以数字开头
                bool isNum = fcName.IsNumeric();
                if (isNum)
                {
                    MessageBox.Show("输出的要素名不规范，不能以数字开头！");
                    return;
                }

                await QueuedTask.Run(() =>
                {
                    // 判断输入要素是否有Z值
                    if (in_fc.IsHasZ())
                    {
                        MessageBox.Show("输入的要素有Z值，请先清除掉！");
                        return;
                    }

                });

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);

                Close();
                await QueuedTask.Run(() =>
                {
                    // 判断一下是否存在目标要素，如果有的话，就删掉重建
                    bool isHaveTarget = GisTool.IsHaveFeaturClass(gdbPath, fcName);

                    if (isHaveTarget)
                    {
                        Arcpy.Delect(out_point);
                    }

                    pw.AddProcessMessage(10, time_base, $"获取目标FeatureLayer");
                    // 获取目标FeatureLayer
                    FeatureLayer featurelayer = in_fc.TargetFeatureLayer();
                    // 获取坐标系
                    SpatialReference sr = featurelayer.GetSpatialReference();
                    // 确保要素类的几何类型是多边形
                    if (featurelayer.ShapeType != esriGeometryType.esriGeometryPolygon)
                    {
                        // 如果不是多边形类型，则输出错误信息并退出函数
                        MessageBox.Show("该要素类不是多边形类型。");
                        return;
                    }

                    pw.AddProcessMessage(20, time_base, $"处理面要素，按西北角起始，顺时针重排");

                    Dictionary<List<List<MapPoint>>, string> mapPoints = new Dictionary<List<List<MapPoint>>, string>();
                    // 遍历面要素类中的所有要素
                    RowCursor cursor = featurelayer.GetSelectCursor();
                    while (cursor.MoveNext())
                    {
                        using var feature = cursor.Current as Feature;
                        // 获取要素的几何
                        Polygon geometry = feature.GetShape() as Polygon;

                        if (geometry != null)
                        {
                            string feature_name = "";
                            if (zdh_field != "")
                            {
                                feature_name = feature[zdh_field]?.ToString();   // 宗地号
                            }
                            // 获取面要素的所有折点【按西北角起始，顺时针重排】
                            mapPoints.Add(geometry.ReshotMapPoint(), feature_name);
                        }

                    }

                    pw.AddProcessMessage(10, time_base, "创建线要素");
                    /// 创建线要素
                    // 创建一个ShapeDescription
                    var shapeDescription = new ShapeDescription(GeometryType.Polyline, sr)
                    {
                        HasM = false,
                        HasZ = false
                    };
                    // 定义4个字段
                    var polygonIndex = new ArcGIS.Core.Data.DDL.FieldDescription("原要素编码", FieldType.Integer);
                    var pointIndex = new ArcGIS.Core.Data.DDL.FieldDescription("序号", FieldType.String);
                    var distance = new ArcGIS.Core.Data.DDL.FieldDescription("长度", FieldType.Double);

                    var bs = pointIndex;
                    if (zdh_field != "")
                    {
                        bs = new ArcGIS.Core.Data.DDL.FieldDescription(zdh_field, FieldType.String);
                    }

                    // 打开数据库gdb
                    using (Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath))))
                    {
                        // 收集字段列表
                        var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>()
                        {
                            polygonIndex,pointIndex, distance
                        };

                        if (zdh_field != "")
                        {
                            fieldDescriptions.Add(bs);
                        }

                        // 创建FeatureClassDescription
                        var fcDescription = new FeatureClassDescription(fcName, fieldDescriptions, shapeDescription);
                        // 创建SchemaBuilder
                        SchemaBuilder schemaBuilder = new SchemaBuilder(gdb);
                        // 将创建任务添加到DDL任务列表中
                        schemaBuilder.Create(fcDescription);
                        // 执行DDL
                        bool success = schemaBuilder.Build();

                        // 创建要素并添加到要素类中
                        using (FeatureClass featureClass = gdb.OpenDataset<FeatureClass>(fcName))
                        {
                            /// 构建线要素
                            // 创建编辑操作对象
                            EditOperation editOperation = new EditOperation();
                            editOperation.Callback(context =>
                            {
                                // 获取要素定义
                                FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition();
                                // 循环创建点
                                int index = 1;
                                foreach (var mp in mapPoints)
                                {
                                    int pointIndex = 1;  // 起始点序号

                                    var mpList = mp.Key;
                                    for (int j = 0; j < mpList.Count; j++)
                                    {
                                        for (int k = 0; k < mpList[j].Count - 1; k++)
                                        {
                                            // 创建RowBuffer
                                            using RowBuffer rowBuffer = featureClass.CreateRowBuffer();
                                            // 创建点集合
                                            MapPoint initPt = mpList[j][k];
                                            MapPoint nextPt = mpList[j][k + 1];

                                            List<MapPoint> points = new List<MapPoint>
                                            {
                                                initPt, nextPt
                                            };

                                            // 写入字段值和标记代码
                                            rowBuffer["原要素编码"] = index;

                                            rowBuffer["序号"] = k + 1;

                                            if (zdh_field != "")
                                            {
                                                rowBuffer[zdh_field] = mp.Value;
                                            }

                                            // 距离
                                            var d = GeometryEngine.Instance.Distance(nextPt, initPt);

                                            rowBuffer["长度"] = Math.Round(d, ptDigit);

                                            // 创建线几何
                                            Polyline polylineWithAttrs = PolylineBuilderEx.CreatePolyline(points);

                                            // 给新添加的行设置形状
                                            rowBuffer[featureClassDefinition.GetShapeField()] = polylineWithAttrs;

                                            // 在表中创建新行
                                            using Feature feature = featureClass.CreateRow(rowBuffer);
                                            context.Invalidate(feature);      // 标记行为无效状态

                                            pointIndex++;
                                        }
                                    }
                                    index++;
                                }

                            }, featureClass);

                            // 执行编辑操作
                            editOperation.Execute();
                            // 加载结果图层
                            MapCtlTool.AddFeatureLayerToMap(out_point);
                        }
                    }

                    // 保存
                    Project.Current.SaveEditsAsync();
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
            string url = "https://blog.csdn.net/xcc34452366/article/details/135838160?spm=1001.2014.3001.5502";
            UITool.Link2Web(url);
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_field);
        }
    }
}
