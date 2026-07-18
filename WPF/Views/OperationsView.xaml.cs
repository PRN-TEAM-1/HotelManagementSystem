using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPF.ViewModels;

namespace WPF.Views;

public partial class OperationsView : UserControl
{
    public OperationsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void SetActiveTab(Button activeButton)
    {
        var accentBrush = (Brush)FindResource("AccentBrush");
        var surfaceBrush = (Brush)FindResource("SurfaceBrush");
        var inverseTextBrush = (Brush)FindResource("TextInverseBrush");
        var primaryTextBrush = (Brush)FindResource("TextPrimaryBrush");

        foreach (var button in new[] { CheckInTabButton, CheckoutTabButton, ServiceManagementTabButton, ServiceOrderTabButton, BillingTabButton, CustomerTabButton, RoomTypeTabButton, RoomTabButton })
        {
            var isActive = ReferenceEquals(button, activeButton);
            button.Background = isActive ? accentBrush : surfaceBrush;
            button.Foreground = isActive ? inverseTextBrush : primaryTextBrush;
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            try
            {
                await vm.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải dữ liệu Operations:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            NavigateToCheckIn();
        }
    }

    private void OnCheckInClick(object sender, RoutedEventArgs e)
    {
        NavigateToCheckIn();
    }

    private void OnCheckoutClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.CheckoutViewModel;
            ContentFrame.Content = new CheckoutView { DataContext = vm.CheckoutViewModel };
            SetActiveTab(CheckoutTabButton);
        }
    }

    private void OnServiceManagementClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.ServiceManagementViewModel;
            ContentFrame.Content = new ServiceManagementView { DataContext = vm.ServiceManagementViewModel };
            SetActiveTab(ServiceManagementTabButton);
        }
    }

    private void OnServiceOrderClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.ServiceOrderViewModel;
            ContentFrame.Content = new ServiceOrderView { DataContext = vm.ServiceOrderViewModel };
            SetActiveTab(ServiceOrderTabButton);
        }
    }

    private async void OnBillingClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.BillingViewModel;
            ContentFrame.Content = new BillingView { DataContext = vm.BillingViewModel };
            SetActiveTab(BillingTabButton);

            try
            {
                await vm.BillingViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải dữ liệu Billing:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void OnCustomerManagementClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.CustomerManagementViewModel;
            ContentFrame.Content = new CustomerManagementView { DataContext = vm.CustomerManagementViewModel };
            SetActiveTab(CustomerTabButton);
        }
    }

    private void OnRoomTypeClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.RoomTypeManagementViewModel;
            ContentFrame.Content = new RoomTypeManagementView { DataContext = vm.RoomTypeManagementViewModel };
            SetActiveTab(RoomTypeTabButton);
        }
    }

    private void OnRoomClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.RoomManagementViewModel;
            ContentFrame.Content = new RoomManagementView { DataContext = vm.RoomManagementViewModel };
            SetActiveTab(RoomTabButton);
        }
    }

    private void NavigateToCheckIn()
    {
        if (DataContext is OperationsViewModel vm)
        {
            vm.CurrentViewModel = vm.CheckInViewModel;
            ContentFrame.Content = new CheckInView { DataContext = vm.CheckInViewModel };
            SetActiveTab(CheckInTabButton);
        }
    }
}
