﻿using ArcGIS.Core.CIM;
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

namespace CCTool.Scripts.DataPross.Excel
{
    internal class ShowPolygon2BoundaryPolyline : Button
    {

        private Polygon2BoundaryPolyline _polygon2boundarypolyline = null;

        protected override void OnClick()
        {
            //already open?
            if (_polygon2boundarypolyline != null)
                return;
            _polygon2boundarypolyline = new Polygon2BoundaryPolyline();
            _polygon2boundarypolyline.Owner = FrameworkApplication.Current.MainWindow;
            _polygon2boundarypolyline.Closed += (o, e) => { _polygon2boundarypolyline = null; };
            _polygon2boundarypolyline.Show();
            //uncomment for modal
            //_polygon2boundarypolyline.ShowDialog();
        }

    }
}
