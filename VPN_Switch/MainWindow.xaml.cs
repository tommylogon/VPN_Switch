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
using System.Threading;

namespace VPN_Switch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool isConnected = false;
        public static Process rasdial = new Process();
        public NetworkInterface netinterface;

        public MainWindow()
        {
            InitializeComponent();
            SetupRasDial();
            VPN_Controller.CheckConnection();
            lbl_CurrentIP.Content = VPN_Controller.GetLocalIPAddress();
            lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus;
            ReadPhonebook();
            StartBackgroundChecker();
        }

        private void StartBackgroundChecker()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += CheckConnection_Background;
            worker.RunWorkerAsync();
        }

        public void CheckConnection_Background(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                VPN_Controller.CheckConnection();

                //lbl_ConnectionStatus.Content = VPN_Controller.ConnectionStatus; };

                e.Result = VPN_Controller.ConnectionStatus;
                Thread.Sleep(5000);
            }
        }

        private void SetupRasDial()
        {
            rasdial.StartInfo.FileName = "rasdial.exe";
            rasdial.StartInfo.RedirectStandardError = true;
            rasdial.StartInfo.RedirectStandardOutput = true;
            rasdial.StartInfo.UseShellExecute = false;
        }

        //ROW TEST
        private void CreateVPNRow(string vpnName)
        {
            SetIcon("nok.png", out BitmapImage bitmap, out Image image);

            WrapPanel wp = new WrapPanel();

            Image img = new Image
            {
                Width = 15
            };
            img.Margin = new Thickness(2, 2, 2, 2);
            img.Source = bitmap;

            Button btn = new Button
            {
                Content = vpnName
            };
            btn.Margin = new Thickness(2, 2, 2, 2);
            btn.Click += Connect_Clicked;

            Button rdp = new Button
            {
                Content = "RDP"
            };
            //rdp.Click += ConnectRDP;
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
                    if (VPN_Controller.CheckConnection())
                    {
                        SetIcon("ok.png", out BitmapImage bitmap, out Image image);
                        item.Icon = image;
                    }
                    else
                    {
                        SetIcon("nok.png", out BitmapImage bitmap, out Image image);
                        item.Icon = image;
                        //MessageBox.Show("Connection Failed,\n Please check Username and/or Password", "Error");
                    }
                }
                else if (imageholder is Image img)
                {
                    if (VPN_Controller.CheckConnection())
                    {
                        SetIcon("ok.png", out BitmapImage bitmap, out Image image);
                        img.Source = bitmap;
                    }
                    else
                    {
                        SetIcon("nok.png", out BitmapImage bitmap, out Image image);
                        img.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (VPN_Controller.CheckConnection())
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
            bitmap.UriSource = new Uri(Environment.CurrentDirectory + @"\" + iconName);
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
            System.Windows.Application.Current.Shutdown();
        }
    }
}