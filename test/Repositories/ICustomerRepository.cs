using test.Models; // 引用 Models 命名空间
using System;
using System.Collections.Generic;

namespace test.Repositories 
{
    // 顾客数据仓储接口。
    public interface ICustomerRepository
    {
        Customer GetCustomerByID(string customerId);
        Customer GetCustomerByPhoneNum(string phoneNum); // 用于注册时检查手机号是否重复
        void AddCustomer(Customer customer, string plainPassword); // 注册新顾客
        void UpdateCustomer(Customer customer); // 更新顾客基本信息和 VipLevel
        void DeleteCustomer(string customerId); // 删除顾客

        VIPCard GetVIPCardByCustomerID(string customerId);
        void AddVIPCard(VIPCard vipCard);
        void UpdateVIPCardPoints(string customerId, int pointsChange); // 增减积分
        void UpdateCustomerVipLevel(string customerId, int newVipLevel); // 更新顾客会员等级

        // 新增方法：用于获取密码哈希和盐值，因为 UserBase 中包含了这些字段
        Tuple<string, string> GetCustomerPasswordHashAndSalt(string customerId);
    }
}
