using System;

namespace ConsoleApp1.Models
{
    // CustomerPortrait 类用于映射 customer portrait 表
    public class CustomerPortrait
    {
        public string CustomerID { get; set; }
        public string Genre { get; set; }

        // 默认构造函数
        public CustomerPortrait() { }

        // 带参数的构造函数
        public CustomerPortrait(string customerID, string genre)
        {
            CustomerID = customerID;
            Genre = genre;
        }

        // 打印顾客画像信息的方法
        public void DisplayPortraitInfo()
        {
            Console.WriteLine($"顾客ID: {CustomerID}");
            Console.WriteLine($"电影类型: {Genre}");
        }
    }
}
