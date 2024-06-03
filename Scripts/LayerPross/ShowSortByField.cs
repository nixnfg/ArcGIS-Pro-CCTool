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

namespace CCTool.Scripts.LayerPross
{
    internal class ShowSortByField : Button
    {

        private SortByField _sortbyfield = null;

        protected override void OnClick()
        {
            //already open?
            if (_sortbyfield != null)
                return;
            _sortbyfield = new SortByField();
            _sortbyfield.Owner = FrameworkApplication.Current.MainWindow;
            _sortbyfield.Closed += (o, e) => { _sortbyfield = null; };
            _sortbyfield.Show();
            //uncomment for modal
            //_sortbyfield.ShowDialog();
        }

    }
}
