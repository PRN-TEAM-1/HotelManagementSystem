using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPF.ViewModels;

namespace WPF.Views;

public partial class ReportsView : UserControl
{
    public ReportsView()
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

        foreach (var button in new[] { DashboardTabButton, OccupancyTabButton, RevenueTabButton, ServiceUsageTabButton })
        {
            var isActive = ReferenceEquals(button, activeButton);
            button.Background = isActive ? accentBrush : surfaceBrush;
            button.Foreground = isActive ? inverseTextBrush : primaryTextBrush;
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReportsViewModel vm)
        {
            try
            {
                await vm.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải dữ liệu Reports:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            NavigateToDashboard();
        }
    }

    private void OnDashboardClick(object sender, RoutedEventArgs e)
    {
        NavigateToDashboard();
    }

    private async void OnOccupancyClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReportsViewModel vm)
        {
            vm.CurrentViewModel = vm.OccupancyReportViewModel;
            ContentFrame.Content = new OccupancyReportView
            {
                DataContext = vm.OccupancyReportViewModel
            };

            SetActiveTab(OccupancyTabButton);

            try
            {
                await vm.OccupancyReportViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải dữ liệu Occupancy Report:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void OnRevenueClick(object sender, RoutedEventArgs e)
    {
        ContentFrame.Content = new RevenueReportView
        {
            DataContext = new RevenueReportViewModel()
        };

        SetActiveTab(RevenueTabButton);
    }

    private void OnServiceUsageClick(object sender, RoutedEventArgs e)
    {
        ShowPlaceholder("Báo cáo sử dụng dịch vụ (Service Usage Report) sẽ được triển khai ở task sau.");
        SetActiveTab(ServiceUsageTabButton);
    }

    private void NavigateToDashboard()
    {
        if (DataContext is ReportsViewModel vm)
        {
            vm.CurrentViewModel = vm.DashboardViewModel;
            ContentFrame.Content = new DashboardView { DataContext = vm.DashboardViewModel };
            SetActiveTab(DashboardTabButton);
        }
    }

    private void ShowPlaceholder(string message)
    {
        ContentFrame.Content = new UserControl
        {
            Content = new TextBlock
            {
                Text = message,
                FontSize = 16,
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(40)
            }
        };
    }
}
