using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;
using FluentWindow = Wpf.Ui.Controls.FluentWindow;

namespace WPF.Views;

public partial class LoginWindow : FluentWindow
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LoginViewModel viewModel || sender is not PasswordBox passwordBox)
        {
            return;
        }

        viewModel.Password = passwordBox.Password;
        if (VisiblePasswordInput != null && VisiblePasswordInput.Text != passwordBox.Password)
        {
            VisiblePasswordInput.Text = passwordBox.Password;
        }
    }

    private void OnVisiblePasswordChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is not LoginViewModel viewModel || sender is not TextBox textBox)
        {
            return;
        }

        viewModel.Password = textBox.Text;
        if (PasswordBoxInput != null && PasswordBoxInput.Password != textBox.Text)
        {
            PasswordBoxInput.Password = textBox.Text;
        }
    }
}
