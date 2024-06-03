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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.CusTool
{
    internal class ShowPolygon2BoundaryPolyline2 : Button
    {

        private Polygon2BoundaryPolyline2 _polygon2boundarypolyline2 = null;

        protected override void OnClick()
        {
            //already open?
            if (_polygon2boundarypolyline2 != null)
                return;
            _polygon2boundarypolyline2 = new Polygon2BoundaryPolyline2();
            _polygon2boundarypolyline2.Owner = FrameworkApplication.Current.MainWindow;
            _polygon2boundarypolyline2.Closed += (o, e) => { _polygon2boundarypolyline2 = null; };
            _polygon2boundarypolyline2.Show();
            //uncomment for modal
            //_polygon2boundarypolyline2.ShowDialog();
        }

    }
}
