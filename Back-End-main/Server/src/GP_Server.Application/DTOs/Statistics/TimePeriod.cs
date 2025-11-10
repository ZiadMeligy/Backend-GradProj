using System.ComponentModel;

namespace GP_Server.Application.DTOs.Statistics;

public enum TimePeriod
{
    [Description("Last Week")]
    LastWeek = 1,
    
    [Description("Last Month")]
    LastMonth = 2,
    
    [Description("Last 3 Months")]
    Last3Months = 3,
    
    [Description("Last 6 Months")]
    Last6Months = 4,
    
    [Description("Last 9 Months")]
    Last9Months = 5,
    
    [Description("Last Year")]
    LastYear = 6,
    
    [Description("All Time")]
    AllTime = 7
}
