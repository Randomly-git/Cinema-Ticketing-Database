using System;

namespace ConsoleApp1.Models
{
    // OrderForProducts 类用于映射 order for products 表
    public class OrderForProducts
    {
        // 订单号 (PK)
        public int OrderID { get; set; }
        public string ProductName { get; set; }
        public string State { get; set; }
        public string CustomerID { get; set; }
        public int Purchasenum { get; set; }
        public DateTime Day { get; set; }
        public string PMethod { get; set; }
        public int Price { get; set; }

        // 默认构造函数
        public OrderForProducts() { }

        // 带参数的构造函数
        public OrderForProducts(int orderID, string productName, string state, string customerID, int purchasenum, DateTime day, string pMethod, int price)
        {
            OrderID = orderID;
            ProductName = productName;
            State = state;
            CustomerID = customerID;
            Purchasenum = purchasenum;
            Day = day;
            PMethod = pMethod;
            Price = price;
        }

        // 打印订单信息的方法
        public void DisplayOrderInfo()
        {
            Console.WriteLine($"订单号: {OrderID}");
            Console.WriteLine($"周边商品名: {ProductName}");
            Console.WriteLine($"订单状态: {State}");
            Console.WriteLine($"顾客ID: {CustomerID}");
            Console.WriteLine($"购买数量: {Purchasenum}");
            Console.WriteLine($"日期: {Day.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"支付方式: {PMethod}");
            Console.WriteLine($"支付金额: {Price}");
        }
    }
}
