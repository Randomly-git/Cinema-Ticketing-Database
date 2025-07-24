using System;

namespace test.Models // 请替换为你的项目命名空间
{
    /// 对应数据库中的 cast 表，存储演职人员信息。
    public class Cast
    {
        public string MemberName { get; set; } // 人员姓名
        public string Role { get; set; }       // 角色（主角、配角、导演等）
        public string FilmName { get; set; }   // 电影名称

        public override string ToString()
        {
            return $"姓名: {MemberName}, 角色: {Role}";
        }
    }
}
