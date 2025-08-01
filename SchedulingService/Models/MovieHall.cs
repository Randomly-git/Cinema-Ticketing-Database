using System;

/// <summary>
/// 影厅模型 (对应数据库中的 moviehall 表)
/// </summary>
public class MovieHall
{
    public int HallNo { get; set; } // PK: 影厅号
    public int Lines { get; set; } // 总行数
    public int Columns { get; set; } // 总列数
    public string Category { get; set; } // 影厅种类
}
