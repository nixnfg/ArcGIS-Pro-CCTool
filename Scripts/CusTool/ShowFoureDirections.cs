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
    internal class ShowFoureDirections : Button
    {

        private FoureDirections _fouredirections = null;

        protected override void OnClick()
        {
            //already open?
            if (_fouredirections != null)
                return;
            _fouredirections = new FoureDirections();
            _fouredirections.Owner = FrameworkApplication.Current.MainWindow;
            _fouredirections.Closed += (o, e) => { _fouredirections = null; };
            _fouredirections.Show();
            //uncomment for modal
            //_fouredirections.ShowDialog();
        }

    }
}
