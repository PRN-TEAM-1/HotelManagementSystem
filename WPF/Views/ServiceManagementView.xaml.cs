using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class ServiceManagementView : UserControl
{
    public ServiceManagementView()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ServiceManagementViewModel viewModel)
        {
            viewModel.ResetFormCommand.Execute(null);
            viewModel.SelectedService = null;
        }
    }
}
