using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BusinessObjects.DTOs.Reports;
using Microsoft.Win32;
using Services.Interfaces;
using WPF.Commands;
using WPF.Utilities;

namespace WPF.ViewModels;

public sealed class OccupancyReportViewModel : BaseViewModel
{
    private readonly IOccupancyReportService _occupancyReportService;

    private DateTime _startDate = DateTime.Today.AddDays(-7);
    private DateTime _endDate = DateTime.Today;
    private int _totalRoomNights;
    private string _message = string.Empty;

    public override string Title => "Occupancy Report";

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

    public int TotalRoomNights
    {
        get => _totalRoomNights;
        set => SetProperty(ref _totalRoomNights, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ObservableCollection<OccupancyReportDto> OccupancyReports { get; } = new();

    public ICommand FilterCommand { get; }

    public ICommand ExportCommand { get; }

    public OccupancyReportViewModel(IOccupancyReportService occupancyReportService)
    {
        _occupancyReportService = occupancyReportService;

        FilterCommand = new RelayCommand(LoadData);
        ExportCommand = new RelayCommand(ExportCsv);

        LoadData();
    }

    private void LoadData()
    {
        OccupancyReports.Clear();
        TotalRoomNights = 0;
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

        var result = _occupancyReportService.GetOccupancyReport(filter);

        foreach (var item in result)
        {
            OccupancyReports.Add(item);
        }

        TotalRoomNights = result.Sum(x => x.TotalNightsBooked);

        if (!result.Any())
        {
            Message = "No occupancy data found for selected date range.";
        }
    }

    private void ExportCsv()
    {
        if (!OccupancyReports.Any())
        {
            MessageBox.Show("No data.");
            return;
        }

        var dialog = new SaveFileDialog();

        dialog.Filter = "CSV (*.csv)|*.csv";

        dialog.FileName = "OccupancyReport.csv";

        if (dialog.ShowDialog() == true)
        {
            CsvExporter.ExportToCsv(
                OccupancyReports,
                dialog.FileName);
        }
    }
}