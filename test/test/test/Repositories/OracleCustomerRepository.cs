using test.Helpers;
using test.Models; 
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace test.Repositories 
{
    /// <summary>
    /// Oracle 数据库的顾客数据仓储实现。
    /// </summary>
    public class OracleCustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;
        private const string SchemaName = "";

        public OracleCustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public OracleConnection GetConnection() // 将此方法设为 public，以便 UserService 可以访问
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 根据ID获取顾客信息。
        /// </summary>
        public Customer GetCustomerById(string customerId)
        {
            Customer customer = null;
            using (var connection = GetConnection())
            {
                // CUSTOMER 表有 CUSTOMERID, NAME, PHONENUM, VIPLEVEL, PASSWORD_HASH, SALT 字段
                // VIPCARD 表有 CUSTOMERID, POINTS
                string sql = $@"SELECT C.CUSTOMERID, C.NAME, C.PHONENUM, C.VIPLEVEL, C.PASSWORD_HASH, C.SALT,
                                       VC.POINTS AS VIP_POINTS
                                FROM {SchemaName}CUSTOMER C
                                LEFT JOIN {SchemaName}VIPCARD VC ON C.CUSTOMERID = VC.CUSTOMERID
                                WHERE C.CUSTOMERID = :customerId";

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
                                PasswordHash = reader["PASSWORD_HASH"].ToString(), // 读取 PasswordHash
                                Salt = reader["SALT"].ToString() // 读取 Salt
                            };

                            // 如果有 VIPCard 信息，则填充
                            if (reader["VIP_POINTS"] != DBNull.Value)
                            {
                                customer.VIPCard = new VIPCard
                                {
                                    CustomerID = customer.CustomerID,
                                    Points = Convert.ToInt32(reader["VIP_POINTS"])
                                };
                            }
                        }
                    }
                }
            }
            return customer;
        }

        /// <summary>
        /// 根据手机号获取顾客信息（用于注册时检查重复）。
        /// </summary>
        public Customer GetCustomerByPhoneNum(string phoneNum)
        {
            Customer customer = null;
            using (var connection = GetConnection())
            {
                string sql = $"SELECT CUSTOMERID, NAME, PHONENUM, VIPLEVEL, PASSWORD_HASH, SALT FROM {SchemaName}CUSTOMER WHERE PHONENUM = :phoneNum";
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
                                PasswordHash = reader["PASSWORD_HASH"].ToString(),
                                Salt = reader["SALT"].ToString()
                            };
                            customer.VIPCard = GetVIPCardByCustomerID(customer.CustomerID);
                        }
                    }
                }
            }
            return customer;
        }

        /// <summary>
        /// 添加新顾客。
        /// </summary>
        public void AddCustomer(Customer customer, string plainPassword) // !!! 修复点：添加 plainPassword 参数 !!!
        {
            // 密码处理：生成盐并哈希密码
            string salt = test.Helpers.PasswordHelper.GenerateSalt(); // 使用完整命名空间
            string passwordHash = test.Helpers.PasswordHelper.HashPassword(plainPassword, salt); // 使用完整命名空间

            using (var connection = GetConnection())
            {
                string sql = @"INSERT INTO " + SchemaName + @"CUSTOMER (CUSTOMERID, NAME, PHONENUM, VIPLEVEL, PASSWORD_HASH, SALT)
                               VALUES (:customerId, :name, :phoneNum, :vipLevel, :passwordHash, :salt)";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", customer.CustomerID));
                    command.Parameters.Add(new OracleParameter("name", customer.Name));
                    command.Parameters.Add(new OracleParameter("phoneNum", customer.PhoneNum));
                    command.Parameters.Add(new OracleParameter("vipLevel", customer.VipLevel));
                    command.Parameters.Add(new OracleParameter("passwordHash", passwordHash)); // 直接使用 Customer 对象的哈希和盐
                    command.Parameters.Add(new OracleParameter("salt", salt));

                    command.ExecuteNonQuery();
                }
            }
            // 注册成功后，为新用户添加 VIPCard 记录 (默认0积分)
            AddVIPCard(new VIPCard { CustomerID = customer.CustomerID, Points = 0 });
        }

        /// <summary>
        /// 更新顾客基本信息。
        /// </summary>
        public void UpdateCustomer(Customer customer)
        {
            using (var connection = GetConnection())
            {
                string sql = $"UPDATE {SchemaName}CUSTOMER SET NAME = :name, PHONENUM = :phoneNum, VIPLEVEL = :vipLevel WHERE CUSTOMERID = :customerId";
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

        /// <summary>
        /// 删除顾客。
        /// 注意：删除顾客前必须先删除所有关联的子记录，以避免违反外键约束。
        /// </summary>
        public void DeleteCustomer(string customerId)
        {
            using (var connection = GetConnection())
            {
                // 1. 删除 ORDERFORTICKETS 表中与该顾客关联的记录
                // 假设 ORDERFORTICKETS 表的 CustomerID 列没有 ON DELETE CASCADE
                string deleteOrdersSql = "DELETE FROM ORDERFORTICKETS WHERE CUSTOMERID = :customerId";
                using (var cmd = new OracleCommand(deleteOrdersSql, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("customerId", customerId));
                    cmd.ExecuteNonQuery();
                }

                // 2. 删除 VIPCARD 表中与该顾客关联的记录
                // 假设 VIPCARD 表的 CustomerID 列有 ON DELETE CASCADE
                string deleteVipSql = "DELETE FROM VIPCARD WHERE CUSTOMERID = :customerId";
                using (var cmd = new OracleCommand(deleteVipSql, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("customerId", customerId));
                    cmd.ExecuteNonQuery();
                }

                // 3. 最后删除 CUSTOMER 表中的记录
                string deleteCustomerSql = "DELETE FROM CUSTOMER WHERE CUSTOMERID = :customerId";
                using (var cmd = new OracleCommand(deleteCustomerSql, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("customerId", customerId));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 根据顾客ID获取会员卡信息。
        /// </summary>
        public VIPCard GetVIPCardByCustomerID(string customerId)
        {
            VIPCard vipCard = null;
            using (var connection = GetConnection())
            {
                string sql = $"SELECT CUSTOMERID, POINTS FROM {SchemaName}VIPCARD WHERE CUSTOMERID = :customerId";
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

        /// <summary>
        /// 添加会员卡记录（通常在注册用户时调用）。
        /// </summary>
        public void AddVIPCard(VIPCard vipCard)
        {
            using (var connection = GetConnection())
            {
                string sql = $"INSERT INTO {SchemaName}VIPCARD (CUSTOMERID, POINTS) VALUES (:customerId, :points)";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("customerId", vipCard.CustomerID));
                    command.Parameters.Add(new OracleParameter("points", vipCard.Points));
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 增减会员积分。
        /// </summary>
        public void UpdateVIPCardPoints(string customerId, int pointsChange)
        {
            using (var connection = GetConnection())
            {
                string sql = $"UPDATE {SchemaName}VIPCARD SET POINTS = POINTS + :pointsChange WHERE CUSTOMERID = :customerId";
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
        public void UpdateCustomerVipLevel(string customerId, int newVipLevel)
        {
            using (var connection = GetConnection())
            {
                string sql = $"UPDATE {SchemaName}CUSTOMER SET VIPLEVEL = :newVipLevel WHERE CUSTOMERID = :customerId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("newVipLevel", newVipLevel));
                    command.Parameters.Add(new OracleParameter("customerId", customerId));
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 获取顾客的密码哈希和盐值，用于认证。
        /// </summary>
        /// <param name="customerId">用户ID。</param>
        /// <returns>包含密码哈希和盐值的 Tuple<string, string>，如果未找到则为 null。</returns>
        public Tuple<string, string> GetCustomerPasswordHashAndSalt(string customerId)
        {
            using (var connection = GetConnection())
            {
                string sql = $"SELECT PASSWORD_HASH, SALT FROM {SchemaName}CUSTOMER WHERE CUSTOMERID = :customerId";
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

