﻿using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing;
using Aspose.Cells;
using Aspose.Cells.Drawing;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CheckBox = System.Windows.Controls.CheckBox;
using Field = ArcGIS.Core.Data.Field;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using MessageBox = System.Windows.Forms.MessageBox;
using Polygon = ArcGIS.Core.Geometry.Polygon;
using Row = ArcGIS.Core.Data.Row;

namespace CCTool.Scripts.GTApp.SJSB
{
    /// <summary>
    /// Interaction logic for JSYDBP_DC.xaml
    /// </summary>
    public partial class JSYDBP_DC : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public JSYDBP_DC()
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
        string tool_name = "建设用地报批要素转TXT";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string folder_path = txtFolder.Text;
                string folder_txt = txtFolder2.Text;
                var cb_shps = listbox_shp.Items;
               
                string field_jzd = combox_jzd.Text;
                string field_mj = combox_mj.Text;
                string field_bh = combox_bh.Text;
                string field_dkmc = combox_dkmc.Text;
                string field_lx = combox_dklx.Text;
                string field_tfh = combox_tfh.Text;
                string field_dlmc = combox_dlmc.Text;
                string field_dlbm = combox_dlbm.Text;

                int digit_xy = int.Parse(combox_digit.Text);
                string txt_head = txtBox_head.Text;

                bool xyReserve = (bool)check_xy.IsChecked;
                bool haveJ = (bool)check_xy_J.IsChecked;


