using System;
using System.Collections.Generic;

namespace ConsoleApp1.Models
{
    public class Moviehall
    {
        public int HallNo { get; set; }
        public int Lines { get; set; }
        public int Columns { get; set; }
        public string Category { get; set; }

        // 每个影厅管理自己的座位
        public List<SeatHall> Seats { get; set; } = new List<SeatHall>();

        public Moviehall() { }

        public Moviehall(int hallNo, int lines, int columns, string category)
        {
            HallNo = hallNo;
            Lines = lines;
            Columns = columns;
            Category = category;

            // 自动生成所有座位并分类
            for (int i = 1; i <= lines; i++)
            {
                for (int j = 1; j <= columns; j++)
                {
                    string seatCategory;

                    if (i <= lines / 3) // 前 1/3 行
                    {
                        seatCategory = "前排";
                    }
                    else if (i <= (2 * lines) / 3) // 中间 1/3 行
                    {
                        seatCategory = "最佳观影区";
                    }
                    else // 最后 1/3 行
                    {
                        seatCategory = "后排";
                    }

                    Seats.Add(new SeatHall(hallNo, i, j, seatCategory));
                }
            }

        }

        public void DisplayMoviehallInfo()
        {
            Console.WriteLine($"影厅号: {HallNo} (座位数: {Seats.Count})");
            Console.WriteLine($"总行数: {Lines}, 总列数: {Columns}");
            Console.WriteLine($"影厅种类: {Category}");
        }
    }
}
