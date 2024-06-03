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

namespace CCTool.Scripts.FeaturePross
{
    internal class ShowAreaStatisticsByField : Button
    {

        private AreaStatisticsByField _areastatisticsbyfield = null;

        protected override void OnClick()
        {
            //already open?
            if (_areastatisticsbyfield != null)
                return;
            _areastatisticsbyfield = new AreaStatisticsByField();
            _areastatisticsbyfield.Owner = FrameworkApplication.Current.MainWindow;
            _areastatisticsbyfield.Closed += (o, e) => { _areastatisticsbyfield = null; };
            _areastatisticsbyfield.Show();
            //uncomment for modal
            //_areastatisticsbyfield.ShowDialog();
        }

    }
}
