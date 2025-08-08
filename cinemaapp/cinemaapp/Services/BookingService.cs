using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Linq;
using test.Models;
using test.Repositories;

namespace test.Services 
{
    /// <summary>
    /// 购票业务服务实现。
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IShowingRepository _showingRepository;
        private readonly IFilmRepository _filmRepository; // 仍然需要 FilmRepository 来获取电影的 NormalPrice
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository; // 订单仓库
        private readonly DatabaseService _dbService;
        private readonly string _connectionString; // 需要连接字符串来创建事务

        public BookingService(IShowingRepository showingRepository, IFilmRepository filmRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository, DatabaseService dbService, string connectionString)
        {
            _showingRepository = showingRepository;
            _filmRepository = filmRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _dbService = dbService;
            _connectionString = connectionString;
        }

        /// <summary>
        /// 购买电影票。
        /// </summary>
        /// <param name="sectionId">场次ID。</param>
        /// <param name="lineNo">座位行号。</param>
        /// <param name="columnNo">座位列号。</param>
        /// <param name="customerId">顾客ID。</param>
        /// <param name="paymentMethod">支付方式。</param>
        /// <returns>购买成功的订单对象。</returns>
        /// <exception cref="ArgumentException">如果场次、座位或顾客无效。</exception>
        /// <exception cref="InvalidOperationException">如果座位已被预订或购票失败。</exception>
        public OrderForTickets PurchaseTicket(int sectionId, string lineNo, int columnNo, string customerId, string paymentMethod)
        {
            // 使用事务确保原子性操作
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                OracleTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted); // 使用 ReadCommitted 隔离级别

                try
                {
                    // 1. 验证顾客是否存在
                    var customer = _customerRepository.GetCustomerById(customerId);
                    if (customer == null)
                    {
                        throw new ArgumentException($"顾客ID {customerId} 不存在。");
                    }

                    // 2. 获取场次信息以验证和获取票价
                    var section = _showingRepository.GetSectionById(sectionId);
                    if (section == null)
                    {
                        throw new ArgumentException($"场次ID {sectionId} 不存在。");
                    }

                    // 3. 检查座位是否可用
                    var soldTickets = _showingRepository.GetSoldSeatsForSection(sectionId); // 获取已售座位
                    if (soldTickets.Any(t => t.LineNo == lineNo && t.ColumnNo == columnNo && t.State == "已售出"))
                    {
                        throw new InvalidOperationException($"座位 {lineNo}{columnNo} 在场次 {sectionId} 已被预订。");
                    }

                    // 4. 计算票价
                    // 获取电影的 NormalPrice
                    decimal ticketUnitPrice = _filmRepository.GetFilmByName(section.FilmName)?.NormalPrice ?? 0;
                    if (ticketUnitPrice <= 0)
                    {
                        throw new InvalidOperationException($"无法获取电影 '{section.FilmName}' 的票价。");
                    }

                    // 5. 创建 Ticket 对象 (先插入 Ticket 表)
                    Ticket newTicket = new Ticket
                    {
                        TicketID = Guid.NewGuid().ToString(), // 生成唯一票ID
                        Price = ticketUnitPrice, // 单张票的售价
                        Rating = 0, // 初始评分为0
                        SectionID = sectionId,
                        LineNo = lineNo,
                        ColumnNo = columnNo,
                        State = "已售出" // 票的状态为已售出
                    };
                    _showingRepository.AddTicket(newTicket, transaction); // 插入票务记录

                    // 6. 创建 OrderForTickets 对象 (插入订单表)
                    OrderForTickets newOrder = new OrderForTickets
                    {
                        // OrderID 将由数据库生成或通过序列获取
                        TicketID = newTicket.TicketID,
                        State = "有效", // 订单初始状态为有效
                        CustomerID = customerId,
                        Day = DateTime.Today, // 订单日期
                        PaymentMethod = paymentMethod,
                        TotalPrice = ticketUnitPrice // 假设每张票一个订单，所以订单总价等于票价
                    };
                    _orderRepository.AddOrderForTickets(newOrder, transaction); // 插入订单记录

                    // 7. 更新顾客积分 (可选，但推荐)
                    // 假设每张票增加 10 积分
                    _customerRepository.UpdateVIPCardPoints(customerId, 10); // 增加积分

                    transaction.Commit(); // 提交事务
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"成功购买票并生成订单: {newOrder.ToString()}");
                    Console.ResetColor();
                    return newOrder;
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // 回滚事务
                    throw new InvalidOperationException($"购票失败: {ex.Message}", ex);
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

