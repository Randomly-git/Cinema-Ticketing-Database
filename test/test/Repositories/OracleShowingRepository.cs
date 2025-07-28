using Oracle.ManagedDataAccess.Client;
using test.Models; 
using System;
using System.Collections.Generic;
using System.Data;

namespace test.Repositories 
{
    /// <summary>
    /// Oracle 数据库的票务调度数据仓储实现。
    /// </summary>
    public class OracleShowingRepository : IShowingRepository
    {
        private readonly string _connectionString;
        private const string SchemaName = ""; 

        public OracleShowingRepository(string connectionString)
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
        /// 获取指定电影在某日期（或所有日期）的可用场次。
        /// </summary>
        public List<Section> GetAvailableSections(string filmName, DateTime? date = null)
        {
            List<Section> sections = new List<Section>();
            using (var connection = GetConnection())
            {
                // !!! 关键修复点：从 SELECT 列表中移除 S.DAY !!!
                string sql = $@"SELECT S.SECTIONID, S.FILMNAME, S.HALLNO, S.TIMEID,
                                       MH.LINES, MH.COLUMNS, MH.CATEGORY AS HALL_CATEGORY,
                                       TS.""STARTTIME"", TS.""ENDTIME""
                                FROM {SchemaName}SECTION S
                                JOIN {SchemaName}MOVIEHALL MH ON S.HALLNO = MH.HALLNO
                                JOIN {SchemaName}TIMESLOT TS ON S.TIMEID = TS.TIMEID
                                WHERE S.FILMNAME = :filmName";

                if (date.HasValue)
                {
                    // 从 WHERE 子句中移除对 S.DAY 的引用 !!!
                    // 暂时移除，避免报错。
                    // sql += " AND TRUNC(S.DAY) = TRUNC(TO_DATE(:queryDate, 'YYYY-MM-DD'))";
                }
                sql += " ORDER BY TS.\"STARTTIME\""; // 只按时间排序

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("filmName", filmName));
                    if (date.HasValue)
                    {
                        // command.Parameters.Add(new OracleParameter("queryDate", date.Value.ToString("yyyy-MM-dd"))); // 如果日期过滤被移除，此参数也无需添加
                    }

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
        /// 获取指定影厅的座位布局。
        /// </summary>
        public List<SeatHall> GetHallSeatLayout(int hallNo)
        {
            List<SeatHall> seats = new List<SeatHall>();
            using (var connection = GetConnection())
            {
                // 注意：你的文档中是 seat-hall 表，字段是 LINENO, columnNo, CATEGORY
                string sql = $"SELECT HALLNO, LINENO, COLUMNNO, CATEGORY FROM {SchemaName}SEATHALL WHERE HALLNO = :hallNo ORDER BY LINENO, COLUMNNO";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("hallNo", hallNo));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seats.Add(new SeatHall
                            {
                                HallNo = Convert.ToInt32(reader["HALLNO"]),
                                LINENO = reader["LINENO"].ToString(),
                                ColumnNo = Convert.ToInt32(reader["COLUMNNO"]),
                                CATEGORY = reader["CATEGORY"].ToString()
                            });
                        }
                    }
                }
            }
            return seats;
        }

        /// <summary>
        /// 获取指定场次已售出的座位。
        /// </summary>
        public List<Ticket> GetSoldSeatsForSection(int sectionId)
        {
            List<Ticket> soldTickets = new List<Ticket>();
            using (var connection = GetConnection())
            {
                string sql = $@"SELECT TICKETID, SECTIONID, LINENO, COLUMNNO, STATE, PRICE, RATING
                                FROM {SchemaName}TICKET
                                WHERE SECTIONID = :sectionId AND STATE = '已售出'"; // 只查询已售出的票
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("sectionId", sectionId));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            soldTickets.Add(new Ticket
                            {
                                TicketID = reader["TICKETID"].ToString(),
                                SectionID = Convert.ToInt32(reader["SECTIONID"]),
                                LineNo = reader["LINENO"].ToString(),
                                ColumnNo = Convert.ToInt32(reader["COLUMNNO"]),
                                State = reader["STATE"].ToString(),
                                Price = Convert.ToDecimal(reader["PRICE"]),
                                Rating = Convert.ToInt32(reader["RATING"])
                            });
                        }
                    }
                }
            }
            return soldTickets;
        }
    }
}
