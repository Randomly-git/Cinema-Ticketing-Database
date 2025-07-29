using Oracle.ManagedDataAccess.Client; // 确保引用此命名空间
using System;
using System.Collections.Generic; // 用于 List<T> 和 Dictionary<TKey, TValue>
using System.Linq; // 用于 LINQ 扩展方法
using test.Models;
using test.Repositories;
using test.Services;

namespace test // 确保这个命名空间与你的项目命名空间一致
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- 启动电影院购票管理系统测试 ---");

            // 直接在这里定义连接字符串
            // 请务必替换为你的实际数据库信息
            // 确保 User Id, Password, PORT, SERVICE_NAME, DBA Privilege=SYSDBA; 完全正确
            string connectionString = "Data Source=//8.148.76.54:1524/orclpdb1;User Id=cbc;Password=123456";

            // 验证连接字符串是否有效
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("错误：连接字符串为空。请在 Program.cs 中设置正确的连接字符串！");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            // 尝试进行一次简单的数据库连接测试，确保连接字符串本身没问题
            Console.WriteLine("\n--- 首次数据库连接测试 ---");
            using (OracleConnection testConnection = new OracleConnection(connectionString))
            {
                try
                {
                    testConnection.Open();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("数据库连接成功！");
                    Console.ResetColor();
                }
                catch (OracleException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"数据库连接失败：{ex.Message}");
                    Console.WriteLine($"Oracle 错误码：{ex.Number}");
                    Console.WriteLine("请检查：连接字符串、端口、服务名、用户名、密码、DBA Privilege，以及云服务器防火墙/白名单。");
                    Console.ResetColor();
                    Console.ReadKey();
                    return; // 连接失败则退出程序
                }
            }

            // 实例化数据访问层和业务逻辑层
            // 用户管理相关
            ICustomerRepository customerRepository = new OracleCustomerRepository(connectionString);
            IUserService userService = new UserService(customerRepository);

            // 电影和场次相关
            IFilmRepository filmRepository = new OracleFilmRepository(connectionString);
            IFilmService filmService = new FilmService(filmRepository); // 确保 FilmService 已实现 IFilmService
            IShowingRepository showingRepository = new OracleShowingRepository(connectionString);
            IShowingService showingService = new ShowingService(showingRepository, filmRepository); // 确保 ShowingService 已实现 IShowingService

            // 订单和购票相关
            IOrderRepository orderRepository = new OracleOrderRepository(connectionString);
            IBookingService bookingService = new BookingService(showingRepository, filmRepository, customerRepository, orderRepository, connectionString);


            // --- 测试顾客注册和登录 (保留用户管理模块的测试) ---
            Console.WriteLine("\n--- 测试顾客注册和登录 ---");
            string testCustomerId = "CUST001"; // 用于测试购票的顾客ID
            string testCustomerPassword = "customerPass";
            string testCustomerName = "测试顾客";
            string testCustomerPhone = "22222222";

            try
            {
                // 尝试删除已存在的测试用户，确保每次测试都是干净的注册
                userService.DeleteCustomerAccount(testCustomerId);
                Console.WriteLine($"已尝试删除旧用户 {testCustomerId} (如果存在)。");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"删除旧用户 {testCustomerId} 失败: {ex.Message}");
                Console.ResetColor();
            }

            try
            {
                Customer newCustomer = new Customer
                {
                    CustomerID = testCustomerId,
                    Username = testCustomerId, // 假设 CustomerID 就是登录的 Username
                    Name = testCustomerName,
                    PhoneNum = testCustomerPhone
                };
                userService.RegisterCustomer(newCustomer, testCustomerPassword);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"顾客 {newCustomer.Name} (ID: {newCustomer.CustomerID}) 注册成功。");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"顾客 {testCustomerId} 注册失败: {ex.Message}");
                Console.ResetColor();
            }

            Customer loggedInCustomer = null;
            try
            {
                loggedInCustomer = userService.AuthenticateCustomer(testCustomerId, testCustomerPassword);
                if (loggedInCustomer != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"顾客 {loggedInCustomer.Name} (ID: {loggedInCustomer.CustomerID}) 登录成功。");
                    Console.WriteLine($"会员等级: {loggedInCustomer.VipLevel}, 积分: {loggedInCustomer.VIPCard?.Points ?? 0}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"顾客 {testCustomerId} 登录失败：用户名或密码错误。");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"登录 {testCustomerId} 发生异常: {ex.Message}");
                Console.ResetColor();
            }

            // --- 测试订票功能 ---
            Console.WriteLine("\n--- 测试订票功能 ---");

            if (loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("用户未登录，无法进行订票测试。请先确保用户能够成功登录。");
                Console.ResetColor();
            }
            else
            {
                // 1. 获取所有电影列表
                Console.WriteLine("\n--- 获取所有电影列表 ---");
                List<Film> films = filmService.GetAvailableFilms();
                if (films.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"成功获取 {films.Count} 部电影：");
                    Console.ResetColor();
                    foreach (var film in films)
                    {
                        Console.WriteLine($"- {film.FilmName} ({film.Genre})");
                    }

                    // 2. 选择一部电影进行测试 (选择第一部电影)
                    Film selectedFilm = films.FirstOrDefault();
                    if (selectedFilm != null)
                    {
                        Console.WriteLine($"\n--- 选取电影 '{selectedFilm.FilmName}' 进行场次查询 ---");

                        // 3. 获取指定电影今天的场次
                        // 注意：这里仍然传入 DateTime.Today，但由于 SECTION 表已移除 DAY 字段，
                        // OracleShowingRepository.GetAvailableSections 方法已不再使用日期过滤。
                        // 如果你需要按日期过滤，请确保 SECTION 表有 DAY 字段并重新启用相关代码。
                        List<Section> sections = showingService.GetFilmShowings(selectedFilm.FilmName, DateTime.Today);
                        if (sections.Any())
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"成功获取 '{selectedFilm.FilmName}' 的 {sections.Count} 个场次：");
                            Console.ResetColor();
                            foreach (var section in sections)
                            {
                                Console.WriteLine($"- 场次ID: {section.SectionID}, 影厅: {section.HallNo}, 时段: {section.TimeSlot.StartTime:hh\\:mm}-{section.TimeSlot.EndTime:hh\\:mm}");
                            }

                            // 4. 选择第一个场次进行座位查询和订票
                            Section selectedSection = sections.First();
                            Console.WriteLine($"\n--- 选取场次 ID: {selectedSection.SectionID} 进行座位查询和订票 ---");

                            // 5. 获取可用座位
                            try
                            {
                                Dictionary<string, List<string>> availableSeats = showingService.GetAvailableSeats(selectedSection);
                                if (availableSeats.Any())
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"场次 ID: {selectedSection.SectionID} 的可用座位：");
                                    Console.ResetColor();
                                    foreach (var row in availableSeats.OrderBy(r => r.Key))
                                    {
                                        Console.WriteLine($"  行 {row.Key}: {string.Join(", ", row.Value)}");
                                    }

                                    // 6. 尝试购买第一排第一个可用座位
                                    string bookingLineNo = availableSeats.Keys.First();
                                    int bookingColumnNo = int.Parse(availableSeats[bookingLineNo].First());
                                    string paymentMethod = "支付宝"; // 示例支付方式

                                    Console.WriteLine($"\n--- 尝试为顾客 {loggedInCustomer.Name} 购买场次 {selectedSection.SectionID} 的座位 {bookingLineNo}{bookingColumnNo} ---");
                                    try
                                    {
                                        OrderForTickets bookedOrder = bookingService.PurchaseTicket(selectedSection.SectionID, bookingLineNo, bookingColumnNo, loggedInCustomer.CustomerID, paymentMethod);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"订票成功！订单信息：{bookedOrder.ToString()}");
                                        Console.ResetColor();

                                        // 验证座位是否已售出
                                        Console.WriteLine($"\n--- 验证座位 {bookingLineNo}{bookingColumnNo} 是否已售出 ---");
                                        Dictionary<string, List<string>> updatedAvailableSeats = showingService.GetAvailableSeats(selectedSection);
                                        if (!updatedAvailableSeats.Any(r => r.Key == bookingLineNo && r.Value.Contains(bookingColumnNo.ToString())))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"验证成功：座位 {bookingLineNo}{bookingColumnNo} 已不再可用。");
                                            Console.ResetColor();
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"验证失败：座位 {bookingLineNo}{bookingColumnNo} 仍然可用 (应已售出)。");
                                            Console.ResetColor();
                                        }
                                    }
                                    catch (Exception bookingEx)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"订票失败：{bookingEx.Message}");
                                        Console.ResetColor();
                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"场次 ID: {selectedSection.SectionID} 没有可用座位。无法进行订票测试。");
                                    Console.ResetColor();
                                }
                            }
                            catch (Exception seatEx)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"获取可用座位失败：{seatEx.Message}");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"电影 '{selectedFilm.FilmName}' 没有找到场次。请确保 SECTION 表中有数据。");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("没有找到任何电影，无法进行订票测试。请确保 FILM 表中有数据。");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("没有找到任何电影，无法进行订票测试。请确保 FILM 表中有数据。");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\n--- 电影院购票管理系统测试完成。按任意键退出。 ---");
            Console.ReadKey();
        }
    }
}
