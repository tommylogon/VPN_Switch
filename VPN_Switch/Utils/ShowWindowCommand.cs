using System;
using System.Windows;
using System.Windows.Input;

namespace VPN_Switch.Utils
{
    public class ShowWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Application.Current.MainWindow.Show();
        }
    }
}