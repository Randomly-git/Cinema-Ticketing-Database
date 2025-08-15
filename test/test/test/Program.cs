using Oracle.ManagedDataAccess.Client; // 确保引用此命名空间
using System;
using System.Collections.Generic; // 用于 List<T> 和 Dictionary<TKey, TValue>
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq; 
using System.Text;
using test.Models;
using test.Repositories;
using test.Services;

namespace test 
{
    class Program
    {
        // 当前登录的顾客和管理员
        private static Customer _loggedInCustomer = null;
        private static Administrator _loggedInAdmin = null; // 新增：管理员登录状态

        // 服务实例
        private static IUserService _userService;
        private static DatabaseService _dbService;
        private static IFilmService _filmService;
        private static IShowingService _showingService;
        private static IBookingService _bookingService;
        private static IAdministratorService _adminService; // 管理员服务
        private static ISchedulingService _schedulingService;
        private static IProductService _productService; // 周边产品服务
        private static IRatingService _ratingService;  // 影评服务

        // 仓库实例 (某些操作可能需要直接访问，例如管理员删除)
        private static ICustomerRepository _customerRepository;
        private static IOrderRepository _orderRepository; // 订单仓库
        private static IFilmRepository _filmRepository; // 电影仓库
        private static IRelatedProductRepository _relatedProductRepository; // 周边产品仓库
        private static IOrderForProductRepository _orderForProductRepository; // 周边产品订单仓库
        private static IRatingRepository _ratingRepository;// 影评仓库

