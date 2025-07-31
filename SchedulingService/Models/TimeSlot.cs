using System;

/// <summary>
/// 时段模型
/// 根据最新理解，STARTTIME和ENDTIME存储完整的日期和时间。
/// </summary>
public class TimeSlot
{
    public string TimeID { get; set; } // 时段号，主键
    public DateTime StartTime { get; set; } // 电影开场时间 (包含日期和时间)
    public DateTime EndTime { get; set; } // 电影散场时间 (包含日期和时间)
}
