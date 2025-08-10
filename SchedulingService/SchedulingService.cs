using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

/// <summary>
/// 影片排片服务类，负责管理电影场次的增删改查。
/// </summary>
public class SchedulingService
{
    private readonly string _connectionString;
    private const int BUFFER_MINUTES = 15; // 每场电影之间的缓冲时间（分钟）

    /// <summary>
    /// 构造函数，需要传入数据库连接字符串。
    /// </summary>
    /// <param name="connectionString">Oracle数据库连接字符串。</param>
    public SchedulingService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // --- 辅助方法：获取基础数据 (用于下拉列表或验证) ---

    /// <summary>
    /// 获取所有电影列表。
    /// 包含更多信息以支持智能排片。
    /// </summary>
    /// <returns>电影对象列表。</returns>
    public List<Film> GetAllFilms()
    {
        List<Film> films = new List<Film>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            string sql = "SELECT filmName, filmLength, score, admissions, boxOffice FROM film";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        films.Add(new Film
                        {
                            FilmName = reader["filmName"].ToString(),
                            FilmLength = Convert.ToInt32(reader["filmLength"]),
                            Score = reader["score"] != DBNull.Value ? Convert.ToDecimal(reader["score"]) : 0m,
                            Admissions = reader["admissions"] != DBNull.Value ? Convert.ToInt32(reader["admissions"]) : 0,
                            BoxOffice = reader["boxOffice"] != DBNull.Value ? Convert.ToDecimal(reader["boxOffice"]) : 0m
                        });
                    }
                }
            }
        }
        return films;
    }

    /// <summary>
    /// 获取所有影厅列表。
    /// </summary>
    /// <returns>影厅对象列表。</returns>
    public List<MovieHall> GetAllMovieHalls()
    {
        List<MovieHall> halls = new List<MovieHall>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            string sql = "SELECT hallNo, lines, columns, category FROM moviehall";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        halls.Add(new MovieHall
                        {
                            HallNo = Convert.ToInt32(reader["hallNo"]),
                            Lines = Convert.ToInt32(reader["lines"]),
                            Columns = Convert.ToInt32(reader["columns"]),
                            Category = reader["category"].ToString()
                        });
                    }
                }
            }
        }
        return halls;
    }

    // --- 核心排片功能 ---

    /// <summary>
    /// 手动添加新的电影场次 (排片)。
    /// 此方法会同时更新 section 和 timeslot 表。
    /// </summary>
    /// <param name="filmName">电影名称。</param>
    /// <param name="hallNo">影厅号。</param>
    /// <param name="scheduleStartTime">排片开始时间。</param>
    /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
    public (bool Success, string Message) AddSection(string filmName, int hallNo, DateTime scheduleStartTime)
    {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            OracleTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                // 1. 获取电影时长以计算结束时间
                string filmLengthSql = "SELECT filmLength FROM film WHERE filmName = :filmName";
                int filmLength;
                using (OracleCommand filmCmd = new OracleCommand(filmLengthSql, connection))
                {
                    filmCmd.Transaction = transaction;
                    filmCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    var result = filmCmd.ExecuteScalar();
                    if (result == null)
                    {
                        transaction.Rollback();
                        return (false, "指定的电影不存在。");
                    }
                    filmLength = Convert.ToInt32(result);
                }

                DateTime scheduleEndTime = scheduleStartTime.AddMinutes(filmLength);

                // 2. 检查排片冲突 (使用数据库实时检查)
                if (IsSectionConflicting(hallNo, scheduleStartTime, scheduleEndTime, connection, transaction))
                {
                    transaction.Rollback();
                    return (false, "排片冲突：该影厅在指定时段已有排片。");
                }

                // 3. 为timeslot表获取新的timeID
                string getTimeIdSeqSql = "SELECT TIMESLOT_SEQ.NEXTVAL FROM DUAL";
                int nextTimeId;
                using (OracleCommand timeIdCmd = new OracleCommand(getTimeIdSeqSql, connection))
                {
                    timeIdCmd.Transaction = transaction;
                    nextTimeId = Convert.ToInt32(timeIdCmd.ExecuteScalar());
                }
                string newTimeID = $"TS{nextTimeId}";

                // 4. 插入新记录到timeslot表
                string insertTimeslotSql = "INSERT INTO timeslot (timeID, startTime, endTime) VALUES (:timeID, :startTime, :endTime)";
                using (OracleCommand insertTimeslotCmd = new OracleCommand(insertTimeslotSql, connection))
                {
                    insertTimeslotCmd.Transaction = transaction;
                    insertTimeslotCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                    insertTimeslotCmd.Parameters.Add(new OracleParameter("startTime", scheduleStartTime));
                    insertTimeslotCmd.Parameters.Add(new OracleParameter("endTime", scheduleEndTime));
                    insertTimeslotCmd.ExecuteNonQuery();
                }

                // 5. 为section表获取新的sectionID
                string getSectionIdSeqSql = "SELECT SECTION_SEQ.NEXTVAL FROM DUAL";
                int nextSectionId;
                using (OracleCommand sectionIdCmd = new OracleCommand(getSectionIdSeqSql, connection))
                {
                    sectionIdCmd.Transaction = transaction;
                    nextSectionId = Convert.ToInt32(sectionIdCmd.ExecuteScalar());
                }

                // 6. 插入新记录到section表
                string insertSectionSql = "INSERT INTO section (sectionID, filmName, hallNo, timeID) VALUES (:sectionID, :filmName, :hallNo, :timeID)";
                using (OracleCommand insertSectionCmd = new OracleCommand(insertSectionSql, connection))
                {
                    insertSectionCmd.Transaction = transaction;
                    insertSectionCmd.Parameters.Add(new OracleParameter("sectionID", nextSectionId));
                    insertSectionCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    insertSectionCmd.Parameters.Add(new OracleParameter("hallNo", hallNo));
                    insertSectionCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                    insertSectionCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return (true, $"排片成功！场次号: {nextSectionId}, 时段号: {newTimeID}");
            }
            catch (OracleException ex)
            {
                transaction?.Rollback();
                return (false, $"Oracle数据库错误: {ex.Message} (错误代码: {ex.Number})");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return (false, $"添加排片时发生异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 检查指定影厅、时段的排片是否存在冲突 (使用数据库实时查询)。
    /// </summary>
    private bool IsSectionConflicting(int hallNo, DateTime newStartTime, DateTime newEndTime, OracleConnection connection, OracleTransaction transaction)
    {
        string sql = @"
            SELECT COUNT(*)
            FROM section s
            JOIN timeslot ts ON s.timeID = ts.timeID
            WHERE s.hallNo = :hallNo
            AND (ts.startTime < :newEndTime AND ts.endTime > :newStartTime)";

        using (OracleCommand command = new OracleCommand(sql, connection))
        {
            command.Transaction = transaction;
            command.Parameters.Add(new OracleParameter("hallNo", hallNo));
            command.Parameters.Add(new OracleParameter("newStartTime", newStartTime));
            command.Parameters.Add(new OracleParameter("newEndTime", newEndTime));
            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
    }

    /// <summary>
    /// 检查指定影厅、时段的排片是否存在冲突 (使用内存中的已排场次列表)。
    /// </summary>
    private bool IsSectionConflicting(int hallNo, DateTime newStartTime, DateTime newEndTime, List<Section> existingSectionsInHall)
    {
        return existingSectionsInHall.Any(s =>
            s.HallNo == hallNo && // 显式检查影厅号，以防万一
            (s.ScheduleStartTime < newEndTime && s.ScheduleEndTime > newStartTime)
        );
    }


    /// <summary>
    /// 查询指定日期范围内的所有排片。
    /// </summary>
    /// <param name="startDate">查询开始日期。</param>
    /// <param name="endDate">查询结束日期。</param>
    /// <returns>场次对象列表，包含关联的电影、影厅和时段信息。</returns>
    public List<Section> GetSectionsByDateRange(DateTime startDate, DateTime endDate)
    {
        List<Section> sections = new List<Section>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            string sql = @"
                SELECT
                    s.sectionID, s.filmName, s.hallNo, s.timeID,
                    mh.category AS HallCategory,
                    ts.startTime, ts.endTime
                FROM
                    section s
                JOIN
                    moviehall mh ON s.hallNo = mh.hallNo
                JOIN
                    timeslot ts ON s.timeID = ts.timeID
                WHERE
                    ts.startTime BETWEEN :startDate AND :endDate
                ORDER BY
                    ts.startTime, s.hallNo";

            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.Parameters.Add(new OracleParameter("startDate", startDate.Date));
                command.Parameters.Add(new OracleParameter("endDate", endDate.Date.AddDays(1).AddSeconds(-1))); // 查询到结束日期当天最后一秒

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sections.Add(new Section
                        {
                            SectionID = Convert.ToInt32(reader["sectionID"]),
                            FilmName = reader["filmName"].ToString(),
                            HallNo = Convert.ToInt32(reader["hallNo"]),
                            TimeID = reader["timeID"].ToString(),
                            HallCategory = reader["HallCategory"].ToString(),
                            ScheduleStartTime = Convert.ToDateTime(reader["startTime"]),
                            ScheduleEndTime = Convert.ToDateTime(reader["endTime"])
                        });
                    }
                }
            }
        }
        return sections;
    }

    /// <summary>
    /// 根据场次ID删除排片。
    /// </summary>
    /// <param name="sectionId">要删除的场次ID。</param>
    /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
    public (bool Success, string Message) DeleteSection(int sectionId)
    {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            OracleTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                // 1. 检查该场次是否有已售出的票，如果有，则不允许删除
                string checkTicketsSql = "SELECT COUNT(*) FROM ticket WHERE sectionID = :sectionID AND state = '已售出'";
                using (OracleCommand checkCmd = new OracleCommand(checkTicketsSql, connection))
                {
                    checkCmd.Transaction = transaction;
                    checkCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    int soldTicketsCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (soldTicketsCount > 0)
                    {
                        transaction.Rollback();
                        return (false, "该场次已有已售出的电影票，不允许删除排片。");
                    }
                }

                // 2. 获取对应的timeID
                string getTimeIdSql = "SELECT timeID FROM section WHERE sectionID = :sectionID";
                string timeID;
                using (OracleCommand getTimeIdCmd = new OracleCommand(getTimeIdSql, connection))
                {
                    getTimeIdCmd.Transaction = transaction;
                    getTimeIdCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    var result = getTimeIdCmd.ExecuteScalar();
                    if (result == null)
                    {
                        transaction.Rollback();
                        return (false, "排片删除失败：未找到指定场次。");
                    }
                    timeID = result.ToString();
                }

                // 3. 删除 ticket 表中依赖该场次的记录
                string deleteTicketsSql = "DELETE FROM ticket WHERE sectionID = :sectionID";
                using (OracleCommand deleteTicketsCmd = new OracleCommand(deleteTicketsSql, connection))
                {
                    deleteTicketsCmd.Transaction = transaction;
                    deleteTicketsCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    deleteTicketsCmd.ExecuteNonQuery();
                }

                // 4. 删除 section 表中的场次记录
                string deleteSectionSql = "DELETE FROM section WHERE sectionID = :sectionID";
                using (OracleCommand command = new OracleCommand(deleteSectionSql, connection))
                {
                    command.Transaction = transaction;
                    command.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    command.ExecuteNonQuery();
                }

                // 5. 删除 timeslot 表中的时段记录
                string deleteTimeslotSql = "DELETE FROM timeslot WHERE timeID = :timeID";
                using (OracleCommand deleteTimeslotCmd = new OracleCommand(deleteTimeslotSql, connection))
                {
                    deleteTimeslotCmd.Transaction = transaction;
                    deleteTimeslotCmd.Parameters.Add(new OracleParameter("timeID", timeID));
                    deleteTimeslotCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return (true, "排片删除成功。");
            }
            catch (OracleException ex)
            {
                transaction?.Rollback();
                return (false, $"Oracle数据库错误: {ex.Message} (错误代码: {ex.Number})");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return (false, $"删除排片时发生异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 批量排片功能：为指定电影在给定日期范围内自动创建指定数量的场次。
    /// 此方法会尝试在找到的第一个可用时段进行排片，并优先分散到不同影厅。
    /// </summary>
    /// <param name="filmName">要排片的电影名称。</param>
    /// <param name="startDate">排片开始日期。</param>
    /// <param name="endDate">排片结束日期。</param>
    /// <param name="maxSessionsToSchedule">最多创建的场次数量。</param>
    /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
    public (bool Success, string Message) BatchScheduleFilm(string filmName, DateTime startDate, DateTime endDate, int maxSessionsToSchedule)
    {
        List<string> scheduledMessages = new List<string>();
        int sessionsScheduledCount = 0;

        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();

            // 获取电影时长
            string filmLengthSql = "SELECT filmLength FROM film WHERE filmName = :filmName";
            int filmLength;
            using (OracleCommand filmCmd = new OracleCommand(filmLengthSql, connection))
            {
                filmCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                var result = filmCmd.ExecuteScalar();
                if (result == null)
                {
                    return (false, "指定的电影不存在，无法进行批量排片。");
                }
                filmLength = Convert.ToInt32(result);
            }

            // 获取所有影厅
            List<MovieHall> halls = GetAllMovieHalls();
            if (!halls.Any())
            {
                return (false, "数据库中没有可用的影厅，无法进行批量排片。");
            }

            // 预加载指定日期范围内的所有现有排片，按影厅分组，以便进行内存冲突检查
            Dictionary<int, List<Section>> existingSectionsByHall = new Dictionary<int, List<Section>>();
            List<Section> allExistingSections = GetSectionsByDateRange(startDate, endDate);
            foreach (var hall in halls)
            {
                existingSectionsByHall[hall.HallNo] = allExistingSections.Where(s => s.HallNo == hall.HallNo).ToList();
            }

            // 遍历日期范围
            for (DateTime currentDay = startDate.Date; currentDay <= endDate.Date; currentDay = currentDay.AddDays(1))
            {
                // 定义每天的营业时间范围
                DateTime startOfDayOperatingHours = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 9, 0, 0); // 9:00 AM
                DateTime endOfDayOperatingHours = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 23, 0, 0); // 11:00 PM

                // 初始化当天排片的起始时间点
                DateTime currentTimePointer = startOfDayOperatingHours;

                // 在当前日期内循环，尝试在不同的影厅排片
                // 这个循环条件保证了在达到最大场次或超出营业时间前持续尝试排片
                while (sessionsScheduledCount < maxSessionsToSchedule && currentTimePointer < endOfDayOperatingHours)
                {
                    bool scheduledInThisPass = false; // 标记本轮（所有影厅）是否有任何影厅成功排片

                    // 优先在不同影厅排片 (轮询所有影厅)
                    foreach (var hall in halls)
                    {
                        if (sessionsScheduledCount >= maxSessionsToSchedule) break; // 如果已达到目标数量，立即退出

                        DateTime potentialStartTime = currentTimePointer;
                        DateTime potentialEndTime = potentialStartTime.AddMinutes(filmLength + BUFFER_MINUTES); // 考虑影片时长和缓冲

                        // 如果潜在结束时间超出营业时间，则跳过此影厅的此时间段
                        if (potentialEndTime > endOfDayOperatingHours || potentialEndTime.Date > currentDay.Date)
                        {
                            continue;
                        }

                        // 使用内存中的数据进行冲突检查
                        if (!IsSectionConflicting(hall.HallNo, potentialStartTime, potentialEndTime, existingSectionsByHall[hall.HallNo]))
                        {
                            OracleTransaction transaction = null;
                            try
                            {
                                transaction = connection.BeginTransaction();

                                // 为timeslot表获取新的timeID
                                string getTimeIdSeqSql = "SELECT TIMESLOT_SEQ.NEXTVAL FROM DUAL";
                                int nextTimeId;
                                using (OracleCommand timeIdCmd = new OracleCommand(getTimeIdSeqSql, connection))
                                {
                                    timeIdCmd.Transaction = transaction;
                                    nextTimeId = Convert.ToInt32(timeIdCmd.ExecuteScalar());
                                }
                                string newTimeID = $"TS{nextTimeId}";

                                // 插入新记录到timeslot表
                                string insertTimeslotSql = "INSERT INTO timeslot (timeID, startTime, endTime) VALUES (:timeID, :startTime, :endTime)";
                                using (OracleCommand insertTimeslotCmd = new OracleCommand(insertTimeslotSql, connection))
                                {
                                    insertTimeslotCmd.Transaction = transaction;
                                    insertTimeslotCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                                    insertTimeslotCmd.Parameters.Add(new OracleParameter("startTime", potentialStartTime));
                                    insertTimeslotCmd.Parameters.Add(new OracleParameter("endTime", potentialEndTime));
                                    insertTimeslotCmd.ExecuteNonQuery();
                                }

                                // 为section表获取新的sectionID
                                string getSectionIdSeqSql = "SELECT SECTION_SEQ.NEXTVAL FROM DUAL";
                                int nextSectionId;
                                using (OracleCommand sectionIdCmd = new OracleCommand(getSectionIdSeqSql, connection))
                                {
                                    sectionIdCmd.Transaction = transaction;
                                    nextSectionId = Convert.ToInt32(sectionIdCmd.ExecuteScalar());
                                }

                                // 插入新记录到section表
                                string insertSectionSql = "INSERT INTO section (sectionID, filmName, hallNo, timeID) VALUES (:sectionID, :filmName, :hallNo, :timeID)";
                                using (OracleCommand insertSectionCmd = new OracleCommand(insertSectionSql, connection))
                                {
                                    insertSectionCmd.Transaction = transaction;
                                    insertSectionCmd.Parameters.Add(new OracleParameter("sectionID", nextSectionId));
                                    insertSectionCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                                    insertSectionCmd.Parameters.Add(new OracleParameter("hallNo", hall.HallNo));
                                    insertSectionCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                                    insertSectionCmd.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                scheduledMessages.Add($"成功批量排片: 电影 '{filmName}', 影厅 {hall.HallNo}, 开始时间 {potentialStartTime:yyyy-MM-dd HH:mm}");
                                sessionsScheduledCount++;
                                scheduledInThisPass = true; // 标记本轮有排片成功

                                // 立即更新内存中的排片列表，确保后续冲突检查的准确性
                                existingSectionsByHall[hall.HallNo].Add(new Section
                                {
                                    SectionID = nextSectionId,
                                    FilmName = filmName,
                                    HallNo = hall.HallNo,
                                    TimeID = newTimeID,
                                    ScheduleStartTime = potentialStartTime,
                                    ScheduleEndTime = potentialEndTime
                                });
                            }
                            catch (Exception ex)
                            {
                                transaction?.Rollback(); // 如果事务已开始，则回滚
                                scheduledMessages.Add($"批量排片失败: 影厅 {hall.HallNo}, 时间 {potentialStartTime:yyyy-MM-dd HH:mm} - {ex.Message}");
                            }
                        }
                    }

                    // 如果本轮（遍历所有影厅后）没有任何排片成功，那么推进时间，避免死循环
                    if (!scheduledInThisPass)
                    {
                        currentTimePointer = currentTimePointer.AddMinutes(BUFFER_MINUTES);
                    }
                    else
                    {
                        // 如果本轮有成功排片，将时间推进到所有成功排片的最晚结束时间 + 缓冲时间，或者简单推进 BUFFER_MINUTES
                        // 简单推进 BUFFER_MINUTES 在轮询影厅时效果可能更好，因为它允许在下一轮继续尝试所有影厅
                        currentTimePointer = currentTimePointer.AddMinutes(BUFFER_MINUTES);
                    }
                }
            }
        }

        if (sessionsScheduledCount == 0)
        {
            return (false, $"未能在指定日期范围内为电影 '{filmName}' 找到可用的排片时段。");
        }
        else
        {
            return (true, $"批量排片完成。共排片 {sessionsScheduledCount} 场。\n" + string.Join("\n", scheduledMessages));
        }
    }

    /// <summary>
    /// 智能自动排片功能：根据电影优先级和分配策略在给定日期范围内自动创建场次。
    /// </summary>
    /// <param name="startDate">排片开始日期。</param>
    /// <param name="endDate">排片结束日期。</param>
    /// <param name="targetSessionsPerDay">每天每个影厅的目标场次数量（用于计算总目标场次）。</param>
    /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
    public (bool Success, string Message) SmartAutoScheduleFilm(DateTime startDate, DateTime endDate, int targetSessionsPerDay = 3)
    {
        List<string> scheduledMessages = new List<string>();
        int totalSessionsScheduled = 0;

        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();

            // 1. 获取所有电影，并按评分和观影人次降序排序 (优先级最高在前面)
            var films = GetAllFilms()
                        .OrderByDescending(f => f.Score)
                        .ThenByDescending(f => f.Admissions)
                        .ToList();

            // 检查电影列表是否为空，如果为空则直接返回
            if (!films.Any())
            {
                return (false, "数据库中没有可用的电影，无法进行智能自动排片。");
            }

            // 2. 获取所有影厅
            List<MovieHall> halls = GetAllMovieHalls();
            if (!halls.Any())
            {
                return (false, "数据库中没有可用的影厅，无法进行智能自动排片。");
            }

            // 3. 计算总目标场次 (S)
            int totalDays = (endDate.Date - startDate.Date).Days + 1;
            int totalTargetSessions = totalDays * halls.Count * targetSessionsPerDay;

            if (totalTargetSessions <= 0)
            {
                return (true, "没有目标场次或日期范围无效，无需排片。");
            }

            // 4. 根据策略分配每部电影的目标场次数量 (filmTargetAllocation)
            Dictionary<string, int> filmTargetAllocation = films.ToDictionary(f => f.FilmName, f => 0);
            int filmsCount = films.Count;

            double ratio = (double)totalTargetSessions / filmsCount;

            if (ratio <= 1.6) // 策略 1: 总数÷电影总数<=1.6
            {
                int remainingSessionsToAllocate = totalTargetSessions;
                foreach (var film in films) // 遍历优先级最高的电影
                {
                    if (remainingSessionsToAllocate <= 0) break;

                    int sessionsForCurrentFilm = (int)Math.Floor(remainingSessionsToAllocate / 2.0);
                    // 确保即使取整为0，只要有剩余场次就至少分配1场给高优先级电影
                    if (sessionsForCurrentFilm == 0 && remainingSessionsToAllocate > 0)
                    {
                        sessionsForCurrentFilm = 1;
                    }
                    sessionsForCurrentFilm = Math.Min(sessionsForCurrentFilm, remainingSessionsToAllocate); // 不分配超过剩余的

                    filmTargetAllocation[film.FilmName] += sessionsForCurrentFilm;
                    remainingSessionsToAllocate -= sessionsForCurrentFilm;
                }

                // 分配因 Math.Floor 产生的剩余零头，优先给高优先级电影
                int leftover = totalTargetSessions - filmTargetAllocation.Sum(kv => kv.Value);
                int currentFilmIndexForLeftover = 0;
                while (leftover > 0 && currentFilmIndexForLeftover < filmsCount)
                {
                    filmTargetAllocation[films[currentFilmIndexForLeftover].FilmName]++;
                    leftover--;
                    currentFilmIndexForLeftover++;
                }
                // 如果一轮后还有剩余（例如，目标场次很高，电影数量很少，每部都加了1场但还不够），则继续轮询
                currentFilmIndexForLeftover = 0; // 重置索引以便再次轮询
                while (leftover > 0)
                {
                    filmTargetAllocation[films[currentFilmIndexForLeftover % filmsCount].FilmName]++;
                    leftover--;
                    currentFilmIndexForLeftover++;
                }
            }
            else // 策略 2: 总数÷电影总数>1.6
            {
                double n_cycles_double = ratio / 1.6;
                int n_cycles = (int)Math.Floor(n_cycles_double);

                int s_base = (int)Math.Floor(1.6 * n_cycles * filmsCount);
                int s_remainder = totalTargetSessions - s_base; // 这是 'm' 部分

                // 先按照 1/2 策略分配 S_base 部分
                int remainingSessionsForBase = s_base;
                foreach (var film in films)
                {
                    if (remainingSessionsForBase <= 0) break;

                    int sessionsForCurrentFilm = (int)Math.Floor(remainingSessionsForBase / 2.0);
                    if (sessionsForCurrentFilm == 0 && remainingSessionsForBase > 0)
                    {
                        sessionsForCurrentFilm = 1;
                    }
                    sessionsForCurrentFilm = Math.Min(sessionsForCurrentFilm, remainingSessionsForBase);

                    filmTargetAllocation[film.FilmName] += sessionsForCurrentFilm;
                    remainingSessionsForBase -= sessionsForCurrentFilm;
                }

                // 将因 S_base 分配造成的零头也加入 s_remainder 进行统一处理
                int currentLeftoverFromBase = s_base - filmTargetAllocation.Sum(kv => kv.Value);
                s_remainder += currentLeftoverFromBase;

                // 将 s_remainder (剩余部分) 均分，优先分给此次智能排片中排片数最少的电影
                var filmsSortedByCurrentAllocation = filmTargetAllocation.OrderBy(kv => kv.Value).ToList();
                int idx = 0;
                while (s_remainder > 0)
                {
                    // 轮询分配给当前分配场次最少的电影
                    if (idx >= filmsSortedByCurrentAllocation.Count) idx = 0; // 确保索引不越界，循环分配
                    filmTargetAllocation[filmsSortedByCurrentAllocation[idx].Key]++;
                    s_remainder--;
                    idx++;
                }
            }


            // 5. 实际排片执行循环
            // filmActualScheduledCount 用于跟踪每部电影实际已排的场次，以确保不超过 filmTargetAllocation
            Dictionary<string, int> filmActualScheduledCount = films.ToDictionary(f => f.FilmName, f => 0);

            // 预加载指定日期范围内的所有现有排片，按影厅分组，以便进行内存冲突检查
            Dictionary<int, List<Section>> existingSectionsByHall = new Dictionary<int, List<Section>>();
            List<Section> allExistingSections = GetSectionsByDateRange(startDate, endDate);
            foreach (var hall in halls)
            {
                existingSectionsByHall[hall.HallNo] = allExistingSections.Where(s => s.HallNo == hall.HallNo).ToList();
            }

            // 新增：用于在实际排片时轮询电影列表的索引，以打乱排片顺序
            int currentFilmRoundRobinIndex = 0;

            // 遍历日期范围
            for (DateTime currentDay = startDate.Date; currentDay <= endDate.Date; currentDay = currentDay.AddDays(1))
            {
                // 遍历每个影厅
                foreach (var hall in halls)
                {
                    int sessionsScheduledInHallToday = 0; // 跟踪当前影厅当天已排场次
                    DateTime currentSlotStartTime = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 9, 0, 0); // 每天从9点开始尝试
                    DateTime endOfDayOperatingHours = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 23, 0, 0); // 每天到23点结束

                    // 尝试在当前影厅的当前日期内排片，直到达到目标数量或没有更多空间
                    while (sessionsScheduledInHallToday < targetSessionsPerDay && currentSlotStartTime < endOfDayOperatingHours)
                    {
                        bool slotFilled = false;
                        DateTime potentialStartTime = currentSlotStartTime;
                        DateTime potentialEndTime = default(DateTime); // 初始化 potentialEndTime
                        Film selectedFilmForSlot = null; // 初始化 selectedFilmForSlot

                        // 寻找合适的电影填充当前时段，使用轮询机制打乱顺序
                        for (int i = 0; i < films.Count; i++)
                        {
                            // 计算当前轮次要尝试的电影索引 (循环索引)
                            int filmIndexToTry = (currentFilmRoundRobinIndex + i) % films.Count;
                            var film = films[filmIndexToTry];

                            // 检查该电影是否还有需要排的场次
                            if (filmActualScheduledCount[film.FilmName] < filmTargetAllocation[film.FilmName])
                            {
                                potentialEndTime = potentialStartTime.AddMinutes(film.FilmLength); // 赋值

                                // 检查潜在结束时间是否超出当天营业时间
                                if (potentialEndTime > endOfDayOperatingHours)
                                {
                                    continue; // 电影太长或时间太晚，跳过此电影，尝试下一部
                                }

                                // 检查与现有排片是否冲突
                                if (!IsSectionConflicting(hall.HallNo, potentialStartTime, potentialEndTime, existingSectionsByHall[hall.HallNo]))
                                {
                                    selectedFilmForSlot = film; // 找到合适的电影
                                    slotFilled = true; // 标记已找到并排片

                                    // 更新轮询索引：从刚刚排片成功的电影的下一部开始
                                    currentFilmRoundRobinIndex = (filmIndexToTry + 1) % films.Count;
                                    break; // 跳出电影循环，准备排片
                                }
                            }
                        }

                        // 如果找到了合适的电影并可以排片
                        if (slotFilled && selectedFilmForSlot != null)
                        {
                            OracleTransaction transaction = null;
                            try
                            {
                                transaction = connection.BeginTransaction();

                                // 获取新的TimeID和SectionID
                                string getTimeIdSeqSql = "SELECT TIMESLOT_SEQ.NEXTVAL FROM DUAL";
                                int nextTimeId;
                                using (OracleCommand timeIdCmd = new OracleCommand(getTimeIdSeqSql, connection))
                                {
                                    timeIdCmd.Transaction = transaction;
                                    nextTimeId = Convert.ToInt32(timeIdCmd.ExecuteScalar());
                                }
                                string newTimeID = $"TS{nextTimeId}";

                                // 插入timeslot表
                                string insertTimeslotSql = "INSERT INTO timeslot (timeID, startTime, endTime) VALUES (:timeID, :startTime, :endTime)";
                                using (OracleCommand insertTimeslotCmd = new OracleCommand(insertTimeslotSql, connection))
                                {
                                    insertTimeslotCmd.Transaction = transaction;
                                    insertTimeslotCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                                    insertTimeslotCmd.Parameters.Add(new OracleParameter("startTime", potentialStartTime));
                                    insertTimeslotCmd.Parameters.Add(new OracleParameter("endTime", potentialEndTime));
                                    insertTimeslotCmd.ExecuteNonQuery();
                                }

                                // 插入section表
                                string getSectionIdSeqSql = "SELECT SECTION_SEQ.NEXTVAL FROM DUAL";
                                int nextSectionId;
                                using (OracleCommand sectionIdCmd = new OracleCommand(getSectionIdSeqSql, connection))
                                {
                                    sectionIdCmd.Transaction = transaction;
                                    nextSectionId = Convert.ToInt32(sectionIdCmd.ExecuteScalar());
                                }

                                string insertSectionSql = "INSERT INTO section (sectionID, filmName, hallNo, timeID) VALUES (:sectionID, :filmName, :hallNo, :timeID)";
                                using (OracleCommand insertSectionCmd = new OracleCommand(insertSectionSql, connection))
                                {
                                    insertSectionCmd.Transaction = transaction;
                                    insertSectionCmd.Parameters.Add(new OracleParameter("sectionID", nextSectionId));
                                    insertSectionCmd.Parameters.Add(new OracleParameter("filmName", selectedFilmForSlot.FilmName));
                                    insertSectionCmd.Parameters.Add(new OracleParameter("hallNo", hall.HallNo));
                                    insertSectionCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                                    insertSectionCmd.ExecuteNonQuery();
                                }

                                transaction.Commit(); // 提交事务
                                scheduledMessages.Add($"成功排片 (智能): 电影 '{selectedFilmForSlot.FilmName}', 影厅 {hall.HallNo}, 开始时间 {potentialStartTime:yyyy-MM-dd HH:mm}");
                                totalSessionsScheduled++;
                                sessionsScheduledInHallToday++;
                                filmActualScheduledCount[selectedFilmForSlot.FilmName]++; // 实际排片计数加1

                                // 立即更新内存中的排片列表，确保后续冲突检查的准确性
                                existingSectionsByHall[hall.HallNo].Add(new Section
                                {
                                    SectionID = nextSectionId,
                                    FilmName = selectedFilmForSlot.FilmName,
                                    HallNo = hall.HallNo,
                                    TimeID = newTimeID,
                                    ScheduleStartTime = potentialStartTime,
                                    ScheduleEndTime = potentialEndTime
                                });
                            }
                            catch (Exception ex)
                            {
                                transaction?.Rollback(); // 如果事务已开始，则回滚
                                scheduledMessages.Add($"智能排片失败: 电影 '{selectedFilmForSlot.FilmName}', 影厅 {hall.HallNo}, 时间 {potentialStartTime:yyyy-MM-dd HH:mm} - {ex.Message}");
                            }
                        }

                        // 移动到下一个潜在的开始时间，考虑缓冲时间
                        if (slotFilled && selectedFilmForSlot != null)
                        {
                            currentSlotStartTime = potentialEndTime.AddMinutes(BUFFER_MINUTES); // 使用 potentialEndTime
                        }
                        else
                        {
                            currentSlotStartTime = currentSlotStartTime.AddMinutes(BUFFER_MINUTES); // 如果当前时段没有电影能排，则尝试下一个固定间隔时间
                        }
                    }
                }
            }
        }

        if (totalSessionsScheduled == 0)
        {
            return (false, $"未能在指定日期范围内找到可用的排片时段进行智能自动排片。");
        }
        else
        {
            return (true, $"智能自动排片完成。共排片 {totalSessionsScheduled} 场。\n" + string.Join("\n", scheduledMessages));
        }
    }
}
