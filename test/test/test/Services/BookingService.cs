using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic; // 需要 List
using System.Data;
using System.Linq;
using test.Models;
using test.Repositories;

namespace test.Services
{
    public class BookingService : IBookingService
    {
        private readonly IShowingRepository _showingRepository;
        private readonly IFilmRepository _filmRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly DatabaseService _dbService;
        private readonly string _connectionString;

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
        /// 购买电影票。现在会计算动态价格和折扣。
        /// </summary>
        public OrderForTickets PurchaseTicket(int sectionId, string lineNo, int columnNo, string customerId, string paymentMethod)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                OracleTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    // 1. 验证顾客
                    var customer = _customerRepository.GetCustomerById(customerId);
                    if (customer == null) throw new ArgumentException($"顾客ID {customerId} 不存在。");

                    // 2. 验证场次
                    var section = _showingRepository.GetSectionById(sectionId);
                    if (section == null) throw new ArgumentException($"场次ID {sectionId} 不存在。");

                    // 3. 检查座位是否可用
                    var soldTickets = _showingRepository.GetSoldSeatsForSection(sectionId);
                    if (soldTickets.Any(t => t.LineNo == lineNo && t.ColumnNo == columnNo && t.State == "已售出"))
                    {
                        throw new InvalidOperationException($"座位 {lineNo}{columnNo} 在场次 {sectionId} 已被预订。");
                    }

                    // 4. 计算最终票价
                    decimal finalTicketPrice = CalculateFinalTicketPrice(section, customer, lineNo);

                    // 5. 获取VIP卡信息（用于积分支付或累积积分）
                    var vipCard = _customerRepository.GetVIPCardByCustomerID(customerId);
                    if (vipCard == null)
                        throw new InvalidOperationException("获取积分信息失败。");

                    // 6. 积分支付逻辑
                    if (paymentMethod == "积分支付")
                    {
                        decimal requiredPoints = finalTicketPrice * 10; // 10积分=1元
                        if (vipCard.Points < requiredPoints)
                            throw new InvalidOperationException($"积分不足，需要 {requiredPoints} 积分，当前积分 {vipCard.Points}。");

                        // 扣除积分
                        _customerRepository.UpdateVIPCardPoints(customerId, -Convert.ToInt32(requiredPoints));
                    }

                    // 7. 创建 Ticket 对象
                    Ticket newTicket = new Ticket
                    {
                        TicketID = Guid.NewGuid().ToString(),
                        Price = finalTicketPrice,
                        SectionID = sectionId,
                        LineNo = lineNo,
                        ColumnNo = columnNo,
                        State = "已售出"
                    };
                    _showingRepository.AddTicket(newTicket, transaction);

                    // 8. 创建 OrderForTickets 对象
                    OrderForTickets newOrder = new OrderForTickets
                    {
                        TicketID = newTicket.TicketID,
                        State = "有效",
                        CustomerID = customerId,
                        Day = DateTime.Today,
                        PaymentMethod = paymentMethod,
                        TotalPrice = finalTicketPrice
                    };
                    _orderRepository.AddOrderForTickets(newOrder, transaction);

                    // 9. 购买完成后累积积分（非积分支付）
                    if (paymentMethod != "积分支付")
                    {
                        int pointsEarned = 10; // 单张票10积分
                        _customerRepository.UpdateVIPCardPoints(customerId, pointsEarned);
                    }

