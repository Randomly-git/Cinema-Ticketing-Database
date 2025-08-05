using System;
using test.Models;

namespace test.Models 
{
    /// 对应数据库中的 section 表，存储场次信息。
    public class Section
    {
        public int SectionID { get; set; }   // 场次号，PK
        public string FilmName { get; set; } // 电影名，FK
        public int HallNo { get; set; }      // 影厅号，FK
        public string TimeID { get; set; }   // 时段号，FK

        // 导航属性：方便获取关联的电影、影厅和时段信息
        public Film Film { get; set; }
        public MovieHall MovieHall { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public override string ToString()
        {
            return $"场次号: {SectionID}, 电影: {FilmName}, 影厅: {HallNo}, 时段: {TimeSlot?.ToString() ?? TimeID}";
        }
    }
}
