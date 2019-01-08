using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace VPN_Switch
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public string VPN_Name { get; set; }
        public string Message { get; set; }

        public Dialog()
        {
            InitializeComponent();
        }
        public void SetDialogMessage()
        {
            dialog_message.Text = Message;
        }
        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(dialog_username.Text) || !string.IsNullOrEmpty(dialog_password.Password))
            {
                VPN_Controller.OpenConnection(VPN_Name, dialog_username.Text, dialog_password.Password);
                this.Close();
            }
        }
    }
}