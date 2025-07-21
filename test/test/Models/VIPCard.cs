using System;

namespace test.Models 
{
    /// 对应数据库中的 VIP card 表
    public class VIPCard
    {
        public string CustomerID { get; set; } // 顾客ID，外键，同时是主键
        public int Points { get; set; } // 会员积分

        public override string ToString()
        {
            return $"会员卡积分: {Points}";
        }
    }
}
