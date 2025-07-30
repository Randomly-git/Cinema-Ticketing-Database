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
                string sql = $@"INSERT INTO {SchemaName}ORDERFORTICKETS
                               (TICKETID, STATE, CUSTOMERID, DAY, ""PMETHOD"", PRICE)
                               VALUES (:ticketId, :state, :customerId, :day, :paymentMethod, :totalPrice)";

                command = new OracleCommand(sql, connection);
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

                command.ExecuteNonQuery();

                // 通过 SELECT MAX(ORDERID) 获取生成的 ORDERID
                // 这种方法在并发环境下不安全，仅用于调试目的。
                string getMaxOrderIdSql = $"SELECT MAX(ORDERID) FROM {SchemaName}ORDERFORTICKETS";
                using (var getMaxOrderIdCommand = new OracleCommand(getMaxOrderIdSql, connection))
                {
                    if (transaction != null)
                    {
                        getMaxOrderIdCommand.Transaction = transaction;
                    }
                    object result = getMaxOrderIdCommand.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        order.OrderID = Convert.ToInt32(result);
                        Console.WriteLine($"订单记录（含所有字段）插入成功，并通过 MAX(ORDERID) 获取到Order ID: {order.OrderID}。");
                    }
                    else
                    {
                        Console.WriteLine("警告：未能通过 MAX(ORDERID) 获取 Order ID。请检查表是否为空或插入失败。");
                    }
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
        public List<OrderForTickets> GetOrdersForCustomer(string customerId)
        {
            List<OrderForTickets> orders = new List<OrderForTickets>();
            using (var connection = GetConnection())
            {
                // 确保 "PMETHOD" 用双引号包裹
                string sql = $@"SELECT ORDERID, TICKETID, STATE, CUSTOMERID, DAY, ""PMETHOD"", PRICE
                                FROM {SchemaName}ORDERFORTICKETS
                                WHERE CUSTOMERID = :customerId
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
    }
}

