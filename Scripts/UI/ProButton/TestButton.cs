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
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using CCTool.Scripts.ToolManagers;
using NPOI.SS.Util;
using NPOI.SS.Formula.Functions;
using Aspose.Cells;
using Aspose.Cells.Rendering;
using Aspose.Cells.Drawing;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.Util;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math.EC;
using ArcGIS.Core.Internal.CIM;
using Polyline = ArcGIS.Core.Geometry.Polyline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Org.BouncyCastle.Math.Primes;
using NPOI.HPSF;
using Microsoft.VisualBasic;
using static NPOI.HSSF.UserModel.HeaderFooter;
using static System.Windows.Forms.MonthCalendar;
using System.Reflection;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;
using System.Windows.Controls;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment;
using HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment;
using NPOI.OpenXmlFormats.Vml;
using ArcGIS.Desktop.Internal.Catalog;
using static NPOI.POIFS.Crypt.CryptoFunctions;
using Camera = ArcGIS.Desktop.Mapping.Camera;
using Aspose.Words.Drawing;
using System.Threading;
using MathNet.Numerics.Distributions;
using ArcGIS.Desktop.Internal.GeoProcessing;
using System.Runtime.Intrinsics.Arm;
using ArcGIS.Desktop.Internal.Mapping.Ribbon;
using ArcGIS.Core.Internal.Geometry;

namespace CCTool.Scripts.UI.ProButton
{
    internal class TestButton : Button
    {

        // 定义一个进度框
        private ProcessWindow processwindow = null;

        protected override void OnClick()
        {
            //Ogr.RegisterAll();// 注册所有的驱动
            //Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            //Gdal.SetConfigOption("SHAPE_ENCODING", "");

            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库

            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, "进度");
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行工具…………" + time_base + "\r", Brushes.Green);

                pw.AddProcessMessage(100, time_base, "工具运行完成", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

    }
}
