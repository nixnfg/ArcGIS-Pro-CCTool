using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using Microsoft.Office.Core;
using NPOI.OpenXmlFormats.Spreadsheet;
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

namespace CCTool.Scripts.DataPross.FeatureClasses
{
    /// <summary>
    /// Interaction logic for DissolveAsField.xaml
    /// </summary>
    public partial class DissolveAsField : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public DissolveAsField()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "融合同类碎图斑";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToComboxPlus(combox_fc.ComboxText(), combox_field);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.ComboxText();
                string fc_field = combox_field.ComboxText();
                double minArea = double.Parse(text_mj.Text);

                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (fc_path == "" || fc_field == "" || minArea == 0)
                {
                    MessageBox.Show("有必选参数为空或输入错误！！！");
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
                    pw.AddProcessMessage(10, time_base, $"合并碎图斑，依据字段【{fc_field}】, 最小面积【{minArea}】");
                    DisloveTB(fc_path, fc_field, minArea);
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/137628819";
            UITool.Link2Web(url);
        }


        public async void DisloveTB(string fc_path, string fc_field, double minArea)
        {
            await QueuedTask.Run(() =>
            {
                // 获取原始图层和标识图层
                FeatureLayer originFeatureLayer = fc_path.TargetFeatureLayer();
                // 获取原始图层和标识图层的要素类
                FeatureClass originFeatureClass = fc_path.TargetFeatureClass();

                string OID = GisTool.GetIDFieldNameFromTarget(fc_path);

                // 初始化要合并的要素OID
                List<List<long>> objectIDList = new List<List<long>>();

                // 获取目标图层和源图层的要素游标
                using (RowCursor originCursor = originFeatureClass.Search())
                {
                    // 遍历源图层的要素
                    while (originCursor.MoveNext())
                    {
                        using Feature originFeature = (Feature)originCursor.Current;

                        // 用来标识的字段值
                        var originField = originFeature[fc_field];
                        if (originField is null)
                        {
                            continue;
                        }
                        string originFieldValue = originField.ToString();

                        // 初始化目标ID
                        List<long> objectIDDic = new List<long>();
                        long targetID = 0;
                        double targetArea = 0;

                        // 获取源要素的几何
                        ArcGIS.Core.Geometry.Geometry originGeometry = originFeature.GetShape();
                        // 要素面积
                        double originArea = (originGeometry as ArcGIS.Core.Geometry.Polygon).Area;

                        // 如果大于面积阀值，则跳过
                        if (originArea > minArea)
                        {
                            continue;
                        }

                        // ID值
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
                                using Feature identityFeature = (Feature)identityCursor.Current;
                                // 获取目标要素的几何
                                ArcGIS.Core.Geometry.Geometry identityGeometry = identityFeature.GetShape();

                                // 要素面积
                                double identityArea = (identityGeometry as ArcGIS.Core.Geometry.Polygon).Area;

                                // 计算源要素与目标要素的重叠面积
                                ArcGIS.Core.Geometry.Geometry intersection = GeometryEngine.Instance.Intersection(identityGeometry, originGeometry);
                                // 如果存在相交，则进行下一步处理
                                if (intersection != null)
                                {
                                    // 坡度字段值
                                    var identityField = identityFeature[fc_field];
                                    if (identityField is null) { continue; }

                                    string identityFieldValue = identityField.ToString();
                                    // objectID值
                                    long identityID = long.Parse(identityFeature[OID].ToString());
                                    // id不同，且字段值相同 的情况下再处理
                                    if (identityID != originID && identityFieldValue == originFieldValue)
                                    {
                                        if (identityArea > targetArea)
                                        {
                                            // 更新ID和面积
                                            targetID = identityID;
                                            targetArea = identityArea;
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

                // 合并
                foreach (var objectIDDic in objectIDList)
                {
                    EditTool.MergeFeatures(originFeatureLayer, objectIDDic);
                }

                // 循环处理
                double initMinArea = fc_path.GetPolygonMinArea();
                if (initMinArea < minArea)
                {
                    DisloveTB(fc_path, fc_field, minArea);
                }
            });
        }
    }
}
