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
    internal class ShowFindAcuteAngle : Button
    {

        private FindAcuteAngle _findacuteangle = null;

        protected override void OnClick()
        {
            //already open?
            if (_findacuteangle != null)
                return;
            _findacuteangle = new FindAcuteAngle();
            _findacuteangle.Owner = FrameworkApplication.Current.MainWindow;
            _findacuteangle.Closed += (o, e) => { _findacuteangle = null; };
            _findacuteangle.Show();
            //uncomment for modal
            //_findacuteangle.ShowDialog();
        }

    }
}
