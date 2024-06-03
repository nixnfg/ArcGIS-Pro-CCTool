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

namespace CCTool.Scripts.GTApp.SJSB
{
    internal class ShowGD_CD : Button
    {

        private GD_CD _gd_cd = null;

        protected override void OnClick()
        {
            //already open?
            if (_gd_cd != null)
                return;
            _gd_cd = new GD_CD();
            _gd_cd.Owner = FrameworkApplication.Current.MainWindow;
            _gd_cd.Closed += (o, e) => { _gd_cd = null; };
            _gd_cd.Show();
            //uncomment for modal
            //_gd_cd.ShowDialog();
        }

    }
}
