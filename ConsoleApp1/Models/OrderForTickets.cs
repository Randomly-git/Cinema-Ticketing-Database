using System;

namespace ConsoleApp1.Models
{
    // OrderForTickets 类用于映射 order for tickets 表
    public class OrderForTickets
    {
        public int OrderID { get; set; }
        public string TicketID { get; set; }
        public string State { get; set; }
        public string CustomerID { get; set; }
        public DateTime Day { get; set; }
        public string PaymentMethod { get; set; }
        public int Price { get; set; }

        // 默认构造函数
        public OrderForTickets() { }

        // 带参数的构造函数
        public OrderForTickets(int orderID, string ticketID, string state, string customerID, DateTime day, string paymentMethod, int price)
        {
            OrderID = orderID;
            TicketID = ticketID;
            State = state;
            CustomerID = customerID;
            Day = day;
            PaymentMethod = paymentMethod;
            Price = price;
        }

        // 打印订单信息的方法
        public void DisplayOrderInfo()
        {
            Console.WriteLine($"订单号: {OrderID}");
            Console.WriteLine($"票号: {TicketID}");
            Console.WriteLine($"订单状态: {State}");
            Console.WriteLine($"顾客ID: {CustomerID}");
            Console.WriteLine($"日期: {Day.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"支付方式: {PaymentMethod}");
            Console.WriteLine($"支付金额: {Price}");
        }
    }
}
