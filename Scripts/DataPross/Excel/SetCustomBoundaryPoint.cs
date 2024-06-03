using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.MessageWindow;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.DataPross.Excel
{
    internal class SetCustomBoundaryPoint : MapTool
    {
        public SetCustomBoundaryPoint()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
            UseSnapping= true;
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                e.Handled = true; //Handle the event args to get the call to the corresponding async method
        }

        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            return QueuedTask.Run(() =>
            {
                // 将鼠标点击的位置转换为地图坐标
                var mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
                // 十进制坐标
                double x_dec = mapPoint.X;
                double y_dec = mapPoint.Y;
                List<double> xy = new List<double>() { x_dec, y_dec};

                // 获取活动地图视图中选定的要素集合
                var selectedSet = MapView.Active.Map.GetSelection();
                // 如果没有选择图斑，返回
                if (selectedSet.Count == 0)
                {
                    MessageBox.Show("请选择一个图斑。");
                    return;
                }

                // 将选定的要素集合转换为字典形式，就取第一个
                var layer = selectedSet.ToDictionary().FirstOrDefault();

                // 获取图层
                FeatureLayer featurelayer = layer.Key as FeatureLayer;
                // 获取cursor
                RowCursor cursor = featurelayer.GetSelectCursor();

                while (cursor.MoveNext())
                {
                    using var feature = cursor.Current as Feature;
                    // 获取要素的几何
                    Geometry geometry = feature.GetShape();

                    if (geometry.GeometryType != GeometryType.Polygon)
                    {
                        MessageBox.Show("请选择一个面要素图斑。");
                        return;
                    }

                    Polygon polygon = geometry as Polygon;

                    if (geometry != null)
                    {
                        // 面要素的所有折点进行重排【按西北角起始，顺时针重排】
                        Polygon resultPolygon = polygon.ReshotMapPointReturnPolygonByCustom(xy);
                        // 重新设置要素并保存
                        feature.SetShape(resultPolygon);
                        feature.Store();
                    }
                }

                MessageBox.Show("起始点设置成功。");
            });
        }
    }
}
