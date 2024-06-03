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
    internal class ShowYZT_DR : Button
    {

        private YZT_DR _yzt_dr = null;

        protected override void OnClick()
        {
            //already open?
            if (_yzt_dr != null)
                return;
            _yzt_dr = new YZT_DR();
            _yzt_dr.Owner = FrameworkApplication.Current.MainWindow;
            _yzt_dr.Closed += (o, e) => { _yzt_dr = null; };
            _yzt_dr.Show();
            //uncomment for modal
            //_yzt_dr.ShowDialog();
        }

    }
}
