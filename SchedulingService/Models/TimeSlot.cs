using System;

public class TimeSlot
{
    public string TimeID { get; set; } // 时段号，主键
    public TimeSpan StartTime { get; set; } // 电影开场时间
    public TimeSpan EndTime { get; set; } // 电影散场时间
}
