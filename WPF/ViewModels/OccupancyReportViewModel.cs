using System.Collections.ObjectModel;
using System.Windows.Input;
using BusinessObjects.DTOs.Reports;
using WPF.Commands;
using WPF.Utilities;

namespace WPF.ViewModels;

public sealed class OccupancyReportViewModel : BaseViewModel
{
    private DateTime _startDate = DateTime.Today.AddDays(-7);

    private DateTime _endDate = DateTime.Today;

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

    public ObservableCollection<OccupancyReportDto> OccupancyReports { get; } = new();

    public ICommand FilterCommand { get; }

    public ICommand ExportCommand { get; }

    public OccupancyReportViewModel()
    {
        FilterCommand = new RelayCommand(LoadMockData);
        ExportCommand = new RelayCommand(ExportCsv);

        LoadMockData();
    }

    private void LoadMockData()
    {
        OccupancyReports.Clear();

        OccupancyReports.Add(new OccupancyReportDto
        {
            RoomNumber = "101",
            RoomType = "Phòng Standard",
            TotalNightsBooked = 5,
            OccupancyRate = 71.43
        });

        OccupancyReports.Add(new OccupancyReportDto
        {
            RoomNumber = "202",
            RoomType = "Phòng Deluxe",
            TotalNightsBooked = 4,
            OccupancyRate = 57.14
        });

        OccupancyReports.Add(new OccupancyReportDto
        {
            RoomNumber = "301",
            RoomType = "Phòng Suite VIP",
            TotalNightsBooked = 6,
            OccupancyRate = 85.71
        });
    }

    private void ExportCsv()
    {
        CsvExporter.ExportToCsv(
            OccupancyReports,
            $"occupancy-report-{StartDate:yyyyMMdd}-{EndDate:yyyyMMdd}.csv");
    }
}