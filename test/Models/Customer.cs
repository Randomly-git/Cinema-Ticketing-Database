using System;
using System.Collections.Generic;
using test.Models;

namespace test.Models 
{
    /// 顾客类，继承自 UserBase，代表普通顾客和会员顾客。
    /// 对应数据库中的 customer 表。
    public class Customer : UserBase
    {
        public string Name { get; set; }
        public string PhoneNum { get; set; }

        // 导航属性：方便获取会员卡信息
        public VIPCard VIPCard { get; set; }

        public Customer() : base() // 调用基类的构造函数
        {
            // 可以在这里设置 Customer 特有的默认值或初始化
        }

        public override string GetDisplayName()
        {
            return Name; // 顾客通常显示姓名
        }

        public override string ToString()
        {
            string baseInfo = base.ToString();
            return $"{baseInfo}, 姓名: {Name}, 电话: {PhoneNum}, {VIPCard?.ToString() ?? "无会员卡"}";
        }
    }
}