                // 判断参数是否选择完全
                if (folder_path == "" || folder_txt == "" || cb_shps.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                // 获取所有选中的shp
                List<string> list_shpPath = new List<string>();
                foreach (CheckBox shp in cb_shps)
                {
                    if (shp.IsChecked == true)
                    {
                        list_shpPath.Add(folder_path + shp.Content);
                    }
                }

                pw.AddMessage("获取参数", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    foreach (string fullPath in list_shpPath)
                    {
                        // 初始化写入txt的内容
                        string txt_all = txt_head + "\r\n" + "[地块坐标]" + "\r\n";

                        pw.AddProcessMessage(10, time_base, fullPath);
                        string shp_name = fullPath[(fullPath.LastIndexOf(@"\") + 1)..];  // 获取要素名
                        string shp_path = fullPath[..(fullPath.LastIndexOf(@"\"))];  // 获取shp名

                        // 打开shp
                        FileSystemConnectionPath fileConnection = new FileSystemConnectionPath(new Uri(shp_path), FileSystemDatastoreType.Shapefile);
                        using FileSystemDatastore shapefile = new FileSystemDatastore(fileConnection);
                        // 获取FeatureClass
                        FeatureClass featureClass = shapefile.OpenDataset<FeatureClass>(shp_name);

                        using (RowCursor rowCursor = featureClass.Search())
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Feature feature = rowCursor.Current as Feature)
                                {
                                    // 获取地块名称，地块性质
                                    Row row = feature as Row;

                                    string ft_jzd = "";
                                    if (field_jzd != "") { ft_jzd = row[field_jzd]?.ToString() ?? ""; }
                                    string ft_mj = "";
                                    if (field_mj != "") { ft_mj = row[field_mj]?.ToString() ?? ""; }
                                    string ft_bh = "";
                                    if (field_bh != "") { ft_bh = row[field_bh]?.ToString() ?? ""; }
                                    string ft_dkmc = "";
                                    if (field_dkmc != "") { ft_dkmc = row[field_dkmc]?.ToString() ?? ""; }
                                    string ft_lx = "";
                                    if (field_lx != "") { ft_lx = row[field_lx]?.ToString() ?? ""; }
                                    string ft_tfh = "";
                                    if (field_tfh != "") { ft_tfh = row[field_tfh]?.ToString() ?? ""; }
                                    string ft_dlmc = "";
                                    if (field_dlmc != "") { ft_dlmc = row[field_dlmc]?.ToString() ?? ""; }
                                    string ft_dlbm = "";
                                    if (field_dlbm != "") { ft_dlbm = row[field_dlbm]?.ToString() ?? ""; }

                                    // 获取面要素的JSON文字
                                    Polygon polygon = feature.GetShape() as Polygon;
                                    // 重排，从西北角开始
                                    List<List<MapPoint>> mapPoints = polygon.ReshotMapPoint();

                                    // 加一行title
                                    int count = 0;    // 点的个数
                                    foreach (var points in mapPoints)
                                    {
                                        count += points.Count;
                                    }

                                    string title = $"{count-1},{ft_mj},{ft_bh},{ft_dkmc},{ft_lx},{ft_tfh},{ft_dlmc},{ft_dlbm},@" + "\r\n";
                                    txt_all += title;

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
                                                txt_all += $"{jFront}{ptIndex},{i + 1},{XX},{YY}\r\n";
                                            }
                                            else
                                            {
                                                // 加入文本
                                                txt_all += $"{jFront}{ptIndex},{i + 1},{YY},{XX}\r\n";
                                            }
                                            // 如果不是最后一个点，增加点号
                                            if (j != mapPoints[i].Count - 1)
                                            {
                                                index++;
                                            }
                                        }
                                        lastNum += (mapPoints[i].Count - 1);
                                    }
                                }
                            }
                        }
                        // 写入txt文件
                        string txtPath = @$"{folder_txt}\{shp_name.Replace(".shp", "")}.txt";
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

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开SHP文件夹
            string folder = UITool.OpenDialogFolder();
            txtFolder.Text = folder;
            // 清除listbox
            listbox_shp.Items.Clear();
            // 生成SHP要素列表
            if (txtFolder.Text != "")
            {
                // 获取所有shp文件
                var files = folder.GetAllFiles(".shp");
                foreach (var file in files)
                {
                    // 将shp文件做成checkbox放入列表中
                    CheckBox cb = new CheckBox();
                    cb.Content = file.Replace(folder, "");
                    cb.IsChecked = true;
                    listbox_shp.Items.Add(cb);
                }
            }
        }

        private void openTXTFolderButton_Click(object sender, RoutedEventArgs e)
        {
            txtFolder2.Text = UITool.OpenDialogFolder();
        }
        private void combox_jzd_Open(object sender, EventArgs e)
        {
            try
            {
                string folder_path = txtFolder.Text;
                var cb_shps = listbox_shp.Items;
                // 获取所有选中的shp
                string shpPath = "";
                foreach (CheckBox shp in cb_shps)
                {
                    if (shp.IsChecked == true)
                    {
                        shpPath = folder_path + shp.Content;
                    }
                    break;
                }

                UITool.AddFieldsToCombox(shpPath, combox_jzd);

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }

        private void combox_mj_Open(object sender, EventArgs e)
        {
            try
            {
                string folder_path = txtFolder.Text;
                var cb_shps = listbox_shp.Items;
                // 获取所有选中的shp
                string shpPath = "";
                foreach (CheckBox shp in cb_shps)
                {
                    if (shp.IsChecked == true)
                    {
                        shpPath = folder_path + shp.Content;
                    }
                    break;
                }

                UITool.AddFieldsToCombox(shpPath, combox_mj);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }



        private void combox_bh_Open(object sender, EventArgs e)
        {
            string folder_path = txtFolder.Text;
            var cb_shps = listbox_shp.Items;
            // 获取所有选中的shp
            string shpPath = "";
            foreach (CheckBox shp in cb_shps)
            {
                if (shp.IsChecked == true)
                {
                    shpPath = folder_path + shp.Content;
                }
                break;
            }

            UITool.AddFieldsToCombox(shpPath, combox_bh);
        }

        private void combox_dkmc_Open(object sender, EventArgs e)
        {
            string folder_path = txtFolder.Text;
            var cb_shps = listbox_shp.Items;
            // 获取所有选中的shp
            string shpPath = "";
            foreach (CheckBox shp in cb_shps)
            {
                if (shp.IsChecked == true)
                {
                    shpPath = folder_path + shp.Content;
                }
                break;
            }

            UITool.AddTextFieldsToCombox(shpPath, combox_dkmc);
        }

        private void combox_dklx_Open(object sender, EventArgs e)
        {
            string folder_path = txtFolder.Text;
            var cb_shps = listbox_shp.Items;
            // 获取所有选中的shp
            string shpPath = "";
            foreach (CheckBox shp in cb_shps)
            {
                if (shp.IsChecked == true)
                {
                    shpPath = folder_path + shp.Content;
                }
                break;
            }

            UITool.AddTextFieldsToCombox(shpPath, combox_dklx);
        }

        private void combox_tfh_Open(object sender, EventArgs e)
        {
            string folder_path = txtFolder.Text;
            var cb_shps = listbox_shp.Items;
            // 获取所有选中的shp
            string shpPath = "";
            foreach (CheckBox shp in cb_shps)
            {
                if (shp.IsChecked == true)
                {
                    shpPath = folder_path + shp.Content;
                }
                break;
            }

            UITool.AddTextFieldsToCombox(shpPath, combox_tfh);
        }

        private void combox_dlmc_Open(object sender, EventArgs e)
        {
            string folder_path = txtFolder.Text;
            var cb_shps = listbox_shp.Items;
            // 获取所有选中的shp
            string shpPath = "";
            foreach (CheckBox shp in cb_shps)
            {
                if (shp.IsChecked == true)
                {
                    shpPath = folder_path + shp.Content;
                }
                break;
            }

            UITool.AddTextFieldsToCombox(shpPath, combox_dlmc);
        }

        private void combox_dlbm_Open(object sender, EventArgs e)
        {
            string folder_path = txtFolder.Text;
            var cb_shps = listbox_shp.Items;
            // 获取所有选中的shp
            string shpPath = "";
            foreach (CheckBox shp in cb_shps)
            {
                if (shp.IsChecked == true)
                {
                    shpPath = folder_path + shp.Content;
                }
                break;
            }

            UITool.AddTextFieldsToCombox(shpPath, combox_dlbm);
        }


    }
}
