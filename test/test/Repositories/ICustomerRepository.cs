using test.Models; // 引用 Models 命名空间
using System;
using System.Collections.Generic;

namespace test.Repositories 
{
    /// <summary>
    /// 顾客数据仓储接口。
    /// </summary>
    public interface ICustomerRepository
    {
        Customer GetCustomerById(string customerId); // 根据ID获取顾客信息
        // UpdateCustomerPoints 已移除，积分更新通过 VIPCard 仓库处理

        // 新增：用于注册时获取密码哈希和盐值的方法
        Tuple<string, string> GetCustomerPasswordHashAndSalt(string customerId); // 新增统一方法

        void AddCustomer(Customer customer, string plainPassword); // 添加新顾客（明文密码由 DAL 层处理哈希）

        Customer GetCustomerByPhoneNum(string phoneNum); // 根据手机号获取顾客信息
        void UpdateCustomer(Customer customer); // 更新顾客基本信息
        void DeleteCustomer(string customerId); // 删除顾客
        VIPCard GetVIPCardByCustomerID(string customerId); // 获取会员卡信息
        void AddVIPCard(VIPCard vipCard); // 添加会员卡记录
        void UpdateVIPCardPoints(string customerId, int pointsChange); // 增减会员积分
        void UpdateCustomerVipLevel(string customerId, int newVipLevel); // 更新顾客的会员等级
    }
}
