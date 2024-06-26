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

namespace CCTool.Scripts.GHApp.SD
{
    internal class ShowStatisticsSDL : Button
    {

        private StatisticsSDL _statisticssdl = null;

        protected override void OnClick()
        {
            //already open?
            if (_statisticssdl != null)
                return;
            _statisticssdl = new StatisticsSDL();
            _statisticssdl.Owner = FrameworkApplication.Current.MainWindow;
            _statisticssdl.Closed += (o, e) => { _statisticssdl = null; };
            _statisticssdl.Show();
            //uncomment for modal
            //_statisticssdl.ShowDialog();
        }

    }
}
