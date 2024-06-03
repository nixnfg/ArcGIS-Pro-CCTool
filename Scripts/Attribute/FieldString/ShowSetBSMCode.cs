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

namespace CCTool.Scripts.Attribute.FieldString
{
    internal class ShowSetBSMCode : Button
{

    private SetBSMCode _setbsmcode = null;

    protected override void OnClick()
    {
        //already open?
        if (_setbsmcode != null)
            return;
        _setbsmcode = new SetBSMCode();
        _setbsmcode.Owner = FrameworkApplication.Current.MainWindow;
        _setbsmcode.Closed += (o, e) => { _setbsmcode = null; };
        _setbsmcode.Show();
         //uncomment for modal
         //_setbsmcode.ShowDialog();
}

}
}
