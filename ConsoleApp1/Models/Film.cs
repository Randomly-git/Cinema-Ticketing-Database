using System;

namespace ConsoleApp1.新文件夹
{
    // Film 类用于映射电影表
    public class Film
    {
        public string FilmName { get; set; }
        public string Genre { get; set; }
        public int FilmLength { get; set; }
        public decimal NormalPrice { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Admissions { get; set; }
        public int BoxOffice { get; set; }
        public int Score { get; set; }

        // 默认构造函数
        public Film()
        {
        }

        // 带参数的构造函数
        public Film(string filmName, string genre, int filmLength, decimal normalPrice, DateTime releaseDate, DateTime? endDate, int admissions, int boxOffice, int score)
        {
            FilmName = filmName;
            Genre = genre;
            FilmLength = filmLength;
            NormalPrice = normalPrice;
            ReleaseDate = releaseDate;
            EndDate = endDate;
            Admissions = admissions;
            BoxOffice = boxOffice;
            Score = score;
        }

        // 打印电影信息的方法
        public void DisplayFilmInfo()
        {
            Console.WriteLine($"电影名称: {FilmName}");
            Console.WriteLine($"电影类型: {Genre}");
            Console.WriteLine($"电影时长: {FilmLength} 分钟");
            Console.WriteLine($"标准票价: {NormalPrice:C2}");
            Console.WriteLine($"上映日期: {ReleaseDate.ToShortDateString()}");
            Console.WriteLine($"撤档日期: {(EndDate.HasValue ? EndDate.Value.ToShortDateString() : "暂无撤档")}");
            Console.WriteLine($"观影人次: {Admissions}");
            Console.WriteLine($"票房: {BoxOffice} 元");
            Console.WriteLine($"评分: {Score}/10");
        }
    }
}
