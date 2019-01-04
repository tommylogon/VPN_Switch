using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Forms;
using System.Drawing;
using Hardcodet.Wpf.TaskbarNotification;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using DotRas;
using System.Net.NetworkInformation;
using MessageBox = System.Windows.MessageBox;
using MenuItem = System.Windows.Controls.MenuItem;

namespace VPN_Switch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();

            lbl_CurrentIP.Content = "Current ip is: " + GetLocalIPAddress();

            ReadPhonebook();
        }

        private bool CheckConnection()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface Interface in interfaces)
                {
                    if (Interface.OperationalStatus == OperationalStatus.Up)
                    {
                        if ((Interface.NetworkInterfaceType == NetworkInterfaceType.Ppp) && (Interface.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                        {
                            IPv4InterfaceStatistics statistics = Interface.GetIPv4Statistics();
                            MessageBox.Show(Interface.Name + " " + Interface.NetworkInterfaceType.ToString() + " " + Interface.Description);
                            return true;
                        }
                    }
                }
                //MessageBox.Show("VPN Connection is lost!");
            }
            return false;
        }

        private void ShowWindow_Clicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void OpenConnection()
        {
            Process.Start(@"rasdial.exe", "hhm-vpn " + txt_username.Text + " " + txt_password.Text);
            lbl_CurrentIP.Content = "Current ip is: " + GetLocalIPAddress();
        }

        private void CloseConnection()
        {
            Process.Start(@"rasdial.exe", "hhm-vpn /d");
        }

        // minimize to system tray when applicaiton is minimized
        //protected override void OnStateChanged(EventArgs e)
        //{
        //    if (WindowState == WindowState.Minimized) this.Hide();

        //    base.OnStateChanged(e);
        //}

        // minimize to system tray when applicaiton is closed
        protected override void OnClosing(CancelEventArgs e)
        {
            // setting cancel to true will cancel the close request
            // so the application is not closed
            e.Cancel = true;

            this.Hide();

            base.OnClosing(e);
        }

        private void VPN_Entry_Clicked(object sender, RoutedEventArgs e)
        {
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            //Check if connected to vpn, if true, disconnect, if false, connect to vpn
            if (CheckConnection())
            {
                CloseConnection();
            }
            else
            {
                OpenConnection();
            }
        }

        private void ReadPhonebook()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                          @"\Microsoft\Network\Connections\Pbk\rasphone.pbk";

            RasPhoneBook pbk = new RasPhoneBook();
            pbk.Open(path);

            foreach (RasEntry entry in pbk.Entries)
            {
                System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem();
                item.Click += Connect_Clicked;
                item.Header = entry.Name;
                TbI.ContextMenu.Items.Add(item);
            }
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void Disconnect_clicked(object sender, RoutedEventArgs e)
        {
            CloseConnection();
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}