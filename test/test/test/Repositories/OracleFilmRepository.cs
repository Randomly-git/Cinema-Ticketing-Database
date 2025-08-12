using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using test.Models;

namespace test.Repositories
{
    /// <summary>
    /// Oracle 数据库的影片数据仓储实现。
    /// </summary>
    public class OracleFilmRepository : IFilmRepository
    {
        private readonly string _connectionString;
        private const string SchemaName = "";

        public OracleFilmRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private OracleConnection GetConnection()
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 根据电影名称获取电影详情。
        /// </summary>
        public Film GetFilmByName(string filmName)
        {
            Film film = null;
            using (var connection = GetConnection())
            {
                string sql = $"SELECT FILMNAME, GENRE, FILMLENGTH, NORMALPRICE, RELEASEDATE, ENDDATE, ADMISSIONS, BOXOFFICE, SCORE, RATINGNUM FROM {SchemaName}FILM WHERE FILMNAME = :filmName";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal normalPrice = 0m;
                            int normalPriceOrdinal = reader.GetOrdinal("NORMALPRICE");
                            if (!reader.IsDBNull(normalPriceOrdinal))
                            {
                                // 修复点：先读取为字符串，再尝试解析为 decimal
                                string priceString = reader[normalPriceOrdinal].ToString();
                                if (!decimal.TryParse(priceString, NumberStyles.Any, CultureInfo.InvariantCulture, out normalPrice))
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"严重警告：在读取电影 '{filmName}' 的 NORMALPRICE 时，无法将字符串 '{priceString}' 转换为 decimal 类型。");
                                    Console.WriteLine("请检查数据库中 FILM 表的 NORMALPRICE 列的数据是否为有效数字，或尝试更新 Oracle.ManagedDataAccess 驱动。");
                                    Console.ResetColor();
                                    normalPrice = 0m; // 转换失败时使用默认值
                                }
                            }

