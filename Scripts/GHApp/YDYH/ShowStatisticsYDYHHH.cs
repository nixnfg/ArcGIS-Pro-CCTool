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

namespace CCTool.Scripts.GHApp.YDYH
{
    internal class ShowStatisticsYDYHHH : Button
    {

        private StatisticsYDYHHH _statisticsydyhhh = null;

        protected override void OnClick()
        {
            //already open?
            if (_statisticsydyhhh != null)
                return;
            _statisticsydyhhh = new StatisticsYDYHHH();
            _statisticsydyhhh.Owner = FrameworkApplication.Current.MainWindow;
            _statisticsydyhhh.Closed += (o, e) => { _statisticsydyhhh = null; };
            _statisticsydyhhh.Show();
            //uncomment for modal
            //_statisticsydyhhh.ShowDialog();
        }

    }
}
