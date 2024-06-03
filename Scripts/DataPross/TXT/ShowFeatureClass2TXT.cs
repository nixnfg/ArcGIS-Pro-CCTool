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

namespace CCTool.Scripts.DataPross.TXT
{
    internal class ShowFeatureClass2TXT : Button
    {

        private FeatureClass2TXT _featureclass2txt = null;

        protected override void OnClick()
        {
            //already open?
            if (_featureclass2txt != null)
                return;
            _featureclass2txt = new FeatureClass2TXT();
            _featureclass2txt.Owner = FrameworkApplication.Current.MainWindow;
            _featureclass2txt.Closed += (o, e) => { _featureclass2txt = null; };
            _featureclass2txt.Show();
            //uncomment for modal
            //_featureclass2txt.ShowDialog();
        }

    }
}
