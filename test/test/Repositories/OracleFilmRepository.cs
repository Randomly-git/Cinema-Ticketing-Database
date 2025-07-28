using Oracle.ManagedDataAccess.Client;
using test.Models; 
using System;
using System.Collections.Generic;
using System.Data;

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
                string sql = $"SELECT FILMNAME, GENRE, FILMLENGTH, NORMALPRICE, RELEASEDATE, ENDDATE, ADMISSIONS, BOXOFFICE, SCORE FROM {SchemaName}FILM WHERE FILMNAME = :filmName";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("filmName", filmName));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            film = new Film
                            {
                                FilmName = reader["FILMNAME"].ToString(),
                                Genre = reader["GENRE"].ToString(),
                                FilmLength = Convert.ToInt32(reader["FILMLENGTH"]),
                                NormalPrice = Convert.ToDecimal(reader["NORMALPRICE"]),
                                ReleaseDate = Convert.ToDateTime(reader["RELEASEDATE"]),
                                EndDate = reader["ENDDATE"] as DateTime?, // 可空日期
                                Admissions = Convert.ToInt32(reader["ADMISSIONS"]),
                                BoxOffice = Convert.ToInt32(reader["BOXOFFICE"]),
                                Score = Convert.ToInt32(reader["SCORE"])
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
                string sql = $"SELECT FILMNAME, GENRE, FILMLENGTH, NORMALPRICE, RELEASEDATE, ENDDATE, ADMISSIONS, BOXOFFICE, SCORE FROM {SchemaName}FILM";
                using (var command = new OracleCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            films.Add(new Film
                            {
                                FilmName = reader["FILMNAME"].ToString(),
                                Genre = reader["GENRE"].ToString(),
                                FilmLength = Convert.ToInt32(reader["FILMLENGTH"]),
                                NormalPrice = Convert.ToDecimal(reader["NORMALPRICE"]),
                                ReleaseDate = Convert.ToDateTime(reader["RELEASEDATE"]),
                                EndDate = reader["ENDDATE"] as DateTime?,
                                Admissions = Convert.ToInt32(reader["ADMISSIONS"]),
                                BoxOffice = Convert.ToInt32(reader["BOXOFFICE"]),
                                Score = Convert.ToInt32(reader["SCORE"])
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
                                // Day 属性已从 Section 模型中移除，此处不再尝试读取和赋值
                                MovieHall = new MovieHall // 填充关联的影厅信息
                                {
                                    HallNo = Convert.ToInt32(reader["HALLNO"]),
                                    Lines = Convert.ToInt32(reader["LINES"]),
                                    ColumnsCount = Convert.ToInt32(reader["COLUMNS"]),
                                    Category = reader["HALL_CATEGORY"].ToString()
                                },
                                TimeSlot = new TimeSlot // 填充关联的时段信息
                                {
                                    TimeID = reader["TIMEID"].ToString(),
                                    StartTime = Convert.ToDateTime(reader["STARTTIME"]).TimeOfDay,
                                    EndTime = Convert.ToDateTime(reader["ENDTIME"]).TimeOfDay
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
                                StartTime = Convert.ToDateTime(reader["STARTTIME"]).TimeOfDay,
                                EndTime = Convert.ToDateTime(reader["ENDTIME"]).TimeOfDay
                            };
                        }
                    }
                }
            }
            return timeSlot;
        }
    }
}

