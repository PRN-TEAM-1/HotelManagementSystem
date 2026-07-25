namespace BusinessObjects.DTOs.Dashboard
{
    public class TodayOperationSummaryDto
    {
        public int TodayCheckIns { get; set; }

        public int TodayCheckOuts { get; set; }
        public int ActiveStays { get; set; }
        public int DueArrivals { get; set; }
        public int DueDepartures { get; set; }
    }
}