                            film = new Film
                            {
                                FilmName = !reader.IsDBNull(reader.GetOrdinal("FILMNAME")) ? reader["FILMNAME"].ToString() : string.Empty,
                                Genre = !reader.IsDBNull(reader.GetOrdinal("GENRE")) ? reader["GENRE"].ToString() : string.Empty,
                                FilmLength = !reader.IsDBNull(reader.GetOrdinal("FILMLENGTH")) ? Convert.ToInt32(reader["FILMLENGTH"]) : 0,
                                NormalPrice = normalPrice,
                                ReleaseDate = !reader.IsDBNull(reader.GetOrdinal("RELEASEDATE")) ? Convert.ToDateTime(reader["RELEASEDATE"]) : DateTime.MinValue,
                                EndDate = reader["ENDDATE"] as DateTime?, 
                                Admissions = !reader.IsDBNull(reader.GetOrdinal("ADMISSIONS")) ? Convert.ToInt32(reader["ADMISSIONS"]) : 0,
                                BoxOffice = !reader.IsDBNull(reader.GetOrdinal("BOXOFFICE")) ? Convert.ToInt32(reader["BOXOFFICE"]) : 0,
                                Score = !reader.IsDBNull(reader.GetOrdinal("SCORE")) ? Convert.ToDecimal(reader["SCORE"]) : 0m,
                                RatingNum = !reader.IsDBNull(reader.GetOrdinal("RATINGNUM")) ? Convert.ToInt32(reader["RATINGNUM"]) : 0
                            };
                        }
                    }
                }
            }
            return film;
        }

        /// <summary>
        /// 获取所有电影列表。
        /// </summary>
        public List<Film> GetAllFilms()
        {
            List<Film> films = new List<Film>();
            using (var connection = GetConnection())
            {
                string sql = $"SELECT FILMNAME, GENRE, FILMLENGTH, NORMALPRICE, RELEASEDATE, ENDDATE, ADMISSIONS, BOXOFFICE, SCORE, RATINGNUM FROM {SchemaName}FILM";
                using (var command = new OracleCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal normalPrice = 0m;
                            int normalPriceOrdinal = reader.GetOrdinal("NORMALPRICE");
                            if (!reader.IsDBNull(normalPriceOrdinal))
                            {
                                string priceString = reader[normalPriceOrdinal].ToString();
                                if (!decimal.TryParse(priceString, NumberStyles.Any, CultureInfo.InvariantCulture, out normalPrice))
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"严重警告：在读取电影 '{reader["FILMNAME"]}' 的 NORMALPRICE 时，无法将字符串 '{priceString}' 转换为 decimal 类型。");
                                    Console.WriteLine("请检查数据库中 FILM 表的 NORMALPRICE 列的数据是否为有效数字，或尝试更新 Oracle.ManagedDataAccess 驱动。");
                                    Console.ResetColor();
                                    normalPrice = 0m; // 转换失败时使用默认值
                                }
                            }

                            films.Add(new Film
                            {
                                FilmName = reader["FILMNAME"] == DBNull.Value ? null : reader["FILMNAME"].ToString(),
                                Genre = reader["GENRE"] == DBNull.Value ? null : reader["GENRE"].ToString(),
                                FilmLength = reader["FILMLENGTH"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FILMLENGTH"]),
                                NormalPrice = normalPrice, // 使用转换后的值
                                ReleaseDate = reader["RELEASEDATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["RELEASEDATE"]),
                                EndDate = reader["ENDDATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ENDDATE"]),
                                Admissions = reader["ADMISSIONS"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ADMISSIONS"]),
                                BoxOffice = reader["BOXOFFICE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BOXOFFICE"]),
                                Score = reader["SCORE"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["SCORE"]),
                                RatingNum = reader["RATINGNUM"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RATINGNUM"])
                            });

                        }
                    }
                }
            }
            return films;
        }

        /// <summary>
        /// 获取指定电影的演职人员。
        /// </summary>
        public List<Cast> GetCastByFilmName(string filmName)
        {
            List<Cast> castMembers = new List<Cast>();
            using (var connection = GetConnection())
            {
                string sql = $"SELECT MEMBERNAME, ROLE, FILMNAME FROM {SchemaName}CAST WHERE FILMNAME = :filmName";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            castMembers.Add(new Cast
                            {
                                MemberName = reader["MEMBERNAME"].ToString(),
                                Role = reader["ROLE"].ToString(),
                                FilmName = reader["FILMNAME"].ToString()
                            });
                        }
                    }
                }
            }
            return castMembers;
        }

        /// <summary>
        /// 获取指定电影的所有场次。
        /// </summary>
        public List<Section> GetSectionsByFilmName(string filmName)
        {
            List<Section> sections = new List<Section>();
            using (var connection = GetConnection())
            {
                // 联结 section、moviehall 和 timeslot 表以获取完整的场次信息
                string sql = $@"SELECT S.SECTIONID, S.FILMNAME, S.HALLNO, S.TIMEID,
                                   MH.LINES, MH.COLUMNS, MH.CATEGORY AS HALL_CATEGORY,
                                   TS.""STARTTIME"", TS.""ENDTIME""
                            FROM {SchemaName}SECTION S
                            JOIN {SchemaName}MOVIEHALL MH ON S.HALLNO = MH.HALLNO
                            JOIN {SchemaName}TIMESLOT TS ON S.TIMEID = TS.TIMEID
                            WHERE S.FILMNAME = :filmName
                            ORDER BY TS.""STARTTIME""";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sections.Add(new Section
                            {
                                SectionID = Convert.ToInt32(reader["SECTIONID"]),
                                FilmName = reader["FILMNAME"].ToString(),
                                HallNo = Convert.ToInt32(reader["HALLNO"]),
                                TimeID = reader["TIMEID"].ToString(),
                                MovieHall = new MovieHall
                                {
                                    HallNo = Convert.ToInt32(reader["HALLNO"]),
                                    Lines = Convert.ToInt32(reader["LINES"]),
                                    ColumnsCount = Convert.ToInt32(reader["COLUMNS"]),
                                    Category = reader["HALL_CATEGORY"].ToString()
                                },
                                TimeSlot = new TimeSlot
                                {
                                    TimeID = reader["TIMEID"].ToString(),
                                    // 修改为使用完整的DateTime而不是TimeOfDay
                                    StartTime = Convert.ToDateTime(reader["STARTTIME"]),
                                    EndTime = Convert.ToDateTime(reader["ENDTIME"])
                                }
                            });
                        }
                    }
                }
            }
            return sections;
        }

        /// <summary>
        /// 根据影厅号获取影厅信息。
        /// </summary>
        public MovieHall GetMovieHallByHallNo(int hallNo)
        {
            MovieHall movieHall = null;
            using (var connection = GetConnection())
            {
                string sql = $"SELECT HALLNO, LINES, COLUMNS, CATEGORY FROM {SchemaName}MOVIEHALL WHERE HALLNO = :hallNo";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("hallNo", hallNo));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            movieHall = new MovieHall
                            {
                                HallNo = Convert.ToInt32(reader["HALLNO"]),
                                Lines = Convert.ToInt32(reader["LINES"]),
                                ColumnsCount = Convert.ToInt32(reader["COLUMNS"]),
                                Category = reader["CATEGORY"].ToString()
                            };
                        }
                    }
                }
            }
            return movieHall;
        }

        /// <summary>
        /// 根据时段ID获取时段信息。
        /// </summary>
        public TimeSlot GetTimeSlotByID(string timeId)
        {
            TimeSlot timeSlot = null;
            using (var connection = GetConnection())
            {
                string sql = $"SELECT TIMEID, \"STARTTIME\", \"ENDTIME\" FROM {SchemaName}TIMESLOT WHERE TIMEID = :timeId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("timeId", timeId));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            timeSlot = new TimeSlot
                            {
                                TimeID = reader["TIMEID"].ToString(),
                                // 修改为直接使用 DateTime 而不取 TimeOfDay
                                StartTime = Convert.ToDateTime(reader["STARTTIME"]),
                                EndTime = Convert.ToDateTime(reader["ENDTIME"])
                            };
                        }
                    }
                }
            }
            return timeSlot;
        }

        //管理员部分
        public void AddFilm(Film film)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                string sql = $@"INSERT INTO {SchemaName}FILM 
                           (FILMNAME, GENRE, FILMLENGTH, NORMALPRICE, RELEASEDATE, 
                            ENDDATE, ADMISSIONS, BOXOFFICE, SCORE)
                           VALUES 
                           (:filmname, :genre, :filmlength, :normalprice, :releasedate,
                            :enddate, :admissions, :boxoffice, :score)";

                using (var command = new OracleCommand(sql, connection))
                {
                    // 映射电影属性到数据库字段（移除FILMID相关逻辑）
                    command.Parameters.Add(new OracleParameter("filmname", film.FilmName));
                    command.Parameters.Add(new OracleParameter("genre", film.Genre));
                    command.Parameters.Add(new OracleParameter("filmlength", film.FilmLength));
                    command.Parameters.Add(new OracleParameter("normalprice", film.NormalPrice));
                    command.Parameters.Add(new OracleParameter("releasedate", film.ReleaseDate));
                    command.Parameters.Add(new OracleParameter("enddate", film.EndDate));
                    command.Parameters.Add(new OracleParameter("admissions", film.Admissions));
                    command.Parameters.Add(new OracleParameter("boxoffice", film.BoxOffice));
                    command.Parameters.Add(new OracleParameter("score", film.Score));
                    command.Parameters.Add(new OracleParameter("ratingnum", film.RatingNum));
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateFilm(Film film)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                // 使用FILMNAME作为更新条件（假设它是唯一标识）
                string sql = $@"UPDATE {SchemaName}FILM SET
                           GENRE = :genre,
                           FILMLENGTH = :filmlength,
                           NORMALPRICE = :normalprice,
                           RELEASEDATE = :releasedate,
                           ENDDATE = :enddate,
                           SCORE = :score
                           WHERE FILMNAME = :filmname";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("genre", film.Genre));
                    command.Parameters.Add(new OracleParameter("filmlength", film.FilmLength));
                    command.Parameters.Add(new OracleParameter("normalprice", film.NormalPrice));
                    command.Parameters.Add(new OracleParameter("releasedate", film.ReleaseDate));
                    command.Parameters.Add(new OracleParameter("enddate", film.EndDate));
                    command.Parameters.Add(new OracleParameter("score", film.Score));
                    command.Parameters.Add(new OracleParameter("ratingnum", film.RatingNum));
                    // 使用FILMNAME作为查询条件（替代原FILMID）
                    command.Parameters.Add(new OracleParameter("filmname", film.FilmName));

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new KeyNotFoundException("电影不存在");
                    }
                }
            }
        }

        public void UpdateAverageScore(Film film, int newScore, int addOrSub = 1)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();

                // 先获取当前 SCORE 和 RATINGNUM
                decimal currentScore;
                int currentRatingNum;
                string selectSql = $@"SELECT SCORE, RATINGNUM FROM {SchemaName}FILM WHERE FILMNAME = :filmname";

                using (var selectCommand = new OracleCommand(selectSql, connection))
                {
                    selectCommand.Parameters.Add(new OracleParameter("filmname", film.FilmName));

                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new KeyNotFoundException("电影不存在");
                        }

                        currentScore = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);// 当前平均分
                        currentRatingNum = reader.GetInt32(1); // 当前评分人数
                    }
                }

                // 根据 addOrSub 计算新的平均分和评分人数
                int newRatingNum = currentRatingNum + addOrSub;
                if (newRatingNum < 0) throw new InvalidOperationException("评分人数不能小于 0");

                decimal updatedScore;
                if (addOrSub > 0)
                {
                    updatedScore = (currentScore * currentRatingNum + newScore) / newRatingNum;
                }
                else // 如果是减少评分（例如撤销评分）
                {
                    if (newRatingNum == 0)
                        updatedScore = 0;
                    else
                        updatedScore = (currentScore * currentRatingNum - newScore) / newRatingNum;
                }

                // 保留一位小数
                updatedScore = Math.Round(updatedScore, 1);

                // 更新数据库
                string updateSql = $@"UPDATE {SchemaName}FILM SET 
                               SCORE = :score,
                               RATINGNUM = :ratingNum
                               WHERE FILMNAME = :filmname";

                using (var updateCommand = new OracleCommand(updateSql, connection))
                {
                    updateCommand.Parameters.Add(new OracleParameter("score", updatedScore));
                    updateCommand.Parameters.Add(new OracleParameter("ratingNum", newRatingNum));
                    updateCommand.Parameters.Add(new OracleParameter("filmname", film.FilmName));

                    updateCommand.ExecuteNonQuery();
                }
            }
        }



        public bool HasRelatedSections(string filmName)
        {
            // 调整为使用FILMNAME关联场次表
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                string sql = $@"SELECT COUNT(1) FROM {SchemaName}SECTIONS
                           WHERE FILMNAME = :filmname AND SHOWTIME > SYSDATE";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("filmname", filmName));
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
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
                                    FilmName = reader.IsDBNull(reader.GetOrdinal("filmName")) ? null : reader["filmName"].ToString(),
                                    Genre = reader.IsDBNull(reader.GetOrdinal("genre")) ? null : reader["genre"].ToString(),
                                    FilmLength = reader.IsDBNull(reader.GetOrdinal("filmLength")) ? 0 : Convert.ToInt32(reader["filmLength"]),
                                    NormalPrice = reader.IsDBNull(reader.GetOrdinal("normalPrice")) ? 0m : Convert.ToDecimal(reader["normalPrice"]),
                                    ReleaseDate = reader.IsDBNull(reader.GetOrdinal("releaseDate")) ? (DateTime?)null : Convert.ToDateTime(reader["releaseDate"]),
                                    EndDate = reader.IsDBNull(reader.GetOrdinal("endDate")) ? (DateTime?)null : Convert.ToDateTime(reader["endDate"]),
                                    Admissions = reader.IsDBNull(reader.GetOrdinal("admissions")) ? 0 : Convert.ToInt32(reader["admissions"]),
                                    BoxOffice = reader.IsDBNull(reader.GetOrdinal("boxOffice")) ? 0 : Convert.ToInt32(reader["boxOffice"]),
                                    Score = reader.IsDBNull(reader.GetOrdinal("score")) ? 0m : Convert.ToDecimal(reader["score"])
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
                    // 模糊查询，使用 LIKE 和绑定带通配符的参数
                    string sql = "SELECT memberName, filmName, role FROM cast WHERE memberName LIKE :memberName";

                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        // 传入的参数加上前后通配符，实现模糊匹配
                        command.Parameters.Add(new OracleParameter("memberName", "%" + memberName + "%"));
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                details.Add(new Cast
                                {
                                    MemberName = reader["memberName"].ToString(), // 数据库返回的完整名字
                                    FilmName = reader["filmName"].ToString(),
                                    Role = reader["role"].ToString()
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
}

