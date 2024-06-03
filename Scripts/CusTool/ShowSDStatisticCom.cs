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
    internal class ShowSDStatisticCom : Button
    {

        private SDStatisticCom _sdstatisticcom = null;

        protected override void OnClick()
        {
            //already open?
            if (_sdstatisticcom != null)
                return;
            _sdstatisticcom = new SDStatisticCom();
            _sdstatisticcom.Owner = FrameworkApplication.Current.MainWindow;
            _sdstatisticcom.Closed += (o, e) => { _sdstatisticcom = null; };
            _sdstatisticcom.Show();
            //uncomment for modal
            //_sdstatisticcom.ShowDialog();
        }

    }
}
