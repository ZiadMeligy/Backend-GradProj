using GP_Server.Application.DTOs.Statistics;

namespace GP_Server.Application.Helpers;

public static class TimePeriodHelper
{
    public static DateTime GetStartDateForPeriod(TimePeriod period)
    {
        var now = DateTime.UtcNow;
        
        return period switch
        {
            TimePeriod.LastWeek => now.AddDays(-7),
            TimePeriod.LastMonth => now.AddMonths(-1),
            TimePeriod.Last3Months => now.AddMonths(-3),
            TimePeriod.Last6Months => now.AddMonths(-6),
            TimePeriod.Last9Months => now.AddMonths(-9),
            TimePeriod.LastYear => now.AddYears(-1),
            TimePeriod.AllTime => DateTime.MinValue,
            _ => DateTime.MinValue
        };
    }
    
    public static (DateTime StartDate, DateTime EndDate) GetDateRangeForPeriod(TimePeriod period)
    {
        var endDate = DateTime.UtcNow;
        var startDate = GetStartDateForPeriod(period);
        
        return (startDate, endDate);
    }
}
