using CinemaTicketSystem.Services;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

class TicketProgram
{
    // 硬编码的数据库连接字符串
    private const string ConnectionString = "User Id=SYS;Password=Sun20040921;Data Source=127.0.0.1:1521/orcl;DBA Privilege=SYSDBA;";

    static void Main(string[] args)
    {
        // 初始化服务
        var dbService = new DatabaseService(ConnectionString);
        var ticketService = new TicketService(dbService);
        var orderService = new OrderService(dbService, ticketService);
        var filmService = new FilmService(dbService); // 新增FilmService用于获取电影信息

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== 影院购票系统 ===");
            Console.WriteLine("1. 购票");
            Console.WriteLine("2. 查看某顾客的所有已支付订单");
            Console.WriteLine("3. 退票");
            Console.WriteLine("4. 退出");
            Console.WriteLine("5. 查看所有有效票号");
            Console.Write("请选择操作：");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": // 购票
                    HandlePurchaseTicketNewFlow(filmService, orderService, ticketService);
                    break;

                case "2": // 查看所有订单
                    DisplayCustomerPaidOrders(orderService);
                    break;

                case "3": // 退票
                    HandleRefundTicket(orderService);
                    break;

                case "4": // 退出
                    Console.WriteLine("退出程序...");
                    return;

                case "5": // 查看所有有效票号
                    DisplayValidTickets(ticketService);
                    break;

                default:
                    Console.WriteLine("无效选择，请重新输入。");
                    break;
            }

