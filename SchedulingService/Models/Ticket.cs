using System;

/// <summary>
/// 电影票模型 (对应数据库中的 ticket 表)
/// </summary>
public class Ticket
{
    public string TicketID { get; set; } // PK: 票号
    public decimal Price { get; set; } // 实际售价
    public int? Rating { get; set; } // 评分 (单次观影)
    public int SectionID { get; set; } // FK: 场次号
    public int LineNo { get; set; } // 座位行号
    public int ColumnNo { get; set; } // 座位列号
    public string State { get; set; } // 状态 (已售出、未售出)
}
