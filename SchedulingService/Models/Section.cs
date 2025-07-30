using System;

public class Section
{
    public int SectionID { get; set; } // 场次号，主键
    public string FilmName { get; set; } // 电影名，外键
    public int HallNo { get; set; } // 影厅号，外键
    public string TimeID { get; set; } // 时段号，外键
    // public DateTime Day { get; set; } // 临时注释掉：SECTION表暂时没有DAY列

    // 为了方便显示和业务逻辑，可以添加以下属性，它们从关联表中获取数据
    public string HallCategory { get; set; } // 影厅种类 (来自MovieHall)
    public TimeSpan StartTime { get; set; } // 开场时间 (来自TimeSlot)
    public TimeSpan EndTime { get; set; } // 散场时间 (来自TimeSlot)
}
