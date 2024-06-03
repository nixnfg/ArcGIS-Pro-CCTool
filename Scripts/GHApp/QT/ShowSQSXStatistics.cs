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

namespace CCTool.Scripts.GHApp.QT
{
    internal class ShowSQSXStatistics : Button
    {

        private SQSXStatistics _sqsxstatistics = null;

        protected override void OnClick()
        {
            //already open?
            if (_sqsxstatistics != null)
                return;
            _sqsxstatistics = new SQSXStatistics();
            _sqsxstatistics.Owner = FrameworkApplication.Current.MainWindow;
            _sqsxstatistics.Closed += (o, e) => { _sqsxstatistics = null; };
            _sqsxstatistics.Show();
            //uncomment for modal
            //_sqsxstatistics.ShowDialog();
        }

    }
}
