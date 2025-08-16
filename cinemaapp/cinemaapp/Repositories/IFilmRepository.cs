using System;
using test.Models;
using System.Collections.Generic;

namespace test.Repositories
{
    /// 影片数据仓储接口。
    public interface IFilmRepository
    {
        Film GetFilmByName(string filmName); // 根据电影名称获取电影详情
        List<Film> GetAllFilms(); // 获取所有电影列表
        List<Cast> GetCastByFilmName(string filmName); // 获取指定电影的演职人员
        List<Section> GetSectionsByFilmName(string filmName); // 获取指定电影的所有场次
        MovieHall GetMovieHallByHallNo(int hallNo); // 根据影厅号获取影厅信息
        TimeSlot GetTimeSlotByID(string timeId); // 根据时段ID获取时段信息

        (decimal BoxOffice, int TicketsSold, decimal OccupancyRate) GetMovieStatistics(string filmName);  // 查询影片的排档和撤档信息，以及当前正在上映的场次。

        List<Cast> GetCastCrewDetails(string memberName); // 查询影片概况信息，包括类型、演职人员、评分、票房和当前场次。

        Film GetMovieOverview(string filmName); // 根据演职人员姓名，查询其参演的所有电影。

        (Film FilmInfo, List<Section> Sessions) GetMovieSchedulingInfo(string filmName); // 查询指定电影的数据统计信息，包括票价、总票房、已售票数和上座率。

        //管理员特权部分


        // 添加新电影
        void AddFilm(Film film);

        // 更新电影信息
        void UpdateFilm(Film film);

        // 只更新电影的平均分（避免冗余覆盖）
        void UpdateAverageScore(Film film, int newScore, int addOrSub = 1);


        // 检查电影是否存在关联场次
        bool HasRelatedSections(string filmName);
    }
}
