using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace test.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public OracleConnection GetConnection()
        {
            return new OracleConnection(_connectionString);
        }

        public DataTable ExecuteQuery(string sql, params OracleParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new OracleCommand(sql, connection))
                {
                    cmd.Parameters.AddRange(parameters);
                    using (var adapter = new OracleDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public int ExecuteNonQuery(string sql, params OracleParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new OracleCommand(sql, connection))
                {
                    cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string sql, params OracleParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new OracleCommand(sql, connection))
                {
                    cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }
        // 添加事务开始方法
        public OracleTransaction BeginTransaction()
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection.BeginTransaction();
        }
    }
}