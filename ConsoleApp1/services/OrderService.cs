using ConsoleApp1.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Data;
using System.Globalization;
using static System.Collections.Specialized.BitVector32;

namespace CinemaTicketSystem.Services
{
    public class OrderService
    {
        private readonly DatabaseService _dbService;
        private readonly TicketService _ticketService;

        public OrderService(DatabaseService dbService, TicketService ticketService)
        {
            _dbService = dbService;
            _ticketService = ticketService;
        }

        public int CreateOrder(string customerId, string ticketId, decimal price, string paymentMethod = "现金")
        {
            const string sql = @"
    INSERT INTO orderfortickets (
        OrderID, 
        CustomerID, 
        TicketID, 
        State, 
        PMethod, 
        Price, 
        Day
    ) VALUES (
        orderfortickets_seq.NEXTVAL, 
        :customerId, 
        :ticketId, 
        '已支付', 
        :paymentMethod, 
        :price, 
        SYSDATE
    ) RETURNING OrderID INTO :orderId";

            var orderIdParam = new OracleParameter("orderId", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            var parameters = new[]
            {
        new OracleParameter("customerId", OracleDbType.Varchar2) { Value = customerId },
        new OracleParameter("ticketId", OracleDbType.Varchar2) { Value = ticketId },
        new OracleParameter("paymentMethod", OracleDbType.Varchar2)
        {
            Value = !string.IsNullOrWhiteSpace(paymentMethod) ? paymentMethod : "现金"
        },
        new OracleParameter("price", OracleDbType.Decimal) { Value = price },
        orderIdParam
    };

            try
            {
                int affectedRows = _dbService.ExecuteNonQuery(sql, parameters);

                //Console.WriteLine($"[DEBUG] 受影响行数: {affectedRows}");
                //Console.WriteLine($"[DEBUG] 输出参数值: {orderIdParam.Value}");

                if (orderIdParam.Value == null || orderIdParam.Value == DBNull.Value)
                {
                    throw new Exception("未能获取生成的订单ID");
                }

                // 修复类型转换问题
                int generatedId;
                if (orderIdParam.Value is OracleDecimal oracleDecimal)
                {
                    generatedId = oracleDecimal.ToInt32();
                }
                else
                {
                    generatedId = Convert.ToInt32(orderIdParam.Value);
                }

                if (generatedId <= 0)
                {
                    throw new Exception($"生成的订单ID无效: {generatedId}");
                }

                return generatedId;
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"[ORACLE ERROR] 错误代码: {ex.Number}, 错误信息: {ex.Message}");
                throw new Exception("数据库操作失败", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 创建订单异常: {ex.Message}");
                throw;
            }
        }

        public bool CompleteOrder(int orderId)
        {
            string sql = "UPDATE orderfortickets SET State = '已支付' WHERE OrderID = :orderId";
            var parameter = new OracleParameter("orderId", OracleDbType.Int32)
            {
                Value = orderId
            };

            return _dbService.ExecuteNonQuery(sql, parameter) > 0;
        }

        public bool RefundOrder(int orderId)
        {
            string sql = "UPDATE orderfortickets SET State = '已退票' WHERE OrderID = :orderId";
            var parameter = new OracleParameter("orderId", OracleDbType.Int32)
            {
                Value = orderId
            };

            return _dbService.ExecuteNonQuery(sql, parameter) > 0;
        }

        public DataTable GetAllOrders()
        {
            string sql = @"
                SELECT o.OrderID, o.CustomerID, c.Name AS CustomerName, 
                       o.TicketID, t.SectionID, s.FilmName, 
                       o.State, o.PMethod, o.Price, o.Day
                FROM orderfortickets o
                JOIN customer c ON o.CustomerID = c.CustomerID
                JOIN ticket t ON o.TicketID = t.TicketID
                JOIN section s ON t.SectionID = s.SectionID
                ORDER BY o.Day DESC";

            return _dbService.ExecuteQuery(sql);
        }

        // 在OrderService中添加这个方法
        public DataTable GetCustomerOrders(string customerId, string state = null)
        {
            string sql = @"
    SELECT 
        o.OrderID, 
        f.FilmName, 
        o.TicketID, 
        o.Price, 
        o.Day
    FROM 
        orderfortickets o
        JOIN ticket t ON o.TicketID = t.TicketID
        JOIN section s ON t.SectionID = s.SectionID
        JOIN film f ON s.FilmName = f.FilmName
    WHERE 
        o.CustomerID = :customerId";

            // 添加状态筛选条件
            if (!string.IsNullOrEmpty(state))
            {
                sql += " AND o.State = :state";
            }

            var parameters = new List<OracleParameter>
    {
        new OracleParameter("customerId", OracleDbType.Varchar2) { Value = customerId }
    };

            if (!string.IsNullOrEmpty(state))
            {
                parameters.Add(new OracleParameter("state", OracleDbType.Varchar2) { Value = state });
            }

            return _dbService.ExecuteQuery(sql, parameters.ToArray());
        }

        public bool PurchaseTicket(
    string customerId,
    int sectionId,
    int selectedRow,
    int selectedColumn,
    out int orderId,
    out string seatInfo,
    string paymentMethod = "现金")
        {
            orderId = 0;
            seatInfo = $"{selectedRow}排{selectedColumn}座";

            //Console.WriteLine("[DEBUG] 开始购票流程 ================");
            //Console.WriteLine($"[DEBUG] 参数: 顾客ID={customerId}, 场次ID={sectionId}, 座位={selectedRow}排{selectedColumn}座");

            using (var transaction = _dbService.BeginTransaction())
            {
                try
                {
                    // 1. 检查并锁定座位
                    //Console.WriteLine("[DEBUG] 正在锁定座位...");
                    if (!_ticketService.LockSpecificSeat(sectionId, selectedRow, selectedColumn))
                    {
                        Console.WriteLine("[ERROR] 座位锁定失败：可能已被占用或不存在");
                        transaction.Rollback();
                        return false;
                    }
                    //Console.WriteLine("[DEBUG] 座位锁定成功");

                    // 2. 获取票信息
                    //Console.WriteLine("[DEBUG] 正在查询票信息...");
                    var ticket = _ticketService.GetTicketBySeat(sectionId, selectedRow, selectedColumn);
                    if (ticket == null)
                    {
                        Console.WriteLine("[ERROR] 未找到对应的票信息");
                        transaction.Rollback();
                        return false;
                    }
                    //Console.WriteLine($"[DEBUG] 票状态: {ticket.State}, 票价: {ticket.Price}");

                    if (ticket.State != "锁定中")
                    {
                        Console.WriteLine($"[ERROR] 票状态不符合预期: {ticket.State}");
                        transaction.Rollback();
                        return false;
                    }

                    // 3. 创建订单
                    //Console.WriteLine("[DEBUG] 正在创建订单...");
                    orderId = CreateOrder(customerId, ticket.TicketID, ticket.Price, paymentMethod);
                    //Console.WriteLine($"[DEBUG] 生成的订单ID: {orderId}");

                    if (orderId <= 0)
                    {
                        Console.WriteLine("[ERROR] 订单创建失败");
                        transaction.Rollback();
                        return false;
                    }

                    // 4. 标记票为已售
                    //Console.WriteLine("[DEBUG] 正在更新票状态为已售...");
                    if (!_ticketService.SellTicket(ticket.TicketID))
                    {
                        Console.WriteLine("[ERROR] 票状态更新失败");
                        transaction.Rollback();
                        return false;
                    }

                    // 5. 提交事务
                    transaction.Commit();
                    //Console.WriteLine("[DEBUG] 事务提交成功，购票完成");
                    return true;
                }
                catch (OracleException ex) when (ex.Number == 1745)
                {
                    Console.WriteLine("[ORACLE ERROR] ORA-01745 无效的绑定变量名");
                    Console.WriteLine($"可能原因: SQL中使用了非法变量名（如Oracle保留字）");
                    Console.WriteLine($"建议检查以下内容:");
                    Console.WriteLine($"1. 是否在SQL中使用了'ROW','COLUMN'等保留字");
                    Console.WriteLine($"2. 变量名是否包含特殊字符");
                    transaction.Rollback();
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SYSTEM ERROR] 购票过程异常: {ex.GetType().Name}");
                    Console.WriteLine($"错误详情: {ex.Message}");
                    Console.WriteLine($"调用堆栈:\n{ex.StackTrace}");
                    transaction.Rollback();
                    return false;
                }
            }

        }

