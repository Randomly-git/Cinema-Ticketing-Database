using System;

/// <summary>
/// 电影模型 (对应数据库中的 film 表)
/// </summary>
public class Film
{
    public string FilmName { get; set; } // PK: 电影名称
    public string Genre { get; set; } // 电影类型
    public int FilmLength { get; set; } // 电影时长（分钟）
    public decimal NormalPrice { get; set; } // 标准票价
    public DateTime? ReleaseDate { get; set; } // 上映日期
    public DateTime? EndDate { get; set; } // 撤档日期
    public int Admissions { get; set; } // 观影人次
    public decimal BoxOffice { get; set; } // 票房
    public decimal Score { get; set; } // 评分
}
