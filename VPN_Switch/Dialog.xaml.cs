using System.Windows;
using System.Windows.Input;

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
            Forward_Connect();
        }

        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Forward_Connect();
            }
        }

        private void Forward_Connect()
        {
            if (!string.IsNullOrEmpty(dialog_username.Text) || !string.IsNullOrEmpty(dialog_password.Password))
            {
                VPN_Controller.OpenConnection(VPN_Name, dialog_username.Text, dialog_password.Password);
                this.Close();
            }
        }
    }
}