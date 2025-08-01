using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 影片查询服务，负责封装所有与影片信息、统计相关的数据库查询操作。
/// </summary>
public class QueryService
{
    private readonly string _connectionString;

    public QueryService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 查询影片的排档和撤档信息，以及当前正在上映的场次。
    /// </summary>
    public (Film FilmInfo, List<Section> Sessions) GetMovieSchedulingInfo(string filmName)
    {
        Film film = null;
        List<Section> sessions = new List<Section>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            try
            {
                connection.Open();

                // 1. 从 film 表获取影片的排档信息
                string filmSql = "SELECT filmName, releaseDate, endDate FROM film WHERE filmName = :filmName";
                using (OracleCommand filmCmd = new OracleCommand(filmSql, connection))
                {
                    filmCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (OracleDataReader reader = filmCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            film = new Film
                            {
                                FilmName = reader["filmName"].ToString(),
                                ReleaseDate = reader["releaseDate"] as DateTime?,
                                EndDate = reader["endDate"] as DateTime?
                            };
                        }
                    }
                }

                if (film == null) return (null, null);

                // 2. 获取所有场次信息
                string sessionsSql = @"
                    SELECT
                        s.sectionID, s.filmName, s.hallNo, s.timeID,
                        mh.category AS HallCategory,
                        ts.startTime, ts.endTime
                    FROM
                        section s
                    JOIN
                        timeslot ts ON s.timeID = ts.timeID
                    JOIN
                        moviehall mh ON s.hallNo = mh.hallNo
                    WHERE
                        s.filmName = :filmName
                    ORDER BY
                        ts.startTime";
                using (OracleCommand sessionsCmd = new OracleCommand(sessionsSql, connection))
                {
                    sessionsCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (OracleDataReader reader = sessionsCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sessions.Add(new Section
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
            catch (Exception ex)
            {
                Console.WriteLine($"查询影片排档信息时发生错误: {ex.Message}");
                return (null, null);
            }
        }
        return (film, sessions);
    }

    /// <summary>
    /// 查询影片概况信息，包括类型、演职人员、评分、票房和当前场次。
    /// </summary>
    public Film GetMovieOverview(string filmName)
    {
        Film film = null;
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            try
            {
                connection.Open();

                // 1. 获取基本影片信息、票价和评分
                string filmSql = "SELECT * FROM film WHERE filmName = :filmName";
                using (OracleCommand filmCmd = new OracleCommand(filmSql, connection))
                {
                    filmCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (OracleDataReader reader = filmCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            film = new Film
                            {
                                FilmName = reader["filmName"].ToString(),
                                Genre = reader["genre"].ToString(),
                                FilmLength = Convert.ToInt32(reader["filmLength"]),
                                NormalPrice = Convert.ToDecimal(reader["normalPrice"]),
                                ReleaseDate = reader["releaseDate"] as DateTime?,
                                EndDate = reader["endDate"] as DateTime?,
                                Admissions = Convert.ToInt32(reader["admissions"]),
                                BoxOffice = Convert.ToDecimal(reader["boxOffice"]),
                                Score = Convert.ToDecimal(reader["score"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询影片概况时发生错误: {ex.Message}");
                return null;
            }
        }
        return film;
    }

    /// <summary>
    /// 根据演职人员姓名，查询其参演的所有电影。
    /// </summary>
    public List<Cast> GetCastCrewDetails(string memberName)
    {
        var details = new List<Cast>();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            try
            {
                connection.Open();
                string sql = "SELECT filmName, role FROM cast WHERE memberName = :memberName";
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("memberName", memberName));
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            details.Add(new Cast
                            {
                                FilmName = reader["filmName"].ToString(),
                                Role = reader["role"].ToString(),
                                MemberName = memberName
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询演职人员信息时发生错误: {ex.Message}");
            }
        }
        return details;
    }

    /// <summary>
    /// 查询指定电影的数据统计信息，包括票价、总票房、已售票数和上座率。
    /// </summary>
    public (decimal BoxOffice, int TicketsSold, decimal OccupancyRate) GetMovieStatistics(string filmName)
    {
        decimal totalBoxOffice = 0;
        int ticketsSold = 0;
        decimal occupancyRate = 0;

        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            try
            {
                connection.Open();

                // 1. 获取总票房和已售票数
                string ticketsSql = @"
                    SELECT
                        SUM(t.price) AS TotalBoxOffice, COUNT(t.ticketID) AS TicketsSold
                    FROM
                        ticket t
                    JOIN
                        section s ON t.sectionID = s.sectionID
                    WHERE
                        s.filmName = :filmName AND t.state = '已售出'";
                using (OracleCommand ticketsCmd = new OracleCommand(ticketsSql, connection))
                {
                    ticketsCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (OracleDataReader reader = ticketsCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalBoxOffice = reader["TotalBoxOffice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBoxOffice"]) : 0m;
                            ticketsSold = reader["TicketsSold"] != DBNull.Value ? Convert.ToInt32(reader["TicketsSold"]) : 0;
                        }
                    }
                }

                // 2. 统计总座次
                string seatsSql = @"
                    SELECT
                        SUM(mh.lines * mh.columns) AS TotalSeats
                    FROM
                        section s
                    JOIN
                        moviehall mh ON s.hallNo = mh.hallNo
                    WHERE
                        s.filmName = :filmName";
                using (OracleCommand seatsCmd = new OracleCommand(seatsSql, connection))
                {
                    seatsCmd.Parameters.Add(new OracleParameter("filmName", filmName));
                    var totalSeats = seatsCmd.ExecuteScalar();
                    int totalSeatsCount = totalSeats != DBNull.Value ? Convert.ToInt32(totalSeats) : 0;

                    // 3. 计算上座率
                    if (totalSeatsCount > 0)
                    {
                        occupancyRate = (decimal)ticketsSold / totalSeatsCount;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询电影数据统计时发生错误: {ex.Message}");
            }
        }
        return (totalBoxOffice, ticketsSold, occupancyRate);
    }
}
