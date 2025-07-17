using System;

namespace ConsoleApp1.Models
{
    // Ticket 类用于映射 Ticket 表
    public class Ticket
    {
        // 票号 (PK)
        public string TicketID { get; set; }
        public int Price { get; set; }
        public int Rating { get; set; }
        public int SectionID { get; set; }
        public int LineNo { get; set; }
        public int ColumnNo { get; set; }
        public string State { get; set; }

        // 默认构造函数
        public Ticket() { }

        // 带参数的构造函数
        public Ticket(string ticketID, int price, int rating, int sectionID, int lineNo, int columnNo, string state)
        {
            TicketID = ticketID;
            Price = price;
            Rating = rating;
            SectionID = sectionID;
            LineNo = lineNo;
            ColumnNo = columnNo;
            State = state;
        }

        // 打印票务信息的方法
        public void DisplayTicketInfo()
        {
            Console.WriteLine($"票号: {TicketID}");
            Console.WriteLine($"票价: {Price} 元");
            Console.WriteLine($"评分: {Rating}/10");
            Console.WriteLine($"场次号: {SectionID}");
            Console.WriteLine($"座位行号: {LineNo}");
            Console.WriteLine($"座位列号: {ColumnNo}");
            Console.WriteLine($"状态: {State}");
        }
    }
}
