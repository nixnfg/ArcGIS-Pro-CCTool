using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace CCTool.Scripts.DataPross.TXT
{
    internal class ShowTXT2SHP : Button
{

    private TXT2SHP _txt2shp = null;

    protected override void OnClick()
    {
        //already open?
        if (_txt2shp != null)
            return;
        _txt2shp = new TXT2SHP();
        _txt2shp.Owner = FrameworkApplication.Current.MainWindow;
        _txt2shp.Closed += (o, e) => { _txt2shp = null; };
        _txt2shp.Show();
         //uncomment for modal
         //_txt2shp.ShowDialog();
}

}
}
