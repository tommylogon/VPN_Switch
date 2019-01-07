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
using System.Threading;

namespace VPN_Switch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool isConnected = false;
        private Process rasdial = new Process();
        public NetworkInterface netinterface;

        public MainWindow()
        {
            InitializeComponent();

            rasdial.StartInfo.FileName = "rasdial.exe";
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
                            netinterface = Interface;
                            return true;
                        }
                    }
                }
            }
            netinterface = null;
            return false;
        }

        private void ShowWindow_Clicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void OpenConnection()
        {
            rasdial.StartInfo.Arguments = "hhm-vpn " + txt_username.Text + " " + txt_password.Text;
            rasdial.Start();
            rasdial.WaitForExit();

            if (CheckConnection())
            {
                lbl_ConnectionStatus.Content = "Your are connected to " + netinterface.Name;
                lbl_CurrentIP.Content = "Current ip is: " + GetLocalIPAddress();
            }
            else
            {
                lbl_ConnectionStatus.Content = "You are disconnected";
            }
        }

        private void CloseConnection()
        {
            rasdial.StartInfo.Arguments = "hhm-vpn /d";
            rasdial.Start();
            rasdial.WaitForExit();

            if (!CheckConnection())
            {
                lbl_ConnectionStatus.Content = "You are disconnected";
            }
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
                if (!CheckConnection())
                {
                    MessageBox.Show("Connection Failed,\n Please check Username and/or Password", "Error");
                }
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
                MenuItem item = new MenuItem();
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