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
    internal class ShowZF_DC : Button
    {

        private ZF_DC _zf_dc = null;

        protected override void OnClick()
        {
            //already open?
            if (_zf_dc != null)
                return;
            _zf_dc = new ZF_DC();
            _zf_dc.Owner = FrameworkApplication.Current.MainWindow;
            _zf_dc.Closed += (o, e) => { _zf_dc = null; };
            _zf_dc.Show();
            //uncomment for modal
            //_zf_dc.ShowDialog();
        }

    }
}
