using System;
using System.Collections.Generic; 

namespace test.Models 
{
    /// 所有用户类型的基类，包含用户共有的核心身份和认证属性。
    /// 这是一个抽象类，不能直接实例化，只能被继承。
    public abstract class UserBase
    {
        // 身份标识：对应数据库 customer 表的 customerID，用户的唯一ID
        public string CustomerID { get; set; }

        // 用户名：用于登录认证
        // 在你的 customer 表中，可能直接使用 CustomerID 作为登录名，或者 PhoneNum 作为登录名。
        // 如果是 PhoneNum，那么这个 Username 属性可以映射到 PhoneNum。
        // 这里为了通用性，保留 Username 概念。
        public string Username { get; set; }

        // 密码哈希：存储用户密码的哈希值，用于登录验证
        // 数据库中应有对应的字段（例如：PASSWORD_HASH）
        public string PasswordHash { get; set; }

        // 盐值：与密码哈希配合使用，增强密码安全性
        // 数据库中应有对应的字段（例如：SALT）
        public string Salt { get; set; }

        // 会员等级：对应 customer 表的 vipLevel
        // 这个字段可以作为区分用户类型（普通顾客、会员顾客）的标识
        // 0 = 无会员/普通顾客
        // 1-3 = 不同等级的会员顾客
        public int VipLevel { get; set; }

        // 运行时角色列表：用于在用户登录后，存储其当前拥有的角色
        // 这是一个运行时属性，不在数据库中直接存储，而是根据 VipLevel 或其他逻辑判断后填充
        public List<string> Roles { get; set; } = new List<string>();

        // 构造函数：可以在这里进行一些通用的初始化
        public UserBase()
        {
            // 默认新用户为无会员（0级）
            VipLevel = 0;
        }

        // 抽象方法或虚方法（可选）：
        // 如果所有用户类型都有一个共同但实现不同的行为，可以在这里定义。
        // 例如，一个获取用户显示名称的方法：
        public virtual string GetDisplayName()
        {
            // 默认返回用户名，派生类可以重写以返回更友好的名称
            return Username;
        }

        // 示例：一个用于调试或日志记录的基本信息方法
        public override string ToString()
        {
            return $"用户ID: {CustomerID}, 用户名: {Username}, 会员等级: {VipLevel}";
        }
    }
}
