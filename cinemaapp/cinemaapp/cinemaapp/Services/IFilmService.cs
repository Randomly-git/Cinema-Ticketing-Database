using test.Models; // 引用 Models 命名空间
using test.Repositories; // 引用 Repositories 命名空间
using System;
using System.Collections.Generic;

namespace test.Services
{
    // 添加统计数据类
    public class FilmStatistic
    {
        public string FilmName { get; set; }
        public decimal BoxOffice { get; set; }
        public decimal Score { get; set; }
        public int Admissions { get; set; }
    }

    //影片业务服务接口。
    public interface IFilmService
    {
        List<Film> GetAvailableFilms(); // 获取所有当前可供查询的电影列表
        Film GetFilmDetails(string filmName); // 获取电影详细信息，包括演职人员和场次
        List<Cast> GetFilmCast(string filmName); // 获取电影演职人员
        List<Section> GetFilmSections(string filmName); // 获取电影场次
        List<Film> GetFilmStatistics(string sortBy);//排序电影统计信息

    }
}
