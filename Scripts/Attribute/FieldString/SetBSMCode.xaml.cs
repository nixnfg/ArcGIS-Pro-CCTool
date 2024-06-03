using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Vml;
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

namespace CCTool.Scripts.Attribute.FieldString
{
    /// <summary>
    /// Interaction logic for SetBSMCode.xaml
    /// </summary>
    public partial class SetBSMCode : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SetBSMCode()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "BSM编码";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToComboxPlus(combox_fc);
        }

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取数据
                string layer_path = combox_fc.ComboxText();
                string front = textBox_front.Text;
                bool isByLength = (bool)rb_bsmLength.IsChecked;
                bool isSort = (bool)checkBox_sort.IsChecked;

                string leng = textBox_len.Text ?? "";     // 自定义字段长度
                int BLength = int.Parse(leng);

                // 获取默认数据库
                var gdb = Project.Current.DefaultGeodatabasePath;

                // 判断参数是否选择完全
                if (layer_path == "" || front == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                await QueuedTask.Run(() =>
                {
                    // 检查是否有BSM字段
                    bool isHave = GisTool.IsHaveFieldInTarget(layer_path, "BSM");
                    if (!isHave)
                    {
                        MessageBox.Show("图层属性表不包含BSM字段！");
                        return;
                    }
                });

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(20, "获取OID字段");
                    // 获取OID字段
                    string oidField = GisTool.GetIDFieldNameFromTarget(layer_path);

                    // 字段长度
                    int len = BLength;
                    if (isByLength)
                    {
                        len = GisTool.GetFieldFromString(layer_path, "BSM").Length;
                    }
                    // 是否要重排
                    if (isSort)
                    {
                        pw.AddProcessMessage(20, time_base, "按空间位置重新排序【左上至右下】");
                        // 排序
                        Arcpy.Sort(layer_path, gdb + @"\sort_fc", "Shape ASCENDING", "UL");

                        // 获取图层及原要素的路径
                        FeatureLayer featureLayer = layer_path.TargetFeatureLayer();
                        string fc_path = layer_path.LayerSourcePath();
                        // 获取符号系统
                        CIMRenderer cr = featureLayer.GetRenderer();
                        // 更新要素
                        Arcpy.Update(fc_path, gdb + @"\sort_fc", gdb + @"\sort_fc2");
                        Arcpy.CopyFeatures(gdb + @"\sort_fc2", fc_path, true);
                        // 应用符号系统
                        featureLayer.SetRenderer(cr);
                    }

                    pw.AddProcessMessage(30, time_base, "计算BSM");
                    // 计算BSM
                    Arcpy.CalculateField(layer_path, "BSM", $"'{front}'+'0' * ({len} - len(str(!{oidField}!))-{front.Length}) + str(!{oidField}!)");

                });
                pw.AddProcessMessage(10, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void btn_help_click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/136716272?spm=1001.2014.3001.5502";
            UITool.Link2Web(url);
        }
    }
}
