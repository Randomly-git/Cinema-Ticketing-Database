using System;
using System.Collections.Generic;
using test.Models;

/// <summary>
/// 影片排片服务接口，定义电影场次管理的核心操作。
/// </summary>
namespace test.Services
{
    public interface ISchedulingService
    {
        // --- 基础数据获取方法 ---

        /// <summary>
        /// 获取所有电影列表。
        /// </summary>
        /// <returns>电影对象列表。</returns>
        List<Film> GetAllFilms();

        /// <summary>
        /// 获取所有影厅列表。
        /// </summary>
        /// <returns>影厅对象列表。</returns>
        List<MovieHall> GetAllMovieHalls();

        // --- 核心排片操作 ---

        /// <summary>
        /// 添加新的电影场次 (排片)。
        /// </summary>
        /// <param name="filmName">电影名称。</param>
        /// <param name="hallNo">影厅号。</param>
        /// <param name="scheduleStartTime">排片开始时间。</param>
        /// <returns>操作结果和消息。</returns>
        (bool Success, string Message) AddSection(string filmName, int hallNo, DateTime scheduleStartTime);

        /// <summary>
        /// 查询指定日期范围内的所有排片。
        /// </summary>
        /// <param name="startDate">查询开始日期。</param>
        /// <param name="endDate">查询结束日期。</param>
        /// <returns>场次对象列表。</returns>
        List<Section> GetSectionsByDateRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 根据场次ID删除排片。
        /// </summary>
        /// <param name="sectionId">要删除的场次ID。</param>
        /// <returns>操作结果和消息。</returns>
        (bool Success, string Message) DeleteSection(int sectionId);

        // --- 批量排片功能 ---
        (bool Success, string Message) BatchScheduleFilm(string filmName, DateTime startDate, DateTime endDate, int maxSessionsToSchedule);

        // --- 智能自动排片功能 ---
        (bool Success, string Message) SmartAutoScheduleFilm(DateTime startDate, DateTime endDate, int targetSessionsPerDay = 3);
    }
}