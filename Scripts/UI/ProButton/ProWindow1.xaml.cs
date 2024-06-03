using ArcGIS.Desktop.Framework;
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

namespace CCTool.Scripts.UI.ProButton
{
    /// <summary>
    /// Interaction logic for ProWindow1.xaml
    /// </summary>
    public partial class ProWindow1 : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ProWindow1()
        {
            InitializeComponent();

            List<Color> colors = new List<Color>();
            colors.Add(new Color() { ImageName = "/CCTool;component/Data/Icons/filter.png", Name= "浅粉红"});
            colors.Add(new Color() { ImageName = "/CCTool;component/Data/Icons/list.png", Name = "粉红" });
            colors.Add(new Color() { ImageName = "/CCTool;component/Data/Icons/addLayer.png", Name = "深红" });

            listbox01.ItemsSource = colors;
            combox01.ItemsSource = colors;

        }


        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            var commandId = "esri_mapping_ClearSelectionButton";
            var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
            if (iCommand != null)
            {
                if (iCommand.CanExecute(null))
                {
                    iCommand.Execute(null);
                }
            }
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            var commandId = "esri_editing_Attributes_UnselectContextMenuItem";
            var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
            if (iCommand != null)
            {
                if (iCommand.CanExecute(null))
                {
                    iCommand.Execute(null);
                }
            }
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            var commandId = "esri_core_saveProjectButton";
            var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
            if (iCommand != null)
            {
                if (iCommand.CanExecute(null))
                {
                    iCommand.Execute(null);
                }
            }
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            var commandId = "esri_core_editCopyButton";
            var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
            if (iCommand != null)
            {
                if (iCommand.CanExecute(null))
                {
                    iCommand.Execute(null);
                }
            }
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            Color aa =combox01.SelectedItem as Color;
            MessageBox.Show(aa.Name);
        }

        private void tb_Drop(object sender, DragEventArgs e)
        {
            string msg = "Drop";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            }

            MessageBox.Show(msg);
        }
    }


    public class Color
    {
        public string ImageName { get; set; }
        public string Name { get; set; }
    }
}