                    transaction.Commit();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"成功购买票并生成订单: {newOrder.ToString()}");
                    Console.ResetColor();
                    return newOrder;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException($"购票失败: {ex.Message}", ex);
                }
            }
        }


        /// <summary>
        /// [新增] 购买多张电影票（如双人票）。
        /// 处理多张票的购买，并确保所有票都在一个事务中完成。
        /// </summary>
        /// <param name="sectionId">场次ID</param>
        /// <param name="seats">要购买的座位列表</param>
        /// <param name="customerId">顾客ID</param>
        /// <param name="paymentMethod">支付方式</param>
        /// <returns>成功生成的订单列表</returns>
        public List<OrderForTickets> PurchaseMultipleTickets(int sectionId, List<SeatHall> seats, string customerId, string paymentMethod, decimal pointsToUse = 0)
        {
            if (seats == null || !seats.Any())
                throw new ArgumentException("必须至少选择一个座位。");

            var distinctSeats = seats.GroupBy(s => $"{s.LINENO}-{s.ColumnNo}").Count();
            if (distinctSeats < seats.Count)
                throw new InvalidOperationException("座位选择重复，请勿选择同一个座位多次。");

            var createdOrders = new List<OrderForTickets>();

            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                OracleTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    var customer = _customerRepository.GetCustomerById(customerId);
                    if (customer == null) throw new ArgumentException($"顾客ID {customerId} 不存在。");

                    var section = _showingRepository.GetSectionById(sectionId);
                    if (section == null) throw new ArgumentException($"场次ID {sectionId} 不存在。");

                    var soldTickets = _showingRepository.GetSoldSeatsForSection(sectionId);
                    foreach (var seat in seats)
                    {
                        if (soldTickets.Any(t => t.LineNo == seat.LINENO && t.ColumnNo == seat.ColumnNo && t.State == "已售出"))
                            throw new InvalidOperationException($"座位 {seat.LINENO}{seat.ColumnNo} 在场次 {sectionId} 已被预订。");
                    }

                    // 获取VIP卡信息
                    var vipCard = _customerRepository.GetVIPCardByCustomerID(customerId);
                    if (vipCard == null)
                        throw new InvalidOperationException("获取积分信息失败。");

                    decimal totalPrice = 0m;
                    List<decimal> seatPrices = new List<decimal>();
                    foreach (var seat in seats)
                    {
                        decimal finalTicketPrice = CalculateFinalTicketPrice(section, customer, seat.LINENO);
                        seatPrices.Add(finalTicketPrice);
                        totalPrice += finalTicketPrice;
                    }

                    // 判断积分支付是否足够
                    if (paymentMethod == "积分支付")
                    {
                        decimal requiredPoints = totalPrice * 10;
                        if (vipCard.Points < requiredPoints)
                            throw new InvalidOperationException($"积分不足，需要 {requiredPoints} 积分，当前积分 {vipCard.Points}。");

                        // 扣除积分
                        _customerRepository.UpdateVIPCardPoints(customerId, -Convert.ToInt32(requiredPoints));
                    }

                    foreach (var seat in seats)
                    {
                        decimal finalTicketPrice = CalculateFinalTicketPrice(section, customer, seat.LINENO);

                        Ticket newTicket = new Ticket
                        {
                            TicketID = Guid.NewGuid().ToString(),
                            Price = finalTicketPrice,
                            SectionID = sectionId,
                            LineNo = seat.LINENO,
                            ColumnNo = seat.ColumnNo,
                            State = "已售出"
                        };
                        _showingRepository.AddTicket(newTicket, transaction);

                        OrderForTickets newOrder = new OrderForTickets
                        {
                            TicketID = newTicket.TicketID,
                            State = "有效",
                            CustomerID = customerId,
                            Day = DateTime.Today,
                            PaymentMethod = paymentMethod,
                            TotalPrice = finalTicketPrice
                        };
                        _orderRepository.AddOrderForTickets(newOrder, transaction);

                        createdOrders.Add(newOrder);
                    }

                    // 购买完成加积分
                    int pointsEarned = 10 * seats.Count;
                    if (paymentMethod != "积分支付") // 用现金支付才加积分
                        _customerRepository.UpdateVIPCardPoints(customerId, pointsEarned);

                    transaction.Commit();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"成功购买 {seats.Count} 张票并生成 {createdOrders.Count} 个订单。");
                    Console.ResetColor();

                    return createdOrders;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException($"购票失败: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 包含了影厅、座区、时段和会员等级的定价逻辑。
        /// </summary>
        /// <param name="section">场次信息</param>
        /// <param name="customer">顾客信息</param>
        /// <param name="lineNo">座位行号</param>
        /// <returns>计算后的最终票价</returns>
        public decimal CalculateFinalTicketPrice(Section section, Customer customer, string lineNo)
        {
            // 1. 获取基础票价
            var film = (_filmRepository as IFilmRepository)?.GetFilmByName(section.FilmName);
            if (film == null || film.NormalPrice <= 0)
            {
                throw new InvalidOperationException($"无法获取电影 '{section.FilmName}' 的基础票价。");
            }
            decimal currentPrice = film.NormalPrice;
            Console.WriteLine($"基础票价: {currentPrice}");

            // 2. 根据影厅类型调整价格
            var hall = (_orderRepository as OracleOrderRepository)?.GetMovieHallByNo(section.HallNo);
            if (hall != null)
            {
                switch (hall.Category.ToUpper())
                {
                    case "IMAX":
                        currentPrice *= 1.5m; // IMAX厅价格上浮50%
                        Console.WriteLine($"应用IMAX厅加成后价格: {currentPrice}");
                        break;
                    case "VIP":
                        currentPrice *= 2.0m; // VIP厅价格翻倍
                        Console.WriteLine($"应用VIP厅加成后价格: {currentPrice}");
                        break;
                        // 默认普通厅不加价
                }
            }

            // 3. 根据座区调整价格
            string category = null;

            // 从 _orderRepository 获取连接字符串并实例化 OracleShowingRepository
            var orderRepo = _orderRepository as OracleOrderRepository;
            if (orderRepo == null)
            {
                throw new InvalidOperationException("无法访问 OracleOrderRepository。");
            }
            var showingRepo = new OracleShowingRepository(_connectionString);
            var seatLayout = showingRepo.GetHallSeatLayout(section.HallNo);

            // 在布局中找到对应的行，取其 CATEGORY
            var seatRow = seatLayout.FirstOrDefault(s =>
                s.LINENO.Equals(lineNo, StringComparison.OrdinalIgnoreCase));

            if (seatRow != null)
            {
                category = seatRow.CATEGORY;
            }

            if (string.IsNullOrEmpty(category))
            {
                Console.WriteLine($"未找到座区信息，按普通区处理: {currentPrice}");
            }
            else
            {
                switch (category.ToUpper())
                {
                    case "A": // 特价区
                        currentPrice -= 3;
                        Console.WriteLine($"应用特价区折扣后价格: {currentPrice}");
                        break;
                    case "B": // 优选区
                        currentPrice += 5;
                        Console.WriteLine($"应用优选区加成后价格: {currentPrice}");
                        break;
                    case "C": // 黄金区
                        currentPrice += 10;
                        Console.WriteLine($"应用黄金区加成后价格: {currentPrice}");
                        break;
                    case "D": // 普通区
                    default:
                        Console.WriteLine($"普通区，价格保持不变: {currentPrice}");
                        break;
                }
            }

            // 4. 根据会员等级应用折扣
            // 假设会员等级越高，折扣越大 (1级不打折, 2级9折, 3级8折...)
            if (customer.VipLevel > 1)
            {
                decimal discountRate = 1.0m - (customer.VipLevel - 1) * 0.1m;
                if (discountRate < 0.5m) discountRate = 0.5m; // 最低五折

                currentPrice *= discountRate;
                Console.WriteLine($"应用会员等级 {customer.VipLevel} 折扣({discountRate})后价格: {currentPrice}");
            }


            // 返回最终价格，保留两位小数
            return Math.Round(currentPrice, 2);
        }

        // 退票
        public bool RefundTicket(int orderId, DateTime refundTime, out decimal refundFee, out decimal refundAmount)
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
            refundAmount = paidPrice - refundFee;

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
                        Console.WriteLine("[ERROR] 更新订单状态失败");
                        return false;
                    }

                    // 4.2 释放座位
                    if (!ReleaseTicket(ticketId))
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

        public bool ReleaseTicket(string ticketId)
        {
            string sql = "UPDATE ticket SET state = '已退票' WHERE ticketID = :ticketId";
            var parameter = new OracleParameter("ticketId", OracleDbType.Varchar2, 40)
            {
                Value = ticketId
            };

            return _dbService.ExecuteNonQuery(sql, parameter) > 0;
        }
    }
}

