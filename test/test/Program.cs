using Oracle.ManagedDataAccess.Client; // 确保引用此命名空间
using System;
using System.Collections.Generic; // 用于 List<T> 和 Dictionary<TKey, TValue>
using System.Configuration;
using System.Linq;
using test.Models;
using test.Repositories;
using test.Services;
using System.Text;

namespace test
{
    class Program
    {
        // 当前登录的顾客和管理员
        private static Customer _loggedInCustomer = null;
        private static Administrator _loggedInAdmin = null; // 管理员登录状态

        // 服务实例
        private static IUserService _userService;
        private static IFilmService _filmService;
        private static IShowingService _showingService;
        private static IBookingService _bookingService;
        private static IAdministratorService _adminService; // 管理员服务

        // 仓库实例 (某些操作可能需要直接访问，例如管理员删除)
        private static ICustomerRepository _customerRepository;
        private static IOrderRepository _orderRepository; // 订单仓库
        private static IFilmRepository _filmRepository;   // 电影仓库


        static void Main(string[] args)
        {
            Console.WriteLine("--- 启动电影院管理系统 ---");

            // 直接在这里定义连接字符串
            // 请务必替换为你的实际数据库信息
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

            // 首次数据库连接测试
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
                    Console.WriteLine("请检查：连接字符串、端口、服务名、以及云服务器防火墙/白名单。");
                    Console.ResetColor();
                    Console.ReadKey();
                    return; // 连接失败则退出程序
                }
            }

            // 实例化所有数据访问层和业务逻辑层
            _customerRepository = new OracleCustomerRepository(connectionString);
            _orderRepository = new OracleOrderRepository(connectionString);
            _filmRepository = new OracleFilmRepository(connectionString);
            IShowingRepository showingRepository = new OracleShowingRepository(connectionString);
            IAdministratorRepository adminRepository = new OracleAdministratorRepository(connectionString); // 管理员仓库

            _userService = new UserService(_customerRepository);
            _filmService = new FilmService(_filmRepository);
            _showingService = new ShowingService(showingRepository, _filmRepository);
            _bookingService = new BookingService(showingRepository, _filmRepository, _customerRepository, _orderRepository, connectionString);
            _adminService = new AdministratorService(adminRepository, _orderRepository, _filmRepository); // 管理员服务

            RunMainMenu();

