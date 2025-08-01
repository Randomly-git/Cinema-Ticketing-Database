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
    /// </summary>
    /// <returns>电影对象列表。</returns>
    public List<Film> GetAllFilms()
    {
        List<Film> films = new List<Film>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            string sql = "SELECT filmName, filmLength FROM film";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        films.Add(new Film
                        {
                            FilmName = reader["filmName"].ToString(),
                            FilmLength = Convert.ToInt32(reader["filmLength"])
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
    /// 添加新的电影场次 (排片)。
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
                        return (false, "指定的电影不存在。");
                    }
                    filmLength = Convert.ToInt32(result);
                }

                DateTime scheduleEndTime = scheduleStartTime.AddMinutes(filmLength);

                // 2. 检查排片冲突
                if (IsSectionConflicting(hallNo, scheduleStartTime, scheduleEndTime, connection, transaction))
                {
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
    /// 检查指定影厅、时段的排片是否存在冲突。
    /// </summary>
    /// <param name="hallNo">影厅号。</param>
    /// <param name="newStartTime">新排片的开始时间。</param>
    /// <param name="newEndTime">新排片的结束时间。</param>
    /// <param name="connection">当前数据库连接。</param>
    /// <param name="transaction">当前事务。</param>
    /// <returns>如果存在冲突则返回true，否则返回false。</returns>
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
                command.Parameters.Add(new OracleParameter("endDate", endDate.Date.AddDays(1).AddSeconds(-1)));

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
}
