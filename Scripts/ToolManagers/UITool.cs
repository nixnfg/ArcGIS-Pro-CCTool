using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing.Controls;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.MessageWindow;
using CCTool.Scripts.ToolManagers;
using Microsoft.Win32;
using NPOI.OpenXmlFormats.Shared;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CCTool.Scripts.Manager
{
    public class UITool
    {
        // 获取富文本的所有文字
        public static string GetRichText(RichTextBox rb)
        {
            TextRange textRange = new TextRange(rb.Document.ContentStart, rb.Document.ContentEnd);
            var text = textRange.Text;

            var result = text.Replace("\n", "").Replace("\r", "");

            if (result is null)
            {
                result = "";
            }

            return result;
        }


        // 将当前地图的所有要素图层加入到Combox中
        public static void AddFeatureLayersToCombox(ComboBox comboBox)
        {
            try
            {
                // 清空combox_field
                comboBox.Items.Clear();
                // 获取当前地图
                Map map = MapView.Active.Map;
                // 获取所有要素图层
                List<string> featureLayers = map.AllFeatureLayers();
                foreach (string featureLayer in featureLayers)
                {
                    comboBox.Items.Add(featureLayer);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                throw;
            }

        }

        // 将当前地图的所有要素图层加入到Combox中
        public static void AddFeatureLayersToComboxPlus(ComboBox comboBox)
        {
            try
            {
                // 获取当前地图
                Map map = MapView.Active.Map;
                // 获取所有要素图层
                List<string> featureLayers = map.AllFeatureLayers();

                List<ComboBoxContent> flc = new List<ComboBoxContent>();

                // 图层图片
                string imagePath = "/CCTool;component/Data/Icons/layer.png";
                // 加到combox中
                foreach (string featureLayer in featureLayers)
                {
                    flc.Add(new ComboBoxContent() { Path = imagePath , Name = featureLayer});
                }
                comboBox.ItemsSource = flc;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                throw;
            }

        }


        // 初始化一个要素图层加入到Combox中
        public static void InitFeatureLayerToComboxPlus(ComboBox comboBox, string lyName)
        {
            try
            {
                // 获取当前地图
                Map map = MapView.Active.Map;
                // 获取要素图层
                FeatureLayer featureLayer = map.FindLayers(lyName).FirstOrDefault() as FeatureLayer;

                List<ComboBoxContent> flc = new List<ComboBoxContent>();

                // 图层图片
                string imagePath = "/CCTool;component/Data/Icons/layer.png";

                // 加到combox中
                flc.Add(new ComboBoxContent() { Path = imagePath, Name = lyName });

                comboBox.ItemsSource = flc;

                comboBox.SelectedIndex= 0;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                throw;
            }

        }

        // 将被选择图斑的要素图层加入到Combox中
        public async static void AddSelectFeatureLayersToCombox(ComboBox comboBox)
        {
            try
            {
                // 设置一个空集合
                List<string> lys = new List<string>();

                await QueuedTask.Run(() =>
                {
                    // 获取活动地图视图中选定的要素集合
                    var selectedSet = MapView.Active.Map.GetSelection();
                    // 将选定的要素集合转换为字典形式
                    var selectedList = selectedSet.ToDictionary();

                    // 遍历每个选定图层及其关联的对象 ID
                    foreach (var layer in selectedList)
                    {
                        // 获取图层和关联的对象 ID
                        FeatureLayer featureLayer = layer.Key as FeatureLayer;
                        if (!lys.Contains(featureLayer.Name))
                        {
                            lys.Add(featureLayer.Name);
                        }
                    }
                });
                /// 加入combox
                // 获取当前地图
                Map map = MapView.Active.Map;
                // 清空combox_field
                comboBox.Items.Clear();
                // 将图层加入combox
                foreach (var layer in lys)
                {
                    // 获取要素图层
                    FeatureLayer featureLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name == layer).FirstOrDefault();
                    comboBox.Items.Add(featureLayer.Name);
                }
                // 设置一个默认项
                if (lys.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }


        // 将被选择图斑的要素图层加入到Combox中
        public async static void AddSelectFeatureLayersToComboxPlus(ComboBox comboBox)
        {
            try
            {
                // 设置一个空集合
                List<string> lys = new List<string>();

                await QueuedTask.Run(() =>
                {
                    // 获取活动地图视图中选定的要素集合
                    var selectedSet = MapView.Active.Map.GetSelection();
                    // 将选定的要素集合转换为字典形式
                    var selectedList = selectedSet.ToDictionary();

                    // 遍历每个选定图层及其关联的对象 ID
                    foreach (var layer in selectedList)
                    {
                        // 获取图层和关联的对象 ID
                        FeatureLayer featureLayer = layer.Key as FeatureLayer;
                        if (!lys.Contains(featureLayer.Name))
                        {
                            lys.Add(featureLayer.Name);
                        }
                    }
                });
                /// 加入combox
                // 获取当前地图
                Map map = MapView.Active.Map;

                // 定义一个空包
                List<ComboBoxContent> flc = new List<ComboBoxContent>();
                // 图层和表的图标
                string imagePath = "/CCTool;component/Data/Icons/layer.png";

                // 将图层加入combox
                foreach (var layer in lys)
                {
                    // 获取要素图层
                    FeatureLayer featureLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name == layer).FirstOrDefault();
                    flc.Add(new ComboBoxContent() { Path = imagePath, Name = featureLayer.Name });
                }

                // 应用
                comboBox.ItemsSource = flc;
                // 设置一个默认项
                if (lys.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }

        // 将当前地图的所有要素图层和独立表加入到Combox中
        public static void AddFeatureLayerAndTableToCombox(ComboBox comboBox)
        {
            // 添加所有要素图层
            AddFeatureLayersToCombox(comboBox);
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> standaloneTables = map.AllStandaloneTables();
            foreach (var standaloneTable in standaloneTables)
            {
                // combox框中添加所有独立表
                comboBox.Items.Add(standaloneTable);
            }
        }

        // 将当前地图的所有要素图层和独立表加入到Combox中
        public static void AddFeatureLayerAndTableToComboxPlus(ComboBox comboBox)
        {
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> featureLayers = map.AllFeatureLayers();
            // 获取所有独立表
            List<string> standaloneTables = map.AllStandaloneTables();
            // 定义一个空包
            List<ComboBoxContent> flc = new List<ComboBoxContent>();

            // 图层和表的图标
            string imagePath = "/CCTool;component/Data/Icons/layer.png";
            string imagePath2 = "/CCTool;component/Data/Icons/table.png";

            // 把图层和表加到combox中
            foreach (string featureLayer in featureLayers)
            {
                flc.Add(new ComboBoxContent() { Path = imagePath, Name = featureLayer });
            }
            foreach (var standaloneTable in standaloneTables)
            {
                flc.Add(new ComboBoxContent() { Path = imagePath2, Name = standaloneTable });
            }
            // 应用
            comboBox.ItemsSource = flc;
        }

        // 将当前地图的所有独立表加入到Combox中
        public static void AddTableToComboxPlus(ComboBox comboBox)
        {
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> standaloneTables = map.AllStandaloneTables();
            foreach (var standaloneTable in standaloneTables)
            {
                // combox框中添加所有独立表
                comboBox.Items.Add(standaloneTable);
            }
        }

        // 将所有布局加入到ListBox中【带复选框】
        public static async void AddLayoutsToListbox(ListBox listBox)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取LayoutProjectItem
            var layoutProjectItem = Project.Current.GetItems<LayoutProjectItem>();

            foreach (LayoutProjectItem layoutItem in layoutProjectItem)
            {
                Layout layout = await QueuedTask.Run(() =>
                {
                    // 获取Layout
                    return layoutItem.GetLayout();
                });

                // 添加CheckBox
                CheckBox cb = new()
                {
                    Content = layout.Name,
                    IsChecked = false
                };
                listBox.Items.Add(cb);
            }
        }

        // 获取ListBox中所有选中的CheckBox【string格式】
        public static List<string> GetStringFromListbox(ListBox listBox)
        {
            // 获取要素
            var items = listBox.Items;
            // 获取所有选中的要素名
            List<string> listName = new List<string>();
            foreach (CheckBox item in items)
            {
                if (item.IsChecked == true)
                {
                    listName.Add(item.Content.ToString());
                }
            }
            // 返回
            return listName;
        }

        // 将所有要素图层加入到ListBox中【带复选框】
        public static void AddFeatureLayersToListbox(ListBox listBox, List<string> excludeItems = null)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> featureLayers = map.AllFeatureLayers();

            foreach (var featureLayer in featureLayers)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(featureLayer))
                    {
                        CheckBox cb = new()
                        {
                            Content = featureLayer,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = featureLayer,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
        }

        // 将所有可见图层加入到ListBox中【带复选框】
        public static void AddCanseeLayersToListbox(ListBox listBox, List<string> excludeItems = null)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> featureLayers = map.CanseeFeatureLayers();

            foreach (var featureLayer in featureLayers)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(featureLayer))
                    {
                        CheckBox cb = new()
                        {
                            Content = featureLayer,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = featureLayer,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
        }

        // 将所有独立表加入到ListBox中【带复选框】
        public static void AddStandaloneTablesToListbox(ListBox listBox, List<string> excludeItems = null)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有独立表
            List<string> standaloneTables = map.AllStandaloneTables();

            foreach (var standaloneTable in standaloneTables)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(standaloneTable))
                    {
                        CheckBox cb = new()
                        {
                            Content = standaloneTable,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = standaloneTable,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
        }

        // 将所有图层和独立表加入到ListBox中【带复选框】
        public static void AddFeatureLayersAndTablesToListbox(ListBox listBox, List<string> excludeItems = null)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层和独立表
            List<string> featureLayers = map.AllFeatureLayers();
            List<string> standaloneTables = map.AllStandaloneTables();

            // 加入图层
            foreach (var featureLayer in featureLayers)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(featureLayer))
                    {
                        CheckBox cb = new()
                        {
                            Content = featureLayer,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = featureLayer,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }


            // 加入独立表
            foreach (var standaloneTable in standaloneTables)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(standaloneTable))
                    {
                        CheckBox cb = new()
                        {
                            Content = standaloneTable,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = standaloneTable,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
        }

        // 全选ListBox部件
        public static void SelectAllItems(ListBox listBox)
        {
            if (listBox.Items.Count == 0)
            {
                MessageBox.Show("列表内没有要素！");
                return;
            }

            var list_field = listBox.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = true;
            }
        }

        // 取消全选ListBox部件
        public static void UnSelectAllItems(ListBox listBox)
        {
            if (listBox.Items.Count == 0)
            {
                MessageBox.Show("列表内没有要素！");
                return;
            }

            var list_field = listBox.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = false;
            }
        }

        // 清空Combox列表
        public static void ClearComboxPlus(ComboBox comboBox)
        {
            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            comboBox.ItemsSource = flc;
        }

        // 将图层字段加入到Combox列表中
        public static async void AddFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName);
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段加入到Combox列表中
        public static async void AddFieldsToComboxPlus(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            var fields = await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                return GisTool.GetFieldsFromTarget(LayerName);
            });

            if (fields is null)
            {
                return;
            }

            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            // 图层图片
            string stringImagePath = "/CCTool;component/Data/Icons/string.png";
            string floatImagePath = "/CCTool;component/Data/Icons/float.png";
            string intgerImagePath = "/CCTool;component/Data/Icons/intger.png";

            // 加到combox中
            foreach (Field field in fields)
            {
                if (field.FieldType == FieldType.String) 
                {
                    flc.Add(new ComboBoxContent() { Path = stringImagePath, Name = field.Name });
                }
                else if (field.FieldType == FieldType.Integer || field.FieldType == FieldType.SmallInteger)
                {
                    flc.Add(new ComboBoxContent() { Path = intgerImagePath, Name = field.Name });
                }
                else if (field.FieldType == FieldType.Single || field.FieldType == FieldType.Double)
                {
                    flc.Add(new ComboBoxContent() { Path = floatImagePath, Name = field.Name });
                }
            }

            comboBox.ItemsSource = flc;
        }

        // 将图层字段【TEXT】加入到Combox列表中
        public static async void AddTextFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                List<Field> fields = GisTool.GetFieldsFromTarget(LayerName, "text");
                if (fields is null)
                {
                    return;
                }
                foreach (Field field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 初始化图层字段加入到Combox列表中
        public static void InitFieldToComboxPlus(ComboBox comboBox, string fieldName, string fieldType)
        {

            List<ComboBoxContent> flc = new List<ComboBoxContent>();

            // 图层图片
            string stringImagePath = "/CCTool;component/Data/Icons/string.png";
            string floatImagePath = "/CCTool;component/Data/Icons/float.png";
            string intgerImagePath = "/CCTool;component/Data/Icons/intger.png";

            // 加到combox中
            if (fieldType == "string")
            {
                flc.Add(new ComboBoxContent() { Path = stringImagePath, Name = fieldName });
            }
            else if (fieldType == "int")
            {
                flc.Add(new ComboBoxContent() { Path = intgerImagePath, Name = fieldName });
            }
            else if (fieldType == "float")
            {
                flc.Add(new ComboBoxContent() { Path = floatImagePath, Name = fieldName });
            }

            comboBox.ItemsSource = flc;

            comboBox.SelectedIndex = 0;
        }

        // 将Excel字段加入到Combox列表中
        public static void AddExcelFieldsToComboxPlus(string excelPath, ComboBox comboBox)
        {
            // 判断输入路径是否为空
            if (excelPath == "")
            {
                MessageBox.Show("输入的Excel路径是否为空");
                return;
            }

            List<string> fields = OfficeTool.GetColListFromExcel(excelPath);

            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            // 图层图片
            string imagePath = "/CCTool;component/Data/Icons/string.png";

            // 加到combox中
            foreach (var field in fields)
            {
                flc.Add(new ComboBoxContent() { Path = imagePath, Name = field });
            }

            comboBox.ItemsSource = flc;
        }


        // 将图层字段【TEXT】加入到Combox列表中
        public static async void AddTextFieldsToComboxPlus(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            var fields = await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                return GisTool.GetFieldsFromTarget(LayerName, "text");
            });

            if (fields is null)
            {
                return;
            }

            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            // 图层图片
            string imagePath = "/CCTool;component/Data/Icons/string.png";

            // 加到combox中
            foreach (var field in fields)
            {
                flc.Add(new ComboBoxContent() { Path = imagePath, Name = field.Name });
            }

            comboBox.ItemsSource = flc;
        }

        // 将图层字段【Float】加入到Combox列表中
        public static async void AddFloatFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName, "float");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Float】加入到Combox列表中
        public static async void AddFloatFieldsToComboxPlus(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            var fields = await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                return GisTool.GetFieldsFromTarget(LayerName, "float");
            });

            if (fields is null)
            {
                return;
            }

            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            // 图层图片
            string imagePath = "/CCTool;component/Data/Icons/float.png";

            // 加到combox中
            foreach (var field in fields)
            {
                flc.Add(new ComboBoxContent() { Path = imagePath, Name = field.Name });
            }

            comboBox.ItemsSource = flc;
        }


        // 将图层字段【Float】加入到Combox列表中
        public static async void AddAllFloatFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName, "float_all");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Float】加入到Combox列表中
        public static async void AddAllFloatFieldsToComboxPlus(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            var fields = await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                return GisTool.GetFieldsFromTarget(LayerName, "float_all");
            });

            if (fields is null)
            {
                return;
            }

            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            // 图层图片
            string floatImagePath = "/CCTool;component/Data/Icons/float.png";
            string floatBImagePath = "/CCTool;component/Data/Icons/floatB.png";

            // 加到combox中
            foreach (var field in fields)
            {
                if (field.IsEditable)
                {
                    flc.Add(new ComboBoxContent() { Path = floatImagePath, Name = field.Name });
                }
                else
                {
                    flc.Add(new ComboBoxContent() { Path = floatBImagePath, Name = field.Name });
                }
            }

            comboBox.ItemsSource = flc;

        }

        // 将图层字段【Int】加入到Combox列表中
        public static async void AddIntFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName, "int");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }


        // 将图层字段加入到Combox列表中
        public static async void AddFieldsToCombox(FeatureLayer layer, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (layer == null)
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(layer);
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【TEXT】加入到Combox列表中
        public static async void AddTextFieldsToCombox(FeatureLayer layer, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (layer == null)
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                List<Field> fields = GisTool.GetFieldsFromTarget(layer, "text");
                if (fields is null)
                {
                    return;
                }
                foreach (Field field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【TEXT】加入到Combox列表中
        public static async void AddTextFieldsToComboxPlus(FeatureLayer layer, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (layer == null)
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            var fields = await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                return GisTool.GetFieldsFromTarget(layer, "text");
            });

            if (fields is null)
            {
                return;
            }

            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            // 图层图片
            string imagePath = "/CCTool;component/Data/Icons/string.png";

            // 加到combox中
            foreach (var field in fields)
            {
                flc.Add(new ComboBoxContent() { Path = imagePath, Name = field.Name });
            }

            comboBox.ItemsSource = flc;
        }

        // 将图层字段【Float】加入到Combox列表中
        public static async void AddFloatFieldsToCombox(FeatureLayer layer, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (layer == null)
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(layer, "float");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Float】加入到Combox列表中
        public static async void AddAllFloatFieldsToCombox(FeatureLayer layer, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (layer == null)
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(layer, "float_all");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Int】加入到Combox列表中
        public static async void AddIntFieldsToCombox(FeatureLayer layer, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (layer == null)
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(layer, "int");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Int】加入到Combox列表中
        public static async void AddIntFieldsToComboxPlus(FeatureLayer layer, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (layer == null)
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            var fields = await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                return GisTool.GetFieldsFromTarget(layer, "int");
            });

            if (fields is null)
            {
                return;
            }

            List<ComboBoxContent> flc = new List<ComboBoxContent>();
            // 图层图片
            string imagePath = "/CCTool;component/Data/Icons/intger.png";

            // 加到combox中
            foreach (var field in fields)
            {
                flc.Add(new ComboBoxContent() { Path = imagePath, Name = field.Name });
            }

            comboBox.ItemsSource = flc;
        }

        // 将当前地图的图层组或单个图层加入到Combox中
        public static void AddLayerGroupToCombox(ComboBox comboBox)
        {
            // 清空combox_field
            comboBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            var lys = map.GetLayersAsFlattenedList().OfType<Layer>().ToList();
            foreach (var ly in lys)
            {
                if (ly.Parent == map)
                {
                    // 向combox框中添加图层
                    if (ly is GroupLayer)
                    {
                        comboBox.Items.Add(@"【组】" + ly.Name);
                    }
                    else
                    {
                        comboBox.Items.Add(ly.Name);
                    }
                }
            }
        }

        // 将布局视图加入到Combox中
        public static void AddAllLayoutToCombox(ComboBox comboBox)
        {
            // 清空combox_field
            comboBox.Items.Clear();
            // 获取当前工程中的布局
            var layouts = Project.Current.GetItems<LayoutProjectItem>().ToList();
            foreach (var layout in layouts)
            {
                // combox框中添加所有要素图层
                comboBox.Items.Add(layout.Name);
            }
        }

        // 将ListBox中选中的要素加入到List中
        public static List<string> GetCheckboxStringFromListBox(ListBox listBox)
        {
            List<string> result = new List<string>();
            foreach (CheckBox checkbox in listBox.Items)
            {
                if (checkbox.IsChecked == true)
                {
                    result.Add(checkbox.Content.ToString());
                }
            }
            return result;
        }

        // 将要素的字段加入到ListBox中
        public static async void AddFieldsToListBox(ListBox listBox, object fcPath, bool isHaveCheck = true)
        {
            // 清除listbox
            listBox.Items.Clear();

            // 获取所有非Geo字段
            var fields = await QueuedTask.Run(() =>
            {
                return GisTool.GetFieldsFromTarget(fcPath);
            });

            foreach (Field field in fields)
            {
                if (field.FieldType == FieldType.Geometry)
                {
                    continue;
                }
                else
                {
                    if (isHaveCheck == true)
                    {
                        // 将filed做成checkbox放入列表中
                        CheckBox cb = new CheckBox();
                        cb.Content = field.Name;
                        cb.IsChecked = false;
                        listBox.Items.Add(cb);
                    }
                    else
                    {
                        listBox.Items.Add(field.Name);
                    }
                }
            }
        }

        // 将指定文字插入富文本中光标所处的位置
        public static void AddTextToRichTextBox(RichTextBox textBox, string text)
        {
            if (textBox is null)
            {
                return;
            }

            textBox.CaretPosition.InsertTextInRun(text);

            if (textBox.CaretPosition.LogicalDirection == LogicalDirection.Backward)
            {
                TextPointer tp = textBox.CaretPosition.GetPositionAtOffset(text.Length, LogicalDirection.Forward);
                if (tp != null)
                {
                    textBox.CaretPosition = tp;
                }
            }
        }

        // 将要素的字段加入到ListBox中【文本型】
        public static async void AddTextFieldsToListBox(ListBox listBox, string fcPath)
        {
            // 清除listbox
            listBox.Items.Clear();

            // 获取所有非Geo字段
            var fields = await QueuedTask.Run(() =>
            {
                return GisTool.GetFieldsFromTarget(fcPath, "text");
            });

            foreach (Field field in fields)
            {
                // 将filed做成checkbox放入列表中
                CheckBox cb = new CheckBox();
                cb.Content = field.Name;
                cb.IsChecked = false;
                listBox.Items.Add(cb);
            }
        }

        // 将要素的字段加入到ListBox中【可编辑浮点型】
        public static async void AddFloatFieldsToListBox(ListBox listBox, string fcPath)
        {
            // 清除listbox
            listBox.Items.Clear();

            // 获取所有非Geo字段
            var fields = await QueuedTask.Run(() =>
            {
                return GisTool.GetFieldsFromTarget(fcPath, "float");
            });

            foreach (Field field in fields)
            {
                // 将filed做成checkbox放入列表中
                CheckBox cb = new CheckBox();
                cb.Content = field.Name;
                cb.IsChecked = false;
                listBox.Items.Add(cb);
            }
        }

        // 将要素的字段加入到ListBox中【所有的浮点型】
        public static async void AddAllFloatFieldsToListBox(ListBox listBox, string fcPath)
        {
            // 清除listbox
            listBox.Items.Clear();

            // 获取所有非Geo字段
            var fields = await QueuedTask.Run(() =>
            {
                return GisTool.GetFieldsFromTarget(fcPath, "float_all");
            });

            foreach (Field field in fields)
            {
                // 将filed做成checkbox放入列表中
                CheckBox cb = new CheckBox();
                cb.Content = field.Name;
                cb.IsChecked = false;
                listBox.Items.Add(cb);
            }
        }


        // 打开Excel文件
        public static string OpenDialogExcel()
        {
            OpenFileDialog openDlg = new OpenFileDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Multiselect = false,             //是否可以多选
                Filter = "Excel文件|*.xlsx;*.xls",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 打开MDB文件
        public static string OpenDialogMDB()
        {
            OpenFileDialog openDlg = new OpenFileDialog()
            {
                Title = "选择一个mdb数据库",        //打开对话框标题
                Multiselect = false,             //是否可以多选
                Filter = "MDB文件|*.mdb",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 打开图片文件
        public static string OpenDialogPicture()
        {
            OpenFileDialog openDlg = new OpenFileDialog()
            {
                Title = "选择一个图片",        //打开对话框标题
                Multiselect = false,             //是否可以多选
                Filter = "图片文件|*.jpg;*.png",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 打开文件夹
        public static string OpenDialogFolder()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个文件夹",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.Folders,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 打开FeatureClass文件
        public static string OpenDialogFeatureClass()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.FeatureClasses_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 打开GDB数据库文件
        public static string OpenDialogGDB()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个gdb数据库",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.Geodatabases,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 打开Table文件
        public static string OpenTable()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个表",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.Tables_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 保存Excel文件
        public static string SaveDialogExcel()
        {
            SaveFileDialog openDlg = new SaveFileDialog()
            {
                Title = "选择一个Excel文件",        //打开对话框标题
                Filter = "Excel文件|*.xlsx",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 保存CAD文件
        public static string SaveDialogCAD()
        {
            SaveFileDialog openDlg = new SaveFileDialog()
            {
                Title = "选择一个CAD文件",        //打开对话框标题
                Filter = "CAD文件|*.dwg",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 保存KMZ文件
        public static string SaveDialogKMZ()
        {
            SaveFileDialog openDlg = new SaveFileDialog()
            {
                Title = "选择一个KMZ文件",        //打开对话框标题
                Filter = "KMZ文件|*.kmz",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 保存图片文件
        public static string SaveDialogPicture()
        {
            SaveFileDialog openDlg = new SaveFileDialog()
            {
                Title = "选择一个图片",        //打开对话框标题
                Filter = "图片文件|*.jpg;*.png",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 保存FeatureClass文件
        public static string SaveDialogFeatureClass()
        {
            SaveItemDialog saveDlg = new SaveItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Filter = ItemFilters.FeatureClasses_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = saveDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = saveDlg.FilePath;
            // 返回路径
            return item;
        }

        // 保存GDB文件
        public static string SaveDialogGDB()
        {
            SaveItemDialog saveDlg = new SaveItemDialog()
            {
                Title = "选择一个gdb数据库",        //打开对话框标题
                Filter = ItemFilters.Geodatabases,       //类型筛选
            };
            // 打开对话框
            bool? ok = saveDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = saveDlg.FilePath;
            // 返回路径
            return item + ".gdb";
        }

        // 保存独立表文件
        public static string SaveDialogTable()
        {
            SaveItemDialog saveDlg = new SaveItemDialog()
            {
                Title = "选择一个表",        //打开对话框标题
                Filter = ItemFilters.Tables_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = saveDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = saveDlg.FilePath;
            // 返回路径
            return item;
        }

        // 打开自定义的进度框
        public static ProcessWindow OpenProcessWindow(ProcessWindow processWindow, string tool_name)
        {
            processWindow = new ProcessWindow();
            processWindow.Owner = FrameworkApplication.Current.MainWindow;
            processWindow.Closed += (o, e) => { processWindow = null; };
            processWindow.Title = tool_name;
            processWindow.Show();
            return processWindow;
        }

        // 打开自定义的XY信息框 
        public static XYInfoWindow OpenXYInfoWindow(XYInfoWindow xyInfoWindow)
        {
            xyInfoWindow = new XYInfoWindow();
            xyInfoWindow.Owner = FrameworkApplication.Current.MainWindow;
            xyInfoWindow.Closed += (o, e) => { xyInfoWindow = null; };
            xyInfoWindow.Show();
            return xyInfoWindow;
        }

        // 网页链接
        public static void Link2Web(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }

    // 图层
    public class ComboBoxContent
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }
}
