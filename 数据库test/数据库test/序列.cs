using System;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace CinemaTicketSystem.Database
{
    public class SequenceCreator
    {
        private readonly string _connectionString;

        public SequenceCreator(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 创建订单序列
        /// </summary>
        public bool CreateOrderSequence()
        {
            const string sequenceName = "orderfortickets_seq";
            const string createSql = @"
                CREATE SEQUENCE orderfortickets_seq
                START WITH 1000
                INCREMENT BY 1
                NOMAXVALUE
                NOCYCLE
                CACHE 20";

            const string checkSql = "SELECT sequence_name FROM user_sequences WHERE sequence_name = 'ORDERFORTICKETS_SEQ'";

            Console.WriteLine($"正在尝试创建序列 {sequenceName}...");

            using (var connection = new OracleConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // 首先检查序列是否已存在
                    using (var checkCmd = new OracleCommand(checkSql, connection))
                    {
                        var exists = checkCmd.ExecuteScalar() != null;
                        if (exists)
                        {
                            Console.WriteLine($"序列 {sequenceName} 已存在，无需创建");
                            return true;
                        }
                    }

                    // 创建序列
                    using (var createCmd = new OracleCommand(createSql, connection))
                    {
                        createCmd.ExecuteNonQuery();
                        Console.WriteLine($"序列 {sequenceName} 创建成功");
                        return true;
                    }
                }
                catch (OracleException ex) when (ex.Number == 2289)
                {
                    Console.WriteLine($"错误：序列名称 {sequenceName} 无效");
                    return false;
                }
                catch (OracleException ex) when (ex.Number == 1031)
                {
                    Console.WriteLine("错误：权限不足，需要CREATE SEQUENCE权限");
                    Console.WriteLine("请让DBA执行：GRANT CREATE SEQUENCE TO 当前用户");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"创建序列时发生错误: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 测试序列是否可用
        /// </summary>
        public bool TestSequence()
        {
            const string testSql = "SELECT orderfortickets_seq.NEXTVAL FROM DUAL";

            try
            {
                using (var connection = new OracleConnection(_connectionString))
                using (var command = new OracleCommand(testSql, connection))
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    Console.WriteLine($"序列测试成功，下一个值为: {result}");
                    return true;
                }
            }
            catch (OracleException ex) when (ex.Number == 2289)
            {
                Console.WriteLine("错误：序列不存在");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试序列时出错: {ex.Message}");
                return false;
            }
        }
    }
}