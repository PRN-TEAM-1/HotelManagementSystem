using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class CheckoutView : UserControl
{
    public CheckoutView()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is CheckoutViewModel viewModel)
        {
            viewModel.SelectedCandidate = null;
        }
    }
}
