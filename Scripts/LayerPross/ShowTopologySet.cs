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
    internal class ShowTopologySet : Button
    {

        private TopologySet _topologyset = null;

        protected override void OnClick()
        {
            //already open?
            if (_topologyset != null)
                return;
            _topologyset = new TopologySet();
            _topologyset.Owner = FrameworkApplication.Current.MainWindow;
            _topologyset.Closed += (o, e) => { _topologyset = null; };
            _topologyset.Show();
            //uncomment for modal
            //_topologyset.ShowDialog();
        }

    }
}