        static void Main(string[] args)
        {
            Console.WriteLine("--- 启动电影院管理系统 ---");

            // 直接在这里定义连接字符串
            // 请务必替换为你的实际数据库信息
            string connectionString = "Data Source=//8.148.76.54:1524/orclpdb1;User Id=cbc;Password=123456;";

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
            IAdministratorRepository adminRepository = new OracleAdministratorRepository(connectionString); // 实例化管理员仓库
            _relatedProductRepository = new OracleRelatedProductRepository(connectionString); // 实例化周边产品仓库
            _orderForProductRepository = new OracleOrderForProductRepository(connectionString); // 实例化周边产品订单仓库
            _ratingRepository = new OracleRatingRepository(connectionString);                  // 实例化影评仓库

            _dbService = new DatabaseService(connectionString);
            _userService = new UserService(_customerRepository);
            _filmService = new FilmService(_filmRepository);
            _showingService = new ShowingService(showingRepository, _filmRepository);
            _bookingService = new BookingService(showingRepository, _filmRepository, _customerRepository, _orderRepository,_dbService ,connectionString);
            // 根据提供的 IAdministratorService 接口和错误信息，AdministratorService 构造函数应接受 3 个参数
            _adminService = new AdministratorService(adminRepository, _orderRepository, _filmRepository,_relatedProductRepository,_orderForProductRepository); // 新增管理员服务
            _productService = new ProductService(_relatedProductRepository, _orderForProductRepository,connectionString); // 实例化周边产品服务
            _schedulingService = new SchedulingService(connectionString);
            _ratingService = new RatingService(_schedulingService, _ratingRepository, _filmRepository, _orderRepository, connectionString);

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
                    Console.WriteLine("4. 管理员登录"); // 新增管理员登录选项
                }
                else if (_loggedInCustomer != null)
                {
                    Console.WriteLine($"当前用户: {_loggedInCustomer.Name} (ID: {_loggedInCustomer.CustomerID}, 等级: {_loggedInCustomer.VipLevel}, 积分: {_loggedInCustomer.VIPCard?.Points ?? 0})");
                    Console.WriteLine("1. 更新个人资料");
                    Console.WriteLine("2. 查看电影排挡");
                    Console.WriteLine("3. 查看电影评分与评论");
                    Console.WriteLine("4. 购票");
                    Console.WriteLine("5. 查看所有有效订单");
                    Console.WriteLine("6. 退票");
                    Console.WriteLine("7. 购买周边");
                    Console.WriteLine("8. 积分兑换周边");
                    Console.WriteLine("9. 影片排档、撤档信息");
                    Console.WriteLine("10. 影片概况查询");
                    Console.WriteLine("11. 演职人员查询");
                    Console.WriteLine("12. 电影数据统计");
                    Console.WriteLine("13. 评价电影");
                    Console.WriteLine("14. 我的用户画像");
                    Console.WriteLine("15. 个性化电影推荐");
                    Console.WriteLine("16. 删除我的账户");
                    Console.WriteLine("17. 用户登出");
                }
                else if (_loggedInAdmin != null)
                {
                    Console.WriteLine($"当前管理员: {_loggedInAdmin.AdminName} (ID: {_loggedInAdmin.AdminID})");
                    Console.WriteLine("1. 电影管理 (增/改)");
                    Console.WriteLine("2. 查看所有订单");
                    Console.WriteLine("3. 单个添加新排片");
                    Console.WriteLine("4. 批量添加新排片");
                    Console.WriteLine("5. 自动排片");
                    Console.WriteLine("6. 查看排片");
                    Console.WriteLine("7. 删除排片");
                    Console.WriteLine("8. 添加周边产品");
                    Console.WriteLine("9. 管理员登出"); 
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
                                ViewFilmRatings();
                                break;
                            case "4":
                                PurchaseTicketMenu();
                                break;
                            case "5":
                                DisplayCustomerPaidOrders();
                                break;
                            case "6":
                                ProcessTicketRefund();
                                break;
                            case "7":
                                PurchaseProductMenu();
                                break;
                            case "8":
                                RedeemReward();
                                break;
                            case "9":
                                GetMovieSchedulingInfoInteractive();
                                break;
                            case "10":
                                GetMovieOverviewInteractive();
                                break;
                            case "11":
                                GetCastCrewDetailsInteractive();
                                break;
                            case "12":
                                GetMovieStatisticsInteractive();
                                break;
                            case "13":
                                RateMovieInteractive();
                                break;
                            case "14":
                                ViewMyProfile();
                                break;
                            case "15":
                                ViewRecommendations();
                                break;
                            case "16":
                                DeleteCustomerAccount();
                                // 如果账户被删除，则退出循环，因为用户已登出
                                if (_loggedInCustomer == null) running = false;
                                break;
                            case "17":
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
                                AddSectionInteractive();
                                break;
                            case "4":
                                BatchScheduleFilmInteractive();
                                break;
                            case "5":
                                SmartAutoScheduleFilmInteractive();
                                break;
                            case "6":
                                ViewSectionsInteractive();
                                break;
                            case "7":
                                DeleteSectionInteractive();
                                break;
                            case "8":
                                GetProductInputFromUser();
                                break; // 添加周边产品
                            case "9": 
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
                    Console.WriteLine($"  上映日期: {selectedFilm.ReleaseDate?.ToShortDateString() ?? "未定"}");
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
                    //List<Section> sections = _showingService.GetFilmShowings(selectedFilm.FilmName, DateTime.Today);
                    //if (!sections.Any())
                    //{
                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    //Console.WriteLine($"\n电影 '{selectedFilm.FilmName}' 今天 ({DateTime.Today.ToShortDateString()}) 没有排挡。");
                    //Console.ResetColor();
                    //return;
                    //}

                    // 获取电影的所有场次（不限日期）
                    List<Section> sections = _showingService.GetFilmShowings(selectedFilm.FilmName);
                    if (!sections.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n电影 '{selectedFilm.FilmName}' 目前没有任何排挡。");
                        Console.ResetColor();
                        return;
                    }

                    Console.WriteLine($"\n场次列表:");
                    for (int i = 0; i < sections.Count; i++)
                    {
                        var section = sections[i];
                        Console.WriteLine($"{i + 1}. 场次ID: {section.SectionID}");
                        Console.WriteLine($"   日期: {section.TimeSlot.StartTime:yyyy-MM-dd dddd}"); // 显示完整日期和星期
                        Console.WriteLine($"   影厅: {section.MovieHall.HallNo}号厅 ({section.MovieHall.Category})");
                        Console.WriteLine($"   时段: {section.TimeSlot.StartTime:HH:mm} - {section.TimeSlot.EndTime:HH:mm}");
                        Console.WriteLine("   ----------------------------");
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
        /// 查看电影评分与评价。
        /// </summary>
        static void ViewFilmRatings()
        {
            Console.WriteLine("\n--- 查看电影评分与评价 ---");
            Console.Write("请输入查看的电影名称: ");
            string filmName = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(filmName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("电影名称不能为空！");
                Console.ResetColor();
                return;
            }

            try
            {
                // 获取该电影的评分数据
                IEnumerable<Rating> ratings = _ratingService.GetFilmRatingDetails(filmName);

                if (!ratings.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"电影 '{filmName}' 暂无评分记录。");
                    Console.ResetColor();
                    return;
                }

                var film = _filmRepository.GetFilmByName(filmName);

                Console.WriteLine($"\n--- 电影 '{filmName}' 评分与评价 ({ratings.Count()}条) ---");
                Console.WriteLine($"总评分: {film.Score:F1}/10.0\n");

                // 按时序从高到低排序
                var sortedRatings = ratings.OrderByDescending(r => r.RatingDate);

                int index = 1;
                foreach (var rating in sortedRatings)
                {
                    Console.WriteLine($"{index++}. 评分: {rating.Score}/10");
                    Console.WriteLine($"   评价: {rating.Comment ?? "无文字评价"}");
                    Console.WriteLine($"   评价时间: {rating.RatingDate:yyyy-MM-dd HH:mm}");

                    Console.WriteLine("   ----------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"获取电影评分失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 购票功能菜单（支持单张及多张购买）。
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
                // 1. 选择电影
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
                    Console.WriteLine($"{i + 1}. {films[i].FilmName} ({films[i].Genre}) - 基础票价: {films[i].NormalPrice:C}");
                }

                Console.Write("请输入电影序号: ");
                if (!int.TryParse(Console.ReadLine(), out int filmChoice) || filmChoice < 0 || filmChoice > films.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的电影选择。");
                    Console.ResetColor();
                    return;
                }
                if (filmChoice == 0) return;
                Film selectedFilm = films[filmChoice - 1];

                // 2. 选择日期
                DateTime selectedDate = GetUserSelectedDate();

                // 3. 选择场次
                List<Section> sections = _showingService.GetFilmShowings(selectedFilm.FilmName, selectedDate);
                if (!sections.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"电影 '{selectedFilm.FilmName}' 在 {selectedDate.ToShortDateString()} 没有可用场次。");
                    Console.ResetColor();
                    return;
                }

                Section selectedSection = SelectSection(sections);
                if (selectedSection == null) return;

                // 4. 显示座位表
                DisplayFullSeatMap(selectedSection);

                // 5. 选择购买数量和座位
                Console.Write("您希望购买几张票? (输入数字, 0 返回): ");
                if (!int.TryParse(Console.ReadLine(), out int ticketCount) || ticketCount <= 0)
                {
                    if (ticketCount == 0) return;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的票数。");
                    Console.ResetColor();
                    return;
                }

                // 调用新的方法来选择多个座位
                List<SeatHall> selectedSeats = SelectMultipleSeats(selectedSection, ticketCount);
                if (selectedSeats == null || !selectedSeats.Any())
                {
                    Console.WriteLine("未选择任何座位，购票已取消。");
                    return;
                }

                // 计算每个座位票价
                List<decimal> seatPrices = new List<decimal>();
                decimal totalPrice = 0m;
                foreach (var seat in selectedSeats)
                {
                    decimal price = _bookingService.CalculateFinalTicketPrice(selectedSection, _loggedInCustomer, seat.LINENO);
                    seatPrices.Add(price);
                    totalPrice += price;
                }

                // 6. 支付流程
                string paymentMethod = null;
                Console.WriteLine("请选择支付方式：");
                Console.WriteLine("1. 现金");
                Console.WriteLine("2. 银行卡");
                Console.WriteLine("3. 微信支付");
                Console.WriteLine("4. 支付宝");
                Console.Write("输入选项: ");
                string payChoice = Console.ReadLine();

                decimal totalPointsNeeded = 0;
                if (payChoice == "1")
                {
                    paymentMethod = "现金";
                }
                else if (payChoice == "2")
                {
                    paymentMethod = "银行卡";
                }
                else if (payChoice == "3")
                {
                    paymentMethod = "微信支付";
                }
                else if (payChoice == "4")
                {
                    paymentMethod = "支付宝";
                }
                else
                {
                    Console.WriteLine("未选择有效支付方式，返回。");
                    return;
                }

                // 7. 最终确认
                Console.WriteLine("\n--- 请确认您的订单 ---");
                // ... 显示订单信息
                Console.Write("确认购买以上所有票? (Y/N): ");
                string confirmation = Console.ReadLine();

                if (confirmation.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    List<OrderForTickets> newOrders = _bookingService.PurchaseMultipleTickets(
                        selectedSection.SectionID,
                        selectedSeats,
                        _loggedInCustomer.CustomerID,
                        paymentMethod,
                        totalPointsNeeded // 传给方法，用于扣积分
                    );

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n操作完成！共 {newOrders.Count} 笔订单已生成。请在“我的订单”中查看详情。");
                    // 刷新用户信息
                    _loggedInCustomer = _customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
                    Console.WriteLine($"您的新积分: {_loggedInCustomer.VIPCard?.Points ?? 0}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("购买已取消。");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[操作失败] 错误: {ex.Message}");

                // if (ex.InnerException != null) Console.WriteLine($"   详细信息: {ex.InnerException.Message}");
                Console.ResetColor();
            }
        }


        /// <summary>
        /// 辅助方法，用于让用户循环选择多个座位。
        /// </summary>
        /// <param name="section">所选场次</param>
        /// <param name="ticketCount">要购买的票数</param>
        /// <returns>包含所选座位的列表，如果用户中途取消则返回null</returns>
        static List<SeatHall> SelectMultipleSeats(Section section, int ticketCount)
        {
            var selectedSeats = new List<SeatHall>();
            var soldSeats = _showingService.GetSoldSeatsForSection(section.SectionID); // 获取已售座位

            for (int i = 0; i < ticketCount; i++)
            {
                bool seatIsValid = false;
                while (!seatIsValid)
                {
                    Console.Write($"请选择第 {i + 1}/{ticketCount} 个座位 (格式如 F8, 0 返回): ");
                    string input = Console.ReadLine();

                    if (input == "0") return null; // 用户取消

                    // 解析输入 (简单的解析逻辑)
                    if (string.IsNullOrEmpty(input) || input.Length < 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("无效的座位格式。");
                        Console.ResetColor();
                        continue;
                    }

                    string lineNo = input.Substring(0, 1).ToUpper();
                    if (!int.TryParse(input.Substring(1), out int columnNo))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("座位格式错误，列号必须是数字。");
                        Console.ResetColor();
                        continue;
                    }

                    // 检查座位是否已被预订 
                    if (soldSeats.Any(s => s.LINENO.Equals(lineNo, StringComparison.OrdinalIgnoreCase) && s.ColumnNo == columnNo))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"座位 {lineNo}{columnNo} 已被预订，请重新选择。");
                        Console.ResetColor();
                        continue;
                    }

                    // 检查座位是否在本次操作中已被选择
                    if (selectedSeats.Any(s => s.LINENO.Equals(lineNo, StringComparison.OrdinalIgnoreCase) && s.ColumnNo == columnNo))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"您已选择过座位 {lineNo}{columnNo}，请勿重复选择。");
                        Console.ResetColor();
                        continue;
                    }

