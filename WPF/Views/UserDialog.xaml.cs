using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class UserDialog : Window
{
    public UserDialog()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UserDialogViewModel viewModel || sender is not PasswordBox passwordBox)
        {
            return;
        }

        viewModel.Password = passwordBox.Password;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UserDialogViewModel viewModel)
        {
            DialogResult = false;
            return;
        }

        if (!viewModel.ValidateForSave())
        {
            return;
        }

        DialogResult = true;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
