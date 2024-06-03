using ApeFree.DataStore.Core;
using ApeFree.DataStore;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using SharpCompress.Common;
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
using System.Xml;
using static System.Net.WebRequestMethods;
using ApeFree.DataStore.Local;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using System.IO;
using CCTool.Scripts.ToolManagers;
using System.Threading;
using CheckBox = System.Windows.Controls.CheckBox;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for ExportLayoutByLayer.xaml
    /// </summary>
    public partial class ExportLayoutByLayer : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        // 设置一个store
        private LocalStore<LayoutSettings> store;

        public ExportLayoutByLayer()
        {
            InitializeComponent();

            string savePath = @"D:\ProSDKSettings\ExportLayoutByLayer.txt";
            // 创建数据存储文件
            store = StoreFactory.Factory.CreateLocalStore<LayoutSettings>(new LocalStoreAccessSettings(savePath));

            // 设置一个初始布局
            var layouts = Project.Current.GetItems<LayoutProjectItem>().ToList();
            comBox_layout.Items.Add(layouts[0].Name);
            comBox_layout.SelectedIndex= 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "按图层导出布局";

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
                string layoutName = comBox_layout.Text;
                string baselayer = comBox_baselayer.ComboxText();

                // 获取导出图层
                List<string> listLayer = UITool.GetStringFromListbox(listBox_layer);

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

                // 判断参数是否选择完全
                if (outputPath == "" || text_dpi.Text == "" || listBox_layer.Items.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                pw.AddProcessMessage(10, "获取相关参数");

                // 获取LayoutProjectItem
                LayoutProjectItem layoutProjectItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals(layoutName));

                await QueuedTask.Run(() =>
                {
                    // 获取图层显示信息
                    Dictionary<string, bool> dic =  MapCtlTool.GetLayerVisible();
                    // 获取图底名称
                    string baseSingleLayer = baselayer.GetLayerSingleName();

                    // 去除数字标记
                    if (baseSingleLayer.Contains("："))
                    {
                        baseSingleLayer = baseSingleLayer[..baseSingleLayer.IndexOf("：")];
                    }

                    // 去除数字标记
                    for (int i = 0; i < listLayer.Count; i++)
                    {
                        string text = listLayer[i];
                        if (text.Contains("："))
                        {
                            text = text[..text.IndexOf("：")];
                        }
                    }

                    foreach (string item in listLayer)
                    {
                        pw.AddProcessMessage(20, time_base, "导出图层：" + item);

                        // 获取layout 
                        Layout layout = layoutProjectItem.GetLayout();
                        // 获取单独图层名
                        string modifyLayer = item.GetLayerSingleName();

                        // 控制图层显示
                        MapCtlTool.ControlLayer(modifyLayer);
                        // 底图控制
                        MapCtlTool.ControlLayerVisible(baseSingleLayer);

                        // JPG图片属性
                        JPEGFormat JPG = new JPEGFormat()
                        {
                            HasWorldFile = true,
                            Resolution = dpi,               // 分辨率
                            OutputFileName = outputPath + @"\" + modifyLayer + @".jpg",      // 输出路径
                        };
                        // PNG图片属性
                        PNGFormat PNG = new PNGFormat()
                        {
                            HasWorldFile = true,
                            HasTransparentBackground = true,   // 透明底
                            Resolution = dpi,               // 分辨率
                            OutputFileName = outputPath + @"\" + modifyLayer + @".png",      // 输出路径
                        };
                        // PDF图片属性
                        PDFFormat PDF = new PDFFormat()
                        {
                            OutputFileName = outputPath + @"\" + modifyLayer + @".pdf",      // 输出路径
                            Resolution = dpi,               // 分辨率
                            DoCompressVectorGraphics = true,   // 是否压缩矢量图形
                            DoEmbedFonts = true,            // 是否执行嵌入字体         
                            HasGeoRefInfo = true,             // 是否具有地理参考信息
                            ImageCompression = ImageCompression.Adaptive,   // 图形压缩.自适应
                            ImageQuality = ImageQuality.Best,           // 图形质量
                            LayersAndAttributes = LayersAndAttributes.LayersAndAttributes   // 图层  属性
                        };

                        pw.AddProcessMessage(0, time_base, "暂停2秒", Brushes.Green);
                        Thread.Sleep(2000);

                        // 导出JPG
                        if (pic_type == "jpg")
                        {
                            layout.Export(JPG);
                        }
                        // 导出PNG
                        if (pic_type == "png")
                        {
                            layout.Export(PNG);
                        }
                        // 导出PDF
                        if (pic_type == "pdf")
                        {
                            layout.Export(PDF);
                        }
                    }

                    // 恢复图层显示信息
                    MapCtlTool.SetLayerVisible(dic);
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }

        private void comBox_layout_DropOpen(object sender, EventArgs e)
        {
            UITool.AddAllLayoutToCombox(comBox_layout);
        }

        // 移除列表中选定的item
        private void btn_delete_layer_Click(object sender, RoutedEventArgs e)
        {
            listBox_layer.Items.Remove(listBox_layer.SelectedItem);
        }

        // 窗体加载
        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                // 加载store
                store.Load();
                // 参数获取
                textFolderPath.Text = store.Value.picPath.ToString();    // 输出图片位置
                text_dpi.Text = store.Value.dpi.ToString();    // DPI
                // 图片格式
                rb_jpg.IsChecked = store.Value.jpg;
                rb_png.IsChecked = store.Value.png;
                rb_pdf.IsChecked = store.Value.pdf;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
        // 窗体关闭
        private void Form_Closed(object sender, EventArgs e)
        {
            try
            {
                // 保存参数
                store.Value.dpi = int.Parse(text_dpi.Text);
                store.Value.picPath = textFolderPath.Text;
                // 图片格式
                store.Value.jpg = (bool)rb_jpg.IsChecked;
                store.Value.png = (bool)rb_png.IsChecked;
                store.Value.pdf = (bool)rb_pdf.IsChecked;
                // 保存store
                store.Save();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void comBox_baselayer_DropOpen(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(comBox_baselayer);
        }
        // 加载图层
        private void listBox_layer_Load(object sender, RoutedEventArgs e)
        {
            UITool.AddCanseeLayersToListbox(listBox_layer);
        }

        private void btn_select_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_layer.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_field = listBox_layer.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = true;
            }
        }

        private void btn_unSelect_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_layer.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_field = listBox_layer.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = false;
            }
        }


    }
    // 参数设置
    public class LayoutSettings
    {
        // 导出图纸的DPI
        public int dpi { get; set; } = 300;
        // 导出图片的位置
        public string picPath { get; set; } = "";
        // 图片格式
        public bool jpg { get; set; } = false;
        public bool pdf { get; set; } = false;
        public bool png { get; set; } = true;
    }
}
