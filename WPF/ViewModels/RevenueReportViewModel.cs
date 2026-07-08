using System.Collections.ObjectModel;
using System.Windows.Input;
using BusinessObjects.DTOs.Reports;
using WPF.Commands;
using WPF.Utilities;

namespace WPF.ViewModels;

public sealed class RevenueReportViewModel : BaseViewModel
{
    private DateTime _startDate = DateTime.Today.AddDays(-7);

    private DateTime _endDate = DateTime.Today;


    public override string Title => "Revenue Report";


    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }


    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }


    public ObservableCollection<RevenueReportDto> RevenueReports { get; } = new();


    public ObservableCollection<ServiceUsageReportDto> ServiceUsageReports { get; } = new();


    public ICommand FilterCommand { get; }


    public ICommand ExportRevenueCommand { get; }


    public ICommand ExportServiceCommand { get; }


    public RevenueReportViewModel()
    {
        FilterCommand = new RelayCommand(LoadMockData);

        ExportRevenueCommand =
            new RelayCommand(ExportRevenueCsv);

        ExportServiceCommand =
            new RelayCommand(ExportServiceCsv);


        LoadMockData();
    }


    private void LoadMockData()
    {
        RevenueReports.Clear();

        ServiceUsageReports.Clear();


        RevenueReports.Add(
            new RevenueReportDto
            {
                Date = DateTime.Today,

                RoomRevenue = 5000000,

                ServiceRevenue = 1200000,

                TotalRevenue = 6200000
            });


        RevenueReports.Add(
            new RevenueReportDto
            {
                Date = DateTime.Today.AddDays(-1),

                RoomRevenue = 4500000,

                ServiceRevenue = 900000,

                TotalRevenue = 5400000
            });


        RevenueReports.Add(
            new RevenueReportDto
            {
                Date = DateTime.Today.AddDays(-2),

                RoomRevenue = 7000000,

                ServiceRevenue = 2000000,

                TotalRevenue = 9000000
            });



        ServiceUsageReports.Add(
            new ServiceUsageReportDto
            {
                ServiceName = "Giặt ủi",

                Category = "Laundry",

                QuantityOrdered = 25,

                TotalRevenue = 1250000
            });


        ServiceUsageReports.Add(
            new ServiceUsageReportDto
            {
                ServiceName = "Nước suối",

                Category = "Drink",

                QuantityOrdered = 50,

                TotalRevenue = 500000
            });
    }


    private void ExportRevenueCsv()
    {
        CsvExporter.ExportToCsv(
            RevenueReports,
            $"revenue-report-{StartDate:yyyyMMdd}-{EndDate:yyyyMMdd}.csv");
    }


    private void ExportServiceCsv()
    {
        CsvExporter.ExportToCsv(
            ServiceUsageReports,
            $"service-report-{StartDate:yyyyMMdd}-{EndDate:yyyyMMdd}.csv");
    }
}