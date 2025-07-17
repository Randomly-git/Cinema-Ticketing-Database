using Oracle.ManagedDataAccess.Client;
using System;

class Program
{
    static void Main()
    {
        //关键修改:用“DBA Privilege-SYSDBA"替代“Connect As=SYSDBA"
        string connectionString = "Data Source=//8.148.76.54:1524/orclpdb1;User Id=SYS;Password=123456;DBA Privilege=SYSDBA";

        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("成功以SYSDBA权限连接到Oracle数据库!");
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Oracle异常:{ex.Message}");
                Console.WriteLine($"错误代码:{ex.Number}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"其他异常:{ex.Message}");
            }
        }
    }
}