using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Vml;
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

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for YED2SD.xaml
    /// </summary>
    public partial class YED2SD : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public YED2SD()
        {
            InitializeComponent();

            //// 预设
            //FeatureClassPath.Text = @"C:\Users\Administrator\Documents\ArcGIS\Projects\Test\Test.gdb\三调转换层";

            //UITool.InitFeatureLayerToComboxPlus(combox_fc, "一调");

        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "一、二调转三调";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void openFeatureClassButton_Click(object sender, RoutedEventArgs e)
        {
            FeatureClassPath.Text = UITool.SaveDialogFeatureClass();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/138462386";
            UITool.Link2Web(url);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string inputFC = combox_fc.ComboxText();
                string outputPath = FeatureClassPath.Text;

                bool isChangeXS = (bool)checkxs.IsChecked;

                // 版本
                string changeType = (rb_1.IsChecked == true) ? "一调" : "二调";

                // 判断参数是否选择完全
                if (outputPath == "" || inputFC == "")
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
                    string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置

                    string output_excel = $@"{def_folder}\一二三调地类名称对比表.xlsx";
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.三调规程.一二三调地类名称对比表.xlsx", output_excel);
                    // 版本确定
                    string sheet = output_excel + @"\一调$";
                    if (changeType == "二调")
                    {
                        sheet = output_excel + @"\二调$";
                    }

                    pw.AddProcessMessage(10, time_base, "开始转换...");
                    // 复制要素并添加一个字段用来参照
                    Arcpy.CopyFeatures(inputFC, outputPath);
                    Arcpy.AddField(outputPath, "参照字段", "TEXT");
                    Arcpy.CalculateField(outputPath, "参照字段", "!DLMC!");
                    // 用地用海编码名称互转
                    GisTool.AttributeMapper(outputPath, "参照字段", "DLMC", sheet);
                    // 更改TKXS为KCXS
                    if (isChangeXS)
                    {
                        pw.AddProcessMessage(40, time_base, "更改TKXS为KCXS");
                        Arcpy.AlterField(outputPath, "TKXS", "KCXS", "KCXS");
                    }

                    // 提取K到备注
                    Arcpy.AddField(outputPath, "BZ","TEXT","备注");
                    Arcpy.CalculateField(outputPath, "BZ", "!ZZSXMC!");

                    pw.AddProcessMessage(10, time_base, "删除中间数据");
                    Arcpy.DeleteField(outputPath, "参照字段");

                    pw.AddProcessMessage(10, time_base, "加载结果");
                    // 将要素类添加到当前地图
                    var map = MapView.Active.Map;
                    LayerFactory.Instance.CreateLayer(new Uri(outputPath), map);

                    // 删除中间数据
                    File.Delete(output_excel);

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
