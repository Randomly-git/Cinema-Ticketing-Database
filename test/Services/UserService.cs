using test.Helpers; // 引用 Helpers 命名空间
using test.Models; // 引用 Models 命名空间
using test.Repositories; // 引用 Repositories 命名空间
using System;
using System.Collections.Generic;
using System.Linq;

namespace test.Services 
{
    /// <summary>
    /// 用户业务服务实现。
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ICustomerRepository _customerRepository;

        public UserService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// 顾客登录认证。
        /// </summary>
        /// <param name="customerId">顾客ID（作为登录名）。</param>
        /// <param name="password">明文密码。</param>
        /// <returns>认证成功的 Customer 对象，否则为 null。</returns>
        public Customer AuthenticateCustomer(string customerId, string password)
        {
            var customer = _customerRepository.GetCustomerByID(customerId);
            if (customer == null)
            {
                return null; // 用户不存在
            }

            // 从数据仓储获取密码哈希和盐值
            Tuple<string, string> credentials = _customerRepository.GetCustomerPasswordHashAndSalt(customerId);
            if (credentials == null)
            {
                // 无法获取认证信息，可能用户未设置密码或数据不完整
                return null;
            }

            string storedPasswordHash = credentials.Item1;
            string storedSalt = credentials.Item2;

            if (PasswordHelper.VerifyPassword(password, storedPasswordHash, storedSalt))
            {
                // 认证成功后，填充用户角色
                customer.Roles = GetCustomerRoles(customer);
                return customer;
            }
            return null; // 密码不匹配
        }

        /// <summary>
        /// 注册新顾客。
        /// </summary>
        /// <param name="customer">顾客对象（包含ID、姓名、手机号）。</param>
        /// <param name="password">明文密码。</param>
        public void RegisterCustomer(Customer customer, string password)
        {
            // 检查手机号是否已注册
            if (_customerRepository.GetCustomerByPhoneNum(customer.PhoneNum) != null)
            {
                throw new InvalidOperationException("手机号已注册。");
            }
            // 检查 CustomerID 是否已存在（如果 CustomerID 是外部生成）
            if (_customerRepository.GetCustomerByID(customer.CustomerID) != null)
            {
                throw new InvalidOperationException("顾客ID已存在。请尝试其他ID。");
            }

            customer.VipLevel = 0; // 新注册用户默认无会员
            _customerRepository.AddCustomer(customer, password); // DAL层会处理密码哈希和VIPCard创建
        }

        /// <summary>
        /// 更新顾客资料。
        /// </summary>
        public void UpdateCustomerProfile(Customer customer)
        {
            var existingCustomer = _customerRepository.GetCustomerByID(customer.CustomerID);
            if (existingCustomer == null)
            {
                throw new ArgumentException("顾客不存在，无法更新。");
            }
            // 确保只更新允许修改的字段，例如姓名和手机号
            existingCustomer.Name = customer.Name;
            existingCustomer.PhoneNum = customer.PhoneNum;
            _customerRepository.UpdateCustomer(existingCustomer);
        }

        /// <summary>
        /// 删除顾客账户。
        /// </summary>
        public void DeleteCustomerAccount(string customerId)
        {
            var customer = _customerRepository.GetCustomerByID(customerId);
            if (customer == null)
            {
                throw new ArgumentException("顾客不存在，无法删除。");
            }
            _customerRepository.DeleteCustomer(customerId);
        }

        /// <summary>
        /// 增加积分。
        /// </summary>
        public void AddPoints(string customerId, int points)
        {
            if (points < 0) throw new ArgumentOutOfRangeException(nameof(points), "积分不能为负数。");
            _customerRepository.UpdateVIPCardPoints(customerId, points);
            UpdateMembershipLevel(customerId); // 积分变化后检查是否需要升级
        }

        /// <summary>
        /// 扣除积分。
        /// </summary>
        public void DeductPoints(string customerId, int points)
        {
            if (points < 0) throw new ArgumentOutOfRangeException(nameof(points), "积分不能为负数。");
            var vipCard = _customerRepository.GetVIPCardByCustomerID(customerId);
            if (vipCard == null || vipCard.Points < points)
            {
                throw new InvalidOperationException("积分不足。");
            }
            _customerRepository.UpdateVIPCardPoints(customerId, -points); // 扣除积分
            UpdateMembershipLevel(customerId); // 积分变化后检查是否需要降级
        }

        /// <summary>
        /// 根据积分更新会员等级。
        /// </summary>
        public void UpdateMembershipLevel(string customerId)
        {
            var vipCard = _customerRepository.GetVIPCardByCustomerID(customerId);
            if (vipCard == null) return;

            int currentPoints = vipCard.Points;
            int newVipLevel = 0; // 默认无会员

            // 根据你的会员等级规则来判断
            // 假设：0-无会员，1-铜牌（100积分），2-银牌（500积分），3-金牌（1000积分）
            if (currentPoints >= 1000)
            {
                newVipLevel = 3;
            }
            else if (currentPoints >= 500)
            {
                newVipLevel = 2;
            }
            else if (currentPoints >= 100)
            {
                newVipLevel = 1;
            }
            else
            {
                newVipLevel = 0;
            }

            // 如果等级发生变化，则更新数据库
            var customer = _customerRepository.GetCustomerByID(customerId);
            if (customer != null && customer.VipLevel != newVipLevel)
            {
                _customerRepository.UpdateCustomerVipLevel(customerId, newVipLevel);
                Console.WriteLine($"用户 {customerId} 会员等级从 {customer.VipLevel} 升级/降级到 {newVipLevel}。");
            }
        }

        /// <summary>
        /// 判断顾客是否拥有某个角色。
        /// </summary>
        public bool IsCustomerInRole(Customer customer, string roleName)
        {
            if (customer == null) return false;

            // 影院管理员：假设有一个固定的 CustomerID 来标识
            // 在此示例中，我们只测试普通顾客和会员顾客的角色
            if (roleName == UserRoles.Administrator) // 如果需要测试管理员，请确保 admin_user_id 存在
            {
                return customer.CustomerID == "admin_user_id"; // 替换为实际的管理员ID
            }

            // 会员顾客：VipLevel > 0
            if (roleName == UserRoles.MemberCustomer && customer.VipLevel > 0)
            {
                return true;
            }

            // 普通顾客：VipLevel == 0
            if (roleName == UserRoles.NormalCustomer && customer.VipLevel == 0)
            {
                return true;
            }

            return false;
        }

        // 辅助方法：根据用户 VipLevel 填充其运行时角色
        private List<string> GetCustomerRoles(Customer customer)
        {
            var roles = new List<string>();
            if (customer == null) return roles;

            // 根据 VipLevel 填充角色
            if (customer.VipLevel == 0)
            {
                roles.Add(UserRoles.NormalCustomer);
            }
            else if (customer.VipLevel > 0)
            {
                roles.Add(UserRoles.NormalCustomer); // 会员也是顾客
                roles.Add(UserRoles.MemberCustomer);
            }

            // 特殊处理管理员角色（如果需要，这里可以根据 CustomerID 判断）
            if (customer.CustomerID == "admin_user_id") // 替换为实际的管理员ID
            {
                roles.Add(UserRoles.Administrator);
            }

            return roles;
        }
    }
}

