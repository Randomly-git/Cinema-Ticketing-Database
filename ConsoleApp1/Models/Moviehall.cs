using System;

namespace ConsoleApp1.Models
{
    // Moviehall 类用于映射 Moviehall 表
    public class Moviehall
    {
        public int HallNo { get; set; }
        public int Lines { get; set; }
        public int Columns { get; set; }
        public string Category { get; set; }

        // 默认构造函数
        public Moviehall() { }

        // 带参数的构造函数
        public Moviehall(int hallNo, int lines, int columns, string category)
        {
            HallNo = hallNo;
            Lines = lines;
            Columns = columns;
            Category = category;
        }

        // 打印影厅信息的方法
        public void DisplayMoviehallInfo()
        {
            Console.WriteLine($"影厅号: {HallNo}");
            Console.WriteLine($"总行数: {Lines}");
            Console.WriteLine($"总列数: {Columns}");
            Console.WriteLine($"影厅种类: {Category}");
        }
    }
}
