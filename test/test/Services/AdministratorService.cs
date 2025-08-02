using test.Models;
using test.Repositories;
using test.Helpers;
using System;
using System.Collections.Generic;

namespace test.Services
{

    /// 管理员业务服务实现
    public class AdministratorService : IAdministratorService
    {
        private readonly IAdministratorRepository _administratorRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFilmRepository _filmRepository; // 新增电影仓库依赖

        // 构造函数注入仓储依赖
        public AdministratorService(IAdministratorRepository administratorRepository, IOrderRepository orderRepository,IFilmRepository filmRepository)
        {
            _administratorRepository = administratorRepository;
            _orderRepository = orderRepository;
            _filmRepository = filmRepository;

        }

        /// 管理员登录认证

        public Administrator AuthenticateAdministrator(string adminId, string password)
        {
            // 验证管理员是否存在
            var admin = _administratorRepository.GetAdministratorByID(adminId);
            if (admin == null)
            {
                return null; // 管理员不存在
            }

            // 获取存储的密码哈希和盐值
            var credentials = _administratorRepository.GetAdministratorPasswordHashAndSalt(adminId);
            if (credentials == null)
            {
                return null; // 认证信息不完整
            }

            // 验证密码
            if (PasswordHelper.VerifyPassword(password, credentials.Item1, credentials.Item2))
            {
                // 认证成功，角色已在构造函数中初始化（默认包含"影院管理员"）
                return admin;
            }
            return null; // 密码错误
        }


        /// 注册新管理员

        public void RegisterAdministrator(Administrator admin, string password)
        {
            // 检查手机号是否已被使用
            if (_administratorRepository.GetAdministratorByPhoneNum(admin.PhoneNum) != null)
            {
                throw new InvalidOperationException("该手机号已注册管理员账号。");
            }

            // 检查管理员ID是否已存在
            if (_administratorRepository.GetAdministratorByID(admin.AdminID) != null)
            {
                throw new InvalidOperationException("管理员ID已存在，请更换ID。");
            }

            // 调用仓储层添加管理员（自动处理密码哈希和盐值）
            _administratorRepository.AddAdministrator(admin, password);
        }


        /// 更新管理员资料（姓名、手机号）

        public void UpdateAdministratorProfile(Administrator admin)
        {
            var existingAdmin = _administratorRepository.GetAdministratorByID(admin.AdminID);
            if (existingAdmin == null)
            {
                throw new ArgumentException("管理员不存在，无法更新。");
            }

            // 仅允许更新姓名和手机号（避免直接修改ID或权限相关字段）
            existingAdmin.AdminName = admin.AdminName;
            existingAdmin.PhoneNum = admin.PhoneNum;

            _administratorRepository.UpdateAdministrator(existingAdmin);
        }


        /// 删除管理员

        public void DeleteAdministrator(string adminId)
        {
            var admin = _administratorRepository.GetAdministratorByID(adminId);
            if (admin == null)
            {
                throw new ArgumentException("管理员不存在，无法删除。");
            }

            _administratorRepository.DeleteAdministrator(adminId);
        }


        /// 判断管理员是否拥有指定角色

        public bool IsAdministratorInRole(Administrator admin, string roleName)
        {
            if (admin == null)
                return false;

            // 管理员默认拥有"影院管理员"角色，可扩展其他角色
            return admin.Roles.Contains(roleName);
        }

        /// <summary>
        /// 获取所有订单（支持按日期范围筛选）
        /// </summary>
        /// <param name="startDate">开始日期（可选）</param>
        /// <param name="endDate">结束日期（可选）</param>
        /// <returns>订单列表</returns>
        public List<OrderForTickets> GetAllOrders(DateTime? startDate = null, DateTime? endDate = null)
        {
            // 调用已完成的代码，查询所有订单：目前只支持按日期范围筛选
            return _orderRepository.GetAllOrders(startDate, endDate);
        }

        /// <summary>
        /// 添加新电影（需管理员权限）
        /// </summary>
        public void AddFilm(Film film)
        {
            // 1. 验证电影是否已存在
            if (_filmRepository.GetFilmByName(film.FilmName) != null)
            {
                throw new InvalidOperationException($"电影《{film.FilmName}》已存在，无法重复添加。");
            }

            // 2. 执行添加操作
            _filmRepository.AddFilm(film);
        }

        /// <summary>
        /// 更新电影信息（需管理员权限）
        /// </summary>
        public void UpdateFilm(Film film)
        {
            // 1. 验证电影是否存在
            var existingFilm = _filmRepository.GetFilmByName(film.FilmName);
            if (existingFilm == null)
            {
                throw new KeyNotFoundException($"电影《{film.FilmName}》不存在，无法更新。");
            }

            // 2. 执行更新操作
            _filmRepository.UpdateFilm(film);
        }

       


    }
}