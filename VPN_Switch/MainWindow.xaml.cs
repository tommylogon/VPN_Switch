using DotRas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Xceed.Wpf.Toolkit;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using MenuItem = System.Windows.Controls.MenuItem;
using System.Collections.Specialized;

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
        public string ExitIcon { get; set; } = Environment.CurrentDirectory + @"\Images\Exit.ico";
        private bool isConnected = false;
        private DispatcherTimer timer;
        public string ButtonClickAction;
        private int SelectedIndex;

        public MainWindow()
        {
            InitializeComponent();

            SetupRasDial();

            ReadPhonebook();

            InitialiseTimer();
            this.DataContext = this;
            VpnList.CollectionChanged += VpnList_CollectionChanged;
        }

        private void VpnList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
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
                System.Windows.MessageBox.Show(ex.Message);
            }
            return null;
        }

        private void Connect(object entry)
        {
            if (entry is VPN vpn)
            {
                isConnected = VPN_Controller.CheckConnection(vpn.Name);
                if (isConnected)
                {
                    VPN_Controller.CloseConnection(vpn.Name);
                }
                else
                {
                    VPN_Controller.OpenConnection(vpn.Name, "", "");
                }
                isConnected = VPN_Controller.CheckConnection(vpn.Name);
                vpn.Image = (BitmapImage)Change_Entry_Icon(vpn.Image);
                vpn.Status = VPN_Controller.ConnectionStatus;
            }

            if (entry is MenuItem item)
            {
                Change_Entry_Icon(item);
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
                TbI.ContextMenu.Items.Insert(TbI.ContextMenu.Items.Count - 1, item);
                //TbI.ContextMenu.Items.Add(item);

                CreateVPNRow(entry.Name);
            }
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            if (dg_DataGrid.SelectedItem != null)
            {
                Connect((VPN)dg_DataGrid.SelectedItem);
            }
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

        private void RefreshDatagrid()
        {
            dg_DataGrid.ItemsSource = null;
            dg_DataGrid.ItemsSource = VpnList;
            dg_DataGrid.Items.Refresh();
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
            //UpdateGUI_Icons();
            foreach (VPN vpn in VpnList)
            {
                vpn.Status = VPN_Controller.ConnectionStatus;
            }
        }

        private void UpdateGUI_Icons(VPN vpn)
        {
            //isConnected = VPN_Controller.CheckConnection(vpn.Name);

            //List<NetworkInterface> interfaceList = new List<NetworkInterface>(VPN_Controller.Netinterfaces);

            //if (isConnected)
            //{
            //    foreach (var connection in interfaceList)
            //    {
            //        foreach (VPN entryVpn in VpnList)
            //        {
            //            if (entryVpn.Name == connection.Name)
            //            {
            //                entryVpn.Image = (BitmapImage)Change_Entry_Icon(vpn.Image);
            //                entryVpn.IP = VPN_Controller.GetLocalIPAddress();
            //                entryVpn.Status = VPN_Controller.ConnectionStatus;
            //            }
            //        }
            //        foreach (MenuItem entry in TbI.ContextMenu.Items)
            //        {
            //            if ((string)entry.Header == connection.Name)
            //            {
            //                entry.Icon = Change_Entry_Icon(entry);
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (VPN vpn in VpnList)
            //    {
            //        vpn.Image = (BitmapImage)Change_Entry_Icon(vpn.Image);
            //        vpn.Status = VPN_Controller.ConnectionStatus;
            //    }
            //    foreach (MenuItem entry in TbI.ContextMenu.Items)
            //    {
            //        if ((string)entry.Header != "Exit" || (string)entry.Header != "Open Window")
            //        {
            //            entry.Icon = Change_Entry_Icon(entry);
            //        }
            //    }
            //}
            //RefreshDatagrid();
            //dg_DataGrid.SelectedIndex = SelectedIndex;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedIndex != -1)
            {
                SelectedIndex = ((DataGrid)sender).SelectedIndex;
            }
        }
    }
}