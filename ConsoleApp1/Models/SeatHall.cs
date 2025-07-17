using System;

namespace ConsoleApp1.Models
{
    // SeatHall 类用于映射 seat-hall 表
    public class SeatHall
    {
        public int HallNo { get; set; }
        public int LineNo { get; set; }
        public int ColumnNo { get; set; }
        public string Category { get; set; }

        // 默认构造函数
        public SeatHall() { }

        // 带参数的构造函数
        public SeatHall(int hallNo, int lineNo, int columnNo, string category)
        {
            HallNo = hallNo;
            LineNo = lineNo;
            ColumnNo = columnNo;
            Category = category;
        }

        // 打印座位信息的方法
        public void DisplaySeatInfo()
        {
            Console.WriteLine($"影厅号: {HallNo}");
            Console.WriteLine($"座位行号: {LineNo}");
            Console.WriteLine($"座位列号: {ColumnNo}");
            Console.WriteLine($"座区: {Category}");
        }
    }
}
