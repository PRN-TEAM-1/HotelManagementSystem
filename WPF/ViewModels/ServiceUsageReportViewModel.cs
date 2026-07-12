using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BusinessObjects.DTOs.Reports;
using Repositories.Implements;
using Services.Implements;
using Services.Interfaces;
using WPF.Commands;
using WPF.Utilities;

namespace WPF.ViewModels;

public sealed class ServiceUsageReportViewModel : BaseViewModel
{
    private readonly IServiceUsageReportService _service;

    private DateTime _startDate = DateTime.Today.AddDays(-7);
    private DateTime _endDate = DateTime.Today;
    private int _totalQuantity;
    private decimal _totalRevenue;
    private string _message = string.Empty;

    public override string Title => "Service Usage Report";

    public ObservableCollection<ServiceUsageReportDto> ServiceUsageReports { get; } = new();

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

    public int TotalQuantity
    {
        get => _totalQuantity;
        set => SetProperty(ref _totalQuantity, value);
    }

    public decimal TotalRevenue
    {
        get => _totalRevenue;
        set => SetProperty(ref _totalRevenue, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand FilterCommand { get; }
    public ICommand ExportCommand { get; }

    public ServiceUsageReportViewModel()
        : this(new ServiceUsageReportService(new ServiceUsageReportRepository()))
    {
    }

    public ServiceUsageReportViewModel(IServiceUsageReportService service)
    {
        _service = service;

        FilterCommand = new RelayCommand(LoadData);
        ExportCommand = new RelayCommand(ExportCsv);

        LoadData();
    }

    private void LoadData()
    {
        ServiceUsageReports.Clear();
        TotalQuantity = 0;
        TotalRevenue = 0;
        Message = string.Empty;

        if (EndDate.Date < StartDate.Date)
        {
            Message = "End date must be greater than or equal to start date.";
            return;
        }

        var filter = new ReportFilterDto
        {
            StartDate = StartDate.Date,
            EndDate = EndDate.Date
        };

        var result = _service.GetServiceUsageReport(filter);

        foreach (var item in result)
        {
            ServiceUsageReports.Add(item);
        }

        TotalQuantity = result.Sum(x => x.QuantityOrdered);
        TotalRevenue = result.Sum(x => x.TotalRevenue);

        if (!result.Any())
        {
            Message = "No service usage data found.";
        }
    }

    private void ExportCsv()
    {
        if (!ServiceUsageReports.Any())
        {
            MessageBox.Show("No data to export.", "Export CSV");
            return;
        }

        CsvExporter.ExportToCsv(
            ServiceUsageReports,
            $"service-usage-report-{StartDate:yyyyMMdd}-{EndDate:yyyyMMdd}.csv");
    }
}