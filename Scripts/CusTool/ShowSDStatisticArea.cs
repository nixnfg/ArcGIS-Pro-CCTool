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
    internal class ShowSDStatisticArea : Button
    {

        private SDStatisticArea _sdstatisticarea = null;

        protected override void OnClick()
        {
            //already open?
            if (_sdstatisticarea != null)
                return;
            _sdstatisticarea = new SDStatisticArea();
            _sdstatisticarea.Owner = FrameworkApplication.Current.MainWindow;
            _sdstatisticarea.Closed += (o, e) => { _sdstatisticarea = null; };
            _sdstatisticarea.Show();
            //uncomment for modal
            //_sdstatisticarea.ShowDialog();
        }

    }
}
