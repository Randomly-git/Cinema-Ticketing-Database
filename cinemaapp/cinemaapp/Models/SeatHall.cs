using System;
using test.Models;

namespace test.Models 
{
    /// <summary>
    /// 对应数据库中的 seat-hall 表，存储影厅座位布局信息。
    /// </summary>
    public class SeatHall
    {
        public int HallNo { get; set; }    // 影厅号，PK, FK
        public string LINENO { get; set; }  // 行号，PK
        public int ColumnNo { get; set; }  // 列号，PK
        public string CATEGORY { get; set; }   // 座区（例如：A区，B区）

        // 导航属性：方便获取关联的影厅信息
        public MovieHall MovieHall { get; set; }

        public override string ToString()
        {
            return $"影厅 {HallNo}, 座位: {LINENO}{ColumnNo}, 区域: {CATEGORY}";
        }
    }
}
