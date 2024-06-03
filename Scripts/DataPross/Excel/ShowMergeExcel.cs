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

namespace CCTool.Scripts.DataPross.Excel
{
    internal class ShowMergeExcel : Button
    {

        private MergeExcel _mergeexcel = null;

        protected override void OnClick()
        {
            //already open?
            if (_mergeexcel != null)
                return;
            _mergeexcel = new MergeExcel();
            _mergeexcel.Owner = FrameworkApplication.Current.MainWindow;
            _mergeexcel.Closed += (o, e) => { _mergeexcel = null; };
            _mergeexcel.Show();
            //uncomment for modal
            //_mergeexcel.ShowDialog();
        }

    }
}
