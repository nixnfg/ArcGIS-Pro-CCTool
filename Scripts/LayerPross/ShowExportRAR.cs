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

namespace CCTool.Scripts.LayerPross
{
    internal class ShowExportRAR : Button
    {

        private ExportRAR _exportrar = null;

        protected override void OnClick()
        {
            //already open?
            if (_exportrar != null)
                return;
            _exportrar = new ExportRAR();
            _exportrar.Owner = FrameworkApplication.Current.MainWindow;
            _exportrar.Closed += (o, e) => { _exportrar = null; };
            _exportrar.Show();
            //uncomment for modal
            //_exportrar.ShowDialog();
        }

    }
}
