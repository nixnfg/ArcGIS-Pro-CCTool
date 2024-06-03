using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
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

namespace CCTool.Scripts.FeaturePross
{
    /// <summary>
    /// Interaction logic for AreaStatisticsByField.xaml
    /// </summary>
    public partial class AreaStatisticsByField : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AreaStatisticsByField()
        {
            InitializeComponent();
            InitiArea();

            // 订阅地图选择更改事件
            MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
        }

        private void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
        {
            // 当选择更改时，自动刷新面积计算
            InitiArea();
        }

        private void combox_fc_Load(object sender, EventArgs e)
        {
            UITool.AddSelectFeatureLayersToComboxPlus(combox_fc);
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToComboxPlus(combox_fc.ComboxText(), combox_field);
        }

        // 统计面积
        public async void InitiArea()
        {
            // 接收参数
            string lyName = combox_fc.ComboxText();
            string fieldName = combox_field.ComboxText();

            if (lyName == "" || fieldName=="")
            {
                return;
            }

            // 初始化变量以存储面要素的数量和各类面积指标
            int polygonCount = 0;
            double polygonArea = 0;

            try
            {
                await QueuedTask.Run(() =>
                {
                    // 获取活动地图视图中选定的要素集合
                    var selectedSet = MapView.Active.Map.GetSelection();

                    // 将选定的要素集合转换为字典形式
                    var selectedList = selectedSet.ToDictionary();
                    // 创建一个新的 Inspector 对象以检索要素属性
                    var inspector = new Inspector();

                    // 遍历每个选定图层及其关联的对象 ID
                    foreach (var selected in selectedList)
                    {
                        // 获取图层和关联的对象 ID
                        FeatureLayer featureLayer = selected.Key as FeatureLayer;
                        // 如果是选择的图层
                        if (featureLayer.Name == lyName)
                        {
                            List<long> oids = selected.Value;
                            // 使用当前图层的第一个对象 ID 加载 Inspector
                            inspector.Load(featureLayer, oids[0]);
                            // 获取选定要素的几何类型
                            var geometryType = inspector.Shape.GeometryType;
                            // 检查几何类型是否为面要素
                            if (geometryType == GeometryType.Polygon)
                            {
                                // 遍历当前图层中的每个对象 ID
                                foreach (var oid in oids)
                                {
                                    // 使用当前对象 ID 加载 Inspector
                                    inspector.Load(featureLayer, oid);
                                    // 计算并累加多边形的面积
                                    var area = inspector[fieldName];
                                    if (area.GetType().ToString()!= "System.DBNull")
                                    {
                                        polygonArea += double.Parse(area.ToString());
                                    }
                                    // 增加面要素的数量
                                    polygonCount++;
                                }
                            }
                        }
                    }
                });

                // 显示结果
                text_area_squ.Text = Math.Round(polygonArea, 2).ToString();

                lb_count.Content = $"所选要素数量：{polygonCount}";

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }


        // 当需要重新计算面积时，可以调用 Initialize 方法，它会调用计算面积方法并刷新窗口内容
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            InitiArea();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {

        }

        private void combox_fc_DropOpen(object sender, EventArgs e)
        {
            UITool.AddSelectFeatureLayersToComboxPlus(combox_fc);
        }

        private void combox_fc_DropClosed(object sender, EventArgs e)
        {
            UITool.ClearComboxPlus(combox_field);
        }

        private void combox_field_DropClosed(object sender, EventArgs e)
        {
            string lyName = combox_fc.ComboxText();
            string fieldName = combox_field.ComboxText();
            if (fieldName != "" && lyName != "")
            {
                InitiArea();
            }
        }
    }

}
