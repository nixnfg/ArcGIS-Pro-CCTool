using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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

namespace CCTool.Scripts.MiniTool.MTool
{
    /// <summary>
    /// Interaction logic for GetComputerID.xaml
    /// </summary>
    public partial class GetComputerID : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public GetComputerID()
        {
            InitializeComponent();
            textID.Text = GetIP();
        }

        // 获取本地地址
        public static string GetIP()
        {
            string ipAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(n => n.OperationalStatus == OperationalStatus.Up)
        .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .SelectMany(n => n.GetIPProperties().UnicastAddresses)
        .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .FirstOrDefault()?.Address.ToString();

            return ipAddress;
        }

    }
}
