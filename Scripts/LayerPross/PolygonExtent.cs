using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Geometry = ArcGIS.Core.Geometry.Geometry;

namespace CCTool.Scripts.UI.ProButton
{
    internal class PolygonExtent : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "面要素计算四至";

        protected override async void OnClick()
        {
            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    Map map = MapView.Active.Map;
                    // 获取图层
                    FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

                    // 如果选择的不是面要素或是无选择，则返回
                    if (featureLayer.ShapeType != esriGeometryType.esriGeometryPolygon || featureLayer == null)
                    {
                        MessageBox.Show("错误！请选择一个面要素！");
                        return;
                    }

                    pw.AddProcessMessage(20, "添加字段");
                    // 添加字段
                    Arcpy.AddField(featureLayer.Name, "东_X", "DOUBLE");
                    Arcpy.AddField(featureLayer.Name, "东_Y", "DOUBLE");
                    Arcpy.AddField(featureLayer.Name, "西_X", "DOUBLE");
                    Arcpy.AddField(featureLayer.Name, "西_Y", "DOUBLE");
                    Arcpy.AddField(featureLayer.Name, "北_X", "DOUBLE");
                    Arcpy.AddField(featureLayer.Name, "北_Y", "DOUBLE");
                    Arcpy.AddField(featureLayer.Name, "南_X", "DOUBLE");
                    Arcpy.AddField(featureLayer.Name, "南_Y", "DOUBLE");


                    pw.AddProcessMessage(30, time_base, "计算四至坐标");

                    using (RowCursor rowCursor = featureLayer.Search())
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Feature feature = rowCursor.Current as Feature)
                            {
                                // 标记一个初始坐标
                                double e_x = 0;
                                double n_y = 0;
                                double w_x = 100000000;
                                double s_y = 100000000;

                                Geometry geometry = feature.GetShape();
                                if (geometry is Polygon polygon)
                                {
                                    // 找出四至点
                                    foreach (var pt in polygon.Points)
                                    {
                                        if (pt.X > e_x) { e_x = pt.X; }
                                        if (pt.Y > n_y) { n_y = pt.Y; }
                                        if (pt.X < w_x) { w_x = pt.X; }
                                        if (pt.Y < s_y) { s_y = pt.Y; }
                                    }
                                    // 标记四至点
                                    foreach (var pt in polygon.Points)
                                    {
                                        if (pt.X == w_x)
                                        {
                                            feature["西_X"] = pt.X;
                                            feature["西_Y"] = pt.Y;
                                        }
                                        if (pt.X == e_x)
                                        {
                                            feature["东_X"] = pt.X;
                                            feature["东_Y"] = pt.Y;
                                        }
                                        if (pt.Y == n_y)
                                        {
                                            feature["北_X"] = pt.X;
                                            feature["北_Y"] = pt.Y;
                                        }
                                        if (pt.Y == s_y)
                                        {
                                            feature["南_X"] = pt.X;
                                            feature["南_Y"] = pt.Y;
                                        }
                                    }
                                }
                                feature.Store();
                            }
                        }
                    }
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
                return;
            }
            
            
        }
    }
}
