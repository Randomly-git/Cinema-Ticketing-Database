using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq; // 用于简化集合操作

public class SchedulingService
{
    private readonly string _connectionString;

    public SchedulingService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Film> GetAllFilms()
    {
        List<Film> films = new List<Film>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            // 修正：表名和列名使用大写以匹配Oracle数据库
            string sql = "SELECT FILMNAME FROM FILM";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        films.Add(new Film { FilmName = reader["FILMNAME"].ToString() });
                    }
                }
            }
        }
        return films;
    }

    public List<MovieHall> GetAllMovieHalls()
    {
        List<MovieHall> halls = new List<MovieHall>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            // 修正：表名和列名使用大写以匹配Oracle数据库，COLUMNS使用双引号
            string sql = "SELECT HALLNO, LINES, \"COLUMNS\", CATEGORY FROM MOVIEHALL";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        halls.Add(new MovieHall
                        {
                            HallNo = Convert.ToInt32(reader["HALLNO"]),
                            Lines = Convert.ToInt32(reader["LINES"]),
                            Columns = Convert.ToInt32(reader["COLUMNS"]), // 修正：读取COLUMNS
                            Category = reader["CATEGORY"].ToString()
                        });
                    }
                }
            }
        }
        return halls;
    }

    public List<TimeSlot> GetAllTimeSlots()
    {
        List<TimeSlot> slots = new List<TimeSlot>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            // 修正：表名和列名使用大写以匹配Oracle数据库
            string sql = "SELECT TIMEID, STARTTIME, ENDTIME FROM TIMESLOT";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Oracle的DATE或TIMESTAMP字段在C#中读取为DateTime，需要转换为TimeSpan只取时间部分
                        DateTime startTimeDt = Convert.ToDateTime(reader["STARTTIME"]);
                        DateTime endTimeDt = Convert.ToDateTime(reader["ENDTIME"]);

                        slots.Add(new TimeSlot
                        {
                            TimeID = reader["TIMEID"].ToString(),
                            StartTime = startTimeDt.TimeOfDay, // 只取时间部分
                            EndTime = endTimeDt.TimeOfDay      // 只取时间部分
                        });
                    }
                }
            }
        }
        return slots;
    }

    public (bool Success, string Message) AddSection(string filmName, int hallNo, string timeID) // 移除 scheduleDay 参数
    {
        // 1. 验证电影、影厅、时段是否存在 (通过调用辅助方法检查)
        if (!GetAllFilms().Any(f => f.FilmName == filmName))
        {
            return (false, "指定的电影不存在。");
        }
        if (!GetAllMovieHalls().Any(mh => mh.HallNo == hallNo))
        {
            return (false, "指定的影厅不存在。");
        }
        if (!GetAllTimeSlots().Any(ts => ts.TimeID == timeID))
        {
            return (false, "指定的时段不存在。");
        }

        if (IsSectionConflicting(hallNo, timeID))
        {
            return (false, "排片冲突：该影厅在该时段已有排片（临时模式下不考虑日期）。");
        }

        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            try
            {
                connection.Open();
                string getNextIdSql = "SELECT SECTION_SEQ.NEXTVAL FROM DUAL";
                int nextSectionId;
                using (OracleCommand idCommand = new OracleCommand(getNextIdSql, connection))
                {
                    nextSectionId = Convert.ToInt32(idCommand.ExecuteScalar());
                }

                string sql = "INSERT INTO SECTION (SECTIONID, FILMNAME, HALLNO, TIMEID) VALUES (:sectionID, :filmName, :hallNo, :timeID)"; // 移除 "DAY"
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("sectionID", nextSectionId));
                    command.Parameters.Add(new OracleParameter("filmName", filmName));
                    command.Parameters.Add(new OracleParameter("hallNo", hallNo));
                    command.Parameters.Add(new OracleParameter("timeID", timeID));

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return (true, $"排片成功！场次号: {nextSectionId}");
                    }
                    else
                    {
                        return (false, "排片失败：数据库未受影响。");
                    }
                }
            }
            catch (OracleException ex)
            {
                // 捕获Oracle特有的异常
                return (false, $"Oracle数据库错误: {ex.Message} (错误代码: {ex.Number})");
            }
            catch (Exception ex)
            {
                // 捕获其他通用异常
                return (false, $"添加排片时发生异常: {ex.Message}");
            }
        }
    }

    private bool IsSectionConflicting(int hallNo, string timeID) // 移除 scheduleDay 参数
    {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            // 临时修改：冲突检查只基于HALLNO和TIMEID，不考虑日期
            string sql = "SELECT COUNT(*) FROM SECTION WHERE HALLNO = :hallNo AND TIMEID = :timeID"; // 移除 "DAY" 条件
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                command.Parameters.Add(new OracleParameter("hallNo", hallNo));
                command.Parameters.Add(new OracleParameter("timeID", timeID));
                // command.Parameters.Add(new OracleParameter("day", scheduleDay.Date)); // 临时移除

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
    }

    public List<Section> GetSectionsByDateRange(DateTime startDate, DateTime endDate) // 保持参数，但在SQL中不使用
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
                    -- s.""DAY"", -- 临时移除
                    mh.CATEGORY AS HALLCATEGORY,
                    ts.STARTTIME,
                    ts.ENDTIME
                FROM
                    SECTION s
                JOIN
                    MOVIEHALL mh ON s.HALLNO = mh.HALLNO
                JOIN
                    TIMESLOT ts ON s.TIMEID = ts.TIMEID
                -- WHERE TRUNC(s.""DAY"") BETWEEN TRUNC(:startDate) AND TRUNC(:endDate) -- 临时移除
                ORDER BY
                    s.SECTIONID, ts.STARTTIME, s.HALLNO"; // 临时修改排序，不使用"DAY"

            using (OracleCommand command = new OracleCommand(sql, connection))
            {
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
                            // Day = Convert.ToDateTime(reader["DAY"]), // 临时移除
                            HallCategory = reader["HALLCATEGORY"].ToString(),
                            StartTime = Convert.ToDateTime(reader["STARTTIME"]).TimeOfDay,
                            EndTime = Convert.ToDateTime(reader["ENDTIME"]).TimeOfDay
                        });
                    }
                }
            }
        }
        return sections;
    }

    public (bool Success, string Message) DeleteSection(int sectionId)
    {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            OracleTransaction transaction = null; // 声明事务对象
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction(); // 开始事务

                string checkTicketsSql = "SELECT COUNT(*) FROM TICKET WHERE SECTIONID = :sectionID AND STATE = '已售出'";
                using (OracleCommand checkCmd = new OracleCommand(checkTicketsSql, connection))
                {
                    checkCmd.Transaction = transaction; // 将事务赋值给命令的Transaction属性
                    checkCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    int soldTicketsCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (soldTicketsCount > 0)
                    {
                        transaction.Rollback(); // 回滚事务
                        return (false, "该场次已有已售出的电影票，不允许删除排片。请先处理相关订单。");
                    }
                }

                string deleteTicketsSql = "DELETE FROM TICKET WHERE SECTIONID = :sectionID";
                using (OracleCommand deleteTicketsCmd = new OracleCommand(deleteTicketsSql, connection))
                {
                    deleteTicketsCmd.Transaction = transaction; // 将事务赋值给命令的Transaction属性
                    deleteTicketsCmd.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    deleteTicketsCmd.ExecuteNonQuery(); // 删除依赖的票务信息
                }

                string deleteSectionSql = "DELETE FROM SECTION WHERE SECTIONID = :sectionID";
                using (OracleCommand command = new OracleCommand(deleteSectionSql, connection))
                {
                    command.Transaction = transaction; // 将事务赋值给命令的Transaction属性
                    command.Parameters.Add(new OracleParameter("sectionID", sectionId));
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        transaction.Commit(); // 提交事务
                        return (true, "排片删除成功。");
                    }
                    else
                    {
                        transaction.Rollback(); // 回滚事务
                        return (false, "排片删除失败：未找到指定场次。");
                    }
                }
            }
            catch (OracleException ex)
            {
                transaction?.Rollback(); // 如果事务已开始，则回滚
                return (false, $"Oracle数据库错误: {ex.Message} (错误代码: {ex.Number})");
            }
            catch (Exception ex)
            {
                transaction?.Rollback(); // 如果事务已开始，则回滚
                return (false, $"删除排片时发生异常: {ex.Message}");
            }
        }
    }

}
