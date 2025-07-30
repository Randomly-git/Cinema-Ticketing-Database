using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration; 
using System.Linq;

class Program
{
    private static SchedulingService _schedulingService;
    private static string _connectionString;

    static void Main(string[] args)
    {
        // 从App.config加载数据库连接字符串
        _connectionString = ConfigurationManager.ConnectionStrings["OracleDbConnection"]?.ConnectionString;
        if (string.IsNullOrEmpty(_connectionString))
        {
            Console.WriteLine("错误: 数据库连接字符串未配置。请检查App.config文件。");
            Console.ReadKey();
            return;
        }

        _schedulingService = new SchedulingService(_connectionString);

        RunApplication();
    }

    /// <summary>
    /// 运行主应用程序循环，显示菜单并处理用户选择。
    /// </summary>
    static void RunApplication()
    {
        bool running = true;
        while (running)
        {
            Console.Clear(); // 清除控制台内容，使界面更整洁
            Console.WriteLine("--- 电影院排片管理系统 (控制台版 - 临时无日期模式) ---");
            Console.WriteLine("1. 添加新排片");
            Console.WriteLine("2. 查看所有排片"); // 修改提示，因为不再按日期过滤
            Console.WriteLine("3. 删除排片");
            Console.WriteLine("4. 退出");
            Console.Write("请选择一个操作 (1-4): ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddSectionInteractive();
                    break;
                case "2":
                    ViewSectionsInteractive();
                    break;
                case "3":
                    DeleteSectionInteractive();
                    break;
                case "4":
                    running = false;
                    Console.WriteLine("感谢使用，再见！");
                    break;
                default:
                    Console.WriteLine("无效的选择，请重新输入。");
                    break;
            }
            if (running)
            {
                Console.WriteLine("\n按任意键继续...");
                Console.ReadKey();
            }
        }
    }

    static void AddSectionInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 添加新排片 ---");

        // 获取电影列表并显示
        var films = _schedulingService.GetAllFilms();
        if (!films.Any())
        {
            Console.WriteLine("数据库中没有可用的电影。请先添加电影信息。");
            return;
        }
        Console.WriteLine("\n可用电影:");
        for (int i = 0; i < films.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {films[i].FilmName}");
        }
        Console.Write("请输入电影编号: ");
        if (!int.TryParse(Console.ReadLine(), out int filmIndex) || filmIndex < 1 || filmIndex > films.Count)
        {
            Console.WriteLine("无效的电影编号。");
            return;
        }
        string filmName = films[filmIndex - 1].FilmName;

        // 获取影厅列表并显示
        var halls = _schedulingService.GetAllMovieHalls();
        if (!halls.Any())
        {
            Console.WriteLine("数据库中没有可用的影厅。请先添加影厅信息。");
            return;
        }
        Console.WriteLine("\n可用影厅:");
        for (int i = 0; i < halls.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {halls[i].HallNo} ({halls[i].Category})");
        }
        Console.Write("请输入影厅编号: ");
        if (!int.TryParse(Console.ReadLine(), out int hallIndex) || hallIndex < 1 || hallIndex > halls.Count)
        {
            Console.WriteLine("无效的影厅编号。");
            return;
        }
        int hallNo = halls[hallIndex - 1].HallNo;

        // 获取时段列表并显示
        var timeSlots = _schedulingService.GetAllTimeSlots();
        if (!timeSlots.Any())
        {
            Console.WriteLine("数据库中没有可用的时段。请先添加时段信息。");
            return;
        }
        Console.WriteLine("\n可用时段:");
        for (int i = 0; i < timeSlots.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {timeSlots[i].TimeID} ({timeSlots[i].StartTime:hh\\:mm}-{timeSlots[i].EndTime:hh\\:mm})");
        }
        Console.Write("请输入时段编号: ");
        if (!int.TryParse(Console.ReadLine(), out int timeSlotIndex) || timeSlotIndex < 1 || timeSlotIndex > timeSlots.Count)
        {
            Console.WriteLine("无效的时段编号。");
            return;
        }
        string timeID = timeSlots[timeSlotIndex - 1].TimeID;

        var result = _schedulingService.AddSection(filmName, hallNo, timeID);
        Console.WriteLine(result.Message);
    }


    static void ViewSectionsInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 查看所有排片 (临时模式，不按日期过滤) ---");
       
        List<Section> sections = _schedulingService.GetSectionsByDateRange(DateTime.MinValue, DateTime.MaxValue); // 传递占位符日期

        if (sections.Count == 0)
        {
            Console.WriteLine("数据库中没有排片信息。");
        }
        else
        {
            Console.WriteLine("\n以下是所有排片信息：");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-10} {5,-10}", "场次ID", "电影名称", "影厅号", "时段ID", "开始", "结束");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            foreach (var section in sections)
            {
                Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-10:hh\\:mm} {5,-10:hh\\:mm}",
                                  section.SectionID,
                                  (section.FilmName.Length > 18 ? section.FilmName.Substring(0, 15) + "..." : section.FilmName), // 截断长电影名
                                  section.HallNo,
                                  section.TimeID,
                                  section.StartTime,
                                  section.EndTime);
            }
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
        }
    }

    static void DeleteSectionInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 删除排片 ---");
        Console.Write("请输入要删除的场次ID: ");
        if (!int.TryParse(Console.ReadLine(), out int sectionIdToDelete))
        {
            Console.WriteLine("无效的场次ID。");
            return;
        }

        Console.Write($"确定要删除场次ID为 {sectionIdToDelete} 的排片吗？ (Y/N): ");
        string confirmation = Console.ReadLine()?.ToUpper();

        if (confirmation == "Y")
        {
            var result = _schedulingService.DeleteSection(sectionIdToDelete);
            Console.WriteLine(result.Message);
        }
        else
        {
            Console.WriteLine("删除操作已取消。");
        }
    }
}
