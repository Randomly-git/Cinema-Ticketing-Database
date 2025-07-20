using System;

namespace ConsoleApp1.Models
{
    // Section 类用于映射 section 表
    public class Section
    {
        public int SectionID { get; set; }
        public string FilmName { get; set; }
        public int HallNo { get; set; }
        public string TimeID { get; set; }

        // 默认构造函数
        public Section() { }

        // 带参数的构造函数
        public Section(int sectionID, string filmName, int hallNo, string timeID)
        {
            SectionID = sectionID;
            FilmName = filmName;
            HallNo = hallNo;
            TimeID = timeID;
        }

        // 打印场次信息的方法
        public void DisplaySectionInfo()
        {
            Console.WriteLine($"场次号: {SectionID}");
            Console.WriteLine($"电影名: {FilmName}");
            Console.WriteLine($"影厅号: {HallNo}");
            Console.WriteLine($"时段号: {TimeID}");
        }
    }
}
