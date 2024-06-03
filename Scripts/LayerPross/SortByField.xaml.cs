using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using MathNet.Numerics.LinearAlgebra.Factorization;
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
using Row = ArcGIS.Core.Data.Row;

namespace CCTool.Scripts.LayerPross
{
    /// <summary>
    /// Interaction logic for SortByField.xaml
    /// </summary>
    public partial class SortByField : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SortByField()
        {
            InitializeComponent();

            comBox_model.Items.Add("左上-->右下");
            comBox_model.Items.Add("右下-->左上");
            comBox_model.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "分区排序";

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            // 获取指标
            string sortField = combox_field.ComboxText();
            string resultField = combox_resultField.ComboxText();
            string model = comBox_model.Text;
            int start =int.Parse(txt_bh.Text);

            // 获取默认数据库
            var gdb = Project.Current.DefaultGeodatabasePath;

            // 判断参数是否选择完全
            if (sortField == "" || resultField == "")
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


                pw.AddProcessMessage(30, time_base, "排序");
                // 获取图层
                FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                // 获取图层要素的路径
                string fc_path = ly.Name.LayerSourcePath();
                // 获取符号系统
                CIMRenderer cr = ly.GetRenderer();
                // 模式转换
                string md = model switch
                {
                    "左上-->右下"=> "UL",
                    "右下-->左上" => "LR",
                    _=>null,
                };
                // 排序
                Arcpy.Sort(ly, gdb + @"\sort_fc", "Shape ASCENDING", md);
                // 更新要素
                Arcpy.Update(fc_path, gdb + @"\sort_fc", gdb + @"\sort_fc2");
                Arcpy.CopyFeatures(gdb + @"\sort_fc2", fc_path, true);
                // 应用符号系统
                ly.SetRenderer(cr);

                pw.AddProcessMessage(30, time_base, "分区赋值");

                // 获取所有字段值
                List<string> values = ly.GetFieldValues(sortField);
                // 设置一个字典保存个数
                Dictionary<string,int> dic = new Dictionary<string,int>();
                foreach (string value in values)
                {
                    dic.Add(value, start);
                }
                // 获取ID字段
                string oidField = GisTool.GetIDFieldNameFromTarget(fc_path);
                // 分区赋值
                RowCursor cursor = ly.GetSelectCursor();
                while (cursor.MoveNext())
                {
                    using Row row = cursor.Current;
                    // 分区名称
                    string area = row[sortField].ToString();
                    // 如果在分区列表中，就更新数量
                    if (dic.ContainsKey(area))
                    {
                        row[resultField] = dic[area];
                        dic[area] += 1;
                    }
                    // 保存
                    row.Store();
                }
            });

            pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);

        }


        private void combox_field_DropOpen(object sender, EventArgs e)
        {
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            UITool.AddTextFieldsToComboxPlus(ly, combox_field);
        }

        private void btn_help_click(object sender, RoutedEventArgs e)
        {
            string url = "";
            UITool.Link2Web(url);
        }

        private void combox_resultField_DropOpen(object sender, EventArgs e)
        {
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            UITool.AddIntFieldsToComboxPlus(ly, combox_resultField);
        }
    }
}
