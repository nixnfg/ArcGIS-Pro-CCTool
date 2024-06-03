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

namespace CCTool.Scripts.CusTool
{
    internal class ShowYED2SD : Button
    {

        private YED2SD _yed2sd = null;

        protected override void OnClick()
        {
            //already open?
            if (_yed2sd != null)
                return;
            _yed2sd = new YED2SD();
            _yed2sd.Owner = FrameworkApplication.Current.MainWindow;
            _yed2sd.Closed += (o, e) => { _yed2sd = null; };
            _yed2sd.Show();
            //uncomment for modal
            //_yed2sd.ShowDialog();
        }

    }
}
