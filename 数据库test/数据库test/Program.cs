using CinemaTicketSystem.Database;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace OracleDataInserter
{
    class Program
    {
        // 数据库连接字符串 - 请修改为您的实际连接信息
        private const string ConnectionString = "User Id=SYS;Password=Sun20040921;Data Source=127.0.0.1:1521/orcl;DBA Privilege=SYSDBA;";

        static void Main(string[] args)
        {
            var sequenceCreator = new SequenceCreator(ConnectionString);
            Console.WriteLine("=== Oracle 数据库数据插入和调试工具 ===");

            using (var connection = new OracleConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("成功连接到Oracle数据库!");

                    while (true)
                    {
                        Console.WriteLine("\n请选择操作:");
                        Console.WriteLine("1. 插入测试数据");
                        Console.WriteLine("2. 显示表数据");
                        Console.WriteLine("3. 执行自定义SQL");
                        Console.WriteLine("4. 清空所有表");
                        Console.WriteLine("5. 退出");
                        Console.WriteLine("6. 建立序列");
                        Console.Write("请输入选择: ");

                        var choice = Console.ReadLine();

                        switch (choice)
                        {
                            case "1":
                                InsertTestData(connection);
                                break;
                            case "2":
                                DisplayTableData(connection);
                                break;
                            case "3":
                                ExecuteCustomSql(connection);
                                break;
                            case "4":
                                ClearAllTables(connection);
                                break;
                            case "5":
                                return;
                            case "6":
                                if (!sequenceCreator.CreateOrderSequence())
                                {
                                    Console.WriteLine("序列创建失败，请检查错误信息");
                                    return;
                                }
                                break;
                            default:
                                Console.WriteLine("无效选择，请重新输入。");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"发生错误: {ex.Message}");
                }
            }
        }

        static void InsertTestData(OracleConnection connection)
        {
            Console.WriteLine("\n=== 插入测试数据 ===");

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. 先插入电影数据（film表没有外键依赖）
                    Console.WriteLine("插入电影数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO film (filmName, genre, filmLength, normalPrice, releaseDate, admissions, boxOffice, score) VALUES " +
                        "('流浪地球2', '科幻', 173, 45.00, TO_DATE('2023-01-22', 'YYYY-MM-DD'), 15000000, 685000000, 9)");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO film (filmName, genre, filmLength, normalPrice, releaseDate, admissions, boxOffice, score) VALUES " +
                        "('满江红', '悬疑', 159, 40.00, TO_DATE('2023-01-22', 'YYYY-MM-DD'), 12000000, 453000000, 8)");

                    // 2. 插入影厅数据（moviehall表没有外键依赖）
                    Console.WriteLine("插入影厅数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO moviehall (hallNo, lines, columns, category) VALUES " +
                        "(1, 10, 15, 'IMAX厅')");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO moviehall (hallNo, lines, columns, category) VALUES " +
                        "(2, 12, 18, '杜比厅')");

                    // 3. 插入时段数据（timeslot表没有外键依赖）
                    Console.WriteLine("插入时段数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO timeslot (timeID, startTime, endTime) VALUES " +
                        "('MORNING', TO_DATE('09:00:00', 'HH24:MI:SS'), TO_DATE('12:00:00', 'HH24:MI:SS'))");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO timeslot (timeID, startTime, endTime) VALUES " +
                        "('AFTERNOON', TO_DATE('13:00:00', 'HH24:MI:SS'), TO_DATE('17:00:00', 'HH24:MI:SS'))");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO timeslot (timeID, startTime, endTime) VALUES " +
                        "('EVENING', TO_DATE('18:00:00', 'HH24:MI:SS'), TO_DATE('22:00:00', 'HH24:MI:SS'))");

                    // 4. 插入折扣数据（依赖timeslot）
                    Console.WriteLine("插入折扣数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO discounts (timeID, discount) VALUES " +
                        "('MORNING', 0.8)");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO discounts (timeID, discount) VALUES " +
                        "('AFTERNOON', 0.9)");

                    // 5. 插入场次数据（依赖film、moviehall和timeslot）
                    Console.WriteLine("插入场次数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO section (sectionID, filmName, hallNo, timeID, day) VALUES " +
                        "(1001, '流浪地球2', 1, 'MORNING', TO_DATE('2023-02-01', 'YYYY-MM-DD'))");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO section (sectionID, filmName, hallNo, timeID, day) VALUES " +
                        "(1002, '满江红', 2, 'EVENING', TO_DATE('2023-02-01', 'YYYY-MM-DD'))");

                    // 6. 插入顾客数据（customer表没有外键依赖）
                    Console.WriteLine("插入顾客数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO customer (customerID, name, phoneNum, vipLevel) VALUES " +
                        "('C20230001', '张三', '13800138001', 1)");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO customer (customerID, name, phoneNum, vipLevel) VALUES " +
                        "('C20230002', '李四', '13800138002', 2)");

                    // 7. 插入VIP卡数据（依赖customer）
                    Console.WriteLine("插入VIP卡数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO VIPcard (customerID, points) VALUES " +
                        "('C20230001', 100)");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO VIPcard (customerID, points) VALUES " +
                        "('C20230002', 200)");

                    // 8. 插入顾客画像数据（依赖customer）
                    Console.WriteLine("插入顾客画像数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO customerprotrait (customerID, genre) VALUES " +
                        "('C20230001', '科幻')");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO customerprotrait (customerID, genre) VALUES " +
                        "('C20230002', '悬疑')");

                    // 9. 插入座位数据（依赖moviehall）
                    Console.WriteLine("插入座位数据...");
                    // 影厅1的座位
                    for (int i = 1; i <= 10; i++)
                    {
                        for (int j = 1; j <= 15; j++)
                        {
                            //string category = i <= 3 ? "前排" : (i <= 7 ? "最佳观影区" : "后排");
                            string category = "1";
                            ExecuteNonQuery(connection, transaction,
                                $"INSERT INTO seathall (hallNo, lineNo, columnNo, category) VALUES " +
                                $"(1, {i}, {j}, '{category}')");
                        }
                    }

                    // 影厅2的座位
                    for (int i = 1; i <= 12; i++)
                    {
                        for (int j = 1; j <= 18; j++)
                        {
                            //string category = i <= 4 ? "前排" : (i <= 8 ? "最佳观影区" : "后排");
                            string category = "1";
                            ExecuteNonQuery(connection, transaction,
                                $"INSERT INTO seathall (hallNo, lineNo, columnNo, category) VALUES " +
                                $"(2, {i}, {j}, '{category}')");
                        }
                    }

                    // 10. 插入票数据（依赖section）
                    Console.WriteLine("插入票数据...");
                    // 场次1001的票
                    for (int i = 1; i <= 10; i++)
                    {
                        for (int j = 1; j <= 15; j++)
                        {
                            ExecuteNonQuery(connection, transaction,
                                $"INSERT INTO ticket (ticketID, price, rating, sectionID, lineNo, columnNo, state) VALUES " +
                                $"('T1001_{i}_{j}', 45, 0, 1001, {i}, {j}, '可售')");
                        }
                    }

                    // 场次1002的票
                    for (int i = 1; i <= 12; i++)
                    {
                        for (int j = 1; j <= 18; j++)
                        {
                            ExecuteNonQuery(connection, transaction,
                                $"INSERT INTO ticket (ticketID, price, rating, sectionID, lineNo, columnNo, state) VALUES " +
                                $"('T1002_{i}_{j}', 40, 0, 1002, {i}, {j}, '可售')");
                        }
                    }

                    // 11. 插入相关产品数据（relatedproduct表没有外键依赖）
                    Console.WriteLine("插入相关产品数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO relatedproduct (productname, price, productnumber) VALUES " +
                        "('爆米花', 25, 100)");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO relatedproduct (productname, price, productnumber) VALUES " +
                        "('可乐', 15, 150)");

                    // 12. 插入订单数据（依赖customer和ticket）
                    //Console.WriteLine("插入票订单数据...");
                    //ExecuteNonQuery(connection, transaction,
                        //"INSERT INTO orderfortickets (orderID, customerID, ticketID, day, state, pmethod, price) VALUES " +
                        //"(5001, 'C20230001', 'T1001_5_8', SYSDATE, '已支付', '微信支付', 45)");

                    //ExecuteNonQuery(connection, transaction,
                        //"INSERT INTO orderfortickets (orderID, customerID, ticketID, day, state, pmethod, price) VALUES " +
                        //"(5002, 'C20230002', 'T1002_6_10', SYSDATE, '待支付', '支付宝', 40)");

                    // 13. 插入产品订单数据（依赖customer和relatedproduct）
                    Console.WriteLine("插入产品订单数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO orderforproducts (orderID, customerID, productname, purchasenum, day, state, pmethod, price) VALUES " +
                        "(6001, 'C20230001', '爆米花', 2, SYSDATE, '已支付', '微信支付', 50)");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO orderforproducts (orderID, customerID, productname, purchasenum, day, state, pmethod, price) VALUES " +
                        "(6002, 'C20230002', '可乐', 1, SYSDATE, '已支付', '支付宝', 15)");

                    // 14. 插入演员数据（依赖film）
                    Console.WriteLine("插入演员数据...");
                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO cast (memberName, role, filmName) VALUES " +
                        "('吴京', '主演', '流浪地球2')");

                    ExecuteNonQuery(connection, transaction,
                        "INSERT INTO cast (memberName, role, filmName) VALUES " +
                        "('沈腾', '主演', '满江红')");

                    transaction.Commit();
                    Console.WriteLine("\n所有测试数据插入成功!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"插入数据失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"内部错误: {ex.InnerException.Message}");
                    }
                    Console.WriteLine($"错误位置: {ex.StackTrace}");
                }
            }
        }

        static void DisplayTableData(OracleConnection connection)
        {
            Console.WriteLine("\n=== 显示表数据 ===");
            Console.WriteLine("1. 电影表");
            Console.WriteLine("2. 影厅表");
            Console.WriteLine("3. 场次表");
            Console.WriteLine("4. 票表");
            Console.WriteLine("5. 顾客表");
            Console.WriteLine("6. 订单表");
            Console.Write("请选择要显示的表: ");

            var tableChoice = Console.ReadLine();
            string tableName = tableChoice switch
            {
                "1" => "FILM",
                "2" => "MOVIEHALL",
                "3" => "SECTION",
                "4" => "TICKET",
                "5" => "CUSTOMER",
                "6" => "ORDERFORTICKETS",
                _ => null
            };

            if (tableName == null)
            {
                Console.WriteLine("无效选择!");
                return;
            }

            try
            {
                var dataTable = new DataTable();
                using (var adapter = new OracleDataAdapter($"SELECT * FROM {tableName}", connection))
                {
                    adapter.Fill(dataTable);
                }

                Console.WriteLine($"\n{tableName} 表数据:");
                foreach (DataRow row in dataTable.Rows)
                {
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        Console.Write($"{col.ColumnName}: {row[col]}  ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine($"共 {dataTable.Rows.Count} 条记录");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询失败: {ex.Message}");
            }
        }

        static void ExecuteCustomSql(OracleConnection connection)
        {
            Console.WriteLine("\n=== 执行自定义SQL ===");
            Console.WriteLine("请输入SQL语句 (输入'exit'退出):");

            while (true)
            {
                Console.Write("SQL> ");
                var sql = Console.ReadLine();

                if (sql.ToLower() == "exit") break;
                if (string.IsNullOrWhiteSpace(sql)) continue;

                try
                {
                    if (sql.Trim().ToUpper().StartsWith("SELECT"))
                    {
                        // 查询语句
                        var dataTable = new DataTable();
                        using (var adapter = new OracleDataAdapter(sql, connection))
                        {
                            adapter.Fill(dataTable);
                        }

                        Console.WriteLine("\n查询结果:");
                        foreach (DataRow row in dataTable.Rows)
                        {
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                Console.Write($"{col.ColumnName}: {row[col]}  ");
                            }
                            Console.WriteLine();
                        }
                        Console.WriteLine($"共 {dataTable.Rows.Count} 条记录");
                    }
                    else
                    {
                        // 非查询语句
                        using (var command = new OracleCommand(sql, connection))
                        {
                            int affected = command.ExecuteNonQuery();
                            Console.WriteLine($"执行成功，影响 {affected} 行");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行失败: {ex.Message}");
                }
            }
        }

        static void ClearAllTables(OracleConnection connection)
        {
            Console.WriteLine("\n=== 清空所有表 ===");
            Console.Write("确定要清空所有表数据吗？(y/n): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToLower() != "y") return;

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 按照从最底层子表到最上层父表的顺序清空
                    // 1. 先清空订单相关表（最底层）
                    ExecuteNonQuery(connection, transaction, "DELETE FROM orderforproducts");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM orderfortickets");

                    // 2. 清空票和VIP卡相关表
                    ExecuteNonQuery(connection, transaction, "DELETE FROM ticket");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM VIPcard");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM customerprotrait");

                    // 3. 清空演员和座位表
                    ExecuteNonQuery(connection, transaction, "DELETE FROM cast");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM seathall");

                    // 4. 清空场次和折扣表
                    ExecuteNonQuery(connection, transaction, "DELETE FROM section");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM discounts");

                    // 5. 清空基础表（没有外键依赖的表）
                    ExecuteNonQuery(connection, transaction, "DELETE FROM timeslot");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM moviehall");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM film");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM customer");
                    ExecuteNonQuery(connection, transaction, "DELETE FROM relatedproduct");

                    transaction.Commit();
                    Console.WriteLine("所有表数据已清空!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"清空表失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"内部错误: {ex.InnerException.Message}");
                    }
                }
            }
        }

        static void ExecuteNonQuery(OracleConnection connection, OracleTransaction transaction, string sql)
        {
            using (var command = new OracleCommand(sql, connection))
            {
                command.Transaction = transaction;  // 单独设置事务
                command.ExecuteNonQuery();
            }
        }
    }
}