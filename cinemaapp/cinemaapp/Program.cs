using cinemaapp.Repositories;
using cinemaapp.Services;
using Oracle.ManagedDataAccess.Client; // 确保引用此命名空间
using System;
using System.Collections.Generic; // 用于 List<T> 和 Dictionary<TKey, TValue>
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;




namespace cinemaapp
{
    static class Program
    {
        // 当前登录的顾客和管理员
        public static Customer _loggedInCustomer = null;
        public static Administrator _loggedInAdmin = null;

        // 服务实例
        public static IUserService _userService;
        public static IFilmService _filmService;
        public static IShowingService _showingService;
        public static IBookingService _bookingService;
        public static IAdministratorService _adminService;
        public static DatabaseService _dbService;
        public static ISchedulingService _schedulingService;
        public static ITicketService _ticketService;


        // 仓库实例
        public static ICustomerRepository _customerRepository;
        public static IOrderRepository _orderRepository;
        public static IFilmRepository _filmRepository;
        public static ITicketRepository _ticketRepository;



        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("--- 启动电影院管理系统 ---");

            // ✅ 连接字符串（请替换为你实际的 Oracle 数据库配置）
            string connectionString = "Data Source=//8.148.76.54:1524/orclpdb1;User Id=cbc;Password=123456";

            // ✅ 数据库连接测试
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("数据库连接成功！");
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("数据库连接失败！\n" + ex.Message,
                    "连接错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // 不启动程序
            }

            // ✅ 初始化数据仓库与服务层
            _customerRepository = new OracleCustomerRepository(connectionString);
            _orderRepository = new OracleOrderRepository(connectionString);
            _filmRepository = new OracleFilmRepository(connectionString);
            IShowingRepository showingRepository = new OracleShowingRepository(connectionString);
            IAdministratorRepository adminRepository = new OracleAdministratorRepository(connectionString); // 管理员仓库
            _ticketRepository = new OracleTicketRepository(connectionString); 


            _dbService = new DatabaseService(connectionString);
            _userService = new UserService(_customerRepository);
            _filmService = new FilmService(_filmRepository);
            _showingService = new ShowingService(showingRepository, _filmRepository);
            _bookingService = new BookingService(showingRepository, _filmRepository, _customerRepository, _orderRepository, _dbService, connectionString);
            _adminService = new AdministratorService(adminRepository, _orderRepository, _filmRepository); // 管理员服务
            _schedulingService = new SchedulingService(connectionString);
            _ticketService = new TicketService(_ticketRepository); 


            // ✅ 启动主窗体（MainForm）
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