        public bool ReleaseTicket(string ticketId)
        {
            string sql = "UPDATE ticket SET state = '已退票' WHERE ticketID = :ticketId";
            var parameter = new OracleParameter("ticketId", OracleDbType.Varchar2, 40)
            {
                Value = ticketId
            };

            return _dbService.ExecuteNonQuery(sql, parameter) > 0;
        }

        public bool RefundTicket(int orderId, DateTime refundTime, out decimal refundFee, out int refundAmount, out string errorMessage)
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
    AND o.state = '有效'";

            var orderParam = new OracleParameter("orderId", OracleDbType.Int32)
            {
                Value = orderId
            };

            var orderInfo = _dbService.ExecuteQuery(getOrderSql, orderParam);
            if (orderInfo.Rows.Count == 0)
            {
                errorMessage = "未找到可退款的订单";
                return false;
            }

            var row = orderInfo.Rows[0];
            string ticketId = row["ticketID"].ToString();
            DateTime showtime = Convert.ToDateTime(row["showtime"]);
            int paidPrice = Convert.ToInt32(row["price"]);

            // 2. 检查电影是否已开始放映
            if (refundTime >= showtime)
            {
                errorMessage = $"电影已开始放映，无法退票\n开始时间: {showtime:yyyy-MM-dd HH:mm}\n当前时间: {refundTime:yyyy-MM-dd HH:mm}";
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
                    string updateOrderSql = "UPDATE orderfortickets SET state = '失效' WHERE orderID = :orderId";
                    var updateParam = new OracleParameter("orderId", OracleDbType.Int32) { Value = orderId };

                    if (_dbService.ExecuteNonQuery(updateOrderSql, updateParam) <= 0)
                    {
                        transaction.Rollback();
                        errorMessage = "更新订单状态失败";
                        return false;
                    }

                    // 4.2 释放座位
                    if (!ReleaseTicket(ticketId))
                    {
                        transaction.Rollback();
                        errorMessage = "释放座位失败";
                        return false;
                    }

                    transaction.Commit();
                    errorMessage = $"退票成功，应退款金额: {refundAmount}元";
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    errorMessage = "退票过程中发生异常: " + ex.Message;
                    return false;
                }
            }
        }

        public bool TryGetRefundInfo(int orderId, DateTime refundTime, out decimal fee, out int refundAmount, out string errorMsg)
        {
            fee = 0m;
            refundAmount = 0;
            errorMsg = "";

            // 查询订单信息
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
    AND o.state = '有效'";

            var param = new OracleParameter("orderId", OracleDbType.Int32) { Value = orderId };
            var dt = _dbService.ExecuteQuery(getOrderSql, param);

            if (dt.Rows.Count == 0)
            {
                errorMsg = "订单不存在或已失效。";
                return false;
            }

            var row = dt.Rows[0];
            DateTime showtime = Convert.ToDateTime(row["showtime"]);
            int paidPrice = Convert.ToInt32(row["price"]);

            if (refundTime >= showtime)
            {
                errorMsg = "电影已开始放映，无法退票。";
                return false;
            }

            fee = CalculateRefundFee(showtime, refundTime, paidPrice);
            refundAmount = paidPrice - (int)fee;
            return true;
        }

    }
}

