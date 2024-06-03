using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
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

namespace CCTool.Scripts.DataPross.TXT
{
    /// <summary>
    /// Interaction logic for FeatureClass2TXT.xaml
    /// </summary>
    public partial class FeatureClass2TXT : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public FeatureClass2TXT()
        {
            InitializeComponent();

            combox_digit.Items.Add("1");
            combox_digit.Items.Add("2");
            combox_digit.Items.Add("3");
            combox_digit.Items.Add("4");
            combox_digit.SelectedIndex = 2;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "要素类转TXT";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string lyName = combox_fc.ComboxText();
                string folder_txt = txtFolder2.Text;
                string field_mc = combox_mc.ComboxText();
                string field_yt = combox_yt.ComboxText();
                int digit_xy = int.Parse(combox_digit.Text);
                string txt_head = txtBox_head.Text;

                bool xyReserve = (bool)check_xy.IsChecked;
                bool haveJ = (bool)check_xy_J.IsChecked;

                bool isMerge = (bool)checkbox_merge.IsChecked;

                string field_mj = combox_mj.ComboxText();
                string field_bh = combox_time.ComboxText();

                // 判断参数是否选择完全
                if (lyName == "" || folder_txt == "" || field_mc == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                pw.AddMessage("获取参数", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    // 获取目标FeatureLayer
                    FeatureLayer featurelayer = lyName.TargetFeatureLayer();
                    // 确保要素类的几何类型是多边形
                    if (featurelayer.ShapeType != esriGeometryType.esriGeometryPolygon)
                    {
                        // 如果不是多边形类型，则输出错误信息并退出函数
                        MessageBox.Show("该要素类不是多边形类型。");
                        return;
                    }

                    // 遍历面要素类中的所有要素(有选择就按选择)
                    RowCursor rowCursor = featurelayer.GetSelectCursor();

                    // txt起始段
                    string txt_title = txt_head + "\r\n" + "[地块坐标]" + "\r\n";
                    // 要素的txt
                    List<string> txt_feature_list = new List<string>();
                    while (rowCursor.MoveNext())
                    {
                        using (Feature feature = rowCursor.Current as Feature)
                        {
                            // 获取单个要素的txt
                            string txt_feature = GetWordFromFeature(feature, field_mc, field_yt, field_mj, field_bh, haveJ, digit_xy, xyReserve);
                            txt_feature_list.Add(txt_feature);

                            // 如果分多个txt
                            if (!isMerge)
                            {
                                // 单个要素的txt_all
                                string txt_all = txt_title + txt_feature;
                                // 写入txt文件
                                string txtPath = @$"{folder_txt}\{feature[field_mc]}.txt";

                                if (File.Exists(txtPath))
                                {
                                    File.Delete(txtPath);
                                }
                                File.WriteAllText(txtPath, txt_all);
                            }
                        }
                    }

                    // 如果合并成一个txt
                    if (isMerge)
                    {
                        string txt_all = txt_title;
                        foreach (var txt in txt_feature_list)
                        {
                            txt_all += txt;
                        }
                        // 写入txt文件
                        string txtPath = @$"{folder_txt}\{lyName}.txt";

                        if (File.Exists(txtPath))
                        {
                            File.Delete(txtPath);
                        }
                        File.WriteAllText(txtPath, txt_all);
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

        // 从要素中获取属性文字
        public string GetWordFromFeature(Feature feature, string field_mc, string field_yt, string field_mj, string field_bh, bool haveJ, int digit_xy, bool xyReserve)
        {
            string txt_feature = "";

            // 获取地块名称，地块性质
            Row row = feature as Row;

            string ft_mc = "";
            if (field_mc != "") { ft_mc = row[field_mc]?.ToString() ?? ""; }
            string ft_yt = "";
            if (field_yt != "") { ft_yt = row[field_yt]?.ToString() ?? ""; }
            string ft_mj = "";
            if (field_mj != "") { ft_mj = row[field_mj]?.ToString() ?? ""; }
            string ft_bh = "";
            if (ft_bh != "") { ft_bh = row[field_bh]?.ToString() ?? ""; }

            // 获取面要素的JSON文字
            ArcGIS.Core.Geometry.Polygon polygon = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;
            // 重排，从西北角开始
            List<List<MapPoint>> mapPoints = polygon.ReshotMapPoint();

            // 加一行title
            int count = 0;    // 点的个数
            foreach (var points in mapPoints)
            {
                count += points.Count;
            }
            string title = $"{count},{ft_mj},{ft_bh},{ft_mc},,,{ft_yt},,@" + "\r\n";
            txt_feature += title;

            int index = 1;   // 点号

            int lastNum = 0;
            for (int i = 0; i < mapPoints.Count; i++)
            {
                for (int j = 0; j < mapPoints[i].Count; j++)
                {
                    // 写入点号
                    // J前缀
                    string jFront = haveJ switch
                    {
                        true => "J",
                        false => "",
                    };
                    // 小数位数补齐0
                    string XX = mapPoints[i][j].X.RoundWithFill(digit_xy);
                    string YY = mapPoints[i][j].Y.RoundWithFill(digit_xy);

                    // 点号计算
                    int ptIndex = index;
                    if (j == mapPoints[i].Count - 1)
                    {
                        ptIndex = lastNum + 1;
                    }

                    // 写入折点的XY值
                    if (xyReserve)
                    {
                        // 加入文本
                        txt_feature += $"{jFront}{ptIndex},{i + 1},{XX},{YY}\r\n";
                    }
                    else
                    {
                        // 加入文本
                        txt_feature += $"{jFront}{ptIndex},{i + 1},{YY},{XX}\r\n";
                    }
                    // 如果不是最后一个点，增加点号
                    if (j != mapPoints[i].Count - 1)
                    {
                        index++;
                    }
                }
                lastNum += (mapPoints[i].Count - 1);
            }
            return txt_feature;
        }


        private void combox_mc_Open(object sender, EventArgs e)
        {
            string lyName = combox_fc.ComboxText();
            UITool.AddTextFieldsToComboxPlus(lyName, combox_mc);

        }

        private void combox_yt_Open(object sender, EventArgs e)
        {
            string lyName = combox_fc.ComboxText();
            UITool.AddTextFieldsToComboxPlus(lyName, combox_yt);
        }


        private void openTXTFolderButton_Click(object sender, RoutedEventArgs e)
        {
            txtFolder2.Text = UITool.OpenDialogFolder();
        }

        private void combox_mj_Open(object sender, EventArgs e)
        {
            string lyName = combox_fc.ComboxText();
            UITool.AddFloatFieldsToComboxPlus(lyName, combox_mj);
        }

        private void combox_time_Open(object sender, EventArgs e)
        {
            string lyName = combox_fc.ComboxText();
            UITool.AddTextFieldsToComboxPlus(lyName, combox_time);
        }

        private void combox_fc_DropOpen(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToComboxPlus(combox_fc);
        }
    }
}
