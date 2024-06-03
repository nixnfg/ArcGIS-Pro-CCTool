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

namespace CCTool.Scripts.DataPross.Excel
{
    /// <summary>
    /// Interaction logic for MergeExcel.xaml
    /// </summary>
    public partial class MergeExcel : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MergeExcel()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "合并Excel表";

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            textExcelPath.Text = UITool.SaveDialogExcel();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/139082544";
            UITool.Link2Web(url);
        }

        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string excelPath = textExcelPath.Text;
                string folderPath = textFolderPath.Text;

                bool isExcel = (bool)rb_excel.IsChecked;

                // 判断参数是否选择完全
                if (excelPath == "" || folderPath == "")
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

                // 合并Excel
                OfficeTool.ExcelMergeSheets(excelPath, folderPath, isExcel);

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
