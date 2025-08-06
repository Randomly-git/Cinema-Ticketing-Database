using test.Models;
using System;
using System.Collections.Generic;

namespace test.Services
{
    /// <summary>
    /// 管理员业务服务接口
    /// </summary>
    public interface IAdministratorService
    {
        // 管理员登录认证
        Administrator AuthenticateAdministrator(string adminId, string password);

        // 新增管理员（需超级管理员权限，此处简化为基础实现）
        void RegisterAdministrator(Administrator admin, string password);

        // 更新管理员信息（不含密码）
        void UpdateAdministratorProfile(Administrator admin);

        // 删除管理员（需超级管理员权限，此处简化为基础实现）
        void DeleteAdministrator(string adminId);

        // 判断管理员是否拥有指定角色（扩展用）
        bool IsAdministratorInRole(Administrator admin, string roleName);

        //新增获取所有订单的方法
        List<OrderForTickets> GetAllOrders(DateTime? startDate = null, DateTime? endDate = null);

        //电影管理
        void AddFilm(Film film); // 添加新电影
        void UpdateFilm(Film film); // 更新电影信息
    }
}