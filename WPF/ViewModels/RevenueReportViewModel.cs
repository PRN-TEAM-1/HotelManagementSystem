using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
    public ObservableCollection<ServiceRevenueChartItem> ServiceRevenueChart { get; } = new();

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
        ServiceRevenueChart.Clear();
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
        var serviceRevenues = _service.GetRevenueByService(filter);

        foreach (var item in revenues)
        {
            RevenueReports.Add(item);
        }

        foreach (var item in paymentMethods)
        {
            PaymentMethodReports.Add(item);
        }

        BuildServiceRevenueChart(serviceRevenues);

        TotalRevenue = revenues.Sum(x => x.TotalRevenue);

        if (!revenues.Any())
        {
            Message = "No revenue data found.";
        }
    }

    public decimal ServiceRevenueTotal =>
        ServiceRevenueChart.Sum(x => x.TotalRevenue);

    private void BuildServiceRevenueChart(IReadOnlyList<ServiceRevenueDto> revenues)
    {
        var positiveItems = revenues.Where(x => x.TotalRevenue > 0).Take(6).ToList();
        var otherItems = revenues.Where(x => x.TotalRevenue > 0).Skip(6).ToList();
        if (otherItems.Count > 0)
        {
            positiveItems.Add(new ServiceRevenueDto
            {
                ServiceName = "Others",
                QuantityOrdered = otherItems.Sum(x => x.QuantityOrdered),
                TotalRevenue = otherItems.Sum(x => x.TotalRevenue)
            });
        }

        var total = positiveItems.Sum(x => x.TotalRevenue);
        if (total <= 0)
        {
            OnPropertyChanged(nameof(ServiceRevenueTotal));
            return;
        }

        string[] colors = ["#1E8BA5", "#F0A33B", "#4E75C9", "#52A675", "#C95E71", "#8B6CC1", "#8A9AAA"];
        var startAngle = -90d;

        for (var index = 0; index < positiveItems.Count; index++)
        {
            var item = positiveItems[index];
            var sweepAngle = (double)(item.TotalRevenue / total) * 360d;
            ServiceRevenueChart.Add(new ServiceRevenueChartItem
            {
                ServiceName = item.ServiceName,
                QuantityOrdered = item.QuantityOrdered,
                TotalRevenue = item.TotalRevenue,
                Percentage = (double)(item.TotalRevenue / total),
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[index % colors.Length])),
                Geometry = CreateDonutSlice(startAngle, sweepAngle)
            });
            startAngle += sweepAngle;
        }

        OnPropertyChanged(nameof(ServiceRevenueTotal));
    }

    private static Geometry CreateDonutSlice(double startAngle, double sweepAngle)
    {
        const double outerRadius = 78;
        const double innerRadius = 45;
        var endAngle = startAngle + Math.Min(sweepAngle, 359.999);
        var largeArc = sweepAngle > 180;

        static Point PointOnCircle(double angle, double radius)
        {
            var radians = angle * Math.PI / 180d;
            return new Point(90 + radius * Math.Cos(radians), 90 + radius * Math.Sin(radians));
        }

        var outerStart = PointOnCircle(startAngle, outerRadius);
        var outerEnd = PointOnCircle(endAngle, outerRadius);
        var innerEnd = PointOnCircle(endAngle, innerRadius);
        var innerStart = PointOnCircle(startAngle, innerRadius);

        var figure = new PathFigure { StartPoint = outerStart, IsClosed = true, IsFilled = true };
        figure.Segments.Add(new ArcSegment(outerEnd, new Size(outerRadius, outerRadius), 0, largeArc, SweepDirection.Clockwise, true));
        figure.Segments.Add(new LineSegment(innerEnd, true));
        figure.Segments.Add(new ArcSegment(innerStart, new Size(innerRadius, innerRadius), 0, largeArc, SweepDirection.Counterclockwise, true));
        return new PathGeometry([figure]);
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

public sealed class ServiceRevenueChartItem
{
    public string ServiceName { get; init; } = string.Empty;
    public int QuantityOrdered { get; init; }
    public decimal TotalRevenue { get; init; }
    public double Percentage { get; init; }
    public Brush Fill { get; init; } = Brushes.Transparent;
    public Geometry Geometry { get; init; } = Geometry.Empty;
}
