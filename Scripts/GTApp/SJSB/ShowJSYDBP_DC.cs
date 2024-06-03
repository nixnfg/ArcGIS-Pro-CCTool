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
    internal class ShowJSYDBP_DC : Button
    {

        private JSYDBP_DC _jsydbp_dc = null;

        protected override void OnClick()
        {
            //already open?
            if (_jsydbp_dc != null)
                return;
            _jsydbp_dc = new JSYDBP_DC();
            _jsydbp_dc.Owner = FrameworkApplication.Current.MainWindow;
            _jsydbp_dc.Closed += (o, e) => { _jsydbp_dc = null; };
            _jsydbp_dc.Show();
            //uncomment for modal
            //_jsydbp_dc.ShowDialog();
        }

    }
}
