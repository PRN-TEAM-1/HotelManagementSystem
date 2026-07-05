using System.Windows;
using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class UserManagementView : UserControl
{
    public UserManagementView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UserManagementViewModel viewModel)
        {
            return;
        }

        try
        {
            await viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to load user management data:{Environment.NewLine}{ex.Message}",
                "User Management",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
