using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.HPSF;
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

namespace CCTool.Scripts.LayerPross
{
    /// <summary>
    /// Interaction logic for ChangeRenderLabel.xaml
    /// </summary>
    public partial class ChangeRenderLabel : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ChangeRenderLabel()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "更改符号系统标注";

        private void combox_field1_DropOpen(object sender, EventArgs e)
        {
            // 获取图层
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            UITool.AddTextFieldsToComboxPlus(ly, combox_field1);
        }

        private void combox_field2_DropOpen(object sender, EventArgs e)
        {
            // 获取图层
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

            UITool.AddTextFieldsToComboxPlus(ly, combox_field2);
        }

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string txtFront = txt_front.Text;
                string txtBack = txt_back.Text;
                string field01 = combox_field1.ComboxText();
                string field02 = combox_field2.ComboxText();

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();   //关闭窗口
                await QueuedTask.Run(() =>
                {
                    // 获取当前地图
                    var map = MapView.Active.Map;
                    // 获取图层
                    FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

                    pw.AddProcessMessage(10, "获取CIMUniqueValueRenderer");
                    // 获取CIMUniqueValueRenderer
                    CIMUniqueValueRenderer uvr = featureLayer.GetRenderer() as CIMUniqueValueRenderer;

                    pw.AddProcessMessage(10, time_base, "获取要用来标注的字段值");
                    // 获取映射字段
                    string mapFieldName = uvr.Fields.FirstOrDefault();
                    // 获取字段值映射表
                    Dictionary<string, string> dic01 = new Dictionary<string, string>();
                    Dictionary<string, string> dic02 = new Dictionary<string, string>();
                    if (field01 != "")
                    {
                        dic01 = featureLayer.Get2FieldValueDic(mapFieldName, field01);
                    }
                    if (field02 != "")
                    {
                        dic02 = featureLayer.Get2FieldValueDic(mapFieldName, field02);
                    }

                    pw.AddProcessMessage(40, time_base, "修改每个标注类别的表达式");

                    CIMUniqueValueClass[] uvClasses = uvr.Groups[0].Classes;
                    // 修改每个标注类别的表达式
                    foreach (CIMUniqueValueClass uvClass in uvClasses)
                    {
                        // 映射字段值
                        string va = uvClass.Values[0].FieldValues[0].ToString();

                        // 获取标注字段值
                        string fieldValue01 = "";
                        string fieldValue02 = "";
                        if (dic01.ContainsKey(va))
                        {
                            fieldValue01 = dic01[va];
                        }
                        if (dic02.ContainsKey(va))
                        {
                            fieldValue02 = dic02[va];
                        }
                        // 标注
                        uvClass.Label = txtFront + fieldValue01 + fieldValue02 + txtBack;
                        
                    }
                    pw.AddProcessMessage(40, time_base, "删除计数值为0的行");
                    // 删除计数值为0的行
                    uvr.Groups[0].Classes = uvClasses.Where(x =>dic01.ContainsKey(x.Values[0].FieldValues[0].ToString())).ToArray();

                    // 应用渲染器
                    featureLayer.SetRenderer(uvr);

                    pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void btn_help_click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/136038237?spm=1001.2014.3001.5501";
            UITool.Link2Web(url);
        }
    }
}
