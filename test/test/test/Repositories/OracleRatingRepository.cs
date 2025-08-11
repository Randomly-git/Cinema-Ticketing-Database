using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using test.Models;

namespace test.Repositories
{
    public class OracleRatingRepository : IRatingRepository
    {
        private readonly string _connectionString;
        private const string SchemaName = "CBC";

        public OracleRatingRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private OracleConnection GetConnection()
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Rating GetRating(string ticketId)
        {
            using (var connection = GetConnection())
            {
                string sql = $@"
                    SELECT TICKETID, SCORE, COMMENT, RATINGDATE 
                    FROM {SchemaName}.RATING 
                    WHERE TICKETID = :ticketId";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add("ticketId", OracleDbType.Varchar2).Value = ticketId;

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Rating
                            {
                                TicketID = reader["TICKETID"].ToString(),
                                Score = Convert.ToInt32(reader["SCORE"]),
                                Comment = reader["COMMENT"]?.ToString(),
                                RatingDate = Convert.ToDateTime(reader["RATINGDATE"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void AddOrUpdateRating(Rating rating)
        {
            if (rating == null) throw new ArgumentNullException(nameof(rating));

            using (var connection = GetConnection())
            {
                bool exists = GetRating(rating.TicketID) != null;

                string sql = exists
                    ? $@"
                        UPDATE {SchemaName}.RATING 
                        SET SCORE = :score, 
                            COMMENT = :comment,
                            RATINGDATE = SYSDATE 
                        WHERE TICKETID = :ticketId"
                    : $@"
                        INSERT INTO {SchemaName}.RATING 
                        (TICKETID, SCORE, COMMENT, RATINGDATE) 
                        VALUES 
                        (:ticketId, :score, :comment, SYSDATE)";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add("score", OracleDbType.Decimal).Value = rating.Score;
                    command.Parameters.Add("comment", OracleDbType.Varchar2).Value =
                        string.IsNullOrEmpty(rating.Comment) ? DBNull.Value : (object)rating.Comment;
                    command.Parameters.Add("ticketId", OracleDbType.Varchar2).Value = rating.TicketID;

                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveRating(string ticketId)
        {
            using (var connection = GetConnection())
            {
                string sql = $@"
                    DELETE FROM {SchemaName}.RATING 
                    WHERE TICKETID = :ticketId";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add("ticketId", OracleDbType.Varchar2).Value = ticketId;
                    command.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<Rating> GetRatingsByTicketIds(IEnumerable<string> ticketIds)
        {
            var ratings = new List<Rating>();
            if (!ticketIds.Any()) return ratings;

            using (var connection = GetConnection())
            {
                string sql = $@"
                    SELECT TICKETID, SCORE, COMMENT, RATINGDATE 
                    FROM {SchemaName}.RATING 
                    WHERE TICKETID IN ({string.Join(",", ticketIds.Select(id => $"'{id}'"))})";

                using (var command = new OracleCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ratings.Add(new Rating
                            {
                                TicketID = reader["TICKETID"].ToString(),
                                Score = Convert.ToInt32(reader["SCORE"]),
                                Comment = reader["COMMENT"]?.ToString(),
                                RatingDate = Convert.ToDateTime(reader["RATINGDATE"])
                            });
                        }
                    }
                }
            }
            return ratings;
        }
    }
}