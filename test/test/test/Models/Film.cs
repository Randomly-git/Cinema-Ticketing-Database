using System;
using System.Collections.Generic;
using test.Models;

namespace test.Models 
{
    /// 对应数据库中的 film 表，存储电影的基本信息。
    public class Film
    {
        public string FilmName { get; set; } // 电影名
        public string Genre { get; set; }    // 电影类型
        public int FilmLength { get; set; }  // 电影时长（分钟）
        public decimal NormalPrice { get; set; } // 标准票价
        public DateTime? ReleaseDate { get; set; } // 上映日期
        public DateTime? EndDate { get; set; } // 撤档日期，可为空
        public int Admissions { get; set; }  // 观影人次
        public int BoxOffice { get; set; }   // 票房
        public decimal Score { get; set; }       // 评分（所有观影人次平均评分）
        public int RatingNum { get; set; }       // 有效的评分数

        // 导航属性：方便获取演职人员和场次信息
        public List<Cast> CastMembers { get; set; } = new List<Cast>();
        public List<Section> Sections { get; set; } = new List<Section>();

        public override string ToString()
        {
            return $"电影名称: {FilmName}, 类型: {Genre}, 时长: {FilmLength}分钟, 评分: {Score}分，参与评分人数: {RatingNum}";
        }
    }
}
