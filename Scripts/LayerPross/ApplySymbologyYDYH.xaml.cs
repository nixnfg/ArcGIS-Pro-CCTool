using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Brushes = System.Windows.Media.Brushes;
using Color = ArcGIS.Core.Internal.CIM.Color;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for ApplySymbologyYDYH.xaml
    /// </summary>
    public partial class ApplySymbologyYDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ApplySymbologyYDYH()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "应用符号系统";

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string field = combox_field.ComboxText();
                bool isDelete0 = (bool)checkBox_delete.IsChecked;
                // 符号系统选择
                string symbol = "";
                if (rb_gk.IsChecked == true){symbol = "gk";}
                else if (rb_gk_new.IsChecked == true) { symbol = "gk_new"; }
                else if(rb_cg.IsChecked == true){ symbol = "cg"; }
                else if (rb_cg_zj.IsChecked == true) { symbol = "cg_zj"; }
                else if (rb_sd.IsChecked == true) { symbol = "sd"; }
                else if (rb_sd202.IsChecked == true) { symbol = "sd202"; }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();   //关闭窗口
                await QueuedTask.Run(() =>
                {
                    // 获取工程默认文件夹位置
                    var def_path = Project.Current.HomeFolderPath;
                    // 获取当前地图
                    var map = MapView.Active.Map;
                    // 获取图层
                    FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

                    // 如果选择的不是面要素或是无选择，则返回
                    if (ly.ShapeType != esriGeometryType.esriGeometryPolygon || ly == null)
                    {
                        pw.AddMessage("错误！请选择一个面要素！", Brushes.Red);
                        return;
                    }
                    
                    if (symbol == "gk")
                    {
                        pw.AddProcessMessage(10, "复制【国空用地(旧版)】图层文件");
                        // 复制符号图层文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.国空用地.lyrx", def_path + @"\国空用地.lyrx");
                        
                        pw.AddProcessMessage(40, time_base, "应用【国空(旧版)】符号系统");
                        // 应用符号系统
                        EditTool.ApplySymbol(ly, field, def_path + @"\国空用地.lyrx");
                        File.Delete(def_path + @"\国空用地.lyrx");
                    }

                    if (symbol == "gk_new")
                    {
                        pw.AddProcessMessage(10, "复制【国空用地(新版)】图层文件");
                        // 复制符号图层文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.国空用地新版.lyrx", def_path + @"\国空用地新版.lyrx");

                        pw.AddProcessMessage(40, time_base, "应用【国空(新版)】符号系统");
                        // 应用符号系统
                        EditTool.ApplySymbol(ly, field, def_path + @"\国空用地新版.lyrx");
                        
                        File.Delete(def_path + @"\国空用地新版.lyrx");
                    }

                    else if (symbol == "cg")
                    {
                        pw.AddProcessMessage(10, "复制【村规用地】图层文件");
                        // 复制符号图层文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.村规用地.lyrx", def_path + @"\村规用地.lyrx");
                        pw.AddProcessMessage(40, time_base, "应用【福建村规】符号系统");
                        // 应用符号系统新
                        EditTool.ApplySymbol(ly, field, def_path + @"\村规用地.lyrx");

                        File.Delete(def_path + @"\村规用地.lyrx");
                    }
                    else if (symbol == "cg_zj")
                    {
                        pw.AddProcessMessage(10, "复制【村规用地】图层文件");
                        // 复制符号图层文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.浙江新版村规用地.lyrx", def_path + @"\浙江新版村规用地.lyrx");
                        pw.AddProcessMessage(40, time_base, "应用【浙江村规】符号系统");
                        // 应用符号系统新
                        EditTool.ApplySymbol(ly, field, def_path + @"\浙江新版村规用地.lyrx");

                        File.Delete(def_path + @"\浙江新版村规用地.lyrx");
                    }
                    else if (symbol == "sd")
                    {
                        pw.AddProcessMessage(10, "复制【三调用地】图层文件");
                        // 复制符号图层文件  
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.三调用地fin.lyrx", def_path + @"\三调用地fin.lyrx");
                        pw.AddProcessMessage(40, time_base, "应用【三调】符号系统");

                        // 应用符号系统
                        LayerDocument lyrFile = new LayerDocument(def_path + @"\三调用地fin.lyrx");

                        CIMLayerDocument cimLyrDoc = lyrFile.GetCIMLayerDocument();

                        CIMUniqueValueRenderer uvr = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer as CIMUniqueValueRenderer;

                        uvr.Fields = new string[] { "DLBM" };
                        // 修改每个标注类别的表达式
                        foreach (CIMUniqueValueClass uvClass in uvr.Groups[0].Classes)
                        {
                            var va = uvClass.Values[0].FieldValues[0].ToString();
                            uvClass.Label = va + DataLib.dic_sdAll[va];
                        }
                        // 应用渲染器
                        ly.SetRenderer(uvr);

                        File.Delete(def_path + @"\三调用地fin.lyrx");
                    }

                    else if (symbol == "sd202")
                    {
                        pw.AddProcessMessage(10, "复制【三调用地_含202】图层文件");
                        // 复制符号图层文件  
                        string lyPath = def_path + @"\三调用地包含202.lyrx";
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.三调用地包含202.lyrx", lyPath);
                        pw.AddProcessMessage(40, time_base, "应用【三调_含202】符号系统");

                        // 应用符号系统
                        LayerDocument lyrFile = new LayerDocument(lyPath);

                        CIMLayerDocument cimLyrDoc = lyrFile.GetCIMLayerDocument();

                        CIMUniqueValueRenderer uvr = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer as CIMUniqueValueRenderer;

                        uvr.Fields = new string[] { "DLBM" };
                        // 修改每个标注类别的表达式
                        foreach (CIMUniqueValueClass uvClass in uvr.Groups[0].Classes)
                        {
                            var va = uvClass.Values[0].FieldValues[0].ToString();
                            uvClass.Label = va + DataLib.dic_sdAll[va];
                        }
                        // 应用渲染器
                        ly.SetRenderer(uvr);


                        //// 应用符号系统新
                        //EditTool.ApplySymbol(ly, field, lyPath);

                        File.Delete(lyPath);
                    }

                    if (isDelete0)
                    {
                        pw.AddProcessMessage(10, time_base, "删除计数为0的值");
                        // 删除计数为0的值
                        EditTool.Delete0uvClass(ly);
                    }

                    pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }


        private void combox_field_DropOpen(object sender, EventArgs e)
        {
            // 获取图层
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

            UITool.AddTextFieldsToComboxPlus(ly, combox_field);
        }

        private void rb_sd_Checked(object sender, RoutedEventArgs e)
        {
            combox_field.IsEnabled = false;
        }

        private void rb_sd_Unchecked(object sender, RoutedEventArgs e)
        {
            combox_field.IsEnabled = true;
        }

        private void btn_help_click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/135619043?spm=1001.2014.3001.5502";
            UITool.Link2Web(url);
        }

    }
}
