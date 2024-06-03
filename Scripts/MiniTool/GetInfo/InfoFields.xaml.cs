using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
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
using System.Threading;
using ArcGIS.Desktop.Mapping;
using System.Security.Cryptography;
using Aspose.Cells;
using NPOI.XWPF.UserModel;
using NPOI.SS.Formula.Functions;

namespace CCTool.Scripts.MiniTool.GetInfo
{
    /// <summary>
    /// Interaction logic for InfoFields.xaml
    /// </summary>
    public partial class InfoFields : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public InfoFields()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "导出字段属性表";


        private void combox_layer_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToComboxPlus(combox_layer);

        }

        // 定义一个空包
        List<FieldAtt> fieldAtt = new List<FieldAtt>();

        private async void combox_layer_DropClosed(object sender, EventArgs e)
        {
            try
            {
                // 定义一个空包
                List<FieldAtt> fieldAtt2 = new List<FieldAtt>();

                // 将字段属性加入到dg中
                string lyName = combox_layer.ComboxText();

                var fields = await QueuedTask.Run(() =>
                {
                    return GisTool.GetFieldsFromTarget(lyName, "allof");
                });
                // 添加数据
                foreach (Field field in fields)
                {
                    // geometry数据也算不可编辑
                    string edit = "";
                    if (field.IsEditable == false || field.FieldType == FieldType.Geometry)
                    {
                        edit = "不可";
                    }
                    else
                    {
                        edit = "可";
                    }

                    fieldAtt2.Add(new FieldAtt()
                    {
                        FieldName = field.Name,
                        AliasName = field.AliasName,
                        FieldType = field.FieldType.ToString(),
                        FieldLength = field.Length.ToString(),
                        IsEditable = edit,
                    }); ;
                }
                // 绑定
                dg.ItemsSource = fieldAtt2;

                // 赋值
                fieldAtt = fieldAtt2;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/135624945?spm=1001.2014.3001.5501";
            UITool.Link2Web(url);
        }

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            textExcelPath.Text = UITool.OpenDialogExcel();
        }
        // 导出excel
        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string excelPath = textExcelPath.Text;
                bool isEditableOut = (bool)checkbox_edit.IsChecked;

                // 判断参数是否选择完全
                if (excelPath == "" || dg.Items.Count == 0)
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


                // 复制Excel模板
                BaseTool.CopyResourceFile(@"CCTool.Data.Excel.字段列表.xlsx", excelPath);

                // 获取工作薄、工作表
                string excelFile = excelPath.GetExcelPath();
                int sheetIndex = excelPath.GetExcelSheetIndex();
                // 打开工作薄
                Workbook wb = OfficeTool.OpenWorkbook(excelFile);
                // 打开工作表
                Worksheet worksheet = wb.Worksheets[sheetIndex];
                // 获取Cells
                Cells cells = worksheet.Cells;

                int index = 1;
                foreach (FieldAtt att in fieldAtt)
                {
                    string FieldName = att.FieldName;
                    string AliasName = att.AliasName;
                    string FieldType = att.FieldType;
                    int FieldLength = int.Parse(att.FieldLength);
                    string Iseditable = att.IsEditable;

                    if (isEditableOut && Iseditable == "不可")     // 如果不可编辑值不导出，就跳过
                    {
                        continue;
                    }
                    else
                    {
                        // 赋值
                        cells[index + 1, 0].Value = index;
                        cells[index + 1, 1].Value = FieldName;
                        cells[index + 1, 2].Value = AliasName;
                        cells[index + 1, 3].Value = FieldType;
                        cells[index + 1, 4].Value = FieldLength;
                    }

                    pw.AddProcessMessage(10, time_base, $"写入字段{FieldName}...");

                    for (int i = 0; i < 5; i++)
                    {
                        // 设置单元格
                        Cell cell = worksheet.Cells[index + 1, i];
                        Aspose.Cells.Style style = cell.GetStyle();

                        style.HorizontalAlignment = TextAlignmentType.Center;
                        style.VerticalAlignment = TextAlignmentType.Center;
                        style.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                        style.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                        style.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                        style.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                        // 设置
                        cell.SetStyle(style);
                    }

                    index++;
                }

                // 保存
                wb.Save(excelFile);
                wb.Dispose();

                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }

    public class FieldAtt
    {
        public string FieldName { get; set; }
        public string AliasName { get; set; }
        public string FieldType { get; set; }
        public string FieldLength { get; set; }
        public string IsEditable { get; set; }
        public string IsGeometry { get; set; }
    }
}
