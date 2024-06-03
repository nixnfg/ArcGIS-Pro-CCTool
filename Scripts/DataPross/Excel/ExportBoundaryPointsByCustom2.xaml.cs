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

namespace CCTool.Scripts.DataPross.Excel
{
    /// <summary>
    /// Interaction logic for ExportBoundaryPointsByCustom2.xaml
    /// </summary>
    public partial class ExportBoundaryPointsByCustom2 : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ExportBoundaryPointsByCustom2()
        {
            InitializeComponent();

            // 初始化combox
            combox_ptDigit.Items.Add("1");
            combox_ptDigit.Items.Add("2");
            combox_ptDigit.Items.Add("3");
            combox_ptDigit.Items.Add("4");
            combox_ptDigit.SelectedIndex = 2;

            combox_sh.Items.Add("按点号排序");
            combox_sh.Items.Add("按部件号排序");
            combox_sh.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "界址点导出Excel";

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

                string dkhField = combox_dkh.ComboxText();
                string xmmcField = combox_xmmc.ComboxText();
                string dkmjField = combox_dkmj.ComboxText();
                string kcdwField = combox_kcdw.ComboxText();

                string zb = txt_zb.Text;
                string js = txt_js.Text;
                string dysj = txt_dysj.Text;

                int ptDigit = int.Parse(combox_ptDigit.Text);
                bool xyReserve = (bool)check_xy.IsChecked;
                bool haveJ = (bool)check_xy_J.IsChecked;
                string sh = combox_sh.Text;     //序号模式

