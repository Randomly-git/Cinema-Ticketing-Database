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
                // !!! 局部修改开始 !!!
                // SQL 查询中彻底移除对 S.DAY 的引用
                string sql = $@"SELECT S.SECTIONID, S.FILMNAME, S.HALLNO, S.TIMEID,
                                       MH.LINES, MH.COLUMNS, MH.CATEGORY AS HALL_CATEGORY,
                                       TS.""STARTTIME"", TS.""ENDTIME""
                                FROM {SchemaName}SECTION S
                                JOIN {SchemaName}MOVIEHALL MH ON S.HALLNO = MH.HALLNO
                                JOIN {SchemaName}TIMESLOT TS ON S.TIMEID = TS.TIMEID
                                WHERE S.FILMNAME = :filmName
                                ORDER BY TS.""STARTTIME"""; // 移除 S.DAY 排序

                // 移除日期过滤逻辑，因为 SECTION 表中没有 DAY 字段
                // if (date.HasValue)
                // {
                //     sql += " AND TRUNC(S.DAY) = TRUNC(TO_DATE(:queryDate, 'YYYY-MM-DD'))";
                // }

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("filmName", filmName));
                    // 移除对 queryDate 参数的添加
                    // if (date.HasValue)
                    // {
                    //     command.Parameters.Add(new OracleParameter("queryDate", date.Value.ToString("yyyy-MM-dd")));
                    // }

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
                                // 彻底移除对 Day 字段的读取和赋值
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
        /// 根据用户提供的实际数据库字段：TICKETID, PRICE, RATING, SECTIONID, LINENO, COLUMNNO, STATE。
        /// </summary>
        public List<Ticket> GetSoldSeatsForSection(int sectionId)
        {
            List<Ticket> soldTickets = new List<Ticket>();
            using (var connection = GetConnection())
            {
                // Ticket 表不再包含 CustomerID
                string sql = $@"SELECT TICKETID, PRICE, RATING, SECTIONID, LINENO, COLUMNNO, STATE
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
                                Price = Convert.ToDecimal(reader["PRICE"]),
                                Rating = Convert.ToInt32(reader["RATING"]),
                                SectionID = Convert.ToInt32(reader["SECTIONID"]),
                                LineNo = reader["LINENO"].ToString(),
                                ColumnNo = Convert.ToInt32(reader["COLUMNNO"]),
                                State = reader["STATE"].ToString()
                            });
                        }
                    }
                }
            }
            return soldTickets;
        }

        /// <summary>
        /// 添加电影票记录。
        /// </summary>
        /// <param name="ticket">要添加的票对象。</param>
        /// <param name="transaction">可选的 Oracle 事务对象。</param>
        public void AddTicket(Ticket ticket, OracleTransaction transaction = null)
        {
            OracleConnection connection = transaction?.Connection ?? GetConnection();
            OracleCommand command = null;

            try
            {
                string sql = $@"INSERT INTO {SchemaName}TICKET
                               (TICKETID, PRICE, RATING, SECTIONID, LINENO, COLUMNNO, STATE)
                               VALUES (:ticketId, :price, :rating, :sectionId, :lineNo, :columnNo, :state)";

                command = new OracleCommand(sql, connection);
                command.Parameters.Add(new OracleParameter("ticketId", ticket.TicketID));
                command.Parameters.Add(new OracleParameter("price", ticket.Price));
                command.Parameters.Add(new OracleParameter("rating", ticket.Rating));
                command.Parameters.Add(new OracleParameter("sectionId", ticket.SectionID));
                command.Parameters.Add(new OracleParameter("lineNo", ticket.LineNo));
                command.Parameters.Add(new OracleParameter("columnNo", ticket.ColumnNo));
                command.Parameters.Add(new OracleParameter("state", ticket.State));

                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                command.ExecuteNonQuery();
            }
            finally
            {
                if (transaction == null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
                command?.Dispose();
            }
        }

        /// <summary>
        /// 根据场次ID获取场次详情。
        /// </summary>
        /// <param name="sectionId">场次ID。</param>
        /// <returns>对应的 Section 对象，如果不存在则为 null。</returns>
        public Section GetSectionById(int sectionId)
        {
            Section section = null;
            using (var connection = GetConnection())
            {
                string sql = $@"SELECT S.SECTIONID, S.FILMNAME, S.HALLNO, S.TIMEID,
                                       MH.LINES, MH.COLUMNS, MH.CATEGORY AS HALL_CATEGORY,
                                       TS.""STARTTIME"", TS.""ENDTIME""
                                FROM {SchemaName}SECTION S
                                JOIN {SchemaName}MOVIEHALL MH ON S.HALLNO = MH.HALLNO
                                JOIN {SchemaName}TIMESLOT TS ON S.TIMEID = TS.TIMEID
                                WHERE S.SECTIONID = :sectionId"; // 根据 sectionId 查询

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("sectionId", sectionId));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            section = new Section
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
                            };
                        }
                    }
                }
            }
            return section;
        }
    }
}

