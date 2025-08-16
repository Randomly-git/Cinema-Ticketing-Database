using test.Models; // 引用 Models 命名空间
using test.Repositories; // 引用 Repositories 命名空间
using System;
using System.Collections.Generic;
using System.Linq;

namespace test.Services 
{
    /// <summary>
    /// 用户业务服务接口。
    /// </summary>
    public interface IUserService
    {
        Customer AuthenticateCustomer(string customerId, string password); // 顾客登录认证
        void RegisterCustomer(Customer customer, string password); // 注册新顾客
        void UpdateCustomerProfile(Customer customer); // 更新顾客资料
        void DeleteCustomerAccount(string customerId); // 删除顾客账户

        void AddPoints(string customerId, int points); // 增加积分
        void DeductPoints(string customerId, int points); // 扣除积分
        void UpdateMembershipLevel(string customerId); // 根据积分更新会员等级

        bool IsCustomerInRole(Customer customer, string roleName); // 判断用户是否拥有某个角色
        // bool IsAdministrator(Customer customer); // 暂时不提供管理员认证，只提供角色判断
    }
}
