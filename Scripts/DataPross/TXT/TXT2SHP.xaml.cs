﻿using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells.Drawing;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Shared;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using static System.Net.Mime.MediaTypeNames;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace CCTool.Scripts.DataPross.TXT
{
    /// <summary>
    /// Interaction logic for TXT2SHP.xaml
    /// </summary>
    public partial class TXT2SHP : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public TXT2SHP()
        {
            InitializeComponent();
            Init();
        }

        // 初始化
        private void Init()
        {
            // combox_sr框中添加几种预制坐标系
            combox_sr.Items.Add("WGS_1984");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_25");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_26");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_27");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_28");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_29");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_30");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_31");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_32");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_33");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_34");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_35");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_36");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_37");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_38");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_39");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_40");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_41");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_42");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_43");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_44");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_45");

            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_75E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_78E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_81E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_84E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_87E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_90E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_93E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_96E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_99E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_102E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_105E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_108E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_111E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_114E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_117E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_120E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_123E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_126E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_129E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_132E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_135E");

        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "TXT文件转SHP";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string folder_path = textFolderPath.Text;
                string shpPath = textFeatureClassPath.Text;
                string spatial_reference = combox_sr.Text;

                var cb_txts = listbox_txt.Items;

                // 判断参数是否选择完全
                if (folder_path == "" || shpPath == "" || spatial_reference == "" || cb_txts.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 获取所有选中的txt
                List<string> list_txtPath = new List<string>();
                foreach (CheckBox shp in cb_txts)
                {
                    if (shp.IsChecked == true)
                    {
                        list_txtPath.Add(folder_path + shp.Content);
                    }
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                Close();
                // 异步执行
                await QueuedTask.Run(() =>
                {
                    foreach (string txtPath in list_txtPath)
                    {
                        string shp_name_old = txtPath[(txtPath.LastIndexOf(@"\") + 1)..].Replace(".txt", "");  // 获取要素名
                        // 预处理一下要素名，避免一些奇奇怪怪的符号
                        string shp_name = shp_name_old.Replace(".", "_");

                        pw.AddProcessMessage(@$"创建要素：{shp_name}", 10, time_base, Brushes.Black);

                        // 创建一个空要素
                        Arcpy.CreateFeatureclass(shpPath, shp_name, "POLYGON", spatial_reference);
                        // 新建字段
                        Arcpy.AddField(@$"{shpPath}\{shp_name}.shp", "文件名", "TEXT");
                        Arcpy.AddField(@$"{shpPath}\{shp_name}.shp", "编号", "TEXT");
                        Arcpy.AddField(@$"{shpPath}\{shp_name}.shp", "名称", "TEXT");
                        Arcpy.AddField(@$"{shpPath}\{shp_name}.shp", "类型", "TEXT");
                        Arcpy.AddField(@$"{shpPath}\{shp_name}.shp", "图幅", "TEXT");
                        Arcpy.AddField(@$"{shpPath}\{shp_name}.shp", "用途", "TEXT");

                        // 打开shp
                        FileSystemConnectionPath fileConnection = new FileSystemConnectionPath(new Uri(shpPath), FileSystemDatastoreType.Shapefile);
                        using FileSystemDatastore shapefile = new FileSystemDatastore(fileConnection);
                        // 获取FeatureClass
                        FeatureClass featureClass = shapefile.OpenDataset<FeatureClass>(shp_name);

                        // 预设文本内容
                        string text = "";
                        // 获取txt文件的编码方式
                        Encoding encoding = OfficeTool.GetEncodingType(txtPath);
                        // 读取【ANSI和UTF-8】的不同+++++++（ANSI为0，UTF-8为3）
                        // 我也不知道具体原理，只是找出差异点作个判断，以后再来解决这个问题------
                        int encoding_index = int.Parse(encoding.Preamble.ToString().Substring(encoding.Preamble.ToString().Length - 2, 1));

                        if (encoding_index == 0)        // ANSI编码的情况
                        {
                            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                            using (StreamReader sr = new StreamReader(txtPath, Encoding.GetEncoding("GBK"))) { text = sr.ReadToEnd(); }
                        }
                        else if (encoding_index == 3)               // UTF8编码的情况
                        {
                            using (StreamReader sr = new StreamReader(txtPath, Encoding.UTF8)) { text = sr.ReadToEnd(); }
                        }

                        // 文本中的【@】符号放前
                        string updata_text = ChangeSymbol(text);

                        // 获取坐标点文本
                        string[] fcs_text = updata_text.Split("@");
                        // 去除第一部分非坐标文本
                        List<string> fcs_text2List = new List<string>(fcs_text);
                        fcs_text2List.RemoveAt(0);

                        // 一个文件可能有多要素
                        foreach (var txt in fcs_text2List)
                        {
                            // 获取要素的部件数
                            int parts = GetCount(txt);

                            // 构建坐标点集合
                            var vertices_list = new List<List<Coordinate2D>>();
                            for (int i = 0; i < parts; i++)
                            {
                                var vertices = new List<Coordinate2D>();
                                vertices_list.Add(vertices);
                            }

                            // 编号、名称、类型、图幅、用途
                            string bh = "";
                            string mc = "";
                            string lx = "";
                            string tf = "";
                            string yt = "";
                            // 根据换行符分解坐标点文本
                            string[] list_point = txt.Split("\n");

                            foreach (var point in list_point)
                            {
                                if (point.StringInCount(",") == 8)     // 名称、地块编号、功能文本
                                {
                                    bh = point.Split(",")[2];
                                    mc = point.Split(",")[3];
                                    lx = point.Split(",")[4];
                                    tf = point.Split(",")[5];
                                    yt = point.Split(",")[6];
                                }
                                else if (point.StringInCount(",") == 3)           // 点坐标文本
                                {
                                    int fid = int.Parse(point.Split(",")[1].Replace(" ", ""));        // 图斑部件号
                                    double lat = double.Parse(point.Split(",")[3].Replace(" ", ""));         // 经度
                                    double lng = double.Parse(point.Split(",")[2].Replace(" ", ""));         // 纬度

                                    vertices_list[fid - 1].Add(new Coordinate2D(lat, lng));    // 加入坐标点集合
                                }
                                else     // 跳过无坐标部份的文本
                                {
                                    continue;
                                }
                            }

                            /// 构建面要素
                            // 创建编辑操作对象
                            EditOperation editOperation = new EditOperation();
                            editOperation.Callback(context =>
                            {
                                // 获取要素定义
                                FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition();
                                // 创建RowBuffer
                                using RowBuffer rowBuffer = featureClass.CreateRowBuffer();

                                // 写入字段值
                                rowBuffer["文件名"] = shp_name.Replace(".txt", "");
                                rowBuffer["编号"] = bh;
                                rowBuffer["名称"] = mc;
                                rowBuffer["类型"] = lx;
                                rowBuffer["图幅"] = tf;
                                rowBuffer["用途"] = yt;

                                PolygonBuilderEx pb = new PolygonBuilderEx(vertices_list[0]);
                                // 如果有空洞，则添加内部Polygon
                                if (vertices_list.Count > 1)
                                {
                                    for (int i = 0; i < vertices_list.Count - 1; i++)
                                    {
                                        pb.AddPart(vertices_list[i + 1]);
                                    }
                                }
                                // 给新添加的行设置形状
                                rowBuffer[featureClassDefinition.GetShapeField()] = pb.ToGeometry();

                                // 在表中创建新行
                                using Feature feature = featureClass.CreateRow(rowBuffer);
                                context.Invalidate(feature);      // 标记行为无效状态
                            }, featureClass);

                            // 执行编辑操作
                            editOperation.Execute();
                        }
                        // 保存编辑
                        Project.Current.SaveEditsAsync();
                    }

                    pw.AddProcessMessage("工具运行完成！！！", 10, time_base, Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }

        private void openFeatureClassButton_Click(object sender, RoutedEventArgs e)
        {
            textFeatureClassPath.Text = UITool.OpenDialogFolder();
        }

        private void openSHPButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开TXT文件夹
            string folder = UITool.OpenDialogFolder();
            textFolderPath.Text = folder;
            // 清除listbox
            listbox_txt.Items.Clear();
            // 生成TXT要素列表
            if (textFolderPath.Text != "")
            {
                // 获取所有shp文件
                var files = folder.GetAllFiles(".txt");
                foreach (var file in files)
                {
                    // 将txt文件做成checkbox放入列表中
                    CheckBox cb = new CheckBox();
                    cb.Content = file.Replace(folder, "");
                    cb.IsChecked = true;
                    listbox_txt.Items.Add(cb);
                }
            }
        }

        // 文本中的【@】符号放前
        public static string ChangeSymbol(string text)
        {
            string[] lins = text.Split('\n');
            string updata_lins = "";
            foreach (string line in lins)
            {

                if (line.Contains("@"))
                {
                    string newline = line.Replace("@", "");
                    newline = "@" + newline;
                    updata_lins += newline + "\n";
                }
                else
                {
                    updata_lins += line + "\n";
                }
            }
            return updata_lins;
        }

        // 获取指标
        public static Dictionary<string, string> GetAtt(string text)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string new_text = text[text.IndexOf("[项目信息]")..text.IndexOf("[属性描述]")];
            string[] lines = new_text.Split("\n");
            foreach (string line in lines)
            {
                if (line.Contains("="))
                {
                    string before = line[..line.IndexOf("=")];
                    string after = line[(line.IndexOf("=") + 1)..];
                    dict.Add(before, after);
                }
            }
            return dict;
        }

        // 获取要素的部件数
        public static int GetCount(string lines)
        {
            List<string> indexs = new List<string>();

            // 根据换行符分解坐标点文本
            string[] list_point = lines.Split("\n");

            foreach (var point in list_point)
            {
                if (point.StringInCount(",") == 3)          // 点坐标文本
                {
                    // 判断是否带空洞
                    string fid = point.Split(",")[1];        // 图斑部件号
                    if (!indexs.Contains(fid))
                    {
                        indexs.Add(fid);
                    }
                }
                else    // 路过非点坐标文本
                {
                    continue;
                }
            }

            return indexs.Count;
        }

        // 获取要素数据
        public static List<string> GetParts(string lines)
        {
            // 获取坐标点文本
            string[] fcs_text = lines.Split("@");

            // 去除第一部分非坐标文本
            List<string> fcs_text2List = new List<string>(fcs_text);
            fcs_text2List.RemoveAt(0);

            List<string> indexs = new List<string>();
            // 获取要素个数
            foreach (var line in fcs_text2List)
            {
                // 根据换行符分解坐标点文本
                string[] list_point = line.Split("\r");
                string fid = list_point[3].Split(",")[1];        // 图斑部件号
                if (!indexs.Contains(fid))
                {
                    indexs.Add(fid);
                }
            }
            // 生成列表
            List<string> parts = new List<string>();
            for (int i = 0; i < indexs.Count; i++)
            {
                parts.Add("");
            }

            foreach (var line in fcs_text2List)
            {
                // 根据换行符分解坐标点文本
                string[] list_point = line.Split("\r");
                int fid = int.Parse(list_point[3].Split(",")[1]);        // 图斑部件号
                parts[fid - 1] += "@";
                parts[fid - 1] += line;
            }
            return parts;
        }
    }
}
