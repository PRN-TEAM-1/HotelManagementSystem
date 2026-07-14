using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BusinessObjects.DTOs.Reports;
using Microsoft.Win32;
using Repositories.Implements;
using Services.Implements;
using Services.Interfaces;
using WPF.Commands;
using WPF.Utilities;

namespace WPF.ViewModels;

public sealed class RevenueReportViewModel : BaseViewModel
{
    private readonly IRevenueReportService _service;

    private DateTime _startDate = DateTime.Today.AddDays(-7);
    private DateTime _endDate = DateTime.Today;
    private decimal _totalRevenue;
    private string _message = string.Empty;

    public override string Title => "Revenue Report";

    public ObservableCollection<RevenueReportDto> RevenueReports { get; } = new();
    public ObservableCollection<PaymentRevenueDto> PaymentMethodReports { get; } = new();

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

    public RevenueReportViewModel()
        : this(new RevenueReportService(new RevenueReportRepository()))
    {
    }

    public RevenueReportViewModel(IRevenueReportService service)
    {
        _service = service;

        FilterCommand = new RelayCommand(LoadData);
        ExportCommand = new RelayCommand(ExportCsv);

        LoadData();
    }

    private void LoadData()
    {
        RevenueReports.Clear();
        PaymentMethodReports.Clear();
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

        var revenues = _service.GetRevenueReport(filter);
        var paymentMethods = _service.GetRevenueByPaymentMethod(filter);

        foreach (var item in revenues)
        {
            RevenueReports.Add(item);
        }

        foreach (var item in paymentMethods)
        {
            PaymentMethodReports.Add(item);
        }

        TotalRevenue = revenues.Sum(x => x.TotalRevenue);

        if (!revenues.Any())
        {
            Message = "No revenue data found.";
        }
    }

    private void ExportCsv()
    {
        if (!RevenueReports.Any())
        {
            MessageBox.Show("No data.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = "RevenueReport.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            CsvExporter.ExportToCsv(
                RevenueReports,
                dialog.FileName);
        }
    }
}