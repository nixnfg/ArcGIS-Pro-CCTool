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
    internal class ShowZJGG_DC : Button
    {

        private ZJGG_DC _zjgg_dc = null;

        protected override void OnClick()
        {
            //already open?
            if (_zjgg_dc != null)
                return;
            _zjgg_dc = new ZJGG_DC();
            _zjgg_dc.Owner = FrameworkApplication.Current.MainWindow;
            _zjgg_dc.Closed += (o, e) => { _zjgg_dc = null; };
            _zjgg_dc.Show();
            //uncomment for modal
            //_zjgg_dc.ShowDialog();
        }

    }
}
