using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;

namespace VPN_Switch
{
    public static class VPN_Controller
    {
        public static string CurrentIP { get; set; }
        public static IList<NetworkInterface> Netinterfaces { get; set; }

        public static bool CheckConnection(string vpnName)
        {
            if (Netinterfaces == null)
            {
                Netinterfaces = new List<NetworkInterface>();
            }
            else
            {
                Netinterfaces.Clear();
            }

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface Interface in interfaces)
                {
                    if (Interface.OperationalStatus == OperationalStatus.Up)
                    {
                        if ((Interface.NetworkInterfaceType == NetworkInterfaceType.Ppp) && (Interface.NetworkInterfaceType != NetworkInterfaceType.Loopback) && Interface.Name == vpnName)
                        {
                            Netinterfaces.Add(Interface);
                        }
                    }
                }
                if (Netinterfaces.Count != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static void CloseConnection(string vpnName)
        {
            MainWindow.rasdial.StartInfo.Arguments = vpnName + " /d";
            MainWindow.rasdial.Start();
            MainWindow.rasdial.WaitForExit();

            //CheckConnection();
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
                Dialog dialog = new Dialog
                {
                    Message = "Please enter your login information",
                    VPN_Name = vpnName
                };
                dialog.SetDialogMessage();
                dialog.Show();
            }
            else if (exitcode == -2147020568)
            {
                MessageBox.Show(exitcode + ": " + error + "\n Already connected to this host.");
            }
            else if (exitcode != 0)
            {
                MessageBox.Show(exitcode + ": " + error);
            }
            
            else
            {
                CheckConnection(vpnName);
            }
        }
    }
}