using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using test.Models;
using test.Repositories;

namespace test.Repositories
{
    /// <summary>
    /// Oracle 数据库的订单数据仓储实现。
    /// </summary>
    public class OracleOrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        private const string SchemaName = "";

        public OracleOrderRepository(string connectionString)
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
        /// 添加电影票订单记录。
        /// </summary>
        /// <param name="order">要添加的订单对象。</param>
        /// <param name="transaction">可选的 Oracle 事务对象。</param>
        public void AddOrderForTickets(OrderForTickets order, OracleTransaction transaction = null)
        {
            OracleConnection connection = transaction?.Connection ?? GetConnection();
            OracleCommand command = null;

            try
            {
                // First get the next sequence value for ORDERID
                string getOrderIdSql = $"SELECT {SchemaName}ORDERFORTICKETS_SEQ.NEXTVAL FROM DUAL";
                using (var getOrderIdCommand = new OracleCommand(getOrderIdSql, connection))
                {
                    if (transaction != null)
                    {
                        getOrderIdCommand.Transaction = transaction;
                    }
                    object result = getOrderIdCommand.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        order.OrderID = Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Failed to generate ORDERID from sequence");
                    }
                }

                // Now insert the record with the generated ORDERID
                string sql = $@"INSERT INTO {SchemaName}ORDERFORTICKETS
                       (ORDERID, TICKETID, STATE, CUSTOMERID, DAY, ""PMETHOD"", PRICE)
                       VALUES (:orderId, :ticketId, :state, :customerId, :day, :paymentMethod, :totalPrice)";

                command = new OracleCommand(sql, connection);
                command.Parameters.Add(new OracleParameter("orderId", order.OrderID));
                command.Parameters.Add(new OracleParameter("ticketId", order.TicketID));
                command.Parameters.Add(new OracleParameter("state", order.State));
                command.Parameters.Add(new OracleParameter("customerId", order.CustomerID));
                command.Parameters.Add(new OracleParameter("day", order.Day));
                command.Parameters.Add(new OracleParameter("paymentMethod", order.PaymentMethod));
                command.Parameters.Add(new OracleParameter("totalPrice", OracleDbType.Varchar2, order.TotalPrice.ToString(System.Globalization.CultureInfo.InvariantCulture), ParameterDirection.Input));

                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    Console.WriteLine($"订单记录插入成功，Order ID: {order.OrderID}。");
                }
                else
                {
                    throw new Exception($"插入失败，影响行数: {rowsAffected}");
                }
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
        /// 根据ticketId获取电影票
        /// </summary>
        public Ticket GetTicketById(string ticketId)
        {
            // 初始化为 null，如果未找到记录则返回 null
            Ticket ticket = null;

            // 使用 try-catch 块来捕获和处理数据库连接或查询中的潜在异常
            try
            {
                // 使用 using 语句确保数据库连接在操作完成后被正确关闭和释放
                using (OracleConnection connection = new OracleConnection(_connectionString))
                {
                    connection.Open();

                    // SQL 查询语句，使用参数化查询来防止 SQL 注入
                    string sql = "SELECT TICKETID, PRICE, SECTIONID, LINENO, COLUMNNO, STATE FROM TICKET WHERE TICKETID = :p_ticketId";

                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        // 添加参数，将传入的 ticketId 绑定到 SQL 查询中
                        command.Parameters.Add(new OracleParameter("p_ticketId", ticketId));

                        // 执行查询并获取数据读取器
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            // 检查是否有数据行可以读取
                            if (reader.Read())
                            {
                                // 如果有，创建一个新的 Ticket 对象并从 reader 中填充数据
                                ticket = new Ticket
                                {
                                    TicketID = reader["TICKETID"].ToString(),
                                    Price = Convert.ToDecimal(reader["PRICE"]),
                                    SectionID = Convert.ToInt32(reader["SECTIONID"]),
                                    LineNo = reader["LINENO"].ToString(),
                                    ColumnNo = Convert.ToInt32(reader["COLUMNNO"]),
                                    State = reader["STATE"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 在控制台或日志中打印异常信息，方便调试
                Console.WriteLine($"查询票务信息时发生错误: {ex.Message}");
            }

            return ticket;
        }

        /// <summary>
        /// 根据 sectionId 获取场次信息
        /// </summary>
        public Section GetSectionById(int sectionId)
        {
            Section section = null;

            try
            {
                using (OracleConnection connection = new OracleConnection(_connectionString))
                {
                    connection.Open();

                    // 查询 SECTION 表对应字段
                    // 如果它们是通过联表查询 TimeSlot 和 MovieHall 得到的，需要写 JOIN 查询
                    string sql = "SELECT SECTIONID, FILMNAME, HALLNO, TIMEID FROM SECTION WHERE SECTIONID = :p_sectionId";

                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        // 绑定参数
                        command.Parameters.Add(new OracleParameter("p_sectionId", sectionId));

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                section = new Section
                                {
                                    SectionID = Convert.ToInt32(reader["SECTIONID"]),
                                    FilmName = reader["FILMNAME"].ToString(),
                                    HallNo = Convert.ToInt32(reader["HALLNO"]),
                                    TimeID = reader["TIMEID"].ToString(),
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询场次信息时发生错误: {ex.Message}");
            }

            return section;
        }


        /// <summary>
        /// 根据订单ID获取电影票订单。
        /// </summary>
        public OrderForTickets GetOrderForTicketsById(int orderId)
        {
            OrderForTickets order = null;
            using (var connection = GetConnection())
            {

                string sql = $@"SELECT ORDERID, TICKETID, STATE, CUSTOMERID, DAY, ""PMETHOD"", PRICE
                                FROM {SchemaName}ORDERFORTICKETS
                                WHERE ORDERID = :orderId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("orderId", orderId));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new OrderForTickets
                            {
                                OrderID = Convert.ToInt32(reader["ORDERID"]),
                                TicketID = reader["TICKETID"].ToString(),
                                State = reader["STATE"].ToString(),
                                CustomerID = reader["CUSTOMERID"].ToString(),
                                Day = Convert.ToDateTime(reader["DAY"]),
                                PaymentMethod = reader["PMETHOD"].ToString(),
                                TotalPrice = Convert.ToDecimal(reader["PRICE"])
                            };
                        }
                    }
                }
            }
            return order;
        }

        /// <summary>
        /// 获取某个顾客的所有电影票订单。
        /// </summary>
        public List<OrderForTickets> GetOrdersForCustomer(string customerId, bool onlyValid = false)
        {
            List<OrderForTickets> orders = new List<OrderForTickets>();
            using (var connection = GetConnection())
            {
                // 构建SQL查询
                string sql = $@"SELECT ORDERID, TICKETID, STATE, CUSTOMERID, DAY, ""PMETHOD"", PRICE
                       FROM {SchemaName}ORDERFORTICKETS
                       WHERE CUSTOMERID = :customerId
                       {(onlyValid ? "AND STATE = '有效'" : "")}
                       ORDER BY DAY DESC, ORDERID DESC";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", customerId));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderForTickets
                            {
                                OrderID = Convert.ToInt32(reader["ORDERID"]),
                                TicketID = reader["TICKETID"].ToString(),
                                State = reader["STATE"].ToString(),
                                CustomerID = reader["CUSTOMERID"].ToString(),
                                Day = Convert.ToDateTime(reader["DAY"]),
                                PaymentMethod = reader["PMETHOD"].ToString(),
                                TotalPrice = Convert.ToDecimal(reader["PRICE"])
                            });
                        }
                    }
                }
            }
            return orders;
        }

        public List<OrderForTickets> GetAllOrders(DateTime? startDate = null, DateTime? endDate = null)
        {
            List<OrderForTickets> orders = new List<OrderForTickets>();
            using (var connection = GetConnection())
            {
                // 基础SQL查询
                string sql = $@"SELECT ORDERID, TICKETID, STATE, CUSTOMERID, DAY, ""PMETHOD"", PRICE
                        FROM {SchemaName}ORDERFORTICKETS
                        WHERE 1=1";

                // 添加日期筛选条件（可选）
                if (startDate.HasValue)
                {
                    sql += " AND DAY >= :startDate";
                }
                if (endDate.HasValue)
                {
                    sql += " AND DAY <= :endDate";
                }

                // 按日期和订单ID降序排序
                sql += " ORDER BY DAY DESC, ORDERID DESC";

                using (var command = new OracleCommand(sql, connection))
                {
                    // 添加参数
                    if (startDate.HasValue)
                    {
                        command.Parameters.Add(new OracleParameter("startDate", startDate.Value));
                    }
                    if (endDate.HasValue)
                    {
                        command.Parameters.Add(new OracleParameter("endDate", endDate.Value));
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderForTickets
                            {
                                OrderID = Convert.ToInt32(reader["ORDERID"]),
                                TicketID = reader["TICKETID"].ToString(),
                                State = reader["STATE"].ToString(),
                                CustomerID = reader["CUSTOMERID"].ToString(),
                                Day = Convert.ToDateTime(reader["DAY"]),
                                PaymentMethod = reader["PMETHOD"].ToString(),
                                TotalPrice = Convert.ToDecimal(reader["PRICE"])
                            });
                        }
                    }
                }
            }
            return orders;
        }

        // 根据影厅号获取影厅信息
        public MovieHall GetMovieHallByNo(int hallNo)
        {
            MovieHall hall = null;
            using (var connection = GetConnection())
            {
                string sql = $@"
            SELECT HALLNO, LINES, COLUMNS, CATEGORY
            FROM {SchemaName}MOVIEHALL
            WHERE HALLNO = :hallNo";

                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("hallNo", hallNo));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hall = new MovieHall
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
            return hall;
        }

    }
}