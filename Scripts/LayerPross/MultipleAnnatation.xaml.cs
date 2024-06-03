using ArcGIS.Core.CIM;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Spreadsheet;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for MultipleAnnatation.xaml
    /// </summary>
    public partial class MultipleAnnatation : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MultipleAnnatation()
        {
            InitializeComponent();
        }

        // 定义一个选定的文本框 
        RichTextBox activeRB = null;

        private void listbox_field_Load(object sender, RoutedEventArgs e)
        {
            FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            UITool.AddFieldsToListBox(listbox_field, featureLayer, false);
        }

        private void listbox_field_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // 如果没有双击item，不做任何事
                if (listbox_field.SelectedItems.Count == 0)
                {
                    return;
                }
                // 获取item
                string text = "[" + listbox_field.SelectedItems[0].ToString() + "]";
                // 写入文本框
                UITool.AddTextToRichTextBox(activeRB, text);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void text_top_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_top;
        }

        private void text_left_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_left;
        }

        private void text_right_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_right;
        }

        private void text_bottom_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_bottom;
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            // 收集输出框
            List<RichTextBox> textboxList = new List<RichTextBox>()
            {
                text_bottom, text_left, text_right,text_top
            };
            // 清除
            foreach (RichTextBox textbox in textboxList)
            {
                textbox.Document.Blocks.Clear();
            }
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string textTop = UITool.GetRichText(text_top);
                string textLeft = UITool.GetRichText(text_left);
                string textRight = UITool.GetRichText(text_right);
                string textBottom = UITool.GetRichText(text_bottom);
                // 如果是空文本框，更新一下
                textTop = textTop == "" ? "\"\"" : textTop;
                textLeft = textLeft == "" ? "\"\"" : textLeft;
                textRight = textRight == "" ? "\"\"" : textRight;
                textBottom = textBottom == "" ? "\"\"" : textBottom;
                // 显示参数
                int text_line = int.Parse(textLine.Text.ToString());
                int text_lineLength = int.Parse(textLineLength.Text.ToString());
                int text_topSpace = int.Parse(textTopBotton.Text.ToString());
                int text_bottomSpace = -text_topSpace;
                // 获取输入数据中的所有字段参数
                string textMerge = (textTop + textLeft + textRight + textBottom);
                string par = textMerge.GetStringInside("[]");
                
                await QueuedTask.Run(() =>
                {
                    // 获取图层
                    FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                    // 获取图层定义
                    CIMFeatureLayer lyrDefn = featureLayer.GetDefinition() as CIMFeatureLayer;

                    // 获取标注
                    var listLabelClasses = lyrDefn.LabelClasses.ToList();
                    CIMLabelClass theLabelClass = listLabelClasses.FirstOrDefault();

                    // 设置标注语言为Arcade
                    theLabelClass.ExpressionEngine = LabelExpressionEngine.Python;
                    // 设置标注内容
                    string code = $"import re\r\nimport math\r\nxs=0.5722\r\n\r\ndef ss(a):\r\n  va_up = re.findall(u'[\\u4e00-\\u9fa5]+',a)\r\n  result_up = ''\r\n  if len(va_up) > 0:\r\n      for i in range(0, len(va_up)):\r\n          result_up += va_up[i]\r\n  len_up=len(result_up)\r\n  len_up_other=len(a)-len_up\r\n  return len_up+len_up_other*xs\r\n\r\ndef FindLabel({par}):\r\n  top={textTop}\r\n  left={textLeft}\r\n  right={textRight}\r\n  bottom={textBottom}\r\n  s_up=math.ceil(ss(top))\r\n  s_down=math.ceil(ss(bottom))\r\n  s = max(s_up,s_down)*{text_lineLength}\r\n\r\n  p=\"<PART position='middle'>\"+\"<CHR spacing = '{text_line}'>\"+\"-\"*s+\"</CHR>\"+\"</PART>\"+\"<PART position='top'>\"+top+\"</PART>\" + \"<PART position='left'>\"+left+\"</PART>\"+\"<PART position='right'>\"+right+\"</PART>\"+\"<PART position='bottom'>\"+bottom+\"</PART>\"\r\n  return p";

                    theLabelClass.Expression = code;

                    // 创建一个标注符号CIMTextSymbol
                    CIMTextSymbol textSymbol = theLabelClass.TextSymbol.Symbol as CIMTextSymbol;
                    // 设置文本前后缩进
                    textSymbol.IndentAfter = 0;
                    textSymbol.IndentBefore = 0;

                    // 创建一个CIMCompositeCallout
                    CIMCompositeCallout ccs = new CIMCompositeCallout();

                    // 创建一个面符号CIMPolygonSymbol
                    CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.CreateRGBColor(170, 241, 247, 0), SimpleFillStyle.Solid);
                    // 设置边框线颜色
                    polySymbol.SetOutlineColor(ColorFactory.Instance.CreateRGBColor(0, 0, 0, 0));

                    // 应用面符号
                    ccs.BackgroundSymbol = polySymbol;

                    // 边距
                    ccs.Margin = new CIMTextMargin()
                    {
                        Left = 0,
                        Top = 0,
                        Right = 0,
                        Bottom = 0
                    };

                    // 设置各部分属性
                    CIMCompositeTextPartPosition top = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Center,
                        YOffset = text_topSpace,
                    };
                    CIMCompositeTextPartPosition left = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Center,
                        HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Right,
                    };
                    CIMCompositeTextPartPosition middle = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Center,
                    };
                    CIMCompositeTextPartPosition right = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Center,
                        HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Left,
                    };
                    CIMCompositeTextPartPosition bottom = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Center,
                        YOffset = text_bottomSpace,
                    };

                    ccs.Top = top;
                    ccs.Left = left;
                    ccs.Middle = middle;
                    ccs.Right = right;
                    ccs.Bottom = bottom;

                    // 应用标注符号
                    textSymbol.Callout = ccs;

                    // 应用标注设置
                    lyrDefn.LabelClasses[0] = theLabelClass; // 假设只有一个标注类别

                    // 应用标注
                    featureLayer.SetDefinition(lyrDefn);

                    // 打开标注
                    if (!featureLayer.IsLabelVisible) { featureLayer.SetLabelVisibility(true); }
                });

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/xcc34452366/article/details/135696466?spm=1001.2014.3001.5501";
            UITool.Link2Web(url);
        }
    }
}
