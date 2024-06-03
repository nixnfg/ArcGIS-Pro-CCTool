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
    internal class ShowZF_DR : Button
    {

        private ZF_DR _zf_dr = null;

        protected override void OnClick()
        {
            //already open?
            if (_zf_dr != null)
                return;
            _zf_dr = new ZF_DR();
            _zf_dr.Owner = FrameworkApplication.Current.MainWindow;
            _zf_dr.Closed += (o, e) => { _zf_dr = null; };
            _zf_dr.Show();
            //uncomment for modal
            //_zf_dr.ShowDialog();
        }

    }
}