            Console.WriteLine("\n--- 电影院管理系统已退出。按任意键退出。 ---");
            Console.ReadKey();
        }
        /// <summary>
        /// 运行主菜单循环。
        /// </summary>
        static void RunMainMenu()
        {
            bool running = true;
            while (running)
            {
                Console.Clear(); // 清屏
                Console.WriteLine("======================================");
                Console.WriteLine("      电影院管理系统主菜单      ");
                Console.WriteLine("======================================");

                if (_loggedInCustomer == null && _loggedInAdmin == null)
                {
                    Console.WriteLine("1. 顾客注册");
                    Console.WriteLine("2. 顾客登录");
                    Console.WriteLine("3. 管理员注册"); // 新增管理员注册选项
                    Console.WriteLine("4. 管理员登录"); // 管理员登录选项
                }
                else if (_loggedInCustomer != null)
                {
                    Console.WriteLine($"当前用户: {_loggedInCustomer.Name} (ID: {_loggedInCustomer.CustomerID}, 等级: {_loggedInCustomer.VipLevel}, 积分: {_loggedInCustomer.VIPCard?.Points ?? 0})");
                    Console.WriteLine("1. 更新个人资料");
                    Console.WriteLine("2. 查看电影排挡");
                    Console.WriteLine("3. 购票");
                    Console.WriteLine("4. 购买周边 (未实现)");
                    Console.WriteLine("5. 删除我的账户");
                    Console.WriteLine("6. 用户登出");
                }
                else if (_loggedInAdmin != null)
                {
                    Console.WriteLine($"当前管理员: {_loggedInAdmin.AdminName} (ID: {_loggedInAdmin.AdminID})");
                    Console.WriteLine("1. 电影管理 (增/改)");
                    Console.WriteLine("2. 查看所有订单");
                    Console.WriteLine("3. 管理员登出");
                }
                Console.WriteLine("0. 退出系统");
                Console.WriteLine("======================================");
                Console.Write("请选择一个操作: ");

                string choice = Console.ReadLine();

                try
                {
                    if (_loggedInCustomer == null && _loggedInAdmin == null) // 未登录状态
                    {
                        switch (choice)
                        {
                            case "1":
                                RegisterCustomer();
                                break;
                            case "2":
                                LoginCustomer();
                                break;
                            case "3": // 管理员注册
                                RegisterAdministrator();
                                break;
                            case "4": // 管理员登录
                                LoginAdministrator();
                                break;
                            case "0":
                                running = false;
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("无效的选择，请重试。");
                                Console.ResetColor();
                                break;
                        }
                    }
                    else if (_loggedInCustomer != null) // 顾客已登录
                    {
                        switch (choice)
                        {
                            case "1":
                                UpdateCustomerProfile();
                                break;
                            case "2":
                                ViewFilmShowings();
                                break;
                            case "3":
                                PurchaseTicketMenu();
                                break;
                            case "4":
                                Console.WriteLine("\n--- 购买周边功能尚未实现。---");
                                break;
                            case "5":
                                DeleteCustomerAccount();
                                // 如果账户被删除，则退出循环，因为用户已登出
                                if (_loggedInCustomer == null) running = false;
                                break;
                            case "6":
                                LogoutCustomer();
                                break;
                            case "0":
                                running = false;
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("无效的选择，请重试。");
                                Console.ResetColor();
                                break;
                        }
                    }
                    else if (_loggedInAdmin != null) // 管理员已登录
                    {
                        switch (choice)
                        {
                            case "1":
                                ManageFilmsMenu(); // 电影管理子菜单
                                break;
                            case "2":
                                ViewAllOrders();
                                break;
                            case "3":
                                LogoutAdministrator();
                                break;
                            case "0":
                                running = false;
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("无效的选择，请重试。");
                                Console.ResetColor();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"操作失败: {ex.Message}");
                    Console.ResetColor();
                }

                if (running) // 如果没有退出系统，则等待用户按键继续
                {
                    Console.WriteLine("\n按任意键继续...");
                    Console.ReadKey();
                }
            }
        }

        // ====================================================================
        // 顾客相关功能
        // ====================================================================

        /// <summary>
        /// 注册新用户功能。
        /// </summary>
        static void RegisterCustomer()
        {
            Console.WriteLine("\n--- 注册新用户 ---");
            Console.Write("请输入新用户的ID (例如: CUST002): ");
            string customerId = Console.ReadLine();
            Console.Write("请输入新用户的姓名: ");
            string name = Console.ReadLine();
            Console.Write("请输入新用户的手机号: ");
            string phoneNum = Console.ReadLine();
            Console.Write("请输入密码: ");
            string password = GetHiddenConsoleInput();
            Console.Write("请再次输入密码确认: ");
            string confirmPassword = GetHiddenConsoleInput();

            if (password != confirmPassword)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("两次输入的密码不一致，请重试。");
                Console.ResetColor();
                return;
            }

            try
            {
                Customer newCustomer = new Customer
                {
                    CustomerID = customerId,
                    Username = customerId, // 假设 CustomerID 就是登录的 Username
                    Name = name,
                    PhoneNum = phoneNum,
                    VipLevel = 0 // 默认新用户为普通顾客
                };
                _userService.RegisterCustomer(newCustomer, password);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"用户 {newCustomer.Name} (ID: {newCustomer.CustomerID}) 注册成功！");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"注册失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 用户登录功能。
        /// </summary>
        static void LoginCustomer()
        {
            Console.WriteLine("\n--- 顾客登录 ---");
            Console.Write("请输入您的用户ID: ");
            string customerId = Console.ReadLine();
            Console.Write("请输入您的密码: ");
            string password = GetHiddenConsoleInput();

            try
            {
                _loggedInCustomer = _userService.AuthenticateCustomer(customerId, password);
                if (_loggedInCustomer != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"登录成功！欢迎您，{_loggedInCustomer.Name}！");
                    Console.ResetColor();
                    // 登录成功后，重新加载完整的顾客信息，包括最新的积分和等级
                    _loggedInCustomer = _customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
                    Console.WriteLine($"会员等级: {_loggedInCustomer.VipLevel}, 积分: {_loggedInCustomer.VIPCard?.Points ?? 0})");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("登录失败：用户ID或密码不正确。");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"登录失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 更新个人资料功能。
        /// </summary>
        static void UpdateCustomerProfile()
        {
            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录。");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("\n--- 更新个人资料 ---");
            Console.WriteLine($"当前姓名: {_loggedInCustomer.Name}");
            Console.Write("请输入新姓名 (留空则不修改): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                _loggedInCustomer.Name = newName;
            }

            Console.WriteLine($"当前手机号: {_loggedInCustomer.PhoneNum}");
            Console.Write("请输入新手机号 (留空则不修改): ");
            string newPhoneNum = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newPhoneNum))
            {
                _loggedInCustomer.PhoneNum = newPhoneNum;
            }

            try
            {
                _userService.UpdateCustomerProfile(_loggedInCustomer);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("个人资料更新成功！");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"更新失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 删除当前登录的用户账户。
        /// </summary>
        static void DeleteCustomerAccount()
        {
            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录。");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("\n--- 删除账户 ---");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"警告：您正在尝试删除账户 {_loggedInCustomer.CustomerID} ({_loggedInCustomer.Name})。此操作不可逆！");
            Console.Write("请输入您的密码以确认删除: ");
            string password = GetHiddenConsoleInput();
            Console.ResetColor();

            try
            {
                // 再次认证密码以确认删除意图
                Customer authenticatedCustomer = _userService.AuthenticateCustomer(_loggedInCustomer.CustomerID, password);
                if (authenticatedCustomer == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("密码不正确，账户删除失败。");
                    Console.ResetColor();
                    return;
                }

                _userService.DeleteCustomerAccount(_loggedInCustomer.CustomerID);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"账户 {_loggedInCustomer.CustomerID} 已成功删除。");
                Console.ResetColor();
                _loggedInCustomer = null; // 清除登录状态
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"删除账户失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 用户登出功能。
        /// </summary>
        static void LogoutCustomer()
        {
            if (_loggedInCustomer != null)
            {
                Console.WriteLine($"\n用户 {_loggedInCustomer.Name} 已登出。");
                _loggedInCustomer = null;
            }
            else
            {
                Console.WriteLine("\n当前没有用户登录。");
            }
        }

        /// <summary>
        /// 查看电影排挡功能。
        /// </summary>
        static void ViewFilmShowings()
        {
            Console.WriteLine("\n--- 查看电影排挡 ---");

            try
            {
                List<Film> films = _filmService.GetAvailableFilms();
                if (!films.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("当前没有可用的电影。");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("当前上映电影列表：");
                for (int i = 0; i < films.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {films[i].FilmName} ({films[i].Genre}) - 票价: {films[i].NormalPrice:C}"); // 显示票价
                }

                Console.Write("请输入电影序号查看排挡和详情 (0 返回主菜单): ");
                if (int.TryParse(Console.ReadLine(), out int filmChoice) && filmChoice > 0 && filmChoice <= films.Count)
                {
                    Film selectedFilm = films[filmChoice - 1];
                    Console.WriteLine($"\n--- 电影 '{selectedFilm.FilmName}' 详情 ---");
                    Console.WriteLine($"  类型: {selectedFilm.Genre}");
                    Console.WriteLine($"  时长: {selectedFilm.FilmLength} 分钟");
                    Console.WriteLine($"  普通票价: {selectedFilm.NormalPrice:C}");
                    Console.WriteLine($"  上映日期: {selectedFilm.ReleaseDate.ToShortDateString()}");
                    Console.WriteLine($"  评分: {selectedFilm.Score}");
                    Console.WriteLine($"  票房: {selectedFilm.BoxOffice}");
                    Console.WriteLine($"  观影人次: {selectedFilm.Admissions}");

                    // 获取并显示演职人员
                    List<Cast> castMembers = _filmService.GetFilmCast(selectedFilm.FilmName);
                    if (castMembers.Any())
                    {
                        Console.WriteLine("\n  --- 演职人员 ---");
                        foreach (var cast in castMembers)
                        {
                            Console.WriteLine($"    - {cast.MemberName} ({cast.Role})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\n  无演职人员信息。");
                    }


                    // 获取今天该电影的场次
                    List<Section> sections = _showingService.GetFilmShowings(selectedFilm.FilmName, DateTime.Today);
                    if (!sections.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n电影 '{selectedFilm.FilmName}' 今天 ({DateTime.Today.ToShortDateString()}) 没有排挡。");
                        Console.ResetColor();
                        return;
                    }

                    Console.WriteLine($"\n场次列表 ({DateTime.Today.ToShortDateString()}):");
                    for (int i = 0; i < sections.Count; i++)
                    {
                        var section = sections[i];
                        Console.WriteLine($"{i + 1}. 场次ID: {section.SectionID}, 影厅: {section.MovieHall.HallNo} ({section.MovieHall.Category}), 时段: {section.TimeSlot.StartTime:hh\\:mm}-{section.TimeSlot.EndTime:hh\\:mm}");
                    }
                }
                else if (filmChoice == 0)
                {
                    return; // 返回主菜单
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的电影选择。");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"查看电影排挡失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 购票功能菜单。
        /// </summary>
        static void PurchaseTicketMenu()
        {
            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录才能购票。");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("\n--- 购票 ---");

            try
            {
                List<Film> films = _filmService.GetAvailableFilms();
                if (!films.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("当前没有可供购票的电影。");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("请选择要购票的电影：");
                for (int i = 0; i < films.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {films[i].FilmName} ({films[i].Genre}) - 票价: {films[i].NormalPrice:C}");
                }

                Console.Write("请输入电影序号 (0 返回主菜单): ");
                if (!int.TryParse(Console.ReadLine(), out int filmChoice) || filmChoice <= 0 || filmChoice > films.Count)
                {
                    if (filmChoice == 0) return;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的电影选择。");
                    Console.ResetColor();
                    return;
                }
                Film selectedFilm = films[filmChoice - 1];

                Console.WriteLine($"\n--- 电影 '{selectedFilm.FilmName}' 的可用场次 ---");
                List<Section> sections = _showingService.GetFilmShowings(selectedFilm.FilmName, DateTime.Today);
                if (!sections.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"电影 '{selectedFilm.FilmName}' 今天 ({DateTime.Today.ToShortDateString()}) 没有可用场次。");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("请选择一个场次：");
                for (int i = 0; i < sections.Count; i++)
                {
                    var section = sections[i];
                    Console.WriteLine($"{i + 1}. 场次ID: {section.SectionID}, 影厅: {section.MovieHall.HallNo}, 时段: {section.TimeSlot.StartTime:hh\\:mm}-{section.TimeSlot.EndTime:hh\\:mm}"); 
                }

                Console.Write("请输入场次序号 (0 返回主菜单): ");
                if (!int.TryParse(Console.ReadLine(), out int sectionChoice) || sectionChoice <= 0 || sectionChoice > sections.Count)
                {
                    if (sectionChoice == 0) return;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的场次选择。");
                    Console.ResetColor();
                    return;
                }
                Section selectedSection = sections[sectionChoice - 1];

                Console.WriteLine($"\n--- 场次 ID: {selectedSection.SectionID} 的可用座位 ---");
                Dictionary<string, List<string>> availableSeats = _showingService.GetAvailableSeats(selectedSection);
                if (!availableSeats.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"场次 ID: {selectedSection.SectionID} 没有可用座位。");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("可用座位：");
                foreach (var row in availableSeats.OrderBy(r => r.Key))
                {
                    Console.WriteLine($"  行 {row.Key}: {string.Join(", ", row.Value)}");
                }

                Console.Write("请输入要购买的座位行号 (例如: A): ");
                string lineNo = Console.ReadLine().ToUpper();
                Console.Write("请输入要购买的座位列号 (例如: 12): ");
                if (!int.TryParse(Console.ReadLine(), out int columnNo))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的列号。");
                    Console.ResetColor();
                    return;
                }

                // 再次验证座位是否真的可用
                if (!availableSeats.ContainsKey(lineNo) || !availableSeats[lineNo].Contains(columnNo.ToString()))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"座位 {lineNo}{columnNo} 不可用或不存在，请重新选择。");
                    Console.ResetColor();
                    return;
                }

                Console.Write("请输入支付方式 (例如: 支付宝, 微信支付): ");
                string paymentMethod = Console.ReadLine();

                Console.WriteLine($"\n--- 正在为顾客 {_loggedInCustomer.Name} 购买场次 {selectedSection.SectionID} 的座位 {lineNo}{columnNo} ---");
                OrderForTickets bookedOrder = _bookingService.PurchaseTicket(selectedSection.SectionID, lineNo, columnNo, _loggedInCustomer.CustomerID, paymentMethod);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"购票成功！订单信息：{bookedOrder.ToString()}");
                Console.ResetColor();

                // 购票成功后，刷新用户积分信息
                _loggedInCustomer = _customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
                Console.WriteLine($"您的新积分: {_loggedInCustomer.VIPCard?.Points ?? 0}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"购票失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        // ====================================================================
        // 管理员相关功能
        // ====================================================================

        /// <summary>
        /// 管理员注册功能。
        /// </summary>
        static void RegisterAdministrator()
        {
            Console.WriteLine("\n--- 注册新管理员 ---");
            Console.Write("请输入新管理员的ID (例如: ADMIN002): ");
            string adminId = Console.ReadLine();
            Console.Write("请输入新管理员的姓名: ");
            string name = Console.ReadLine();
            Console.Write("请输入新管理员的手机号: ");
            string phoneNum = Console.ReadLine();
            Console.Write("请输入密码: ");
            string password = GetHiddenConsoleInput();
            Console.Write("请再次输入密码确认: ");
            string confirmPassword = GetHiddenConsoleInput();

            if (password != confirmPassword)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("两次输入的密码不一致，请重试。");
                Console.ResetColor();
                return;
            }

            try
            {
                Administrator newAdmin = new Administrator
                {
                    AdminID = adminId,
                    AdminName = name,
                    PhoneNum = phoneNum
                };
                _adminService.RegisterAdministrator(newAdmin, password);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"管理员 {newAdmin.AdminName} (ID: {newAdmin.AdminID}) 注册成功！");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"管理员注册失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 管理员登录功能。
        /// </summary>
        static void LoginAdministrator()
        {
            Console.WriteLine("\n--- 管理员登录 ---");
            Console.Write("请输入您的管理员ID: ");
            string adminId = Console.ReadLine();
            Console.Write("请输入您的密码: ");
            string password = GetHiddenConsoleInput();

            try
            {
                _loggedInAdmin = _adminService.AuthenticateAdministrator(adminId, password);
                if (_loggedInAdmin != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"管理员 {(_loggedInAdmin.AdminName ?? _loggedInAdmin.AdminID)} 登录成功！");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("管理员登录失败：ID或密码不正确。");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"登录异常: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 管理员登出功能。
        /// </summary>
        static void LogoutAdministrator()
        {
            if (_loggedInAdmin != null)
            {
                Console.WriteLine($"\n管理员 {(_loggedInAdmin.AdminName ?? _loggedInAdmin.AdminID)} 已登出。");
                _loggedInAdmin = null;
            }
            else
            {
                Console.WriteLine("\n当前没有管理员登录。");
            }
        }

        /// <summary>
        /// 管理电影的子菜单。
        /// </summary>
        static void ManageFilmsMenu()
        {
            bool managingFilms = true;
            while (managingFilms)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine("      管理员 - 电影管理菜单      ");
                Console.WriteLine("======================================");
                Console.WriteLine("1. 添加新电影");
                Console.WriteLine("2. 更新电影信息");
                Console.WriteLine("0. 返回主菜单");
                Console.WriteLine("======================================");
                Console.Write("请选择一个操作: ");

                string choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            AddFilm();
                            break;
                        case "2":
                            UpdateFilm();
                            break;
                        case "0":
                            managingFilms = false;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("无效的选择，请重试。");
                            Console.ResetColor();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"操作失败: {ex.Message}");
                    Console.ResetColor();
                }
                if (managingFilms)
                {
                    Console.WriteLine("\n按任意键继续...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// 管理员：添加电影。
        /// </summary>
        static void AddFilm()
        {
            Console.WriteLine("\n--- 添加新电影 ---");
            Console.Write("请输入电影名称: ");
            string filmName = Console.ReadLine();
            Console.Write("请输入电影类型 (例如: 剧情 / 科幻): ");
            string genre = Console.ReadLine();
            Console.Write("请输入电影时长 (分钟): ");
            if (!int.TryParse(Console.ReadLine(), out int filmLength))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("时长输入无效。");
                Console.ResetColor();
                return;
            }
            Console.Write("请输入普通票价: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal normalPrice))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("票价输入无效。");
                Console.ResetColor();
                return;
            }
            Console.Write("请输入上映日期 (YYYY-MM-DD): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime releaseDate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("日期格式无效。");
                Console.ResetColor();
                return;
            }

            var newFilm = new Film
            {
                FilmName = filmName,
                Genre = genre,
                FilmLength = filmLength,
                NormalPrice = normalPrice,
                ReleaseDate = releaseDate,
                // 其他字段可以根据需要输入或设置为默认值
                Admissions = 0,
                BoxOffice = 0,
                Score = 0
            };

            try
            {
                _adminService.AddFilm(newFilm);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"电影《{newFilm.FilmName}》添加成功！");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"添加电影失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 管理员：更新电影信息。
        /// </summary>
        static void UpdateFilm()
        {
            Console.WriteLine("\n--- 更新电影信息 ---");
            Console.Write("请输入要更新的电影名称: ");
            string filmName = Console.ReadLine();

            Film existingFilm = _filmService.GetFilmDetails(filmName);
            if (existingFilm == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"未找到电影《{filmName}》。");
                Console.ResetColor();
                return;
            }

            Console.WriteLine($"当前电影《{existingFilm.FilmName}》信息:");
            Console.WriteLine($"  类型: {existingFilm.Genre}");
            Console.Write("请输入新类型 (留空则不修改): ");
            string newGenre = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newGenre)) existingFilm.Genre = newGenre;

            Console.WriteLine($"  时长: {existingFilm.FilmLength} 分钟");
            Console.Write("请输入新时长 (分钟, 留空则不修改): ");
            string newLengthStr = Console.ReadLine();
            if (int.TryParse(newLengthStr, out int newLength)) existingFilm.FilmLength = newLength;

            Console.WriteLine($"  普通票价: {existingFilm.NormalPrice:C}");
            Console.Write("请输入新普通票价 (留空则不修改): ");
            string newPriceStr = Console.ReadLine();
            if (decimal.TryParse(newPriceStr, out decimal newPrice)) existingFilm.NormalPrice = newPrice;

            Console.WriteLine($"  上映日期: {existingFilm.ReleaseDate.ToShortDateString()}");
            Console.Write("请输入新上映日期 (YYYY-MM-DD, 留空则不修改): ");
            string newReleaseDateStr = Console.ReadLine();
            if (DateTime.TryParse(newReleaseDateStr, out DateTime newReleaseDate)) existingFilm.ReleaseDate = newReleaseDate;

            Console.WriteLine($"  下映日期: {existingFilm.EndDate?.ToShortDateString() ?? "无"}");
            Console.Write("请输入新下映日期 (YYYY-MM-DD, 留空则不修改, 输入 'null' 清空): ");
            string newEndDateStr = Console.ReadLine();
            if (newEndDateStr.ToLower() == "null")
            {
                existingFilm.EndDate = null;
            }
            else if (DateTime.TryParse(newEndDateStr, out DateTime newEndDate))
            {
                existingFilm.EndDate = newEndDate;
            }

            Console.WriteLine($"  评分: {existingFilm.Score}");
            Console.Write("请输入新评分 (留空则不修改): ");
            string newScoreStr = Console.ReadLine();
            if (int.TryParse(newScoreStr, out int newScore)) existingFilm.Score = newScore;

            Console.WriteLine($"  票房: {existingFilm.BoxOffice}");
            Console.Write("请输入新票房 (留空则不修改): ");
            string newBoxOfficeStr = Console.ReadLine();
            if (int.TryParse(newBoxOfficeStr, out int newBoxOffice)) existingFilm.BoxOffice = newBoxOffice;

            Console.WriteLine($"  观影人次: {existingFilm.Admissions}");
            Console.Write("请输入新观影人次 (留空则不修改): ");
            string newAdmissionsStr = Console.ReadLine();
            if (int.TryParse(newAdmissionsStr, out int newAdmissions)) existingFilm.Admissions = newAdmissions;

            try
            {
                _adminService.UpdateFilm(existingFilm);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"电影《{existingFilm.FilmName}》信息更新成功！");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"更新电影失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 管理员：查看所有订单。
        /// </summary>
        static void ViewAllOrders()
        {
            Console.WriteLine("\n--- 查看所有订单 ---");
            try
            {
                // 调用 GetAllOrders，不传入可选参数，匹配接口签名
                List<OrderForTickets> orders = _adminService.GetAllOrders();
                if (!orders.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("当前没有订单记录。");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("所有订单列表:");
                foreach (var order in orders.OrderByDescending(o => o.Day).ThenByDescending(o => o.OrderID))
                {
                    Console.WriteLine($"- 订单ID: {order.OrderID}, 票号: {order.TicketID}, 顾客ID: {order.CustomerID}, 状态: {order.State}, 金额: {order.TotalPrice:C}, 日期: {order.Day.ToShortDateString()}, 支付方式: {order.PaymentMethod}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"查看所有订单失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 从控制台获取隐藏输入的密码。
        /// </summary>
        /// <returns>用户输入的字符串。</returns>
        static string GetHiddenConsoleInput()
        {
            StringBuilder input = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true); // true 表示不显示按下的键

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine(); // 换行
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b"); // 删除一个字符
                }
                else if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || char.IsSymbol(key.KeyChar))
                {
                    input.Append(key.KeyChar);
                    Console.Write("*"); // 显示星号
                }
            }
            return input.ToString();
        }
    }
}
}



