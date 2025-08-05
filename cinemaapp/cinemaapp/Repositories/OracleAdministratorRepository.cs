using test.Helpers;
using test.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace test.Repositories
{
    /// 管理员数据仓储的Oracle实现
    public class OracleAdministratorRepository : IAdministratorRepository
    {
        private readonly string _connectionString;

        public OracleAdministratorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 获取数据库连接
        private OracleConnection GetConnection()
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// 根据ID获取管理员
        public Administrator GetAdministratorByID(string adminId)
        {
            Administrator admin = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT ADMINID, ADMINNAME, PHONENUM FROM ADMINISTRATOR WHERE ADMINID = :adminId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("adminId", adminId));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            admin = new Administrator
                            {
                                AdminID = reader["ADMINID"].ToString(),
                                AdminName = reader["ADMINNAME"].ToString(),
                                PhoneNum = reader["PHONENUM"].ToString()
                            };
                        }
                    }
                }
            }
            return admin;
        }


        /// 根据手机号获取管理员

        public Administrator GetAdministratorByPhoneNum(string phoneNum)
        {
            Administrator admin = null;
            using (var connection = GetConnection())
            {
                string sql = "SELECT ADMINID, ADMINNAME, PHONENUM FROM ADMINISTRATOR WHERE PHONENUM = :phoneNum";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("phoneNum", phoneNum));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            admin = new Administrator
                            {
                                AdminID = reader["ADMINID"].ToString(),
                                AdminName = reader["ADMINNAME"].ToString(),
                                PhoneNum = reader["PHONENUM"].ToString()
                            };
                        }
                    }
                }
            }
            return admin;
        }


        /// 添加管理员
        public void AddAdministrator(Administrator admin, string plainPassword)
        {
            // 生成盐值和密码哈希
            string salt = PasswordHelper.GenerateSalt();
            string passwordHash = PasswordHelper.HashPassword(plainPassword, salt);

            using (var connection = GetConnection())
            {
                string sql = @"INSERT INTO ADMINISTRATOR 
                               (ADMINID, ADMINNAME, PHONENUM, PASSWORD_HASH, SALT)
                               VALUES (:adminId, :adminName, :phoneNum, :passwordHash, :salt)";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("adminId", admin.AdminID));
                    command.Parameters.Add(new OracleParameter("adminName", admin.AdminName));
                    command.Parameters.Add(new OracleParameter("phoneNum", admin.PhoneNum));
                    command.Parameters.Add(new OracleParameter("passwordHash", passwordHash));
                    command.Parameters.Add(new OracleParameter("salt", salt));
                    command.ExecuteNonQuery();
                }
            }
        }

        /// 更新管理员信息
        public void UpdateAdministrator(Administrator admin)
        {
            using (var connection = GetConnection())
            {
                string sql = @"UPDATE ADMINISTRATOR 
                               SET ADMINNAME = :adminName, PHONENUM = :phoneNum 
                               WHERE ADMINID = :adminId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("adminName", admin.AdminName));
                    command.Parameters.Add(new OracleParameter("phoneNum", admin.PhoneNum));
                    command.Parameters.Add(new OracleParameter("adminId", admin.AdminID));
                    command.ExecuteNonQuery();
                }
            }
        }


        /// 删除管理员

        public void DeleteAdministrator(string adminId)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM ADMINISTRATOR WHERE ADMINID = :adminId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("adminId", adminId));
                    command.ExecuteNonQuery();
                }
            }
        }


        /// 获取管理员密码哈希和盐值

        public Tuple<string, string> GetAdministratorPasswordHashAndSalt(string adminId)
        {
            using (var connection = GetConnection())
            {
                string sql = "SELECT PASSWORD_HASH, SALT FROM ADMINISTRATOR WHERE ADMINID = :adminId";
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("adminId", adminId));
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
