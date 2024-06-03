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
using Polygon = ArcGIS.Core.Geometry.Polygon;
using Polyline = ArcGIS.Core.Geometry.Polyline;
using SpatialReference = ArcGIS.Core.Geometry.SpatialReference;

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for Polygon2BoundaryPolyline2.xaml
    /// </summary>
    public partial class Polygon2BoundaryPolyline2 : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public Polygon2BoundaryPolyline2()
        {
            InitializeComponent();

            // 初始化combox
            combox_sr.Items.Add("CGCS2000");
            combox_sr.Items.Add("北京54");
            combox_sr.Items.Add("西安80");
            combox_sr.Items.Add("WGS1984");
            combox_sr.SelectedIndex = 0;

        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "界线导出Excel";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string in_fc = combox_fc.ComboxText();
                string excelPath = textExcelPath.Text;
                string bjField = combox_field.ComboxText();

                string srName = combox_sr.Text;

                int wid = srName switch
                {
                    "CGCS2000" =>4490,
                    "北京54" =>4214,
                    "西安80" =>4610,
                    "WGS1984" =>4326,
                    _ => 4490,
                };


                // 判断参数是否选择完全
                if (in_fc == "" || excelPath == "" || bjField == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
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
                    pw.AddProcessMessage(10, time_base, $"获取目标FeatureLayer");
                    // 获取目标FeatureLayer
                    FeatureLayer featurelayer = in_fc.TargetFeatureLayer();

                    // 确保要素类的几何类型是多边形
                    if (featurelayer.ShapeType != esriGeometryType.esriGeometryPolygon)
                    {
                        // 如果不是多边形类型，则输出错误信息并退出函数
                        MessageBox.Show("该要素类不是多边形类型。");
                        return;
                    }

                    // 复制Excel表
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.界线描述表.xlsx", excelPath);

                    // 遍历面要素类中的所有要素
                    RowCursor cursor = featurelayer.GetSelectCursor();
                    while (cursor.MoveNext())
                    {
                        using var feature = cursor.Current as Feature;
                        // 获取要素的几何
                        Polygon polygon = feature.GetShape() as Polygon;

                        // 坐标系转成地理坐标
                        SpatialReference sr = SpatialReferenceBuilder.CreateSpatialReference(wid);
                        Polygon projectPolygon = GeometryEngine.Instance.Project(polygon, sr) as Polygon;

                        // 获取面要素的所有点，并按逆时针排序
                        List<List<MapPoint>> mapPoints = projectPolygon.ReshotMapPointWise();
                        // 原来的点集也留一个，用来算界线距离
                        List<List<MapPoint>> originMapPoints = polygon.ReshotMapPointWise();

                        // 获取标记字段
                        string bj = feature[bjField]?.ToString();

                        pw.AddProcessMessage(20, time_base, $"处理图斑： {bj}");

                        // 复制一个sheet
                        OfficeTool.ExcelCopySheet(excelPath, "sheet1", bj);

                        // 打开工作薄
                        Workbook wb = OfficeTool.OpenWorkbook(excelPath);
                        // 打开工作表
                        Worksheet worksheet = wb.Worksheets[bj];
                        // 获取Cells
                        Cells cells = worksheet.Cells;
                        // 当前行
                        int initRow = 2;
                        for (int i = 0; i < mapPoints.Count; i++)
                        {
                            // 当前环的线段号
                            int lineIndex = 1;
                            for (int j = 0; j < mapPoints[i].Count - 1; j++)
                            {
                                // 如果不是第一行，就复制一行
                                if (i != 0 || j != 0)
                                {
                                    cells.CopyRow(cells, 2, initRow);
                                }

                                // 写入点号
                                cells[initRow, 0].Value = $"{lineIndex}";

                                // 计算线段起始点的经纬度坐标
                                double x = mapPoints[i][j].X;
                                double y = mapPoints[i][j].Y;
                                double xx = mapPoints[i][j + 1].X;
                                double yy = mapPoints[i][j + 1].Y;

                                // 写入线段起始点的经纬度
                                cells[initRow, 1].Value = x.ToDecimal();
                                cells[initRow, 2].Value = y.ToDecimal();
                                cells[initRow, 3].Value = xx.ToDecimal();
                                cells[initRow, 4].Value = yy.ToDecimal();

                                // 计算线段起始点的平面投影坐标
                                double ox = originMapPoints[i][j].X;
                                double oy = originMapPoints[i][j].Y;
                                double oxx = originMapPoints[i][j + 1].X;
                                double oyy = originMapPoints[i][j + 1].Y;
                                // 计算当起终点的距离
                                double distance = Math.Round(Math.Sqrt(Math.Pow(oxx - ox, 2) + Math.Pow(oyy - oy, 2)),2);
                                // 写入距离
                                cells[initRow, 7].Value = distance;

                                // 获取起终点的角度
                                List<double> xy1 = new List<double>() { mapPoints[i][j].X, mapPoints[i][j].Y };
                                List<double> xy2 = new List<double>() { mapPoints[i][j + 1].X, mapPoints[i][j + 1].Y };
                                double angle = BaseTool.CalculateAngleFromNorth(xy1, xy2);

                                // 写入角度
                                cells[initRow, 5].Value = $"{Math.Round(angle, 2)}°";

                                // 判断方向
                                string direction = JudgeDirection(angle);
                                // 写入方向
                                cells[initRow, 6].Value = $"{direction}";

                                // 线段号加一
                                lineIndex++;
                                // 当前行进一行
                                initRow++;
                            }
                        }

                        // 保存
                        wb.Save(excelPath);
                        wb.Dispose();
                    }

                    // 删除sheet1
                    OfficeTool.ExcelDeleteSheet(excelPath, "sheet1");

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
            string url = "https://blog.csdn.net/xcc34452366/article/details/138913392";
            UITool.Link2Web(url);
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_field);
        }

        // 判断方向
        public static string JudgeDirection(double angle)
        {
            string d = "";
            if (angle >= 348.75 || angle < 11.25)
            {
                d = "南";
            }
            else if (angle >= 11.25 && angle < 33.75)
            {
                d = "南西南";
            }
            else if (angle >= 33.75 && angle < 56.25)
            {
                d = "西南";
            }
            else if (angle >= 56.25 && angle < 78.75)
            {
                d = "西西南";
            }

            /////////
            if (angle >= 78.75 && angle < 101.25)
            {
                d = "西";
            }
            else if (angle >= 101.25 && angle < 123.75)
            {
                d = "西西北";
            }
            else if (angle >= 123.75 && angle < 146.25)
            {
                d = "西北";
            }
            else if (angle >= 146.25 && angle < 168.75)
            {
                d = "北西北";
            }


            /////////
            if (angle >= 168.75 && angle < 191.25)
            {
                d = "北";
            }
            else if (angle >= 191.25 && angle < 213.75)
            {
                d = "北东北";
            }
            else if (angle >= 213.75 && angle < 236.25)
            {
                d = "东北";
            }
            else if (angle >= 236.25 && angle < 258.75)
            {
                d = "东东北";
            }


            /////////
            if (angle >= 258.75 && angle < 281.25)
            {
                d = "东";
            }
            else if (angle >= 281.25 && angle < 303.75)
            {
                d = "东东南";
            }
            else if (angle >= 303.75 && angle < 326.25)
            {
                d = "东南";
            }
            else if (angle >= 326.25 && angle < 348.75)
            {
                d = "南东南";
            }

            return d;
        }

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            textExcelPath.Text = UITool.SaveDialogExcel();
        }
    }
}
