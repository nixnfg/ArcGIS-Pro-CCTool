using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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
using Geometry = ArcGIS.Core.Geometry.Geometry;

namespace CCTool.Scripts.LayerPross
{
    /// <summary>
    /// Interaction logic for DisslovPD.xaml
    /// </summary>
    public partial class DisslovPD : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public DisslovPD()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "坡度细碎图斑融合";


        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            // 获取指标
            string pdField = combox_field.ComboxText();
            string stringMinArea = txt_area.Text;

            // 判断参数是否选择完全
            if (pdField == "" || stringMinArea == "")
            {
                MessageBox.Show("有必选参数为空！！！");
                return;
            }

            double minArea = int.Parse(stringMinArea);

            // 打开进度框
            ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
            DateTime time_base = DateTime.Now;
            pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
            Close();

            await QueuedTask.Run(() =>
            {
                pw.AddProcessMessage(10, time_base, "获取参数");
                // 获取图层
                FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                string lyName = ly.Name;

                pw.AddProcessMessage(20, time_base, "细碎图斑就近合并至最低等级坡度图斑");
                DislovePD(lyName, pdField, minArea);
            });

            pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);

        }

        public async void DislovePD(string lyName, string pdField, double minArea)
        {
            await QueuedTask.Run(() =>
            {
                // 获取原始图层和标识图层
                FeatureLayer originFeatureLayer = lyName.TargetFeatureLayer();
                // 获取原始图层和标识图层的要素类
                FeatureClass originFeatureClass = lyName.TargetFeatureClass();

                string OID = GisTool.GetIDFieldNameFromTarget(lyName);

                // 初始化要合并的要素OID
                List<List<long>> objectIDList = new List<List<long>>();

                // 获取目标图层和源图层的要素游标
                using (RowCursor originCursor = originFeatureClass.Search())
                {
                    // 遍历源图层的要素
                    while (originCursor.MoveNext())
                    {
                        using (Feature originFeature = (Feature)originCursor.Current)
                        {
                            // 初始化最小坡度
                            int minPD = 10000;
                            // 初始化目标ID
                            List<long> objectIDDic = new List<long>();
                            long targetID = 0;

                            // 获取源要素的几何
                            ArcGIS.Core.Geometry.Geometry originGeometry = originFeature.GetShape();
                            // 要素面积
                            double originArea = (originGeometry as ArcGIS.Core.Geometry.Polygon).Area;

                            // 如果大于面积阀值，则跳过
                            if (originArea > minArea)
                            {
                                continue;
                            }

                            long originID = long.Parse(originFeature[OID].ToString());

                            // 创建空间查询过滤器，以获取与源要素有重叠的目标要素
                            SpatialQueryFilter spatialFilter = new SpatialQueryFilter
                            {
                                FilterGeometry = originGeometry,
                                SpatialRelationship = SpatialRelationship.Intersects
                            };

                            // 在目标图层中查询与源要素重叠的要素
                            using (RowCursor identityCursor = originFeatureClass.Search(spatialFilter))
                            {
                                while (identityCursor.MoveNext())
                                {
                                    using (Feature identityFeature = (Feature)identityCursor.Current)
                                    {
                                        // 获取目标要素的几何
                                        ArcGIS.Core.Geometry.Geometry identityGeometry = identityFeature.GetShape();

                                        // 计算源要素与目标要素的重叠面积
                                        ArcGIS.Core.Geometry.Geometry intersection = GeometryEngine.Instance.Intersection(identityGeometry, originGeometry);
                                        // 如果存在相交，则进行下一步处理
                                        if (intersection != null)
                                        {
                                            // 坡度字段值
                                            int identityFieldValue = (int)identityFeature[pdField];
                                            // objectID值
                                            long identityID = long.Parse(identityFeature[OID].ToString());
                                            // id不同的情况下再处理
                                            if (identityID != originID)
                                            {
                                                if (identityFieldValue < minPD)
                                                {
                                                    minPD = identityFieldValue;
                                                    targetID = identityID;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // 收集要合并的id
                            objectIDDic.Add(targetID);
                            objectIDDic.Add(originID);

                            // 收集所有要合并的ID列表
                            objectIDList.Add(objectIDDic);
                        }
                    }
                }

                // 合并
                foreach (var objectIDDic in objectIDList)
                {
                    EditTool.MergeFeatures(originFeatureLayer, objectIDDic);
                }

                // 循环处理
                double initMinArea = lyName.GetPolygonMinArea();
                if (initMinArea < minArea)
                {
                    DislovePD(lyName, pdField, minArea);
                }
            });
        }

        private void combox_field_DropOpen(object sender, EventArgs e)
        {
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            UITool.AddIntFieldsToComboxPlus(ly, combox_field);
        }

        private void btn_help_click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/135671287?spm=1001.2014.3001.5502";
            UITool.Link2Web(url);
        }
    }
}
