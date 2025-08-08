using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using test.Models;
using test.Repositories;

namespace test.Repositories
{
    /// <summary>
    /// Oracle 数据库的周边产品订单数据仓库实现。
    /// </summary>
    public class OracleOrderForProductRepository : IOrderForProductRepository
    {
        private readonly string _connectionString;

        public OracleOrderForProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 获取 Oracle 序列的下一个值。
        /// </summary>
        /// <param name="connection">已打开的 Oracle 连接。</param>
        /// <param name="sequenceName">序列名称。</param>
        /// <returns>序列的下一个值。</returns>
        private long GetNextSequenceValue(OracleConnection connection, string sequenceName)
        {
            long nextVal = 0;
            string sql = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
            using (OracleCommand command = new OracleCommand(sql, connection))
            {
                nextVal = Convert.ToInt64(command.ExecuteScalar());
            }
            return nextVal;
        }

        /// <summary>
        /// 添加新的周边产品订单。
        /// </summary>
        public void AddOrderForProduct(OrderForProduct order)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                // 在添加订单前，从序列获取新的 OrderID
                order.OrderID = GetNextSequenceValue(connection, "ORDERFORPRODUCTS_SEQ"); // 假设存在名为 ORDERFORPRODUCTS_SEQ 的序列

                string sql = @"INSERT INTO ORDERFORPRODUCTS (ORDERID, CUSTOMERID, PRODUCTNAME, PURCHASENUM, DAY, STATE, PMETHOD, PRICE) 
                               VALUES (:orderId, :customerId, :productName, :purchaseNum, :day, :state, :pMethod, :price)";

                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("orderId", order.OrderID));
                    command.Parameters.Add(new OracleParameter("customerId", order.CustomerID));
                    command.Parameters.Add(new OracleParameter("productName", order.ProductName));
                    command.Parameters.Add(new OracleParameter("purchaseNum", order.PurchaseNum));
                    command.Parameters.Add(new OracleParameter("day", order.Day));
                    command.Parameters.Add(new OracleParameter("state", order.State));
                    command.Parameters.Add(new OracleParameter("pMethod", order.PMethod));
                    command.Parameters.Add(new OracleParameter("price", order.Price)); // 存储购买时的单价

                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 根据顾客ID获取其所有周边产品订单。
        /// </summary>
        public List<OrderForProduct> GetOrdersByCustomerId(string customerId)
        {
            List<OrderForProduct> orders = new List<OrderForProduct>();
            string sql = "SELECT ORDERID, CUSTOMERID, PRODUCTNAME, PURCHASENUM, DAY, STATE, PMETHOD, PRICE FROM ORDERFORPRODUCTS WHERE CUSTOMERID = :customerId ORDER BY DAY DESC";

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", customerId));
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
