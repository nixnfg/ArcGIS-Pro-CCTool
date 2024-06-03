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
using CCTool.Scripts.MessageWindow;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.UI.ProMapTool
{
    internal class GetMapCoordinates : MapTool
    {
        public GetMapCoordinates()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
            UseSnapping= true;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
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

        // 定义一个进度框
        private XYInfoWindow _xyinfowindow = null;

        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            return QueuedTask.Run(() =>
            {
                // 将鼠标点击的位置转换为地图坐标
                var mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
                // 十进制坐标
                double x_dec = mapPoint.X;
                double y_dec = mapPoint.Y;

                // 在UI线程上执行添加item的操作
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // 打开XY信息框
                    XYInfoWindow xyw = UITool.OpenXYInfoWindow(_xyinfowindow);
                    xyw.AddXYInfo(x_dec, y_dec);
                });
            });
        }
    }
}
