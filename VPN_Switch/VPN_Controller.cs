using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VPN_Switch
{
    public static class VPN_Controller
    {
        public static NetworkInterface netinterface { get; set; }
        public static string ConnectionStatus { get; set; }
        public static string CurrentIP { get; set; }

        public static bool CheckConnection()
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
                            ConnectionStatus = "Your are connected to " + netinterface.Name;
                            CurrentIP = GetLocalIPAddress();
                            return true;
                        }
                    }
                }
            }
            netinterface = null;
            ConnectionStatus = "You are disconnected";
            CurrentIP = GetLocalIPAddress();
            return false;
        }

        public static void OpenConnection(string vpnName, string username, string password)
        {
            MainWindow.rasdial.StartInfo.Arguments = vpnName + " " + username + " " + password;
            MainWindow.rasdial.Start();

            string output = MainWindow.rasdial.StandardOutput.ReadToEnd();
            string error = MainWindow.rasdial.StandardError.ReadToEnd();
            var exitcode = MainWindow.rasdial.ExitCode;
            MainWindow.rasdial.WaitForExit();

            if (exitcode == 691)
            {
                Dialog dialog = new Dialog();
                dialog.Message = "Please enter your login information";
                dialog.VPN_Name = vpnName;
                dialog.SetDialogMessage();
                dialog.Show();
            }
            else
            {
                CheckConnection();
            }
        }

        public static void CloseConnection(string vpnName)
        {
            MainWindow.rasdial.StartInfo.Arguments = vpnName + " /d";
            MainWindow.rasdial.Start();
            MainWindow.rasdial.WaitForExit();

            CheckConnection();
        }

        public static string GetLocalIPAddress()
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
    }
}