using test.Helpers; // 引用 Helpers 命名空间
using test.Models; // 引用 Models 命名空间
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace test.Repositories 
{
    public class OracleCustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public OracleCustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private OracleConnection GetConnection()
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Customer GetCustomerByID(string customerId)
        {
            Customer customer = null;
            using (var connection = GetConnection())
            {
                // 从 CUSTOMER 表中读取所有基本信息
                string sql = "SELECT CUSTOMERID, NAME, PHONENUM, VIPLEVEL FROM CUSTOMER WHERE CUSTOMERID = :customerId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", customerId));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                CustomerID = reader["CUSTOMERID"].ToString(),
                                Name = reader["NAME"].ToString(),
                                PhoneNum = reader["PHONENUM"].ToString(),
                                VipLevel = Convert.ToInt32(reader["VIPLEVEL"]),
                                Username = reader["CUSTOMERID"].ToString() // 假设 CustomerID 就是登录的 Username
                            };
                            // 尝试获取 VIPCard 信息
                            customer.VIPCard = GetVIPCardByCustomerID(customerId);
                        }
                    }
                }
            }
            return customer;
        }

        public Customer GetCustomerByPhoneNum(string phoneNum)
        {
            Customer customer = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT CUSTOMERID, NAME, PHONENUM, VIPLEVEL FROM CUSTOMER WHERE PHONENUM = :phoneNum";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("phoneNum", phoneNum));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                CustomerID = reader["CUSTOMERID"].ToString(),
                                Name = reader["NAME"].ToString(),
                                PhoneNum = reader["PHONENUM"].ToString(),
                                VipLevel = Convert.ToInt32(reader["VIPLEVEL"]),
                                Username = reader["CUSTOMERID"].ToString() // 假设 CustomerID 就是登录的 Username
                            };
                            customer.VIPCard = GetVIPCardByCustomerID(customer.CustomerID);
                        }
                    }
                }
            }
            return customer;
        }

        public void AddCustomer(Customer customer, string plainPassword)
        {
            string salt = PasswordHelper.GenerateSalt();
            string passwordHash = PasswordHelper.HashPassword(plainPassword, salt);

            using (var connection = GetConnection())
            {
                // 确保你的 CUSTOMER 表有 PASSWORD_HASH 和 SALT 字段
                string sql = @"INSERT INTO CUSTOMER (CUSTOMERID, NAME, PHONENUM, VIPLEVEL, PASSWORD_HASH, SALT)
                               VALUES (:customerId, :name, :phoneNum, :vipLevel, :passwordHash, :salt)";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", customer.CustomerID));
                    command.Parameters.Add(new OracleParameter("name", customer.Name));
                    command.Parameters.Add(new OracleParameter("phoneNum", customer.PhoneNum));
                    command.Parameters.Add(new OracleParameter("vipLevel", customer.VipLevel));
                    command.Parameters.Add(new OracleParameter("passwordHash", passwordHash));
                    command.Parameters.Add(new OracleParameter("salt", salt));

                    command.ExecuteNonQuery();
                }
            }
            AddVIPCard(new VIPCard { CustomerID = customer.CustomerID, Points = 0 });
        }

        public void UpdateCustomer(Customer customer)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE CUSTOMER SET NAME = :name, PHONENUM = :phoneNum, VIPLEVEL = :vipLevel WHERE CUSTOMERID = :customerId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("name", customer.Name));
                    command.Parameters.Add(new OracleParameter("phoneNum", customer.PhoneNum));
                    command.Parameters.Add(new OracleParameter("vipLevel", customer.VipLevel));
                    command.Parameters.Add(new OracleParameter("customerId", customer.CustomerID));
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteCustomer(string customerId)
        {
            using (var connection = GetConnection())
            {
                // 先删除 VIPCard
                string deleteVipSql = "DELETE FROM VIPCARD WHERE CUSTOMERID = :customerId";
                using (var cmd = new OracleCommand(deleteVipSql, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("customerId", customerId));
                    cmd.ExecuteNonQuery();
                }

                // 再删除 Customer
                string deleteCustomerSql = "DELETE FROM CUSTOMER WHERE CUSTOMERID = :customerId";
                using (var cmd = new OracleCommand(deleteCustomerSql, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("customerId", customerId));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public VIPCard GetVIPCardByCustomerID(string customerId)
        {
            VIPCard vipCard = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT CUSTOMERID, POINTS FROM VIPCARD WHERE CUSTOMERID = :customerId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", customerId));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vipCard = new VIPCard
                            {
                                CustomerID = reader["CUSTOMERID"].ToString(),
                                Points = Convert.ToInt32(reader["POINTS"])
                            };
                        }
                    }
                }
            }
            return vipCard;
        }

        public void AddVIPCard(VIPCard vipCard)
        {
            using (var connection = GetConnection())
            {
                string sql = "INSERT INTO VIPCARD (CUSTOMERID, POINTS) VALUES (:customerId, :points)";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", vipCard.CustomerID));
                    command.Parameters.Add(new OracleParameter("points", vipCard.Points));
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateVIPCardPoints(string customerId, int pointsChange)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE VIPCARD SET POINTS = POINTS + :pointsChange WHERE CUSTOMERID = :customerId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("pointsChange", pointsChange));
                    command.Parameters.Add(new OracleParameter("customerId", customerId));
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 更新顾客的会员等级。
        /// </summary>
        /// <param name="customerId">顾客ID。</param>
        /// <param name="newVipLevel">新的会员等级。</param>
        public void UpdateCustomerVipLevel(string customerId, int newVipLevel)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE CUSTOMER SET VIPLEVEL = :newVipLevel WHERE CUSTOMERID = :customerId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("newVipLevel", newVipLevel));
                    command.Parameters.Add(new OracleParameter("customerId", customerId));
                    command.ExecuteNonQuery();
                }
            }
        }

        public Tuple<string, string> GetCustomerPasswordHashAndSalt(string customerId)
        {
            using (var connection = GetConnection())
            {
                string sql = "SELECT PASSWORD_HASH, SALT FROM CUSTOMER WHERE CUSTOMERID = :customerId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", customerId));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string passwordHash = reader["PASSWORD_HASH"].ToString();
                            string salt = reader["SALT"].ToString();
                            return Tuple.Create(passwordHash, salt);
                        }
                    }
                }
            }
            return null;
        }
    }
}
