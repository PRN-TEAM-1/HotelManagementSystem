using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class CheckInView : UserControl
{
    public CheckInView()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is CheckInViewModel viewModel)
        {
            viewModel.SelectedCandidate = null;
        }
    }
}
