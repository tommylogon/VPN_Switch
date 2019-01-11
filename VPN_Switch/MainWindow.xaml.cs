using DotRas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using MenuItem = System.Windows.Controls.MenuItem;

namespace VPN_Switch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Process rasdial = new Process();
        public NetworkInterface netinterface;
        public ObservableCollection<VPN> VpnList { get; } = new ObservableCollection<VPN>();
        private bool isConnected = false;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            SetupRasDial();

            isConnected = VPN_Controller.CheckConnection();
            //TbI.IconSource = SetIcon()
            lbl_CurrentIP.Content = VPN_Controller.GetLocalIPAddress();

            lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus;

            ReadPhonebook();

            InitialiseTimer();
            this.DataContext = this;
            //dg_DataGrid.ItemsSource = VpnList;
        }

        public string TrayIcon
        {
            get { return Environment.CurrentDirectory + @"\Images\Tray_ok.ico"; }
        }

        // minimize to system tray when applicaiton is closed
        protected override void OnClosing(CancelEventArgs e)
        {
            // setting cancel to true will cancel the close request
            // so the application is not closed
            e.Cancel = true;

            this.Hide();

            base.OnClosing(e);
        }

        private object Change_Entry_Icon(object imageholder)
        {
            BitmapImage newBitMap;
            Image newImage;
            try
            {
                if (isConnected)
                {
                    SetIcon("Connection_ok.ico", out BitmapImage bitmap, out Image image);
                    newBitMap = bitmap;
                    newImage = image;
                }
                else
                {
                    SetIcon("Connection_error.ico", out BitmapImage bitmap, out Image image);
                    newBitMap = bitmap;
                    newImage = image;
                }
                if (imageholder is MenuItem item)
                {
                    return newImage;
                }
                else if (imageholder is Image img)
                {
                    return newBitMap;
                }
                else if (imageholder is BitmapImage)
                {
                    return newBitMap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

        private void Connect(VPN vpn)
        {
            isConnected = VPN_Controller.CheckConnection();

            if (isConnected)
            {
                VPN_Controller.CloseConnection(vpn.Name);
            }
            else
            {
                VPN_Controller.OpenConnection(vpn.Name, "", "");
            }
            isConnected = VPN_Controller.CheckConnection();
            vpn.Image = (BitmapImage)Change_Entry_Icon(vpn.Image);
            vpn.Status = VPN_Controller.ConnectionStatus;

            //foreach (var child in )
            //{
            //    if (child is Image img)
            //    {
            //        Change_Entry_Icon(img);
            //    }
            //}

            lbl_CurrentIP.Content = VPN_Controller.CurrentIP;
            lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus;

            //if (sender is MenuItem item)
            //{
            //    Change_Entry_Icon(item);
            //}
        }

        public class VPN
        {
            public BitmapImage Image { get; set; }
            public string Name { get; set; }
            public ObservableCollection<string> ConnectionTypes { get; set; } = new ObservableCollection<string>();
            public string Status { get; set; }
            public string IP { get; set; }

            public VPN()
            {
                string vpnConnection = "Connect";
                string rdp = "RDP";
                ConnectionTypes.Add(vpnConnection);
                ConnectionTypes.Add(rdp);
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (row.Item is VPN vpn)
                {
                    Connect(vpn);
                }
            }
        }

        private void CreateVPNRow(string vpnName)
        {
            VPN newVPN = new VPN();

            SetIcon("Connection_error.ico", out BitmapImage bitmap, out Image image);

            Image img = new Image
            {
                Width = 25
            };
            img.Margin = new Thickness(2, 2, 2, 2);
            img.Source = bitmap;

            newVPN.Image = bitmap;

            newVPN.Name = vpnName;

            //btn.Click += Connect_Clicked;
            VpnList.Add(newVPN);
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void InitialiseTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
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

                CreateVPNRow(entry.Name);
            }
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            //Connect(sender);
        }

        private void SetIcon(string iconName, out BitmapImage bitmap, out Image image)
        {
            image = new Image();
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Environment.CurrentDirectory + @"\Images\" + iconName);
            bitmap.EndInit();
            // Set Image.Source
            image.Source = bitmap;
        }

        private void SetupRasDial()
        {
            rasdial.StartInfo.FileName = "rasdial.exe";
            rasdial.StartInfo.RedirectStandardError = true;
            rasdial.StartInfo.RedirectStandardOutput = true;
            rasdial.StartInfo.UseShellExecute = false;
            rasdial.StartInfo.CreateNoWindow = true;
        }

        private void ShowWindow_Clicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateGUI_Icons();
        }

        private void UpdateGUI_Icons()
        {
            isConnected = VPN_Controller.CheckConnection();

            List<NetworkInterface> interfaceList = new List<NetworkInterface>(VPN_Controller.Netinterfaces);

            if (isConnected)
            {
                foreach (var connection in interfaceList)
                {
                    foreach (VPN vpn in VpnList)
                    {
                        if (vpn.Name == connection.Name)
                        {
                            vpn.Image = (BitmapImage)Change_Entry_Icon(vpn.Image);
                            vpn.IP = VPN_Controller.GetLocalIPAddress();
                            vpn.Status = VPN_Controller.ConnectionStatus;
                        }
                    }
                    foreach (MenuItem entry in TbI.ContextMenu.Items)
                    {
                        if ((string)entry.Header == connection.Name)
                        {
                            entry.Icon = Change_Entry_Icon(entry);
                        }
                    }
                }
            }
            else
            {
                foreach (VPN vpn in VpnList)
                {
                    vpn.Image = (BitmapImage)Change_Entry_Icon(vpn.Image);
                    vpn.Status = VPN_Controller.ConnectionStatus;
                }
                foreach (MenuItem entry in TbI.ContextMenu.Items)
                {
                    entry.Icon = Change_Entry_Icon(entry);
                }
            }
            dg_DataGrid.ItemsSource = null;
            dg_DataGrid.ItemsSource = VpnList;
            dg_DataGrid.Items.Refresh();
            lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus;
            lbl_CurrentIP.Content = VPN_Controller.CurrentIP;
        }
    }
}