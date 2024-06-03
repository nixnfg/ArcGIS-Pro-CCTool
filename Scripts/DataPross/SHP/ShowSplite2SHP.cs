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

namespace CCTool.Scripts.DataPross.SHP
{
    internal class ShowSplite2SHP : Button
    {

        private Splite2SHP _splite2shp = null;

        protected override void OnClick()
        {
            //already open?
            if (_splite2shp != null)
                return;
            _splite2shp = new Splite2SHP();
            _splite2shp.Owner = FrameworkApplication.Current.MainWindow;
            _splite2shp.Closed += (o, e) => { _splite2shp = null; };
            _splite2shp.Show();
            //uncomment for modal
            //_splite2shp.ShowDialog();
        }

    }
}
