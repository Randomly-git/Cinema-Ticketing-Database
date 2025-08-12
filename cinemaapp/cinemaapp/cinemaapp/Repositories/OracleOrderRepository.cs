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
        public List<OrderForProduct> GetProductOrders(DateTime? startDate = null, DateTime? endDate = null)
        {
            List<OrderForProduct> orders = new List<OrderForProduct>();

            // 基础 SQL（不限制客户ID）
            string sql = "SELECT ORDERID, CUSTOMERID, PRODUCTNAME, PURCHASENUM, DAY, STATE, PMETHOD, PRICE " +
                         "FROM ORDERFORPRODUCTS WHERE 1=1";

            // 参数集合
            List<OracleParameter> parameters = new List<OracleParameter>();

            // 如果提供了开始日期
            if (startDate.HasValue)
            {
                sql += " AND DAY >= :startDate";
                parameters.Add(new OracleParameter("startDate", startDate.Value));
            }

            // 如果提供了结束日期
            if (endDate.HasValue)
            {
                sql += " AND DAY <= :endDate";
                parameters.Add(new OracleParameter("endDate", endDate.Value));
            }

            sql += " ORDER BY DAY DESC";

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderForProduct
                            {
                                OrderID = reader.GetInt64(0),
                                CustomerID = reader.GetString(1),
                                ProductName = reader.GetString(2),
                                PurchaseNum = reader.GetInt32(3),
                                Day = reader.GetDateTime(4),
                                State = reader.GetString(5),
                                PMethod = reader.GetString(6),
                                Price = reader.GetDecimal(7)
                            });
                        }
                    }
                }
            }

            return orders;
        }

    }
}

