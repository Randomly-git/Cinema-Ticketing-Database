using System;

namespace ConsoleApp1.Models
{
    // Discounts 类用于映射 Discounts 表
    public class Discounts
    {
        public string TimeID { get; set; }
        public decimal Discount { get; set; }

        // 默认构造函数
        public Discounts() { }

        // 带参数的构造函数
        public Discounts(string timeID, decimal discount)
        {
            TimeID = timeID;
            Discount = discount;
        }

        // 打印折扣信息的方法
        public void DisplayDiscountInfo()
        {
            Console.WriteLine($"时段号: {TimeID}");
            Console.WriteLine($"打折力度: {Discount:F1}%");
        }
    }
}
