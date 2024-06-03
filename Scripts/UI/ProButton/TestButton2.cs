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
using CCTool.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using Button = ArcGIS.Desktop.Framework.Contracts.Button;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using System.Windows.Forms;
using ArcGIS.Desktop.Mapping;
using Microsoft.Office.Core;
using ArcGIS.Core.Data.DDL;
using FieldDescription = ArcGIS.Core.Data.DDL.FieldDescription;
using Row = ArcGIS.Core.Data.Row;
using ArcGIS.Desktop.Editing.Attributes;
using System.Security.Cryptography;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Core.Internal.CIM;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Net;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.IO.Path;
using ArcGIS.Desktop.Internal.Catalog.Wizards;
using ArcGIS.Desktop.Internal.Layouts.Utilities;
using System.Windows.Documents;
using ActiproSoftware.Windows;
using System.Windows;
using System.Runtime.InteropServices;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.GeoProcessing;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using Polygon = ArcGIS.Core.Geometry.Polygon;
using ArcGIS.Core.Data.Exceptions;
using Table = ArcGIS.Core.Data.Table;
using SpatialReference = ArcGIS.Core.Geometry.SpatialReference;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using CCTool.Scripts.ToolManagers;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using static System.Windows.Forms.MonthCalendar;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using Aspose.Cells.Drawing;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using Aspose.Cells;
using Aspose.Words;

namespace CCTool.Scripts.UI.ProButton
{
    internal class TestButton2 : Button
    {

        // 定义一个进度框
        private ProcessWindow processwindow = null;

        protected override async void OnClick()
        {

            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, "进度");
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);

                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;

                string lyName = "分地块";

                List<string> result = new List<string>();

                await QueuedTask.Run(() =>
                {
                    FeatureClass featureClass = lyName.TargetFeatureClass();

                    // 逐行游标
                    using RowCursor rowCursor = featureClass.Search();
                    while (rowCursor.MoveNext())
                    {
                        using Feature feature = rowCursor.Current as Feature;

                        string va = feature["Name"].ToString();

                        Arcpy.CopyFeatures(feature, @$"{gdb_path}\{va}_结果");
                    }
                });

                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}

