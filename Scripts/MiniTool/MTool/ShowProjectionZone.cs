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

namespace CCTool.Scripts.MiniTool.MTool
{
    internal class ShowProjectionZone : Button
    {

        private ProjectionZone _projectionzone = null;

        protected override void OnClick()
        {
            //already open?
            if (_projectionzone != null)
                return;
            _projectionzone = new ProjectionZone();
            _projectionzone.Owner = FrameworkApplication.Current.MainWindow;
            _projectionzone.Closed += (o, e) => { _projectionzone = null; };
            _projectionzone.Show();
            //uncomment for modal
            //_projectionzone.ShowDialog();
        }

    }
}
