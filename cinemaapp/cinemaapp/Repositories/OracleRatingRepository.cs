using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using test.Models;

namespace test.Repositories
{
    /// <summary>
    /// Oracle 版 Rating 仓储（改进版）
    /// </summary>
    public class OracleRatingRepository : IRatingRepository
    {
        private readonly string _connectionString;
        private const string SchemaName = "CBC"; 

        public OracleRatingRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// 获取并打开连接
        /// </summary>
        private OracleConnection GetConnection()
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 打印 SQL 与参数（用于调试）
        /// </summary>
        private void LogCommand(OracleCommand cmd)
        {
            try
            {
                Console.WriteLine("----- SQL START -----");
                Console.WriteLine(cmd.CommandText);
                Console.WriteLine("Parameters:");
                foreach (OracleParameter p in cmd.Parameters)
                {
                    Console.WriteLine($"  {p.ParameterName} = {(p.Value == null ? "NULL" : p.Value)} (DbType={p.OracleDbType})");
                }
                Console.WriteLine("----- SQL END -----");
            }
            catch
            {
                // 不应抛出异常影响主流程
            }
        }

        /// <summary>
        /// 根据 TicketID 获取单条 Rating
        /// 注：将数据库中的 COMMENT 列以别名 COMMENTS 返回，代码中读取该别名。
        /// </summary>
        public Rating GetRating(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
                return null;

            using (var connection = GetConnection())
            {
                // NOTE: "COMMENT" 被双引号引用，并用别名 COMMENTS 返回，避免关键字冲突
                string sql = $@"
                    SELECT TICKETID, SCORE, ""COMMENT"" AS COMMENTS, RATINGDATE
                    FROM {SchemaName}.RATING
                    WHERE TICKETID = :ticketId";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("ticketId", OracleDbType.Varchar2) { Value = ticketId });

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Rating
                                {
                                    TicketID = reader["TICKETID"].ToString(),
                                    Score = Convert.ToInt32(reader["SCORE"]),
                                    Comment = reader["COMMENTS"] == DBNull.Value ? null : reader["COMMENTS"].ToString(),
                                    RatingDate = Convert.ToDateTime(reader["RATINGDATE"])
                                };
                            }
                        }
                    }
                    catch (OracleException ex)
                    {
                        // 打印 SQL + 参数，帮助定位问题
                        Console.WriteLine($"OracleException in GetRating: Code={ex.Number}, Msg={ex.Message}");
                        LogCommand(command);
                        throw;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 添加或更新评分（在同一连接上先检查存在性）
        /// </summary>
        public void AddOrUpdateRating(Rating rating)
        {
            if (rating == null) throw new ArgumentNullException(nameof(rating));
            if (string.IsNullOrWhiteSpace(rating.TicketID)) throw new ArgumentException("TicketID 不能为空", nameof(rating));

            using (var connection = GetConnection())
            {
                // 在同一连接内检查是否存在
                bool exists = false;
                string existSql = $@"SELECT COUNT(1) FROM {SchemaName}.RATING WHERE TICKETID = :ticketId";
                using (var existCmd = new OracleCommand(existSql, connection))
                {
                    existCmd.Parameters.Add(new OracleParameter("ticketId", OracleDbType.Varchar2) { Value = rating.TicketID });
                    try
                    {
                        var o = existCmd.ExecuteScalar();
                        int count = 0;
                        if (o != null && o != DBNull.Value)
                        {
                            // ExecuteScalar 在 Oracle 中常返回 decimal 类型
                            count = Convert.ToInt32(o);
                        }
                        exists = count > 0;
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine($"OracleException when checking existence: Code={ex.Number}, Msg={ex.Message}");
                        LogCommand(existCmd);
                        throw;
                    }
                }

                // 根据存在性决定 INSERT 或 UPDATE（并引用 "COMMENT" 列）
                string sql = exists
                    ? $@"
                        UPDATE {SchemaName}.RATING
                        SET SCORE = :score,
                            ""COMMENT"" = :p_comment,
                            RATINGDATE = SYSDATE
                        WHERE TICKETID = :p_ticketId"
                    : $@"
                        INSERT INTO {SchemaName}.RATING
                        (TICKETID, SCORE, ""COMMENT"", RATINGDATE)
                        VALUES (:ticketId, :score, :p_comment, SYSDATE)";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("ticketId", OracleDbType.Varchar2) { Value = rating.TicketID });
                    command.Parameters.Add(new OracleParameter("score", OracleDbType.Decimal) { Value = rating.Score });
                    command.Parameters.Add(new OracleParameter("p_comment", OracleDbType.Varchar2)
                    {
                        Value = string.IsNullOrEmpty(rating.Comment) ? DBNull.Value : (object)rating.Comment
                    });

                    try
                    {
                        int rows = command.ExecuteNonQuery();
                        // 可根据需要打印 rows
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine($"OracleException in AddOrUpdateRating: Code={ex.Number}, Msg={ex.Message}");
                        LogCommand(command);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 删除评分
        /// </summary>
        public void RemoveRating(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
                return;

            using (var connection = GetConnection())
            {
                string sql = $@"DELETE FROM {SchemaName}.RATING WHERE TICKETID = :ticketId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("ticketId", OracleDbType.Varchar2) { Value = ticketId });
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine($"OracleException in RemoveRating: Code={ex.Number}, Msg={ex.Message}");
                        LogCommand(command);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 根据多个 TicketID 批量获取评分
        /// - 使用动态参数化 IN 列表（不会直接拼接字符串）
        /// - 如果 ticketIds 很多（>1000）需要分批（Oracle 对 IN 列表有上限限制）
        /// </summary>
        public IEnumerable<Rating> GetRatingsByTicketIds(IEnumerable<string> ticketIds)
        {
            var ratings = new List<Rating>();
            var idList = ticketIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (idList == null || idList.Count == 0) return ratings;

            // Oracle 对 IN 列表有上限（通常 1000），如果超过需要分批处理
            const int OracleInLimit = 1000;
            if (idList.Count > OracleInLimit)
            {
                // 简单分批实现
                for (int offset = 0; offset < idList.Count; offset += OracleInLimit)
                {
                    var batch = idList.Skip(offset).Take(OracleInLimit);
                    ratings.AddRange(GetRatingsByTicketIds_Internal(batch.ToList()));
                }
            }
            else
            {
                ratings.AddRange(GetRatingsByTicketIds_Internal(idList));
            }

            return ratings;
        }

        /// <summary>
        /// 内部实现：对不超过 1000 个 id 的批次做查询
        /// </summary>
        private IEnumerable<Rating> GetRatingsByTicketIds_Internal(List<string> idList)
        {
            var ratings = new List<Rating>();
            using (var connection = GetConnection())
            {
                var paramNames = idList.Select((id, idx) => $":id{idx}").ToArray();
                string sql = $@"
                    SELECT TICKETID, SCORE, ""COMMENT"" AS COMMENTS, RATINGDATE
                    FROM {SchemaName}.RATING
                    WHERE TICKETID IN ({string.Join(",", paramNames)})";

                using (var command = new OracleCommand(sql, connection))
                {
                    for (int i = 0; i < idList.Count; i++)
                    {
                        command.Parameters.Add(new OracleParameter($"id{i}", OracleDbType.Varchar2) { Value = idList[i] });
                    }

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ratings.Add(new Rating
                                {
                                    TicketID = reader["TICKETID"].ToString(),
                                    Score = Convert.ToInt32(reader["SCORE"]),
                                    Comment = reader["COMMENTS"] == DBNull.Value ? null : reader["COMMENTS"].ToString(),
                                    RatingDate = Convert.ToDateTime(reader["RATINGDATE"])
                                });
                            }
                        }
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine($"OracleException in GetRatingsByTicketIds_Internal: Code={ex.Number}, Msg={ex.Message}");
                        LogCommand(command);
                        throw;
                    }
                }
            }
            return ratings;
        }
    }
}
