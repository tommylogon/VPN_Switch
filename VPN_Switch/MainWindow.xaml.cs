using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using DotRas;
using System.Net.NetworkInformation;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Windows.Media.Imaging;

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
            CheckConnection();
            lbl_CurrentIP.Content = GetLocalIPAddress();

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
                            lbl_ConnectionStatus.Content = "Your are connected to " + netinterface.Name;
                            lbl_CurrentIP.Content = GetLocalIPAddress();
                            return true;
                        }
                    }
                }
            }
            netinterface = null;
            lbl_ConnectionStatus.Content = "You are disconnected";
            lbl_CurrentIP.Content = GetLocalIPAddress();
            return false;
        }

        private void ShowWindow_Clicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void OpenConnection()
        {
            rasdial.StartInfo.Arguments = txt_VPN_Name.Text + " " + txt_username.Text + " " + txt_password.Text;
            rasdial.Start();
            rasdial.WaitForExit();
            CheckConnection();
        }

        private void CloseConnection()
        {
            rasdial.StartInfo.Arguments = txt_VPN_Name.Text + " /d";
            rasdial.Start();
            rasdial.WaitForExit();

            CheckConnection();
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

        private void Change_Tray_Entry_Icon(MenuItem item)
        {
            try
            {
                if (CheckConnection())
                {
                    SetIcon(item, "ok.png");
                }
                else
                {
                    SetIcon(item, "nok.png");
                    //MessageBox.Show("Connection Failed,\n Please check Username and/or Password", "Error");
                }
            }
            catch (Exception ex)
            {
            }
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
            if (sender is MenuItem item)
            {
                Change_Tray_Entry_Icon(item);
            }
        }

        private void SetIcon(MenuItem item, string iconName)
        {
            // Create Image and set its width and height
            Image image = new Image();

            // Create a BitmapSource
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Environment.CurrentDirectory + @"\" + iconName);
            bitmap.EndInit();
            // Set Image.Source
            image.Source = bitmap;
            item.Icon = image;
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
                lib_VPNListBox.Items.Add(item.Header);
            }
        }

        private static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string message = "";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    message += ip.ToString() + "\n";
                }
            }
            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }
            else
            {
                throw new Exception("No network adapters with an IPv4 address in the system!");
            }
        }

        private void Disconnect_clicked(object sender, RoutedEventArgs e)
        {
            CloseConnection();
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void VPN_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                txt_VPN_Name.Text = e.AddedItems[0].ToString();
            }
            catch (Exception ex)
            {
            }
        }
    }
}