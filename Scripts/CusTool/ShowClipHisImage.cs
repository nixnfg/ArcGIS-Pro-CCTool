﻿using ArcGIS.Core.CIM;
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
    internal class ShowClipHisImage : Button
    {

        private ClipHisImage _cliphisimage = null;

        protected override void OnClick()
        {
            //already open?
            if (_cliphisimage != null)
                return;
            _cliphisimage = new ClipHisImage();
            _cliphisimage.Owner = FrameworkApplication.Current.MainWindow;
            _cliphisimage.Closed += (o, e) => { _cliphisimage = null; };
            _cliphisimage.Show();
            //uncomment for modal
            //_cliphisimage.ShowDialog();
        }

    }
}
