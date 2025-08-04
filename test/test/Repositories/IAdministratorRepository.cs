using test.Models;
using System;
using System.Collections.Generic;

namespace test.Repositories
{
    /// <summary>
    /// 管理员数据仓储接口
    /// </summary>
    public interface IAdministratorRepository
    {
        // 根据ID获取管理员
        Administrator GetAdministratorByID(string adminId);

        // 根据手机号获取管理员（用于验证手机号唯一性）
        Administrator GetAdministratorByPhoneNum(string phoneNum);

        // 添加管理员（需传入明文密码用于哈希处理）
        void AddAdministrator(Administrator admin, string plainPassword);

        // 更新管理员信息（不含密码）
        void UpdateAdministrator(Administrator admin);

        // 删除管理员
        void DeleteAdministrator(string adminId);

        // 获取管理员密码哈希和盐值（用于登录验证）
        Tuple<string, string> GetAdministratorPasswordHashAndSalt(string adminId);
    }
}