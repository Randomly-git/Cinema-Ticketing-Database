using System;

namespace ConsoleApp1.Models
{
    // Cast 类用于映射 cast 表
    public class Cast
    {
        public string MemberName { get; set; }
        public string Role { get; set; }
        public string FilmName { get; set; }

        // 默认构造函数
        public Cast() { }

        // 带参数的构造函数
        public Cast(string memberName, string role, string filmName)
        {
            MemberName = memberName;
            Role = role;
            FilmName = filmName;
        }

        // 打印演员信息的方法
        public void DisplayCastInfo()
        {
            Console.WriteLine($"人员姓名: {MemberName}");
            Console.WriteLine($"角色: {Role}");
            Console.WriteLine($"电影名称: {FilmName}");
        }
    }
}
