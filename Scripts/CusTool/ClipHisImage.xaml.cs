using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Aspose.Words.Drawing;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace CCTool.Scripts.CusTool
{
    /// <summary>
    /// Interaction logic for ClipHisImage.xaml
    /// </summary>
    public partial class ClipHisImage : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ClipHisImage()
        {
            InitializeComponent();
            // 加载所有影像列表

            AddAllLayersToListbox(listBoxImage);

            // 预设
            //textFolderPath.Text = @"C:\Users\Administrator\Desktop\输出";

            //UITool.InitFeatureLayerToComboxPlus(comBox_layer, "A2021_CopyFeatures");

            //UITool.InitFieldToComboxPlus("A2021_CopyFeatures", comBox_field, "TXTName", "string");

        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "按要素下载历史影像";

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string outputPath = textFolderPath.Text;
                int dpi = int.Parse(text_dpi.Text);
                string pic_type = "";
                string layerName = comBox_layer.ComboxText();
                string fieldName = comBox_field.ComboxText();

                bool isfwVisible = (bool)checkBoxLayer.IsChecked;

                // 获取导出影像列表
                List<string> listImage = UITool.GetStringFromListbox(listBoxImage);

                if (rb_jpg.IsChecked == true)
                {
                    pic_type = "jpg";
                }
                else if (rb_png.IsChecked == true)
                {
                    pic_type = "png";
                }
                else if (rb_pdf.IsChecked == true)
                {
                    pic_type = "pdf";
                }
                else if (rb_tif.IsChecked == true)
                {
                    pic_type = "tif";
                }

                // 判断参数是否选择完全
                if (outputPath == "" || text_dpi.Text == "" || layerName == "" || fieldName == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO1", Brushes.Green);

                Close();

                await QueuedTask.Run(() =>
                {
                    var map = MapView.Active.Map;
                    // 工程默认文件夹位置
                    string folder_path = Project.Current.HomeFolderPath;

                    pw.AddProcessMessage(10, time_base, $"复制并加载影像");
                    // 获取范围图层
                    FeatureLayer fwLayer = map.GetLayersAsFlattenedList().FirstOrDefault(m => m.Name == layerName) as FeatureLayer;

                    // 获取布局
                    // 获取LayoutProjectItem
                    string layoutName = "历史影像布局";
                    LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals(layoutName));
                    //  如果工程中不存在该布局，就加载一个
                    string pagePath = folder_path + @$"\{layoutName}.pagx";
                    if (layoutItem is null)
                    {
                        pw.AddProcessMessage(100, time_base, $"工程没有【{layoutName}】，现在加载", Brushes.Green);
                        // 复制布局
                        BaseTool.CopyResourceFile(@$"CCTool.Data.Layouts.{layoutName}.pagx", pagePath);
                        // 导入布局
                        IProjectItem pagx = ItemFactory.Instance.Create(pagePath) as IProjectItem;
                        Project.Current.AddItem(pagx);
                        layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals(layoutName));
                    }

                    Layout layout = layoutItem.GetLayout();            // 获取Layout                         
                    MapFrame mf = layout.FindElement("地图框") as MapFrame;             // 获取主地图框
                    Map mainMap = mf.Map;                         // 获取主地图

                    // 将影像图层加载到地图
                    foreach (var imagePath in listImage)
                    {
                        LayerFactory.Instance.CreateLayer(new Uri($@"{folder_path}\历史影像\{imagePath}.lyrx"), mainMap);
                    }

                    // 将范围图层复制到地图
                    LayerFactory.Instance.CopyLayer(fwLayer, mainMap);
                    FeatureLayer newLayer = mainMap.GetLayersAsFlattenedList().FirstOrDefault(m => m.Name == layerName) as FeatureLayer;
                    // 设置坐标系和范围图层一致
                    mainMap.SetSpatialReference(newLayer.GetSpatialReference());

                    // 范围图层移动到最上面
                    mainMap.MoveLayer(newLayer, 0);

                    foreach (var imagePath in listImage)
                    {
                        // 创建年份的文件夹，用来存放影像
                        string path = $@"{outputPath}\{imagePath}";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        // 设置指定图层可见
                        Layer layer = mainMap.Layers.FirstOrDefault(ma => ma.Name == imagePath);
                        layer.SetVisibility(true);
                        mainMap.MoveLayer(layer, 1);
                        // 设置范围图层的显示
                        if (isfwVisible)
                        {
                            newLayer.SetVisibility(true);
                        }
                        else
                        {
                            newLayer.SetVisibility(false);
                        }

                        using RowCursor rowCursor = newLayer.Search();
                        while (rowCursor.MoveNext())
                        {
                            using Feature feature = rowCursor.Current as Feature;
                            // 获取标记的字段值
                            string bz = feature[fieldName]?.ToString() ?? "";

                            Geometry geometry = feature.GetShape();
                            
                            // 设置布局大小
                            CIMPage pg = layout.GetPage();
                            Double pgHeight = pg.Height;
                            Double pgWidth = pg.Width;

                            Double fcHeight = geometry.Extent.Height;
                            Double fcWidth = geometry.Extent.Width;

                            // 如果要素更扁、控制宽度，减小高度
                            if (fcHeight/ fcWidth < pg.Height/ pg.Width)
                            {
                                pg.Height = fcHeight * pg.Width / fcWidth;
                            }
                            else
                            {
                                pg.Width = fcWidth * pg.Height / fcHeight;
                            }

                            // 应用布局页面
                            layout.SetPage(pg);

                            // 地图大小设置
                            mf.SetHeight(pg.Height);
                            mf.SetWidth(pg.Width);

                            // 设置视图范围
                            mf.SetCamera(geometry.Extent);

                            pw.AddProcessMessage(0, time_base, $"导出{imagePath}_{bz}", Brushes.Gray);


                            Thread.Sleep(1000);

                            // JPG图片属性
                            JPEGFormat JPG = new JPEGFormat()
                            {
                                HasWorldFile = true,
                                Resolution = dpi,               // 分辨率
                                OutputFileName = @$"{path}\{bz}.jpg",      // 输出路径
                            };
                            // PNG图片属性
                            PNGFormat PNG = new PNGFormat()
                            {
                                HasWorldFile = true,
                                HasTransparentBackground = true,   // 透明底
                                Resolution = dpi,               // 分辨率
                                OutputFileName = @$"{path}\{bz}.png",      // 输出路径
                            };
                            // PNG图片属性
                            TIFFFormat TIF = new TIFFFormat()
                            {
                                HasWorldFile = true,
                                HasTransparentBackground = true,   // 透明底
                                Resolution = dpi,               // 分辨率
                                OutputFileName = @$"{path}\{bz}.tif",      // 输出路径
                            };
                            // PDF图片属性
                            PDFFormat PDF = new PDFFormat()
                            {
                                OutputFileName = @$"{path}\{bz}.pdf",      // 输出路径
                                Resolution = dpi,               // 分辨率
                                DoCompressVectorGraphics = true,   // 是否压缩矢量图形
                                DoEmbedFonts = true,            // 是否执行嵌入字体         
                                HasGeoRefInfo = true,             // 是否具有地理参考信息
                                ImageCompression = ImageCompression.Adaptive,   // 图形压缩.自适应
                                ImageQuality = ImageQuality.Best,           // 图形质量
                                LayersAndAttributes = LayersAndAttributes.LayersAndAttributes   // 图层  属性
                            };

                            // 导出JPG
                            if (pic_type == "jpg")
                            {
                                layout.Export(JPG);
                            }
                            // 导出PNG
                            else if (pic_type == "png")
                            {
                                layout.Export(PNG);
                            }
                            // 导出PDF
                            else if (pic_type == "pdf")
                            {
                                layout.Export(PDF);
                            }
                            // 导出PDF
                            else if (pic_type == "tif")
                            {
                                layout.Export(TIF);
                            }

                            // 还原布局页面
                            pg.Height = pgHeight;
                            pg.Width = pgWidth;
                            layout.SetPage(pg);
                            // 还原地图大小
                            mf.SetHeight(pgHeight);
                            mf.SetWidth(pgWidth);
                        }
                        // 移除图层
                        mainMap.RemoveLayer(layer);

                    }
                    // 移除图层
                    mainMap.RemoveLayer(newLayer);

                    pw.AddProcessMessage(10, time_base, $"删除中间数据");
                    // 删除中间数据
                    Directory.Delete($@"{folder_path}\历史影像", true);
                    if (File.Exists(pagePath))
                    {
                        File.Delete(pagePath);
                    }
                    

                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }

        private void comBox_layer_DropOpen(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(comBox_layer);
        }

        private void comBox_field_DropOpen(object sender, EventArgs e)
        {
            UITool.AddFieldsToComboxPlus(comBox_layer.ComboxText(), comBox_field);
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/138191712";
            UITool.Link2Web(url);
        }

        // 将所有影像图层加入到ListBox中【带复选框】
        public static void AddAllLayersToListbox(ListBox listBox)
        {
            try
            {
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;
                // 复制影像图层
                BaseTool.CopyResourceRar(@"CCTool.Data.Layers.历史影像.rar", folder_path + @"\历史影像.rar");

                // 清空listBox
                listBox.Items.Clear();

                // 获取所有lyrx文件
                string path = folder_path + @"\历史影像";
                List<string> featureLayers = path.GetAllFiles();

                foreach (var featureLayer in featureLayers)
                {
                    string name = featureLayer[(featureLayer.LastIndexOf(@"\") + 1)..].ToString().Replace(".lyrx", "");
                    CheckBox cb = new()
                    {
                        Content = name,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
            
        }
    }
}
