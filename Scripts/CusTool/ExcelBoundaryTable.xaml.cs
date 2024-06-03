using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Framework.Utilities;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Range = Aspose.Cells.Range;

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for ExcelBoundaryTable.xaml
    /// </summary>
    public partial class ExcelBoundaryTable : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ExcelBoundaryTable()
        {
            InitializeComponent();

            // 初始化combox
            //string lyName = "发送2";
            //UITool.InitFeatureLayerToComboxPlus(combox_fc, lyName);
            //textFolderPath.Text = @"C:\Users\Administrator\Desktop\新输出";

            UITool.InitFieldToComboxPlus(combox_zddm, "ZDDM", "string");
            UITool.InitFieldToComboxPlus(combox_qlr, "QSDW", "string");
            UITool.InitFieldToComboxPlus(combox_zl, "坐落", "string");
            UITool.InitFieldToComboxPlus(combox_fl, "法人", "string");
            UITool.InitFieldToComboxPlus(combox_sfz, "法人SHZ", "string");
            UITool.InitFieldToComboxPlus(combox_tfh, "TFH", "string");
            UITool.InitFieldToComboxPlus(combox_zdmj, "ZDMJ", "float");
            UITool.InitFieldToComboxPlus(combox_zzjgdm, "组织JGDM", "string");
            UITool.InitFieldToComboxPlus(combox_bdcdyh, "BDCDYH", "string");

            UITool.InitFieldToComboxPlus(combox_bz, "ZDSZB", "string");
            UITool.InitFieldToComboxPlus(combox_dz, "ZDSZD", "string");
            UITool.InitFieldToComboxPlus(combox_nz, "ZDSZN", "string");
            UITool.InitFieldToComboxPlus(combox_xz, "ZDSZX", "string");

            UITool.InitFieldToComboxPlus(combox_nyd, "NYDMJ", "float");
            UITool.InitFieldToComboxPlus(combox_gd, "GDMJ", "float");
            UITool.InitFieldToComboxPlus(combox_ld, "LDMJ", "float");
            UITool.InitFieldToComboxPlus(combox_cd, "CDMJ", "float");
            UITool.InitFieldToComboxPlus(combox_qt, "QTYDMJ", "float");
            UITool.InitFieldToComboxPlus(combox_jsyd, "JSYDMJ", "float");
            UITool.InitFieldToComboxPlus(combox_wlyd, "WLYDMJ", "float");
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "权属调查表(伊)";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string in_fc = combox_fc.ComboxText();
                string excel_folder = textFolderPath.Text;

                string field_zddm = combox_zddm.ComboxText();
                string field_qlr = combox_qlr.ComboxText();
                string field_zl = combox_zl.ComboxText();
                string field_fl = combox_fl.ComboxText();
                string field_sfz = combox_sfz.ComboxText();
                string field_tfh = combox_tfh.ComboxText();
                string field_zdmj = combox_zdmj.ComboxText();
                string field_zzjgdm = combox_zzjgdm.ComboxText();
                string field_bdcdyh = combox_bdcdyh.ComboxText();

                string field_bz = combox_bz.ComboxText();
                string field_dz = combox_dz.ComboxText();
                string field_nz = combox_nz.ComboxText();
                string field_xz = combox_xz.ComboxText();

                string field_nyd = combox_nyd.ComboxText();
                string field_gd = combox_gd.ComboxText();
                string field_ld = combox_ld.ComboxText();
                string field_cd = combox_cd.ComboxText();
                string field_qt = combox_qt.ComboxText();
                string field_jsyd = combox_jsyd.ComboxText();
                string field_wlyd = combox_wlyd.ComboxText();

                // 判断参数是否选择完全
                if (in_fc == "" || excel_folder == "" || field_zddm == "" || field_qlr == "" || field_zl == ""
                    || field_fl == "" || field_sfz == "" || field_tfh == "" || field_zdmj == "" || field_zzjgdm == ""
                    || field_bdcdyh == "" || field_bz == "" || field_dz == "" || field_nz == "" || field_xz == ""
                    || field_nyd == "" || field_gd == "" || field_ld == "" || field_cd == "" || field_qt == ""
                    || field_jsyd == "" || field_wlyd == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);
                Close();

                await QueuedTask.Run(() =>
                {
                    pw.AddMessage("获取目标FeatureLayer");
                    // 获取目标FeatureLayer
                    FeatureLayer featurelayer = in_fc.TargetFeatureLayer();
                    // 确保要素类的几何类型是多边形
                    if (featurelayer.ShapeType != esriGeometryType.esriGeometryPolygon)
                    {
                        // 如果不是多边形类型，则输出错误信息并退出函数
                        MessageBox.Show("该要素类不是多边形类型。");
                        return;
                    }

                    // 遍历面要素类中的所有要素
                    RowCursor cursor = featurelayer.GetSelectCursor();
                    while (cursor.MoveNext())
                    {
                        using var feature = cursor.Current as Feature;
                        // 获取参数
                        string oidField = GisTool.GetIDFieldNameFromTarget(in_fc);
                        string oid = feature[oidField].ToString();

                        string zddm = feature[field_zddm]?.ToString();
                        string qlr = feature[field_qlr]?.ToString();
                        string zl = feature[field_zl]?.ToString();
                        string fl = feature[field_fl]?.ToString();
                        string sfz = feature[field_sfz]?.ToString();
                        string tfh = feature[field_tfh]?.ToString();
                        string zdmj = feature[field_zdmj]?.ToString();
                        string zzjgdm = feature[field_zzjgdm]?.ToString();
                        string bdcdyh = feature[field_bdcdyh]?.ToString();

                        string bz = feature[field_bz]?.ToString();
                        string dz = feature[field_dz]?.ToString();
                        string nz = feature[field_nz]?.ToString();
                        string xz = feature[field_xz]?.ToString();

                        string nyd = feature[field_nyd]?.ToString();
                        string gd = feature[field_gd]?.ToString();
                        string ld = feature[field_ld]?.ToString();
                        string cd = feature[field_cd]?.ToString();
                        string qt = feature[field_qt]?.ToString();
                        string jsyd = feature[field_jsyd]?.ToString();
                        string wlyd = feature[field_wlyd]?.ToString();

                        pw.AddProcessMessage(20, time_base, $"处理要素：{oid} - 宗地码：{zddm}");

                        // 复制界址点Excel表
                        string excelPath = excel_folder + @$"\权籍调查表_{zddm}.xls";
                        BaseTool.CopyResourceFile(@"CCTool.Data.Excel.权籍调查表模板.xls", excelPath);

                        // 打开工作薄
                        Workbook wb = OfficeTool.OpenWorkbook(excelPath);

                        // 处理sheet_封面
                        Worksheet ws_fm = wb.Worksheets["封面"];
                        Cells cells_fm = ws_fm.Cells;
                        cells_fm["D19"].Value = $"{zddm}";

                        // 处理sheet_基本表
                        Worksheet ws_jbb = wb.Worksheets["基本表"];
                        Cells cells_jbb = ws_jbb.Cells;
                        cells_jbb["D3"].Value = $"{qlr}";
                        cells_jbb["C10"].Value = $"{zl}";
                        cells_jbb["C11"].Value = $"{fl}";
                        cells_jbb["E12"].Value = $"{sfz}";
                        cells_jbb["G16"].Value = $"{zddm}W00000000";
                        cells_jbb["C17"].Value = $"{zddm}";
                        cells_jbb["G17"].Value = $"{zddm}";
                        cells_jbb["E19"].Value = $"{tfh}";
                        cells_jbb["C20"].Value = $"北：{bz}";
                        cells_jbb["E21"].Value = $"东：{dz}";
                        cells_jbb["E22"].Value = $"南：{nz}";
                        cells_jbb["E23"].Value = $"西：{xz}";
                        cells_jbb["E27"].Value = $"{zdmj}";

                        // 处理sheet_宗地分类面积调查表
                        Worksheet ws_mj = wb.Worksheets["宗地分类面积调查表"];
                        Cells cells_mj = ws_mj.Cells;
                        cells_mj["E3"].Value = $"{qlr}";
                        cells_mj["E4"].Value = $"{zddm}";
                        cells_mj["E5"].Value = $"{zddm}W00000000";
                        cells_mj["F6"].Value = $"{nyd}";
                        cells_mj["F7"].Value = $"{gd}";
                        cells_mj["F8"].Value = $"{ld}";
                        cells_mj["F9"].Value = $"{cd}";
                        cells_mj["F10"].Value = $"{qt}";
                        cells_mj["F11"].Value = $"{jsyd}";
                        cells_mj["F12"].Value = $"{wlyd}";

                        // 处理sheet_申请表1
                        Worksheet ws_s1 = wb.Worksheets["申请表1"];
                        Cells cells_s1 = ws_s1.Cells;
                        cells_s1["C10"].Value = $"{qlr}";
                        cells_s1["G11"].Value = $"{zddm}W00000000";
                        cells_s1["C12"].Value = $"{zl}";
                        cells_s1["C13"].Value = $"{fl}";
                        cells_s1["D16"].Value = $"{zl}";
                        cells_s1["D17"].Value = $"{bdcdyh}";
                        cells_s1["D18"].Value = $"宗地面积：{zdmj}";

                        // 保存
                        wb.Save(excelPath);
                        wb.Dispose();
                    }
                    pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }


        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/139114694";
            UITool.Link2Web(url);
        }

        private void combox_zddm_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_zddm);
        }

        private void combox_qlr_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_qlr);
        }

        private void combox_zl_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_zl);
        }

        private void combox_fl_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_fl);
        }

        private void combox_sfz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_sfz);
        }

        private void combox_tfh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_tfh);
        }

        private void combox_zdmj_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_zdmj);
        }

        private void combox_zzjgdm_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_zzjgdm);
        }

        private void combox_bdcdyh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_bdcdyh);
        }

        private void combox_bz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_bz);
        }

        private void combox_dz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_dz);
        }

        private void combox_nz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_nz);
        }

        private void combox_xz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_xz);
        }

        private void combox_nyd_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_nyd);
        }

        private void combox_gd_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_gd);
        }

        private void combox_ld_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_ld);
        }

        private void combox_cd_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_cd);
        }

        private void combox_qt_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_qt);
        }

        private void combox_jsyd_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_jsyd);
        }

        private void combox_wlyd_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_wlyd);
        }
    }
}
