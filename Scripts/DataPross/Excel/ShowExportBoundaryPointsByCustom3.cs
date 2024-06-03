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

namespace CCTool.Scripts.DataPross.Excel
{
    internal class ShowExportBoundaryPointsByCustom3 : Button
    {

        private ExportBoundaryPointsByCustom3 _exportboundarypointsbycustom3 = null;

        protected override void OnClick()
        {
            //already open?
            if (_exportboundarypointsbycustom3 != null)
                return;
            _exportboundarypointsbycustom3 = new ExportBoundaryPointsByCustom3();
            _exportboundarypointsbycustom3.Owner = FrameworkApplication.Current.MainWindow;
            _exportboundarypointsbycustom3.Closed += (o, e) => { _exportboundarypointsbycustom3 = null; };
            _exportboundarypointsbycustom3.Show();
            //uncomment for modal
            //_exportboundarypointsbycustom3.ShowDialog();
        }

    }
}
