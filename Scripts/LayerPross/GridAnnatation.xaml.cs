using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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

namespace CCTool.Scripts.LayerPross
{
    /// <summary>
    /// Interaction logic for GridAnnatation.xaml
    /// </summary>
    public partial class GridAnnatation : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public GridAnnatation()
        {
            InitializeComponent();

        }
        // 定义一个选定的文本框 
        RichTextBox activeRB = null;

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string textTop = UITool.GetRichText(text_top).Replace("{", "${$feature.");
                string textTopLeft = UITool.GetRichText(text_topleft).Replace("{", "${$feature.");
                string textTopRight = UITool.GetRichText(text_topright).Replace("{", "${$feature.");
                string textLeft = UITool.GetRichText(text_left).Replace("{", "${$feature.");
                string textMiddle = UITool.GetRichText(text_middle).Replace("{", "${$feature.");
                string textRight = UITool.GetRichText(text_right).Replace("{", "${$feature.");
                string textBottom = UITool.GetRichText(text_bottom).Replace("{", "${$feature.");
                string textBottomLeft = UITool.GetRichText(text_bottomleft).Replace("{", "${$feature.");
                string textBottomRight = UITool.GetRichText(text_bottomright).Replace("{", "${$feature.");

                int fontSize = int.Parse(txt_fontsize.Text);
                int indent = int.Parse(txt_indent.Text);
                int radius = int.Parse(txt_radius.Text);

                if (textMiddle == "")
                {
                    MessageBox.Show("居中标注不能为空！！！");
                }

                //Close();

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
                    theLabelClass.ExpressionEngine = LabelExpressionEngine.Arcade;
                    // 设置标注内容
                    string code = $"`<PART position=\"top\">{textTop}</PART>" +
                    $"<PART position=\"topleft\">{textTopLeft}</PART>" +
                    $"<PART position=\"topright\">{textTopRight}</PART>" +
                    $"<PART position=\"left\">{textLeft}</PART>" +
                    $"<PART position=\"middle\">{textMiddle}</PART>" +
                    $"<PART position=\"right\">{textRight}</PART>" +
                    $"<PART position=\"bottomleft\">{textBottomLeft}</PART>" +
                    $"<PART position=\"bottomright\">{textBottomRight}</PART>" +
                    $"<PART position=\"bottom\">{textBottom}</PART>`";

                    theLabelClass.Expression = code;

                    // 创建一个标注符号CIMTextSymbol
                    CIMTextSymbol textSymbol = theLabelClass.TextSymbol.Symbol as CIMTextSymbol;
                    // 设置文本前后缩进
                    textSymbol.IndentAfter = indent;
                    textSymbol.IndentBefore = indent;
                    // 设置字体大小
                    textSymbol.SetSize(fontSize);

                    // 创建一个CIMCompositeCallout
                    CIMCompositeCallout ccs = new CIMCompositeCallout();

                    // 创建一个面符号CIMPolygonSymbol
                    CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.CreateRGBColor(170, 241, 247, 50), SimpleFillStyle.Solid);
                    // 设置边框线颜色
                    polySymbol.SetOutlineColor(ColorFactory.Instance.CreateRGBColor(0, 0, 255, 80));

                    // 应用面符号
                    ccs.BackgroundSymbol = polySymbol;
                    // 拐角半径
                    ccs.CornerRadius = radius;


                    // 边距
                    ccs.Margin = new CIMTextMargin()
                    {
                        Left = 2,
                        Top = 2,
                        Right = 2,
                        Bottom = 2
                    };

                    // 设置各部分属性
                    CIMCompositeTextPartPosition top = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Bottom,
                        HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Center,
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
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Top,
                    };
                    CIMCompositeTextPartPosition bottomleft = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Top,
                        HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Right,
                    };
                    CIMCompositeTextPartPosition bottomright = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Top,
                        HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Left,
                    };
                    CIMCompositeTextPartPosition topleft = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Bottom,
                        HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Right,
                    };
                    CIMCompositeTextPartPosition topright = new CIMCompositeTextPartPosition()
                    {
                        IsPartWithinCalloutBox = true,
                        VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Bottom,
                        HorizontalAlignment = ArcGIS.Core.CIM.HorizontalAlignment.Left,
                    };

                    ccs.Top = top;
                    ccs.Left = left;
                    ccs.Middle = middle;
                    ccs.Right = right;
                    ccs.Bottom = bottom;
                    ccs.TopLeft = topleft;
                    ccs.TopRight = topright;
                    ccs.BottomLeft= bottomleft;
                    ccs.BottomRight= bottomright;

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


        private void listbox_field_Load(object sender, RoutedEventArgs e)
        {
            FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            UITool.AddFieldsToListBox(listbox_field, featureLayer, false);
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {

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
                string text = "{" + listbox_field.SelectedItems[0].ToString() + "}";
                // 写入文本框
                UITool.AddTextToRichTextBox(activeRB, text);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void text_topleft_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_topleft;
        }

        private void text_top_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_top;
        }

        private void text_topright_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_topright;
        }

        private void text_left_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_left;
        }

        private void text_middle_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_middle;
        }

        private void text_right_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_right;
        }

        private void text_bottomleft_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_bottomleft;
        }

        private void text_bottom_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_bottom;
        }

        private void text_bottomright_GotFocus(object sender, RoutedEventArgs e)
        {
            activeRB = text_bottomright;
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            // 收集输出框
            List<RichTextBox> textboxList = new List<RichTextBox>()
            {
                text_bottom, text_left, text_right,text_bottomleft,text_bottomright,text_middle,text_top,text_topleft,text_topright
            };
            // 清除
            foreach (RichTextBox textbox in textboxList)
            {
                textbox.Document.Blocks.Clear();
            }
        }
    }
}
