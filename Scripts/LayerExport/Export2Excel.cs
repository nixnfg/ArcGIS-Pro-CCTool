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
using CCTool.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.LayerExport
{
    internal class Export2Excel : Button
    {
        protected override async void OnClick()
        {
            try
            {
                // 获取当前选择的图层
                FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                StandaloneTable standaloneTable = MapView.Active.GetSelectedStandaloneTables().FirstOrDefault();
                if (featureLayer is null && standaloneTable is null)
                {
                    MessageBox.Show("请选择一个要素图层或独立表！");
                    return;
                }
                //  获取导出的excel路径
                string excelPath = UITool.SaveDialogExcel();

                await QueuedTask.Run(() =>
                {
                    if (excelPath!=null)
                    {
                        //  导出
                        Arcpy.TableToExcel(featureLayer, excelPath);
                        MessageBox.Show("导出Excel成功！");
                    }
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.Message);
                return;
            }
            

        }
    }
}