        public bool RefundTicket(int orderId, DateTime refundTime, out decimal refundFee, out int refundAmount)
        {
            refundFee = 0m;
            refundAmount = 0;

            // 1. 获取订单和票信息
            string getOrderSql = @"
SELECT 
    o.ticketID, 
    o.price,
    ts.starttime AS showtime
FROM 
    orderfortickets o
    JOIN ticket t ON o.ticketID = t.ticketID
    JOIN section s ON t.sectionID = s.sectionID
    JOIN timeslot ts ON s.timeID = ts.timeID
WHERE 
    o.orderID = :orderId 
    AND o.state = '已支付'";

            var orderParam = new OracleParameter("orderId", OracleDbType.Int32)
            {
                Value = orderId
            };

            var orderInfo = _dbService.ExecuteQuery(getOrderSql, orderParam);
            if (orderInfo.Rows.Count == 0)
            {
                Console.WriteLine("[ERROR] 未找到可退款的订单");
                return false;
            }

            var row = orderInfo.Rows[0];
            string ticketId = row["ticketID"].ToString();
            DateTime showtime = Convert.ToDateTime(row["showtime"]);
            int paidPrice = Convert.ToInt32(row["price"]);

            // 2. 检查电影是否已开始放映
            if (refundTime >= showtime)
            {
                Console.WriteLine($"[ERROR] 电影已开始放映，无法退票");
                Console.WriteLine($"\t开始时间: {showtime:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"\t当前时间: {refundTime:yyyy-MM-dd HH:mm}");
                return false;
            }

            // 3. 计算手续费和退款金额
            refundFee = CalculateRefundFee(showtime, refundTime, paidPrice);
            refundAmount = paidPrice - (int)refundFee;

            // 4. 执行退款事务
            using (var transaction = _dbService.BeginTransaction())
            {
                try
                {
                    // 4.1 仅更新订单状态
                    string updateOrderSql = "UPDATE orderfortickets SET state = '已退票' WHERE orderID = :orderId";
                    var updateParam = new OracleParameter("orderId", OracleDbType.Int32) { Value = orderId };

                    if (_dbService.ExecuteNonQuery(updateOrderSql, updateParam) <= 0)
                    {
                        transaction.Rollback();
                        Console.WriteLine("[ERROR] 更新订单状态失败");
                        return false;
                    }

                    // 4.2 释放座位
                    if (!_ticketService.ReleaseTicket(ticketId))
                    {
                        transaction.Rollback();
                        Console.WriteLine("[ERROR] 释放座位失败");
                        return false;
                    }

                    transaction.Commit();
                    Console.WriteLine($"[SUCCESS] 退票成功，应退款金额: {refundAmount}元");
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"[ERROR] 退票过程中发生异常: {ex.Message}");
                    return false;
                }
            }
        }

        private decimal CalculateRefundFee(DateTime showtime, DateTime refundTime, int paidPrice)
        {
            TimeSpan timeLeft = showtime - refundTime;
            decimal feeRate;

            if (timeLeft.TotalHours > 24)
            {
                feeRate = 0.1m; // 24小时以上收取10%手续费
            }
            else if (timeLeft.TotalHours > 2)
            {
                feeRate = 0.3m; // 2-24小时收取30%手续费
            }
            else
            {
                feeRate = 0.5m; // 2小时内收取50%手续费
            }

            return paidPrice * feeRate;
        }
        public bool CustomerExists(string customerId)
        {
            string sql = "SELECT COUNT(*) FROM customer WHERE customerID = :customerId";
            var parameter = new OracleParameter("customerId", OracleDbType.Varchar2, 20)
            {
                Value = customerId
            };

            var result = _dbService.ExecuteScalar(sql, parameter);
            return Convert.ToInt32(result) > 0;
        }
    }
}