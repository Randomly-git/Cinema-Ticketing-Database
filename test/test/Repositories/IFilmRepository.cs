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


        //管理员特权部分
       

        // 添加新电影
        void AddFilm(Film film);

        // 更新电影信息
        void UpdateFilm(Film film);


        // 检查电影是否存在关联场次
        bool HasRelatedSections(string filmName);

    }
}
