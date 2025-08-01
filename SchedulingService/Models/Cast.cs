using System;

/// <summary>
/// 演职人员模型 (对应数据库中的 cast 表)
/// </summary>
public class Cast
{
    public string MemberName { get; set; } // PK: 人员姓名
    public string Role { get; set; } // 角色
    public string FilmName { get; set; } // FK: 电影名称
}
