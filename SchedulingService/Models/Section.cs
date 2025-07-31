using System;

/// <summary>
/// 场次模型 (Section)
/// 根据最新理解，SECTION表通过TIMEID关联TIMESLOT表来获取时间信息。
/// </summary>
public class Section
{
    public int SectionID { get; set; } // 场次号，主键
    public string FilmName { get; set; } // 电影名，外键
    public int HallNo { get; set; } // 影厅号，外键
    public string TimeID { get; set; } // 外键，关联TIMESLOT表

    // public DateTime Day { get; set; } // 暂时注释掉：如果TIMESLOT.STARTTIME包含日期，此列可能冗余

    // 为了方便显示，从关联的TIMESLOT表中获取的开始和结束时间
    public DateTime ScheduleStartTime { get; set; }
    public DateTime ScheduleEndTime { get; set; }

    // 为了方便显示，从关联的MovieHall表中获取的影厅类型
    public string HallCategory { get; set; }
}
