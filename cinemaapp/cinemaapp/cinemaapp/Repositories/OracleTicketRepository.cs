using Oracle.ManagedDataAccess.Client;
using System;
using test.Models;

namespace cinemaapp.Repositories
{
    public class OracleTicketRepository : ITicketRepository
    {
        private readonly string _connectionString;

        public OracleTicketRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Ticket GetTicketWithSection(string ticketId)
        {
            string sql = @"
        SELECT
            t.ticketID, t.price, t.rating, t.sectionID, t.lineNo, t.columnNo, t.state,
            s.sectionID AS secID, s.filmName, s.hallNo, s.timeID,
            ts.startTime, ts.endTime,
            mh.category AS HallCategory
        FROM
            ticket t
        JOIN
            section s ON t.sectionID = s.sectionID
        JOIN
            timeslot ts ON s.timeID = ts.timeID
        JOIN
            moviehall mh ON s.hallNo = mh.hallNo
        WHERE
            t.ticketID = :ticketId";

            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();

                using (var cmd = new OracleCommand(sql, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("ticketId", ticketId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Ticket
                            {
                                TicketID = reader["ticketID"]?.ToString(),
                                Price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]),
                                Rating = reader["rating"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rating"]),
                                SectionID = reader["sectionID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sectionID"]),
                                LineNo = reader["lineNo"]?.ToString(),
                                ColumnNo = reader["columnNo"] == DBNull.Value ? 0 : Convert.ToInt32(reader["columnNo"]),
                                State = reader["state"]?.ToString(),

                                Section = new Section
                                {
                                    SectionID = reader["secID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["secID"]),
                                    FilmName = reader["filmName"]?.ToString(),
                                    HallNo = reader["hallNo"] == DBNull.Value ? 0 : Convert.ToInt32(reader["hallNo"]),
                                    TimeID = reader["timeID"]?.ToString(),
                                    ScheduleStartTime = reader["startTime"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["startTime"]),
                                    ScheduleEndTime = reader["endTime"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["endTime"]),
                                    HallCategory = reader["HallCategory"]?.ToString()
                                }
                            };
                        }
                    }
                }
            }

            return null;
        }


    }
}