            Console.WriteLine("\n按任意键继续...");
            Console.ReadKey();
        }
    }

    private static void HandlePurchaseTicketNewFlow(FilmService filmService, OrderService orderService, TicketService ticketService)
    {
        // 1. 验证顾客ID
        string customerId = GetValidCustomerId(orderService);
        if (customerId == null) return;

        // 2. 选择电影
        string selectedFilmName = SelectFilm(filmService);
        if (selectedFilmName == null) return;

        // 3. 选择日期
        DateTime? selectedDay = SelectDate();
        if (!selectedDay.HasValue) return;

        // 4. 选择场次
        int? selectedSectionId = SelectFilmSection(filmService, ticketService, selectedFilmName, selectedDay.Value);
        if (!selectedSectionId.HasValue) return;

        // 5. 选择座位
        var (selectedRow, selectedCol) = SelectSeat(ticketService, selectedSectionId.Value);
        if (selectedRow == -1 || selectedCol == -1) return;

        // 6. 选择支付方式
        string paymentMethod = SelectPaymentMethod();
        if (paymentMethod == null) return;

        // 7. 确认并执行购票
        ConfirmAndPurchase(orderService, ticketService, customerId, selectedSectionId.Value,
                          selectedRow, selectedCol, selectedFilmName, paymentMethod);
    }

    #region Helper Methods
    private static string GetValidCustomerId(OrderService orderService)
    {
        while (true)
        {
            Console.Write("\n请输入顾客ID: ");
            string customerId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(customerId))
            {
                Console.WriteLine("顾客ID不能为空！");
                continue;
            }

            if (!orderService.CustomerExists(customerId))
            {
                Console.WriteLine("顾客ID不存在，请先注册！");
                return null;
            }

            return customerId;
        }
    }

    private static string SelectFilm(FilmService filmService)
    {
        Console.WriteLine("\n=== 当前上映电影 ===");
        var films = filmService.GetAllFilms();
        if (films.Rows.Count == 0)
        {
            Console.WriteLine("当前没有上映的电影。");
            return null;
        }

        var validFilmNames = films.AsEnumerable()
            .Select(row => row["FilmName"].ToString())
            .ToList();

        foreach (DataRow row in films.Rows)
        {
            Console.WriteLine($"{row["FilmName"]} - {row["Genre"]} - 评分:{row["Score"]}/10");
        }

        while (true)
        {
            Console.Write("\n请输入要观看的电影名称: ");
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("电影名称不能为空！");
                continue;
            }

            if (validFilmNames.Contains(input))
            {
                return input;
            }

            Console.WriteLine("输入的电影不存在，请从列表中选择！");
        }
    }

    private static DateTime? SelectDate()
    {
        while (true)
        {
            Console.Write("请输入观影日期(格式: yyyy-MM-dd): ");
            string input = Console.ReadLine();

            if (DateTime.TryParse(input, out DateTime date))
            {
                return date;
            }

            Console.WriteLine("日期格式错误，请按格式输入！");
        }
    }

    private static int? SelectFilmSection(FilmService filmService, TicketService ticketService, string filmName, DateTime day)
    {
        var sections = filmService.GetFilmSections(filmName, day);
        if (sections.Rows.Count == 0)
        {
            Console.WriteLine($"电影《{filmName}》在{day:yyyy-MM-dd}没有排片。");
            return null;
        }

        var validSectionIds = new List<int>();
        Console.WriteLine("\n=== 可选的场次 ===");
        Console.WriteLine("{0,-10} {1,-10} {2,-15} {3,-15} {4,-10}",
            "场次ID", "影厅", "时段", "开始时间", "剩余票数");
        Console.WriteLine(new string('-', 60));

        foreach (DataRow row in sections.Rows)
        {
            int sectionId = Convert.ToInt32(row["SectionID"]);
            validSectionIds.Add(sectionId);
            int availableTickets = ticketService.GetAvailableTicketsCount(sectionId);

            Console.WriteLine("{0,-10} {1,-10} {2,-15} {3,-15} {4,-10}",
                sectionId,
                row["HallNo"],
                row["TimeID"],
                Convert.ToDateTime(row["ShowTime"]).ToString("HH:mm"),
                availableTickets);
        }

        while (true)
        {
            Console.Write("\n请输入要预订的场次ID: ");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out int sectionId))
            {
                Console.WriteLine("场次ID必须是数字！");
                continue;
            }

            if (!validSectionIds.Contains(sectionId))
            {
                Console.WriteLine("输入的场次ID不存在，请从列表中选择！");
                continue;
            }

            if (ticketService.GetAvailableTicketsCount(sectionId) <= 0)
            {
                Console.WriteLine("该场次已售罄，请选择其他场次！");
                continue;
            }

            return sectionId;
        }
    }

    private static (int row, int col) SelectSeat(TicketService ticketService, int sectionId)
    {
        // 显示座位图
        Console.WriteLine("\n=== 座位图 ===");
        var seats = ticketService.GetSeatStatus(sectionId);
        DisplaySeatMap(seats);

        while (true)
        {
            Console.Write("\n请输入要选择的座位（格式：行号,列号 如：3,5）：");
            var input = Console.ReadLine()?.Split(',');

            if (input?.Length != 2 ||
                !int.TryParse(input[0], out int row) ||
                !int.TryParse(input[1], out int col))
            {
                Console.WriteLine("输入格式错误！请按示例格式输入");
                continue;
            }

            var seatStatus = seats.AsEnumerable()
                .FirstOrDefault(r =>
                    Convert.ToInt32(r["lineNo"]) == row &&  // 修改这里
                    Convert.ToInt32(r["columnNo"]) == col); // 修改这里

            if (seatStatus == null)
            {
                Console.WriteLine("无效的座位位置！");
                continue;
            }

            if (seatStatus.Field<string>("state") != "可售")
            {
                Console.WriteLine("该座位已被占用！");
                continue;
            }

            return (row, col);
        }
    }

    private static void DisplaySeatMap(DataTable seats)
    {
        // 获取最大行列数（添加类型转换处理）
        int maxRow = seats.AsEnumerable()
            .Max(r => Convert.ToInt32(r["lineNo"]));  // 显式转换为int

        int maxCol = seats.AsEnumerable()
            .Max(r => Convert.ToInt32(r["columnNo"])); // 显式转换为int

        // 打印列号表头
        Console.Write("   ");
        for (int col = 1; col <= maxCol; col++)
        {
            Console.Write($"{col,-3}");
        }
        Console.WriteLine();

        // 打印座位图
        for (int row = 1; row <= maxRow; row++)
        {
            Console.Write($"{row,2} ");
            for (int col = 1; col <= maxCol; col++)
            {
                var seat = seats.AsEnumerable()
                    .FirstOrDefault(r =>
                        Convert.ToInt32(r["lineNo"]) == row &&
                        Convert.ToInt32(r["columnNo"]) == col);

                Console.Write(seat?["state"].ToString() switch
                {
                    "可售" => "[ ]",
                    "已售" => "[X]",
                    "锁定中" => "[L]",
                    _ => "[?]"
                });
                Console.Write(" ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("图例: [ ]可售 [X]已售 [L]锁定中");
    }

    private static string SelectPaymentMethod()
    {
        var paymentMethods = new Dictionary<int, string>
    {
        {1, "微信支付"},
        {2, "支付宝"},
        {3, "银联卡"},
        {4, "现金"}
    };

        while (true)
        {
            Console.WriteLine("\n=== 请选择支付方式 ===");
            foreach (var method in paymentMethods)
            {
                Console.WriteLine($"{method.Key}. {method.Value}");
            }

            Console.Write("请输入支付方式编号：");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && paymentMethods.ContainsKey(choice))
            {
                return paymentMethods[choice];
            }

            Console.WriteLine("无效的选择，请重新输入！");
            Console.WriteLine("按ESC键取消购票，或其他键继续选择...");
            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
            {
                return null;
            }
        }
    }

    private static void ConfirmAndPurchase(OrderService orderService, TicketService ticketService,
                                     string customerId, int sectionId, int row, int col,
                                     string filmName, string paymentMethod)
    {
        Console.WriteLine($"\n=== 订单确认 ===\n" +
                         $"电影: {filmName}\n" +
                         $"座位: {row}排{col}座\n" +
                         $"支付方式: {paymentMethod}\n" +
                         $"确认购买？(Y/N)");

        if (Console.ReadLine().Trim().ToUpper() != "Y")
        {
            Console.WriteLine("购票已取消");
            return;
        }

        try
        {
            int orderId;
            string seatInfo;
            bool success = orderService.PurchaseTicket(
                customerId, sectionId, row, col, out orderId, out seatInfo, paymentMethod);

            if (success)
            {
                Console.WriteLine($"购票成功！\n订单号: {orderId}\n座位: {seatInfo}");
            }
            else
            {
                Console.WriteLine("购票失败，请稍后重试");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"购票过程中发生错误: {ex.Message}");
        }
    }
    #endregion

    private static void DisplayCustomerPaidOrders(OrderService orderService)
    {
        Console.WriteLine("\n=== 查询顾客的已支付订单 ===");
        Console.Write("请输入顾客ID: ");
        string customerId = Console.ReadLine();

        // 获取该顾客的所有"已支付"订单
        var paidOrders = orderService.GetCustomerOrders(customerId,"已支付");

        if (paidOrders.Rows.Count == 0)
        {
            Console.WriteLine($"\n顾客 {customerId} 没有已支付的订单。");
            Console.WriteLine("可能原因：");
            Console.WriteLine("- 顾客ID输入错误");
            Console.WriteLine("- 该顾客没有订单");
            Console.WriteLine("- 该顾客的订单都已退款或未支付");
            return;
        }

        Console.WriteLine($"\n顾客 {customerId} 的已支付订单：");
        Console.WriteLine("{0,-10} {1,-15} {2,-10} {3,-10} {4,-15}",
            "订单ID", "电影名称", "票号", "价格", "购票日期");
        Console.WriteLine(new string('-', 60));

        foreach (DataRow row in paidOrders.Rows)
        {
            Console.WriteLine("{0,-10} {1,-15} {2,-10} {3,-10} {4,-15}",
                row["OrderID"],
                row["FilmName"],
                row["TicketID"],
                row["Price"],
                Convert.ToDateTime(row["Day"]).ToString("yyyy-MM-dd"));
        }
    }

    private static void HandleRefundTicket(OrderService orderService)
    {
        Console.Write("\n请输入顾客ID: ");
        string customerId = Console.ReadLine();

        // 修改：只获取已支付的订单
        var refundableOrders = orderService.GetCustomerOrders(customerId, "已支付");

        if (refundableOrders.Rows.Count == 0)
        {
            Console.WriteLine("该顾客没有可退款的订单。");
            Console.WriteLine("可能原因：");
            Console.WriteLine("- 没有找到该顾客的订单");
            Console.WriteLine("- 所有订单都已使用或已退款");
            Console.WriteLine("- 没有状态为'已支付'的订单");
            return;
        }

        Console.WriteLine("\n该顾客的可退款订单：");
        Console.WriteLine("{0,-10} {1,-15} {2,-10} {3,-10} {4,-15}",
            "订单ID", "电影名称", "票号", "价格", "购票日期");
        Console.WriteLine(new string('-', 60));

        foreach (DataRow row in refundableOrders.Rows)
        {
            Console.WriteLine("{0,-10} {1,-15} {2,-10} {3,-10} {4,-15}",
                row["OrderID"],
                row["FilmName"],
                row["TicketID"],
                row["Price"],
                Convert.ToDateTime(row["Day"]).ToString("yyyy-MM-dd"));
        }

        Console.Write("\n请输入要退票的订单ID: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId))
        {
            Console.WriteLine("订单ID格式错误。");
            return;
        }

        // 使用固定时间测试
        DateTime refundTime = new DateTime(2025, 6, 30, 12, 30, 0);
        Console.WriteLine($"使用测试时间: {refundTime:yyyy-MM-dd HH:mm}");

        if (orderService.RefundTicket(orderId, refundTime, out decimal fee, out int refundAmount))
        {
            Console.WriteLine("\n退票成功！");
            Console.WriteLine($"手续费: {fee:F2}元");
            Console.WriteLine($"实际退款金额: {refundAmount}元");
            Console.WriteLine($"座位已释放，可重新购买。");
        }
        else
        {
            Console.WriteLine("\n退票失败，可能原因：");
            Console.WriteLine("- 订单ID不正确");
            Console.WriteLine("- 订单状态已变更");
            Console.WriteLine("- 电影已经开始放映");
        }
    }

    private static void DisplayValidTickets(TicketService ticketService)
    {
        Console.WriteLine("\n=== 有效票列表 ===");
        var validTickets = ticketService.GetValidTickets();

        if (validTickets.Rows.Count == 0)
        {
            Console.WriteLine("没有有效票。");
            return;
        }

        Console.WriteLine("{0,-15} {1,-15} {2,-15} {3,-10} {4,-10} {5,-10}",
            "票号", "场次ID", "电影名称", "座位", "状态", "价格");
        Console.WriteLine(new string('-', 75));

        foreach (DataRow row in validTickets.Rows)
        {
            Console.WriteLine("{0,-15} {1,-15} {2,-15} {3,-10} {4,-10} {5,-10}",
                row["TicketID"],
                row["SectionID"],
                row["FilmName"],
                $"行{row["LineNo"]}列{row["ColumnNo"]}",
                row["State"],
                row["Price"]);
        }
    }
}