                // 判断参数是否选择完全
                if (in_fc == "" || dkhField == "" || xmmcField == "" || excel_folder == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
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
                        // 获取要素的几何
                        ArcGIS.Core.Geometry.Polygon geometry = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;
                        // 获取参数
                        string dkh = feature[dkhField]?.ToString();   // 地块号
                        string xmmc = feature[xmmcField]?.ToString();   // 项目名称

                        pw.AddProcessMessage(20, time_base, $"处理要素：{dkh} - {xmmc}");

                        string dkmj = "";                    // 地块面积
                        if (dkmjField != "") { dkmj = feature[dkmjField]?.ToString() ?? ""; }
                        // 转小数点后四位
                        dkmj = (double.Parse(dkmj).RoundWithFill(4));

                        string kcdw = "";                  // 勘测单位
                        if (kcdwField != "") { kcdw = feature[kcdwField]?.ToString() ?? ""; }

                        // 复制界址点Excel表
                        string excelPath = excel_folder + @$"\{dkh} - {xmmc}界址点表.xlsx";
                        BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】界址点表_多页_自定义4.xlsx", excelPath);
                        // 获取工作薄、工作表
                        string excelFile = excelPath.GetExcelPath();
                        int sheetIndex = excelPath.GetExcelSheetIndex();
                        // 打开工作薄
                        Workbook wb = OfficeTool.OpenWorkbook(excelFile);
                        // 打开工作表
                        Worksheet worksheet = wb.Worksheets[sheetIndex];
                        // 获取Cells
                        Cells cells = worksheet.Cells;

                        // 设置分页信息
                        SetPage(geometry, cells, dkh, xmmc, dkmj, kcdw, zb, js, dysj);

                        if (geometry != null)
                        {
                            // 获取面要素的所有折点【按西北角起始，顺时针重排】
                            List<List<MapPoint>> mapPoints = geometry.ReshotMapPoint();

                            int rowIndex = 9;   // 起始行
                            int pointIndex = 1;  // 起始点序号
                            int lastRowCount = 0;  // 上一轮的行数

                            for (int i = 0; i < mapPoints.Count; i++)
                            {
                                // 输出折点的XY值和距离到Excel表
                                double prevX = double.NaN;
                                double prevY = double.NaN;

                                for (int j = 0; j < mapPoints[i].Count; j++)
                                {
                                    // 写入点号
                                    // J前缀
                                    string jFront = haveJ switch
                                    {
                                        true => "J",
                                        false => "",
                                    };
                                    if (pointIndex - lastRowCount == mapPoints[i].Count)    // 找到当前环的最后一点
                                    {
                                        worksheet.Cells[rowIndex, 1].Value = $"{jFront}{lastRowCount + 1 - i}";
                                    }
                                    else
                                    {
                                        worksheet.Cells[rowIndex, 1].Value = $"{jFront}{pointIndex - i}";
                                    }

                                    // 写入序号
                                    if (sh == "按部件号排序")
                                    {
                                        worksheet.Cells[rowIndex, 0].Value = $"{i + 1}";
                                    }
                                    else if (sh == "按点号排序")
                                    {
                                        worksheet.Cells[rowIndex, 0].Value = $"{pointIndex}";
                                    }

                                    double x = Math.Round(mapPoints[i][j].X, ptDigit);
                                    double y = Math.Round(mapPoints[i][j].Y, ptDigit);
                                    // 写入折点的XY值
                                    if (xyReserve)
                                    {
                                        worksheet.Cells[rowIndex, 2].Value = x;
                                        worksheet.Cells[rowIndex, 3].Value = y;
                                    }
                                    else
                                    {
                                        worksheet.Cells[rowIndex, 2].Value = y;
                                        worksheet.Cells[rowIndex, 3].Value = x;
                                    }
                                    // 设置单元格为数字型，小数位数
                                    Aspose.Cells.Style style = worksheet.Cells[rowIndex, 3].GetStyle();
                                    style.Number = 4;   // 数字型
                                    // 小数位数
                                    style.Custom = ptDigit switch
                                    {
                                        1 => "0.0",
                                        2 => "0.00",
                                        3 => "0.000",
                                        4 => "0.0000",
                                        _ => null,
                                    };
                                    // 设置
                                    worksheet.Cells[rowIndex, 2].SetStyle(style);
                                    worksheet.Cells[rowIndex, 3].SetStyle(style);

                                    // 计算当前点与上一个点的距离
                                    if (!double.IsNaN(prevX) && !double.IsNaN(prevY) && rowIndex > 8)
                                    {
                                        double distance = Math.Round(Math.Sqrt(Math.Pow(x - prevX, 2) + Math.Pow(y - prevY, 2)), 2);
                                        // 写入距离【第一行不写入】
                                        worksheet.Cells[rowIndex - 1, 4].Value = distance;
                                        if (j == mapPoints[i].Count - 1)
                                        {
                                            worksheet.Cells[rowIndex + 1, 4].Value = "";
                                        }
                                    }
                                    prevX = x;
                                    prevY = y;

                                    // 设置点距格式
                                    Aspose.Cells.Style style2 = worksheet.Cells[rowIndex, 4].GetStyle();
                                    style2.Number = 4;   // 数字型
                                    // 小数位数
                                    style2.Custom = 2 switch
                                    {
                                        1 => "0.0",
                                        2 => "0.00",
                                        3 => "0.000",
                                        4 => "0.0000",
                                        _ => null,
                                    };
                                    // 设置
                                    worksheet.Cells[rowIndex + 1, 4].SetStyle(style2);

                                    // 是否跨页，
                                    if ((rowIndex-77) % 81 == 0)
                                    {
                                        // 但是如果是最后一个点，不就跨页了
                                        if (pointIndex - lastRowCount != mapPoints[i].Count)
                                        {
                                            rowIndex += 13;
                                            // 点号回退1
                                            j--;
                                            pointIndex--;
                                        }
                                        else
                                        {
                                            rowIndex += 2;
                                        }
                                    }
                                    else
                                    {
                                        rowIndex += 2;
                                    }
                                    pointIndex++;
                                }
                                lastRowCount += mapPoints[i].Count;
                            }
                        }
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

        // 生成分页，并设置分页信息
        public static void SetPage(ArcGIS.Core.Geometry.Polygon geometry, Cells cells, string dkh, string xmmc, string dkmj, string kcdw, string zb, string js, string dysj)
        {
            // 获取要素的界址点总数
            var pointsCount = geometry.Points.Count;
            // 计算要生成的页数
            long pageCount = (int)Math.Ceiling((double)(pointsCount-35) / 34)+1;

            if (pageCount > 1)  // 多于1页
            {
                // 复制分页
                for (int i = 1; i < pageCount; i++)
                {
                    cells.CopyRows(cells, 0, i * 81, 80);
                }
            }
            // 设置单元格
            for (int i = 0; i < pageCount; i++)
            {
                // 填写页码
                cells[i * 81, 4].Value = $"第 {i + 1} 页";
                cells[i * 81 + 1, 4].Value = $"共 {pageCount} 页";
                // 地块号
                cells[i * 81 + 2, 0].Value = $"地块号：{dkh}";
                // 项目名称
                cells[i * 81 + 3, 0].Value = $"项目名称：{xmmc}";
                // 地块面积
                cells[i * 81 + 4, 0].Value = $"地块面积(公顷)：{dkmj}";
                // 勘测单位
                cells[i * 81 + 5, 0].Value = $"勘测单位: {kcdw}";

                // 制表
                cells[i * 81 + 79, 0].Value = $"制表：{zb}";
                // 校审
                cells[i * 81 + 79, 2].Value = $"校审：{js}";
                // 打印时间
                cells[i * 81 + 79, 4].Value = $"{dysj}";
            }
        }

        private void combox_dkh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_dkh);
        }

        private void combox_xmmc_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_xmmc);
        }

        private void combox_dkmj_DropOpen(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_dkmj);
        }

        private void combox_kcdw_DropOpen(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_kcdw);
        }
    }
}
