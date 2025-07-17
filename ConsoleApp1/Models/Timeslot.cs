using System;

namespace ConsoleApp1.Models
{
    // Timeslot 类用于映射 timeslot 表
    public class Timeslot
    {
        public string TimeID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Timeslot() { }

        // 带参数的构造函数
        public Timeslot(string timeID, DateTime startTime, DateTime endTime)
        {
            TimeID = timeID;
            StartTime = startTime;
            EndTime = endTime;
        }

        // 打印时段信息的方法
        public void DisplayTimeslotInfo()
        {
            Console.WriteLine($"时段号: {TimeID}");
            Console.WriteLine($"开场时间: {StartTime}");
            Console.WriteLine($"散场时间: {EndTime}");
        }
    }
}
