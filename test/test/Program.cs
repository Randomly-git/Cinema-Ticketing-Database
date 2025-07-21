using Oracle.ManagedDataAccess.Client; // 确保引用此命名空间

using test.Models;
using test.Repositories;
using test.Services;
using System;
using System.Linq; // 用于 LINQ 扩展方法


namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- 启动顾客用户管理模块测试 ---");

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
            ICustomerRepository customerRepository = new OracleCustomerRepository(connectionString);
            IUserService userService = new UserService(customerRepository);

            // --- 测试顾客注册 ---
            Console.WriteLine("\n--- 测试顾客注册 ---");
            string customerId = "CUST001";
            string customerName = "李四";
            string customerPhone = "13811112222";
            string customerPassword = "customerPass";

            try
            {
                // 尝试删除已存在的测试用户，确保每次测试都是干净的注册
                userService.DeleteCustomerAccount(customerId);
                Console.WriteLine($"已尝试删除旧用户 {customerId} (如果存在)。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除旧用户 {customerId} 失败或不存在: {ex.Message}");
            }

            try
            {
                Customer newCustomer = new Customer
                {
                    CustomerID = customerId,
                    Username = customerId, // 假设 CustomerID 就是登录的 Username
                    Name = customerName,
                    PhoneNum = customerPhone
                };
                userService.RegisterCustomer(newCustomer, customerPassword);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"用户 {newCustomer.Name} (ID: {newCustomer.CustomerID}) 注册成功！");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"用户 {customerId} 注册失败: {ex.Message}");
                Console.ResetColor();
            }

            // --- 测试顾客登录 ---
            Console.WriteLine("\n--- 测试顾客登录 ---");
            Customer loggedInCustomer = null;
            try
            {
                loggedInCustomer = userService.AuthenticateCustomer(customerId, customerPassword);
                if (loggedInCustomer != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"用户 {loggedInCustomer.Name} (ID: {loggedInCustomer.CustomerID}) 登录成功！");
                    Console.ResetColor();
                    Console.WriteLine($"详细信息: {loggedInCustomer.ToString()}");
                    Console.WriteLine($"当前角色: {string.Join(", ", loggedInCustomer.Roles)}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"用户 {customerId} 登录失败：用户名或密码错误。");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"登录 {customerId} 发生异常: {ex.Message}");
                Console.ResetColor();
            }

            // --- 测试顾客权限 ---
            if (loggedInCustomer != null)
            {
                Console.WriteLine("\n--- 测试顾客权限 ---");
                Console.WriteLine($"是否是普通顾客: {userService.IsCustomerInRole(loggedInCustomer, UserRoles.NormalCustomer)}");
                Console.WriteLine($"是否是会员顾客: {userService.IsCustomerInRole(loggedInCustomer, UserRoles.MemberCustomer)}");
                Console.WriteLine($"是否是管理员 (预期为 False): {userService.IsCustomerInRole(loggedInCustomer, UserRoles.Administrator)}");
            }

            // --- 测试积分增加和会员升级 ---
            if (loggedInCustomer != null)
            {
                Console.WriteLine($"\n--- 测试积分增加和会员升级 ---");
                Console.WriteLine($"用户 {loggedInCustomer.Name} 当前积分: {loggedInCustomer.VIPCard?.Points ?? 0}");

                Console.WriteLine("增加 150 积分...");
                try
                {
                    userService.AddPoints(loggedInCustomer.CustomerID, 150);
                    loggedInCustomer = userService.AuthenticateCustomer(customerId, customerPassword); // 重新加载用户数据
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"增加积分后，用户 {loggedInCustomer.Name} 当前积分: {loggedInCustomer.VIPCard?.Points ?? 0}，会员等级: {loggedInCustomer.VipLevel}");
                    Console.ResetColor();
                    Console.WriteLine($"是否是会员顾客: {userService.IsCustomerInRole(loggedInCustomer, UserRoles.MemberCustomer)}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"增加积分失败: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine("再增加 400 积分 (总计 550)...");
                try
                {
                    userService.AddPoints(loggedInCustomer.CustomerID, 400);
                    loggedInCustomer = userService.AuthenticateCustomer(customerId, customerPassword); // 重新加载用户数据
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"再次增加积分后，用户 {loggedInCustomer.Name} 当前积分: {loggedInCustomer.VIPCard?.Points ?? 0}，会员等级: {loggedInCustomer.VipLevel}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"再次增加积分失败: {ex.Message}");
                    Console.ResetColor();
                }

                // 尝试扣除积分
                Console.WriteLine("尝试扣除 50 积分...");
                try
                {
                    userService.DeductPoints(loggedInCustomer.CustomerID, 50);
                    loggedInCustomer = userService.AuthenticateCustomer(customerId, customerPassword);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"扣除积分后，用户 {loggedInCustomer.Name} 当前积分: {loggedInCustomer.VIPCard?.Points ?? 0}，会员等级: {loggedInCustomer.VipLevel}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"扣除积分失败: {ex.Message}");
                    Console.ResetColor();
                }

                // 尝试扣除所有积分导致降级
                Console.WriteLine($"尝试扣除所有积分 ({loggedInCustomer.VIPCard?.Points ?? 0})...");
                try
                {
                    userService.DeductPoints(loggedInCustomer.CustomerID, loggedInCustomer.VIPCard?.Points ?? 0);
                    loggedInCustomer = userService.AuthenticateCustomer(customerId, customerPassword);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"扣除所有积分后，用户 {loggedInCustomer.Name} 当前积分: {loggedInCustomer.VIPCard?.Points ?? 0}，会员等级: {loggedInCustomer.VipLevel}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"扣除所有积分失败: {ex.Message}");
                    Console.ResetColor();
                }
            }


            Console.WriteLine("\n--- 顾客用户管理模块测试完成。按任意键退出。 ---");
            Console.ReadKey();
        }
    }
}