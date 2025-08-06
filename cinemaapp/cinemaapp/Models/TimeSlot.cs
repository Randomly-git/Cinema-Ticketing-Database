using System;

namespace test.Models // 请替换为你的项目命名空间
{
    /// 对应数据库中的 timeslot 表，存储影片播放时间段。
    public class TimeSlot
    {
        public string TimeID { get; set; }   // 时段号，PK
        public DateTime StartTime { get; set; } // 电影开场时间
        public DateTime EndTime { get; set; }   // 电影散场时间

        public override string ToString()
        {
            return $"时段: {StartTime:hh\\:mm}-{EndTime:hh\\:mm}";
        }
    }
}
