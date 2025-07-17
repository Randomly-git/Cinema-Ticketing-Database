using System;

namespace ConsoleApp1.Models
{
    // VIPCard 类用于映射 VIP card 表
    public class VIPCard
    {
        public string CustomerID { get; set; }
        public int Points { get; set; }

        // 默认构造函数
        public VIPCard() { }

        // 带参数的构造函数
        public VIPCard(string customerID, int points)
        {
            CustomerID = customerID;
            Points = points;
        }

        // 打印VIP卡信息的方法
        public void DisplayVIPCardInfo()
        {
            Console.WriteLine($"顾客ID: {CustomerID}");
            Console.WriteLine($"会员积分: {Points}");
        }
    }
}