                    // 检查座位是否存在于影厅中
                    if (!IsValidSeatInHall(section.MovieHall, lineNo, columnNo))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"座位 {lineNo}{columnNo} 不存在于该影厅，请参照座位图选择。");
                        Console.ResetColor();
                        continue;
                    }

                    // 座位有效
                    selectedSeats.Add(new SeatHall { LINENO = lineNo, ColumnNo = columnNo });
                    seatIsValid = true;
                }
            }
            return selectedSeats;
        }

        static bool IsValidSeatInHall(MovieHall hall, string line, int column)
        {
            // 假设行号是 A, B, C...
            int lineIndex = char.ToUpper(line[0]) - 'A';
            return lineIndex >= 0 && lineIndex < hall.Lines && column > 0 && column <= hall.ColumnsCount;
        }


        #region Helper Methods
        private static DateTime GetUserSelectedDate()
        {
            while (true)
            {
                Console.Write("请输入查询日期(格式: yyyy-MM-dd，或直接回车查询今天场次): ");
                string dateInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(dateInput))
                    return DateTime.Today;

                if (DateTime.TryParse(dateInput, out var selectedDate))
                    return selectedDate;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("日期格式不正确，请重新输入！");
                Console.ResetColor();
            }
        }

        private static Section SelectSection(List<Section> sections)
        {
            Console.WriteLine("请选择一个场次：");
            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                Console.WriteLine($"{i + 1}. 场次ID: {section.SectionID}, 影厅: {section.MovieHall.HallNo}, " +
                                 $"时段: {section.TimeSlot.StartTime:hh\\:mm}-{section.TimeSlot.EndTime:hh\\:mm}");
            }

            Console.Write("请输入场次序号 (0 返回主菜单): ");
            if (!int.TryParse(Console.ReadLine(), out int sectionChoice) || sectionChoice <= 0 || sectionChoice > sections.Count)
            {
                if (sectionChoice == 0) return null;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("无效的场次选择。");
                Console.ResetColor();
                return null;
            }
            return sections[sectionChoice - 1];
        }

        private static void DisplayFullSeatMap(Section section)
        {
            Console.WriteLine($"\n--- 场次 ID: {section.SectionID} 的完整座位表 ---");

            // 获取完整座位状态
            var seatStatus = _showingService.GetHallSeatStatus(section);

            // 打印列号标题 (假设每行列数相同)
            var firstRow = seatStatus.First();
            Console.Write("   ");
            for (int col = 1; col <= firstRow.Value.Count; col++)
            {
                Console.Write($"{col.ToString().PadLeft(3)}");
            }
            Console.WriteLine();

            // 打印每行座位状态
            foreach (var row in seatStatus.OrderBy(r => r.Key))
            {
                Console.Write($"{row.Key.PadLeft(2)} ");
                foreach (var seat in row.Value.OrderBy(s => int.Parse(s.Key)))
                {
                    char statusSymbol = seat.Value == SeatStatus.Available ? '○' : '×';
                    Console.ForegroundColor = statusSymbol == '○' ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.Write($"{statusSymbol.ToString().PadLeft(3)}");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            Console.WriteLine("\n图例: ○=可用(绿色)  ×=已售(红色)");
            Console.WriteLine();
        }

        private static (string lineNo, int columnNo) SelectSeat(Section section)
        {
            var availableSeats = _showingService.GetAvailableSeats(section);
            if (!availableSeats.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"场次 ID: {section.SectionID} 没有可用座位。");
                Console.ResetColor();
                return (null, 0);
            }

            while (true)
            {
                Console.Write("请输入要购买的座位行号 (例如: A，输入0取消): ");
                string lineNo = Console.ReadLine().ToUpper();
                if (lineNo == "0") return (null, 0);

                Console.Write("请输入要购买的座位列号 (例如: 12，输入0取消): ");
                if (!int.TryParse(Console.ReadLine(), out int columnNo) || columnNo == 0)
                {
                    if (columnNo == 0) return (null, 0);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的列号。");
                    Console.ResetColor();
                    continue;
                }

                if (availableSeats.ContainsKey(lineNo) && availableSeats[lineNo].Contains(columnNo.ToString()))
                {
                    return (lineNo, columnNo);
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"座位 {lineNo}{columnNo} 不可用或不存在，请重新选择。");
                Console.ResetColor();
            }
        }

        private static string SelectPaymentMethod()
        {
            List<string> paymentMethods = new List<string> { "支付宝", "微信支付", "银行卡", "现金" };

            Console.WriteLine("\n请选择支付方式:");
            for (int i = 0; i < paymentMethods.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {paymentMethods[i]}");
            }

            while (true)
            {
                Console.Write("请输入数字选择支付方式 (1-4，0取消): ");
                string input = Console.ReadLine();

                if (input == "0") return null;

                if (int.TryParse(input, out int selectedPaymentIndex) &&
                    selectedPaymentIndex >= 1 &&
                    selectedPaymentIndex <= paymentMethods.Count)
                {
                    return paymentMethods[selectedPaymentIndex - 1];
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("输入无效，请重新选择！");
                Console.ResetColor();
            }
        }

        private static void ConfirmAndPurchase(Section section, string lineNo, int columnNo, string paymentMethod)
        {
            Console.WriteLine($"\n--- 订单确认 ---");
            Console.WriteLine($"电影: {section.FilmName}");
            Console.WriteLine($"场次: {section.TimeSlot.StartTime:yyyy-MM-dd HH:mm} 影厅 {section.MovieHall.HallNo}");
            Console.WriteLine($"座位: {lineNo}{columnNo}");
            Console.WriteLine($"支付方式: {paymentMethod}");

            Console.Write("\n确认购买吗？(Y/N): ");
            if (Console.ReadLine().Trim().ToUpper() != "Y")
            {
                Console.WriteLine("购票已取消。");
                return;
            }

            OrderForTickets bookedOrder = _bookingService.PurchaseTicket(
                section.SectionID, lineNo, columnNo, _loggedInCustomer.CustomerID, paymentMethod);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n购票成功！订单号: {bookedOrder.OrderID}");
            Console.WriteLine($"票号: {bookedOrder.TicketID}");
            Console.ResetColor();

            // 刷新用户信息
            _loggedInCustomer = _customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
            Console.WriteLine($"您的新积分: {_loggedInCustomer.VIPCard?.Points ?? 0}");
        }
        #endregion

        private static void DisplayCustomerPaidOrders()
        {
            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录才能查看订单。");
                Console.ResetColor();
                return;
            }

            try
            {
                // === 1. 获取并显示电影票订单 ===
                Console.WriteLine("\n=== 您的电影票订单 ===");
                var paidTicketOrders = _orderRepository.GetOrdersForCustomer(_loggedInCustomer.CustomerID, true)
                                                       .Where(o => o.State == "有效")
                                                       .OrderByDescending(o => o.Day)
                                                       .ToList();

                if (paidTicketOrders.Any())
                {
                    foreach (var order in paidTicketOrders)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\n订单ID: {order.OrderID}");
                        Console.ResetColor();

                        Console.WriteLine($"订单日期: {order.Day:yyyy-MM-dd HH:mm}");
                        Console.WriteLine($"电影票ID: {order.TicketID}");
                        Console.WriteLine($"支付方式: {order.PaymentMethod}");
                        Console.WriteLine($"金额: {order.TotalPrice:C}");
                        Console.WriteLine($"状态: {order.State}");
                        Console.WriteLine("----------------------------");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("您没有已支付的电影票订单。");
                    Console.ResetColor();
                }

                // --- 2. 获取并显示周边产品订单 ---
                Console.WriteLine("\n=== 您的周边产品订单 ===");
                // 假设有一个静态的 _orderForProductRepository 实例
                var paidProductOrders = _orderForProductRepository.GetOrdersByCustomerId(_loggedInCustomer.CustomerID)
                                                                .Where(o => o.State == "已完成") // 根据你的ProductService逻辑，状态为"已完成"
                                                                .OrderByDescending(o => o.Day)
                                                                .ToList();

                if (paidProductOrders.Any())
                {
                    foreach (var order in paidProductOrders)
                    {
                        Console.ForegroundColor = ConsoleColor.Green; // 为周边订单使用不同的颜色
                        Console.WriteLine($"\n订单ID: {order.OrderID}");
                        Console.ResetColor();

                        Console.WriteLine($"订单日期: {order.Day:yyyy-MM-dd HH:mm}");
                        Console.WriteLine($"产品名称: {order.ProductName}");
                        Console.WriteLine($"购买数量: {order.PurchaseNum}");
                        Console.WriteLine($"支付方式: {order.PMethod}");
                        Console.WriteLine($"金额: {order.Price * order.PurchaseNum:C}"); // 计算总价
                        Console.WriteLine($"状态: {order.State}");
                        Console.WriteLine("----------------------------");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("您没有周边产品订单。");
                    Console.ResetColor();
                }

                Console.WriteLine($"\n已找到 {paidTicketOrders.Count} 个电影票订单和 {paidProductOrders.Count} 个周边产品订单。");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"获取订单时出错: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ProcessTicketRefund()
        {
            DateTime testRefundTime = new DateTime(2025, 7, 15, 14, 30, 0);

            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录才能退票");
                Console.ResetColor();
                return;
            }
            try
            {
                // 1. 获取当前登录顾客的有效订单
                Console.WriteLine("\n=== 您的有效订单 ===");
                var validOrders = _orderRepository.GetOrdersForCustomer(_loggedInCustomer.CustomerID, onlyValid: true);

                if (!validOrders.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("您没有可退票的有效订单。");
                    Console.ResetColor();
                    return;
                }

                // 2. 显示可退票订单列表
                for (int i = 0; i < validOrders.Count; i++)
                {
                    var order = validOrders[i];
                    Console.WriteLine($"{i + 1}. 订单ID: {order.OrderID}");
                    Console.WriteLine($"   电影票ID: {order.TicketID}");
                    Console.WriteLine($"   订单日期: {order.Day:yyyy-MM-dd}");
                    Console.WriteLine($"   支付金额: {order.TotalPrice:C}");
                    Console.WriteLine("----------------------------");
                }

                // 3. 让用户选择要退的订单
                Console.Write("\n请输入要退票的订单编号(1-{0}): ", validOrders.Count);
                if (!int.TryParse(Console.ReadLine(), out int selectedIndex) ||
                    selectedIndex < 1 || selectedIndex > validOrders.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("输入无效，请重新选择！");
                    Console.ResetColor();
                    return;
                }

                var selectedOrder = validOrders[selectedIndex - 1];

                // 4. 确认退票
                Console.Write($"\n确认要退订单 {selectedOrder.OrderID} 吗？(Y/N): ");
                var confirm = Console.ReadLine().Trim().ToUpper();
                if (confirm != "Y")
                {
                    Console.WriteLine("退票已取消。");
                    return;
                }

                // 5. 执行退票
                decimal refundFee;
                decimal refundAmount;
                bool success = _bookingService.RefundTicket(
                    selectedOrder.OrderID,
                    //DateTime.Now,
                    testRefundTime, // 使用测试时间而非DateTime.Now
                    out refundFee,
                    out refundAmount);
                _ratingService.CancelRating(selectedOrder.OrderID);  // 如果有评论也一并撤销（正常情况下不应该出现）

                if (success)
                {

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n退票成功！");
                    Console.WriteLine($"退款金额: {selectedOrder.TotalPrice}");
                    Console.WriteLine($"退款费用: {refundFee:C}");
                    Console.WriteLine($"实际退还: {selectedOrder.TotalPrice - refundFee:C}");

                    _customerRepository.UpdateVIPCardPoints(_loggedInCustomer.CustomerID, -10);

                    // 刷新用户信息
                    _loggedInCustomer = _customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
                    Console.WriteLine($"您的新积分: {_loggedInCustomer.VIPCard?.Points ?? 0}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n退票失败，请稍后再试或联系客服。");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"退票过程中发生错误: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 交互式获取影片排档信息。
        /// </summary>
        static void GetMovieSchedulingInfoInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 影片排档、撤档信息 ---");
            Console.Write("请输入电影名称: ");
            string filmName = Console.ReadLine();
            var (film, sessions) = _filmRepository.GetMovieSchedulingInfo(filmName);

            if (film != null)
            {
                Console.WriteLine($"\n影片排档信息:");
                Console.WriteLine($"电影名称: {film.FilmName}");
                Console.WriteLine($"上映日期: {film.ReleaseDate?.ToShortDateString() ?? "N/A"}");
                Console.WriteLine($"撤档日期: {film.EndDate?.ToShortDateString() ?? "N/A"}");
                Console.WriteLine("--- 场次信息 ---");
                if (sessions.Any())
                {
                    foreach (var session in sessions)
                    {
                        Console.WriteLine($"- 场次ID: {session.SectionID}, 影厅号: {session.HallNo}, 开始时间: {session.ScheduleStartTime:yyyy-MM-dd HH:mm}, 结束时间: {session.ScheduleEndTime:yyyy-MM-dd HH:mm}");
                    }
                }
                else
                {
                    Console.WriteLine("该影片暂无排片信息。");
                }
            }
            else
            {
                Console.WriteLine($"未找到电影 '{filmName}' 的排档信息。");
            }
        }

        /// <summary>
        /// 交互式获取影片概况。
        /// </summary>
        static void GetMovieOverviewInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 影片概况查询 ---");
            Console.Write("请输入电影名称: ");
            string filmName = Console.ReadLine();
            var overview = _filmRepository.GetMovieOverview(filmName);

            if (overview != null)
            {
                Console.WriteLine($"\n影片概况信息:");
                Console.WriteLine($"电影名称: {overview.FilmName}");
                Console.WriteLine($"类型: {overview.Genre}");
                Console.WriteLine($"时长: {overview.FilmLength} 分钟");
                Console.WriteLine($"标准票价: {overview.NormalPrice:C}");
                Console.WriteLine($"本影院总票房: {overview.BoxOffice:C}");
                Console.WriteLine($"观影人次: {overview.Admissions}");
                Console.WriteLine($"评分: {overview.Score:N1}");
            }
            else
            {
                Console.WriteLine($"未找到电影 '{filmName}' 的概况信息。");
            }
        }

        /// <summary>
        /// 交互式查询演职人员。
        /// </summary>
        static void GetCastCrewDetailsInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 演职人员查询 ---");
            Console.Write("请输入演职人员姓名: ");
            string memberName = Console.ReadLine();
            var castDetails = _filmRepository.GetCastCrewDetails(memberName);

            if (castDetails.Any())
            {
                Console.WriteLine($"\n演职人员 '{memberName}' 的参演电影:");
                foreach (var detail in castDetails)
                {
                    Console.WriteLine($"- 演职人员：{detail.MemberName}，电影名称: {detail.FilmName}, 担任角色: {detail.Role}");
                }
            }
            else
            {
                Console.WriteLine($"未找到演职人员 '{memberName}' 的信息。");
            }
        }

        /// <summary>
        /// 交互式进行电影数据统计。
        /// </summary>
        static void GetMovieStatisticsInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 电影数据统计 ---");
            Console.Write("请输入电影名称: ");
            string filmName = Console.ReadLine();
            var stats = _filmRepository.GetMovieStatistics(filmName);
            var film = _filmRepository.GetMovieOverview(filmName);

            if (film != null)
            {
                Console.WriteLine($"\n电影数据统计:");
                Console.WriteLine($"电影名称: {film.FilmName}");
                Console.WriteLine($"标准票价: {film.NormalPrice:C}");
                Console.WriteLine($"本影院总票房: {stats.BoxOffice:C}");
                Console.WriteLine($"已售票数: {stats.TicketsSold}");
                Console.WriteLine($"上座率: {stats.OccupancyRate:P2}");
            }
            else
            {
                Console.WriteLine($"未找到电影 '{filmName}' 的数据统计信息。");
            }
        }

        static void PurchaseProductMenu()
        {
            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录才能购买周边产品。");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("\n--- 购买周边产品 ---");

            try
            {
                List<RelatedProduct> products = _productService.GetAvailableProducts();
                if (!products.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("当前没有可供购买的周边产品。");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("请选择要购买的周边产品：");
                for (int i = 0; i < products.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {products[i].ProductName} - 价格: {products[i].Price:C}, 库存: {products[i].ProductNumber}");
                }

                Console.Write("请输入产品序号 (0 返回主菜单): ");
                if (!int.TryParse(Console.ReadLine(), out int productChoice) || productChoice <= 0 || productChoice > products.Count)
                {
                    if (productChoice == 0) return;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的产品选择。");
                    Console.ResetColor();
                    return;
                }
                RelatedProduct selectedProduct = products[productChoice - 1];

                Console.Write($"请输入购买数量 (当前库存: {selectedProduct.ProductNumber}): ");
                if (!int.TryParse(Console.ReadLine(), out int purchaseNum) || purchaseNum <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("购买数量必须是大于0的整数。");
                    Console.ResetColor();
                    return;
                }

                Console.Write("请输入支付方式 (例如: 支付宝, 微信支付): ");
                string paymentMethod = Console.ReadLine();

                Console.WriteLine($"\n--- 正在为顾客 {_loggedInCustomer.Name} 购买 {purchaseNum} 个 {selectedProduct.ProductName} ---");
                OrderForProduct newOrder = _productService.PurchaseProduct(selectedProduct.ProductName, purchaseNum, _loggedInCustomer.CustomerID, paymentMethod);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"购买周边产品成功！订单信息：{newOrder.ToString()}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"购买周边产品失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void RedeemReward()
        {
            Console.WriteLine("\n=== 积分兑换 ===");

            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录才能购买周边产品。");
                Console.ResetColor();
                return;
            }

            // 显示可兑换商品列表
            var products = _productService.GetAvailableProducts()
                .Where(p => p.RequiredPoints > 0 && p.ProductNumber > 0)
                .ToList();

            if (products.Count == 0)
            {
                Console.WriteLine("当前没有可兑换的商品！");
                return;
            }

            Console.WriteLine("\n可兑换商品列表（商品名称 - 所需积分 - 当前库存）:");
            foreach (var product in products)
            {
                Console.WriteLine($"- {product.ProductName}：{product.RequiredPoints} 积分/个 (库存: {product.ProductNumber}个)");
            }

            Console.Write("\n请输入要兑换的商品名称: ");
            string productName = Console.ReadLine();

            Console.Write("请输入要兑换的数量: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
            {
                Console.WriteLine("请输入有效的正数数量！");
                return;
            }

            // 执行兑换
            string result = _productService.RedeemProductWithPoints(productName, quantity, _loggedInCustomer.CustomerID);
            Console.WriteLine(result);
        }

        /// <summary>
        /// 交互式电影评分功能
        /// </summary>
        static void RateMovieInteractive()
        {
            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录才能评分。");
                Console.ResetColor();
                return;
            }

            try
            {
                Console.WriteLine("\n--- 为电影评分 ---");

                // 获取用户观看过的电影（通过订单）
                var validOrders = _orderRepository.GetOrdersForCustomer(_loggedInCustomer.CustomerID, true)
                    .Where(o => o.State == "有效")
                    .ToList();

                if (!validOrders.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("您目前没有有效的电影票订单，无法评分。");
                    Console.ResetColor();
                    return;
                }

                var finishedOrders = new List<OrderForTickets>();
                DateTime testRefundTime = new DateTime(2025, 12, 11, 14, 30, 0);

                foreach (var order in validOrders)
                {
                    var ticket = _orderRepository.GetTicketById(order.TicketID);
                    if (ticket == null) continue;
                    var section = _orderRepository.GetSectionById(ticket.SectionID);
                    if (section?.TimeSlot == null) continue;

                    // 如果当前时间早于场次结束时间，则不允许评价
                    if (DateTime.Now >= section.TimeSlot.EndTime)
                    {
                        finishedOrders.Add(order);
                    }
                    else
                    {
                        Console.WriteLine($"* {_ratingService.GetFilmNamebyOrderId(order.OrderID)} 尚未结束（场次结束时间: {section.TimeSlot.EndTime:yyyy-MM-dd HH:mm}）");
                    }
                }

                if(finishedOrders.Count() == 0)
                {
                    Console.WriteLine("您尚未看完任何一部电影，无法评分");
                    return;
                }

                Console.WriteLine("\n您的可评价电影：");
                for (int i = 0; i < finishedOrders.Count(); i++)
                {
                    var order = finishedOrders[i];
                    bool hasRated = _ratingService.HasRated(order.OrderID);
                    string filmName = _ratingService.GetFilmNamebyOrderId(order.OrderID);
                    string ratedStatus = hasRated ? "(已评分)" : "";
                    Console.WriteLine($"{i + 1}. {filmName} {ratedStatus}");
                }

                Console.Write("请选择要评价/撤评/重评的电影(输入序号): ");
                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > finishedOrders.Count())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的选择。");
                    Console.ResetColor();
                    return;
                }

                var selectedOrder = finishedOrders[choice - 1];
                string selectedFilmName = _ratingService.GetFilmNamebyOrderId(selectedOrder.OrderID);

                // 检查是否已评分
                if (_ratingService.HasRated(selectedOrder.OrderID))
                {
                    Console.Write($"您已为《{selectedFilmName}》评分，撤销评论/重新评论/直接退出（D/R/E）: ");
                    string input = Console.ReadLine().Trim().ToUpper();
                    switch (input)
                    {
                        case "D":
                            _ratingService.CancelRating(selectedOrder.OrderID);
                            Console.WriteLine("评论已撤销。");
                            return; // 撤销后直接退出

                        case "R":
                            // 重新评论：先撤销，再评分
                            _ratingService.CancelRating(selectedOrder.OrderID);
                            Console.WriteLine("请重新输入评分和评论。");
                            break;

                        case "E":
                        default:
                            // 直接退出
                            return;
                    }
                }

                Console.Write($"请为《{selectedFilmName}》评分(0-10): ");
                if (!int.TryParse(Console.ReadLine(), out int score) || score < 0 || score > 10)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("评分必须为0到10之间的整数");
                    Console.ResetColor();
                    return;
                }

                Console.Write($"对《{selectedFilmName}》的评论（前200字符有效，可回车略过）: ");
                string comment = Console.ReadLine();

                // 处理评论长度限制
                if (!string.IsNullOrEmpty(comment))
                {
                    // 截断超过200字节的部分
                    comment = comment.Length > 200
                        ? comment.Substring(0, 200)
                        : comment;
                }

                _ratingService.RateOrder(selectedOrder.OrderID, score, comment);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"评分成功！您为《{selectedFilmName}》打了{score}分。");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"评分失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 查看我的用户画像（每个类型的印象分）
        /// </summary>
        static void ViewMyProfile()
        {
            if (_loggedInCustomer == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("请先登录才能查看画像。");
                Console.ResetColor();
                return;
            }

            try
            {
                Console.WriteLine("\n--- 我的用户画像 ---");
                Console.WriteLine("========================");

                // 获取用户画像数据
                var genreImpression = _ratingService.GetUserGenreImpression(_loggedInCustomer.CustomerID);
                var sortedGenres = genreImpression
                    .OrderByDescending(kv => kv.Value)
                    .ToList();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("{0,-12} {1,-8}", "电影类型", "得分");
                Console.ResetColor();
                Console.WriteLine(new string('-', 40));

                // 打印每种类型的数据
                foreach (var genre in sortedGenres)
                {
                    Console.WriteLine("{0,-12} {1,-8:F1}",
                        genre.Key,
                        genre.Value);
                    Console.ResetColor();
                }

                // 打印总结
                if (sortedGenres.Any())
                {
                    var topGenre = sortedGenres.First();
                    Console.WriteLine("\n★ 最偏好类型: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"   {topGenre.Key} (得分: {topGenre.Value:F1})");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n暂无足够数据生成用户画像");
                    Console.ResetColor();
                }

                Console.WriteLine("\n========================");


            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"获取用户画像失败: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 查看个性化推荐的电影
        /// </summary>
        static void ViewRecommendations()
        {
            var recommendations = _ratingService.GetMovieRecommendations(_loggedInCustomer.CustomerID);   // 获取推荐的电影
            Console.WriteLine("\n根据您的喜好，为您推荐未来两个月的热映电影");
            Console.WriteLine(new string('-', 40));
            foreach (var movie in recommendations)
            {
                Console.WriteLine($"电影: {movie.FilmName}");
                Console.WriteLine($"类型: {string.Join("/", movie.Genres)}");
                Console.WriteLine($"最近场次: {movie.NearestScreening:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"当前评分: {movie.Score:F1}/10 ");
                Console.WriteLine($"推荐指数: {movie.RecommendationScore:F1}");
                Console.WriteLine(new string('-', 40));
            }
        }


        // ====================================================================
        // 管理员相关功能
        // ====================================================================

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
                // 移除了 "3. 删除电影"，因为 IAdministratorService 接口中没有此方法
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
                        // 移除了 case "3": DeleteFilm(); break;
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

            Console.WriteLine($"  上映日期: {existingFilm.ReleaseDate?.ToShortDateString() ?? "未定"}");
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
        /// 交互式添加排片功能。
        /// </summary>
        static void AddSectionInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 添加新排片 ---");

            var films = _schedulingService.GetAllFilms();
            if (!films.Any())
            {
                Console.WriteLine("数据库中没有可用的电影。请先添加电影信息。");
                return;
            }
            Console.WriteLine("\n可用电影:");
            for (int i = 0; i < films.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {films[i].FilmName} (时长: {films[i].FilmLength} 分钟)");
            }
            Console.Write("请输入电影编号: ");
            if (!int.TryParse(Console.ReadLine(), out int filmIndex) || filmIndex < 1 || filmIndex > films.Count)
            {
                Console.WriteLine("无效的电影编号。");
                return;
            }
            Film selectedFilm = films[filmIndex - 1];

            var halls = _schedulingService.GetAllMovieHalls();
            if (!halls.Any())
            {
                Console.WriteLine("数据库中没有可用的影厅。请先添加影厅信息。");
                return;
            }
            Console.WriteLine("\n可用影厅:");
            for (int i = 0; i < halls.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {halls[i].HallNo} ({halls[i].Category})");
            }
            Console.Write("请输入影厅编号: ");
            if (!int.TryParse(Console.ReadLine(), out int hallIndex) || hallIndex < 1 || hallIndex > halls.Count)
            {
                Console.WriteLine("无效的影厅编号。");
                return;
            }
            int hallNo = halls[hallIndex - 1].HallNo;

            DateTime scheduleStartTime;
            while (true)
            {
                Console.Write("请输入排片开始时间 (YYYY-MM-DD HH:mm): ");
                string input = Console.ReadLine();

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out scheduleStartTime))
                {
                    break;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("时间格式不正确，请按示例格式输入");
                Console.ResetColor();
            }

            var result = _schedulingService.AddSection(selectedFilm.FilmName, hallNo, scheduleStartTime);
            Console.WriteLine(result.Message);
        }

        /// <summary>
        /// 交互式查看排片功能。
        /// </summary>
        static void ViewSectionsInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 查看排片 ---");
            Console.Write("请输入查询开始日期 (YYYY-MM-DD，留空默认为今天): ");
            string startDateStr = Console.ReadLine();
            DateTime startDate = string.IsNullOrEmpty(startDateStr) ? DateTime.Today : (DateTime.TryParse(startDateStr, out DateTime sDate) ? sDate : DateTime.Today);

            Console.Write("请输入查询结束日期 (YYYY-MM-DD，留空默认为未来7天): ");
            string endDateStr = Console.ReadLine();
            DateTime endDate = string.IsNullOrEmpty(endDateStr) ? DateTime.Today.AddDays(7) : (DateTime.TryParse(endDateStr, out DateTime eDate) ? eDate : DateTime.Today.AddDays(7));

            List<Section> sections = _schedulingService.GetSectionsByDateRange(startDate, endDate);

            if (sections.Count == 0)
            {
                Console.WriteLine($"在 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 之间没有排片信息。");
            }
            else
            {
                Console.WriteLine($"\n以下是 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 的排片信息：");
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-20} {5,-20}", "场次ID", "电影名称", "影厅号", "时段ID", "开始时间", "结束时间");
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                foreach (var section in sections)
                {
                    Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-20:yyyy-MM-dd HH:mm} {5,-20:yyyy-MM-dd HH:mm}",
                                      section.SectionID,
                                      (section.FilmName.Length > 18 ? section.FilmName.Substring(0, 15) + "..." : section.FilmName),
                                      section.HallNo,
                                      section.TimeID,
                                      section.ScheduleStartTime,
                                      section.ScheduleEndTime);
                }
                Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            }
        }

        /// <summary>
        /// 交互式删除排片功能。
        /// </summary>
        static void DeleteSectionInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 删除排片 ---");
            Console.Write("请输入要删除的场次ID: ");
            if (!int.TryParse(Console.ReadLine(), out int sectionIdToDelete))
            {
                Console.WriteLine("无效的场次ID。");
                return;
            }

            Console.Write($"确定要删除场次ID为 {sectionIdToDelete} 的排片吗？ (Y/N): ");
            string confirmation = Console.ReadLine()?.ToUpper();

            if (confirmation == "Y")
            {
                var result = _schedulingService.DeleteSection(sectionIdToDelete);
                Console.WriteLine(result.Message);
            }
            else
            {
                Console.WriteLine("删除操作已取消。");
            }
        }

        /// <summary>
        /// 交互式批量排片功能。
        /// </summary>
        static void BatchScheduleFilmInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 批量排片功能 ---");

            var films = _schedulingService.GetAllFilms();
            if (!films.Any())
            {
                Console.WriteLine("数据库中没有可用的电影。请先添加电影信息。");
                return;
            }
            Console.WriteLine("\n可用电影:");
            for (int i = 0; i < films.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {films[i].FilmName}");
            }
            Console.Write("请输入要批量排片的电影编号: ");
            if (!int.TryParse(Console.ReadLine(), out int filmIndex) || filmIndex < 1 || filmIndex > films.Count)
            {
                Console.WriteLine("无效的电影编号。");
                return;
            }
            string filmName = films[filmIndex - 1].FilmName;

            Console.Write("请输入排片开始日期 (YYYY-MM-DD，留空默认为今天): ");
            string startDateStr = Console.ReadLine();
            DateTime startDate = string.IsNullOrEmpty(startDateStr) ? DateTime.Today : (DateTime.TryParse(startDateStr, out DateTime sDate) ? sDate : DateTime.Today);

            Console.Write("请输入排片结束日期 (YYYY-MM-DD，留空默认为未来7天): ");
            string endDateStr = Console.ReadLine();
            DateTime endDate = string.IsNullOrEmpty(endDateStr) ? DateTime.Today.AddDays(7) : (DateTime.TryParse(endDateStr, out DateTime eDate) ? eDate : DateTime.Today.AddDays(7));

            Console.Write("请输入最多批量创建的场次数量 (例如: 10): ");
            if (!int.TryParse(Console.ReadLine(), out int maxSessions))
            {
                Console.WriteLine("无效的场次数量。");
                return;
            }

            Console.WriteLine($"\n正在为电影 '{filmName}' 在 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 之间批量排片，最多创建 {maxSessions} 场...");
            var result = _schedulingService.BatchScheduleFilm(filmName, startDate, endDate, maxSessions);
            Console.WriteLine(result.Message);
        }

        /// <summary>
        /// 交互式智能自动排片功能。
        /// </summary>
        static void SmartAutoScheduleFilmInteractive()
        {
            Console.Clear();
            Console.WriteLine("--- 自动排片 (智能策略) 功能 ---");

            Console.Write("请输入排片开始日期 (YYYY-MM-DD，留空默认为今天): ");
            string startDateStr = Console.ReadLine();
            DateTime startDate = string.IsNullOrEmpty(startDateStr) ? DateTime.Today : (DateTime.TryParse(startDateStr, out DateTime sDate) ? sDate : DateTime.Today);

            Console.Write("请输入排片结束日期 (YYYY-MM-DD，留空默认为未来7天): ");
            string endDateStr = Console.ReadLine();
            DateTime endDate = string.IsNullOrEmpty(endDateStr) ? DateTime.Today.AddDays(7) : (DateTime.TryParse(endDateStr, out DateTime eDate) ? eDate : DateTime.Today.AddDays(7));

            Console.Write("请输入每天每个影厅的目标场次数量 (例如: 3，留空默认为3): ");
            string targetSessionsStr = Console.ReadLine();
            int targetSessionsPerDay = string.IsNullOrEmpty(targetSessionsStr) ? 3 : (int.TryParse(targetSessionsStr, out int ts) ? ts : 3);
            if (targetSessionsPerDay <= 0) targetSessionsPerDay = 1; // 至少1场

            Console.WriteLine($"\n正在 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 之间进行智能自动排片，每天每个影厅目标 {targetSessionsPerDay} 场...");
            var result = _schedulingService.SmartAutoScheduleFilm(startDate, endDate, targetSessionsPerDay);
            Console.WriteLine(result.Message);
        }

        // 从用户输入获取产品信息
        static void GetProductInputFromUser()
        {
            RelatedProduct product = new RelatedProduct();

            Console.WriteLine("===== 添加新周边产品 =====");

            Console.Write("请输入产品名称：");
            product.ProductName = Console.ReadLine()?.Trim() ?? throw new ArgumentException("产品名称不能为空");

            Console.Write("请输入产品价格：");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
            {
                throw new ArgumentException("价格必须是有效的非负数");
            }
            product.Price = price;

            Console.Write("请输入初始库存数量：");
            if (!int.TryParse(Console.ReadLine(), out int stock) || stock < 0)
            {
                throw new ArgumentException("库存数量必须是有效的非负数");
            }
            product.ProductNumber = stock;

            Console.Write("请输入兑换所需积分：");
            if (!int.TryParse(Console.ReadLine(), out int points) || points < 0)
            {
                throw new ArgumentException("积分必须是有效的非负数");
            }
            product.RequiredPoints = points;
            _adminService.AddMerchandise(product);

        }
    


}
}



