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

        /// <summary>
        /// 智能自动排片功能：根据电影优先级和分配策略在给定日期范围内自动创建场次。
        /// </summary>
        /// <param name="startDate">排片开始日期。</param>
        /// <param name="endDate">排片结束日期。</param>
        /// <param name="targetSessionsPerDay">每天每个影厅的目标场次数量（用于计算总目标场次）。</param>
        /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
        (bool Success, string Message) SmartAutoScheduleFilm(DateTime startDate, DateTime endDate, int targetSessionsPerDay = 3);

        /// <summary>
        /// 改进的智能自动排片功能。
        /// </summary>
        /// <param name="startDate">排片开始日期。</param>
        /// <param name="endDate">排片结束日期。</param>
        /// <param name="targetSessionsPerDay">每天每个影厅的目标场次数量。</param>
        /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
        (bool Success, string Message) ImprovedSmartAutoScheduleFilm(DateTime startDate, DateTime endDate, int targetSessionsPerDay = 3);

        /// <summary>
        /// 批量排片功能：为指定电影在给定日期范围内自动创建指定数量的场次。
        /// 此方法会尝试在找到的第一个可用时段进行排片，并优先分散到不同影厅。
        /// </summary>
        /// <param name="filmName">要排片的电影名称。</param>
        /// <param name="startDate">排片开始日期。</param>
        /// <param name="endDate">排片结束日期。</param>
        /// <param name="maxSessionsToSchedule">最多创建的场次数量。</param>
        /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
        (bool Success, string Message) BatchScheduleFilm(string filmName, DateTime startDate, DateTime endDate, int maxSessionsToSchedule);
    }
}