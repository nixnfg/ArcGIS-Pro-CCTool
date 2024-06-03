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

namespace CCTool.Scripts.DataPross.FeatureClasses
{
    internal class ShowDissolveAsField : Button
    {

        private DissolveAsField _dissolveasfield = null;

        protected override void OnClick()
        {
            //already open?
            if (_dissolveasfield != null)
                return;
            _dissolveasfield = new DissolveAsField();
            _dissolveasfield.Owner = FrameworkApplication.Current.MainWindow;
            _dissolveasfield.Closed += (o, e) => { _dissolveasfield = null; };
            _dissolveasfield.Show();
            //uncomment for modal
            //_dissolveasfield.ShowDialog();
        }

    }
}
