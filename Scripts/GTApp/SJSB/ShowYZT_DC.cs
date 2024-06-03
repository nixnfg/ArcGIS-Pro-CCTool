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

namespace CCTool.Scripts.GTApp.SJSB
{
    internal class ShowYZT_DC : Button
    {

        private YZT_DC _yzt_dc = null;

        protected override void OnClick()
        {
            //already open?
            if (_yzt_dc != null)
                return;
            _yzt_dc = new YZT_DC();
            _yzt_dc.Owner = FrameworkApplication.Current.MainWindow;
            _yzt_dc.Closed += (o, e) => { _yzt_dc = null; };
            _yzt_dc.Show();
            //uncomment for modal
            //_yzt_dc.ShowDialog();
        }

    }
}
