using System;

/// <summary>
/// 场次模型 (Section)
/// SECTION表通过关联TIMESLOT表获取完整的时间信息。
/// </summary>
public class Section
{
    public int SectionID { get; set; } // 场次号，主键
    public string FilmName { get; set; } // 电影名，外键
    public int HallNo { get; set; } // 影厅号，外键
    public string TimeID { get; set; } // 外键，关联TIMESLOT表
    public DateTime ScheduleStartTime { get; set; } // 场次开始时间 (包含日期和时间, 从TIMESLOT联查)
    public DateTime ScheduleEndTime { get; set; } // 场次结束时间 (包含日期和时间, 从TIMESLOT联查)
    public string HallCategory { get; set; } // 影厅种类 (从MovieHall联查得到)
}
