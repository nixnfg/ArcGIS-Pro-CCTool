using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Data;
using CCTool.Scripts.Manager;
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
using Table = ArcGIS.Core.Data.Table;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.ToolManagers;
using ArcGIS.Desktop.Internal.Mapping.Symbology;

namespace CCTool.Scripts.MiniTool.GetInfo
{
    /// <summary>
    /// Interaction logic for InfoDifFieldValue.xaml
    /// </summary>
    public partial class InfoDifFieldValue : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public InfoDifFieldValue()
        {
            InitializeComponent();
        }

        private async void combox_field_DropClosed(object sender, EventArgs e)
        {
            try
            {
                // 清空文本
                tb_message.Document.Blocks.Clear();

                string lyName = combox_layer.ComboxText();
                string fieldName = combox_field.ComboxText();

                Dictionary<string, List<string>> difFieldValues = new Dictionary<string, List<string>>();

                // 获取字段值信息
                Dictionary<string, long> fieldValues = await QueuedTask.Run(() =>
                {
                    return lyName.GetFieldValuesDic(fieldName);
                });

                await QueuedTask.Run(() =>
                {
                    // 获取ID字段，用于标记
                    string idField = GisTool.GetIDFieldNameFromTarget(lyName);

                    // 找出重复值
                    foreach (var fieldValue in fieldValues)
                    {
                        if (fieldValue.Value > 1)    // 如果个数超过1，表示是重复值
                        {
                            // 相同值集合
                            List<string> oids = new List<string>();

                            // 获取Table
                            Table table = lyName.TargetTable();
                            // 逐行找出错误
                            using RowCursor rowCursor = table.Search();
                            while (rowCursor.MoveNext())
                            {
                                using ArcGIS.Core.Data.Row row = rowCursor.Current;
                                // 获取value
                                var va = row[fieldName];
                                if (va != null)
                                {
                                    string result = va.ToString();
                                    // 如果不在列表中，就加入
                                    if (result == fieldValue.Key)
                                    {
                                        oids.Add(row[idField].ToString());     // 加入OID
                                    }
                                }
                            }

                            // 加入集合
                            difFieldValues.Add(fieldValue.Key, oids);
                        }
                    }
                });

                // 加入文本
                tb_message.AddMessage($"获取字段的所有重复值，共计：{difFieldValues.Count}组\r", Brushes.Green);

                int index = 1;
                foreach (var fieldValue in difFieldValues)
                {
                    tb_message.AddMessage($"({index})有{fieldValues[fieldValue.Key]}个重复字段值【{fieldValue.Key}】，重复ID如下：\r", Brushes.Black);

                    foreach (var item in fieldValue.Value)
                    {
                        tb_message.AddMessage($"{item},", Brushes.BlueViolet);
                    }
                    tb_message.AddMessage($"\r", Brushes.BlueViolet);
                    index++;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            string lyName = combox_layer.ComboxText();
            UITool.AddFieldsToComboxPlus(lyName, combox_field);
        }

        private void combox_layer_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToComboxPlus(combox_layer);
        }
    }
}
