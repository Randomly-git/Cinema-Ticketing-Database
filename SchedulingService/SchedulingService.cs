using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq; // 用于简化集合操作

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
            string sql = "SELECT FILMNAME, FILMLENGTH FROM FILM";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.BindByName = true; // 显式按名称绑定
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 健壮性检查：处理 FILMLENGTH 可能为 DBNull 的情况
                        int filmLength = reader["FILMLENGTH"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FILMLENGTH"]);
                        films.Add(new Film
                        {
                            FilmName = reader["FILMNAME"].ToString(),
                            FilmLength = filmLength
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
            string sql = "SELECT HALLNO, LINES, \"COLUMNS\", CATEGORY FROM MOVIEHALL";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.BindByName = true; // 显式按名称绑定
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        halls.Add(new MovieHall
                        {
                            HallNo = Convert.ToInt32(reader["HALLNO"]),
                            Lines = Convert.ToInt32(reader["LINES"]),
                            Columns = Convert.ToInt32(reader["COLUMNS"]),
                            Category = reader["CATEGORY"].ToString()
                        });
                    }
                }
            }
        }
        return halls;
    }

    /// <summary>
    /// 获取所有时段列表。此方法现在返回TIMESLOT表中所有具体的日期时间时段。
    /// </summary>
    /// <returns>时段对象列表。</returns>
    public List<TimeSlot> GetAllTimeSlots()
    {
        List<TimeSlot> slots = new List<TimeSlot>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            string sql = "SELECT TIMEID, STARTTIME, ENDTIME FROM TIMESLOT ORDER BY STARTTIME";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.BindByName = true; // 显式按名称绑定
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        slots.Add(new TimeSlot
                        {
                            TimeID = reader["TIMEID"].ToString(),
                            StartTime = Convert.ToDateTime(reader["STARTTIME"]),
                            EndTime = Convert.ToDateTime(reader["ENDTIME"])
                        });
                    }
                }
            }
        }
        return slots;
    }

    // --- 核心排片功能 ---

    /// <summary>
    /// 添加新的电影场次 (排片)。
    /// </summary>
    /// <param name="filmName">电影名称。</param>
    /// <param name="hallNo">影厅号。</param>
    /// <param name="scheduleStartTime">场次开始的完整日期和时间。</param>
    /// <param name="scheduleEndTime">场次结束的完整日期和时间。</param>
    /// <returns>一个元组，表示操作是否成功以及相应的消息。</returns>
    public (bool Success, string Message) AddSection(string filmName, int hallNo, DateTime scheduleStartTime, DateTime scheduleEndTime)
    {
        // 1. 验证电影、影厅是否存在
        if (!GetAllFilms().Any(f => f.FilmName == filmName))
        {
            return (false, "指定的电影不存在。");
        }
        if (!GetAllMovieHalls().Any(mh => mh.HallNo == hallNo))
        {
            return (false, "指定的影厅不存在。");
        }
        // 2. 验证时间范围是否有效 (结束时间必须晚于开始时间)
        if (scheduleStartTime >= scheduleEndTime)
        {
            return (false, "开始时间必须早于结束时间。");
        }

        // 3. 检查排片冲突 (同一个影厅在同一天的时间段不能重叠)
        if (IsSectionConflicting(hallNo, scheduleStartTime, scheduleEndTime))
        {
            return (false, "排片冲突：该影厅在指定日期和时间段已有排片。");
        }

        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            OracleTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                // 4. 生成新的 TIMEID 并插入到 TIMESLOT 表
                string getNextTimeIdSql = "SELECT TIMESLOT_SEQ.NEXTVAL FROM DUAL";
                string newTimeID;
                using (OracleCommand timeIdCommand = new OracleCommand(getNextTimeIdSql, connection))
                {
                    timeIdCommand.Transaction = transaction;
                    timeIdCommand.BindByName = true; // 显式按名称绑定
                    newTimeID = Convert.ToInt32(timeIdCommand.ExecuteScalar()).ToString();
                }

                string insertTimeSlotSql = "INSERT INTO TIMESLOT (TIMEID, STARTTIME, ENDTIME) VALUES (:timeID, :startTime, :endTime)";
                using (OracleCommand insertTimeSlotCmd = new OracleCommand(insertTimeSlotSql, connection))
                {
                    insertTimeSlotCmd.Transaction = transaction;
                    insertTimeSlotCmd.BindByName = true; // 显式按名称绑定
                    insertTimeSlotCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                    insertTimeSlotCmd.Parameters.Add(new OracleParameter("startTime", scheduleStartTime));
                    insertTimeSlotCmd.Parameters.Add(new OracleParameter("endTime", scheduleEndTime));
                    insertTimeSlotCmd.ExecuteNonQuery();
                }

                // 5. 获取新的 SECTIONID
                string getNextSectionIdSql = "SELECT SECTION_SEQ.NEXTVAL FROM DUAL";
                int nextSectionId;
                using (OracleCommand sectionIdCommand = new OracleCommand(getNextSectionIdSql, connection))
                {
                    sectionIdCommand.Transaction = transaction;
                    sectionIdCommand.BindByName = true; // 显式按名称绑定
                    nextSectionId = Convert.ToInt32(sectionIdCommand.ExecuteScalar());
                }

                // 6. 插入新的场次记录到 SECTION 表，引用新的 TIMEID
                string insertSectionSql = "INSERT INTO SECTION (SECTIONID, FILMNAME, HALLNO, TIMEID) VALUES (:sectionID, :filmName, :hallNo, :timeID)";
                using (OracleCommand insertSectionCmd = new OracleCommand(insertSectionSql, connection))
                {
                    insertSectionCmd.Transaction = transaction;
                    insertSectionCmd.BindByName = true; // 显式按名称绑定
                    insertSectionCmd.Parameters.Add(new OracleParameter("sectionID", nextSectionId));
                    insertSectionCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    insertSectionCmd.Parameters.Add(new OracleParameter("hallNo", hallNo));
                    insertSectionCmd.Parameters.Add(new OracleParameter("timeID", newTimeID));
                    insertSectionCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return (true, $"排片成功！场次号: {nextSectionId}, 时间ID: {newTimeID}");
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
    /// 检查指定影厅在特定日期和时间段是否存在排片冲突。
    /// 冲突逻辑：新时间段与现有时间段是否有重叠。
    /// </summary>
    private bool IsSectionConflicting(int hallNo, DateTime newStartTime, DateTime newEndTime)
    {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            string sql = @"
                SELECT COUNT(*)
                FROM SECTION s
                JOIN TIMESLOT ts ON s.TIMEID = ts.TIMEID
                WHERE s.HALLNO = :hallNo
                AND TRUNC(ts.STARTTIME) = TRUNC(:newStartTime)
                AND (
                    (:newStartTime < ts.ENDTIME AND :newEndTime > ts.STARTTIME)
                )";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.BindByName = true; // 显式按名称绑定
                command.Parameters.Add(new OracleParameter("hallNo", hallNo));
                command.Parameters.Add(new OracleParameter("newStartTime", newStartTime));
                command.Parameters.Add(new OracleParameter("newEndTime", newEndTime));

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
    }

    /// <summary>
    /// 查询指定日期范围内的所有排片。
    /// 现在将联接TIMESLOT表以获取具体的开始和结束时间。
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
                    s.SECTIONID,
                    s.FILMNAME,
                    s.HALLNO,
                    s.TIMEID,
                    mh.CATEGORY AS HALLCATEGORY,
                    ts.STARTTIME AS SCHEDULESTARTTIME,
                    ts.ENDTIME AS SCHEDULEENDTIME
                FROM
                    SECTION s
                JOIN
                    MOVIEHALL mh ON s.HALLNO = mh.HALLNO
                JOIN
                    TIMESLOT ts ON s.TIMEID = ts.TIMEID
                WHERE
                    TRUNC(ts.STARTTIME) BETWEEN TRUNC(:startDate) AND TRUNC(:endDate)
                ORDER BY
                    ts.STARTTIME, s.HALLNO, s.SECTIONID";

            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.BindByName = true; // 显式按名称绑定
                command.Parameters.Add(new OracleParameter("startDate", startDate.Date));
                command.Parameters.Add(new OracleParameter("endDate", endDate.Date));

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sections.Add(new Section
                        {
                            SectionID = Convert.ToInt32(reader["SECTIONID"]),
                            FilmName = reader["FILMNAME"].ToString(),
                            HallNo = Convert.ToInt32(reader["HALLNO"]),
                            TimeID = reader["TIMEID"].ToString(),
                            HallCategory = reader["HALLCATEGORY"].ToString(),
                            ScheduleStartTime = Convert.ToDateTime(reader["SCHEDULESTARTTIME"]),
                            ScheduleEndTime = Convert.ToDateTime(reader["SCHEDULEENDTIME"])
                        });
                    }
                }
            }
        }
        return sections;
    }

    /// <summary>
    /// 根据场次ID删除排片。
    /// 现在会删除SECTION和对应的TIMESLOT记录。
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
                string checkTicketsSql = "SELECT COUNT(*) FROM TICKET WHERE SECTIONID = :sectionID AND STATE = '已售出'";
                using (OracleCommand checkCmd = new OracleCommand(checkTicketsSql, connection))
                {
                    checkCmd.Transaction = transaction;
                    checkCmd.BindByName = true; // 显式按名称绑定
                    checkCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    int soldTicketsCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (soldTicketsCount > 0)
                    {
                        transaction.Rollback();
                        return (false, "该场次已有已售出的电影票，不允许删除排片。请先处理相关订单。");
                    }
                }

                // 2. 获取对应的 TIMEID
                string getTimeIdSql = "SELECT TIMEID FROM SECTION WHERE SECTIONID = :sectionID";
                string timeIdToDelete = null;
                using (OracleCommand getTimeIdCmd = new OracleCommand(getTimeIdSql, connection))
                {
                    getTimeIdCmd.Transaction = transaction;
                    getTimeIdCmd.BindByName = true; // 显式按名称绑定
                    getTimeIdCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    object result = getTimeIdCmd.ExecuteScalar();
                    if (result != null)
                    {
                        timeIdToDelete = result.ToString();
                    }
                }

                if (timeIdToDelete == null)
                {
                    transaction.Rollback();
                    return (false, "删除失败：未找到指定场次对应的时段信息。");
                }

                // 3. 删除 TICKET 表中依赖该 SECTIONID 的记录（如果存在未售出的）
                string deleteTicketsSql = "DELETE FROM TICKET WHERE SECTIONID = :sectionID";
                using (OracleCommand deleteTicketsCmd = new OracleCommand(deleteTicketsSql, connection))
                {
                    deleteTicketsCmd.Transaction = transaction;
                    deleteTicketsCmd.BindByName = true; // 显式按名称绑定
                    deleteTicketsCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    deleteTicketsCmd.ExecuteNonQuery();
                }

                // 4. 删除 SECTION 表中的场次记录
                string deleteSectionSql = "DELETE FROM SECTION WHERE SECTIONID = :sectionID";
                using (OracleCommand deleteSectionCmd = new OracleCommand(deleteSectionSql, connection))
                {
                    deleteSectionCmd.Transaction = transaction;
                    deleteSectionCmd.BindByName = true; // 显式按名称绑定
                    deleteSectionCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    int rowsAffected = deleteSectionCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // 5. 删除 TIMESLOT 表中对应的记录 (假设TIMEID是SECTION独有的)
                        string deleteTimeSlotSql = "DELETE FROM TIMESLOT WHERE TIMEID = :timeID";
                        using (OracleCommand deleteTimeSlotCmd = new OracleCommand(deleteTimeSlotSql, connection))
                        {
                            deleteTimeSlotCmd.Transaction = transaction;
                            deleteTimeSlotCmd.BindByName = true; // 显式按名称绑定
                            deleteTimeSlotCmd.Parameters.Add(new OracleParameter("timeID", timeIdToDelete));
                            deleteTimeSlotCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return (true, "排片删除成功。");
                    }
                    else
                    {
                        transaction.Rollback();
                        return (false, "排片删除失败：未找到指定场次。");
                    }
                }
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
