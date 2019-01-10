using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using DotRas;
using System.Net.NetworkInformation;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Threading;

namespace VPN_Switch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Process rasdial = new Process();
        private bool isConnected = false;
        public NetworkInterface netinterface;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            SetupRasDial();

            isConnected = VPN_Controller.CheckConnection();
            TbI.IconSource = SetIcon()
            lbl_CurrentIP.Content = VPN_Controller.GetLocalIPAddress();

            lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus;

            ReadPhonebook();

            InitialiseTimer();
        }

        public string TrayIcon
        {
            get { return Environment.CurrentDirectory + @"\Images\Tray_ok.ico"; }
        }

        private void SetupRasDial()
        {
            rasdial.StartInfo.FileName = "rasdial.exe";
            rasdial.StartInfo.RedirectStandardError = true;
            rasdial.StartInfo.RedirectStandardOutput = true;
            rasdial.StartInfo.UseShellExecute = false;
            rasdial.StartInfo.CreateNoWindow = true;
        }

        private void CreateVPNRow(string vpnName)
        {
            SetIcon("Connection_error.ico", out BitmapImage bitmap, out Image image);

            WrapPanel wp = new WrapPanel();

            Image img = new Image
            {
                Width = 25
            };
            img.Margin = new Thickness(2, 2, 2, 2);
            img.Source = bitmap;

            Button btn = new Button
            {
                Content = vpnName
            };
            btn.FontSize = 15;
            btn.Margin = new Thickness(2, 2, 2, 2);
            btn.Click += Connect_Clicked;

            Button rdp = new Button
            {
                Content = "RDP"
            };
            //rdp.Click += ConnectRDP;
            rdp.Background = Brushes.Red;
            rdp.Margin = new Thickness(2, 2, 2, 2);
            wp.Children.Add(img);
            wp.Children.Add(btn);
            wp.Children.Add(rdp);

            stkpnl_Container.Children.Add(wp);
        }

        private void ShowWindow_Clicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
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

        private void Change_Entry_Icon(object imageholder)
        {
            try
            {
                if (imageholder is MenuItem item)
                {
                    if (isConnected)
                    {
                        SetIcon("Connection_ok.ico", out BitmapImage bitmap, out Image image);
                        item.Icon = image;
                    }
                    else
                    {
                        SetIcon("Connection_error.ico", out BitmapImage bitmap, out Image image);
                        item.Icon = image;
                        //MessageBox.Show("Connection Failed,\n Please check Username and/or Password", "Error");
                    }
                }
                else if (imageholder is Image img)
                {
                    if (isConnected)
                    {
                        SetIcon("Connection_ok.ico", out BitmapImage bitmap, out Image image);
                        img.Source = bitmap;
                    }
                    else
                    {
                        SetIcon("Connection_error.ico", out BitmapImage bitmap, out Image image);
                        img.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            isConnected = VPN_Controller.CheckConnection();

            if (sender is Button btn)
            {
                if (isConnected)
                {
                    VPN_Controller.CloseConnection((string)btn.Content);
                }
                else
                {
                    VPN_Controller.OpenConnection((string)btn.Content, "", "");
                }

                foreach (var child in ((WrapPanel)btn.Parent).Children)
                {
                    if (child is Image img)
                    {
                        Change_Entry_Icon(img);
                    }
                }
            }
            lbl_CurrentIP.Content = VPN_Controller.GetLocalIPAddress();
            lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus;
            if (sender is MenuItem item)
            {
                Change_Entry_Icon(item);
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
                    foreach (WrapPanel row in stkpnl_Container.Children)
                    {
                        if ((string)((Button)row.Children[1]).Content == connection.Name)
                        {
                            Change_Entry_Icon(row.Children[0]);
                        }
                    }
                    foreach (MenuItem entry in TbI.ContextMenu.Items)
                    {
                        if ((string)entry.Header == connection.Name)
                        {
                            Change_Entry_Icon(entry);
                        }
                    }
                }
            }
            else
            {
                foreach (WrapPanel row in stkpnl_Container.Children)
                {
                    Change_Entry_Icon(row.Children[0]);
                }
                foreach (MenuItem entry in TbI.ContextMenu.Items)
                {
                    Change_Entry_Icon(entry);
                }
            }

            lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus;
            lbl_CurrentIP.Content = VPN_Controller.CurrentIP;
        }

        private void UpdateOnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
        }
    }
}