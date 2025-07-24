using System;

namespace test.Models // 请替换为你的项目命名空间
{
    /// 对应数据库中的 moviehall 表，存储影厅信息。
    public class MovieHall
    {
        public int HallNo { get; set; }      // 影厅号，PK
        public int Lines { get; set; }       // 总行数
        public int ColumnsCount { get; set; } // 总列数 C# 中避免关键字冲突用 ColumnsCount
        public string Category { get; set; } // 影厅种类（如 IMAX、普通厅）

        public override string ToString()
        {
            return $"影厅号: {HallNo}, 种类: {Category}, 容量: {Lines * ColumnsCount}座";
        }
    }
}
