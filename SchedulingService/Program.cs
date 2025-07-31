using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration; // 用于读取App.config
using System.Linq;

// ====================================================================
// 1. 数据模型 (Film.cs, MovieHall.cs, TimeSlot.cs, Section.cs)
//    请确保这些文件已更新为最新版本。
// ====================================================================

// ====================================================================
// 2. 排片服务类 (SchedulingService.cs)
//    请确保该文件是您最新版本。
// ====================================================================

// ====================================================================
// 3. 应用程序入口 (Program.cs)
//    这是您的控制台应用程序的入口点，处理用户输入和输出。
// ====================================================================

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
            Console.WriteLine("--- 电影院排片管理系统 (控制台版) ---");
            Console.WriteLine("1. 添加新排片");
            Console.WriteLine("2. 查看排片");
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

    /// <summary>
    /// 交互式添加排片功能。
    /// 现在根据影片时长计算结束时间。
    /// </summary>
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
            Console.WriteLine($"{i + 1}. {films[i].FilmName} (时长: {films[i].FilmLength} 分钟)");
        }
        Console.Write("请输入电影编号: ");
        if (!int.TryParse(Console.ReadLine(), out int filmIndex) || filmIndex < 1 || filmIndex > films.Count)
        {
            Console.WriteLine("无效的电影编号。");
            return;
        }
        Film selectedFilm = films[filmIndex - 1]; // 获取选中的Film对象
        string filmName = selectedFilm.FilmName;

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

        // 输入开始日期和时间
        Console.Write("请输入排片开始日期和时间 (YYYY-MM-DD HH24:MI): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime scheduleStartTime))
        {
            Console.WriteLine("无效的开始日期时间格式。");
            return;
        }

        // 计算结束时间
        DateTime scheduleEndTime = scheduleStartTime.AddMinutes(selectedFilm.FilmLength);
        Console.WriteLine($"计算出的结束时间为: {scheduleEndTime:yyyy-MM-dd HH:mm}");


        // 调用服务添加排片
        var result = _schedulingService.AddSection(filmName, hallNo, scheduleStartTime, scheduleEndTime);
        Console.WriteLine(result.Message);
    }

    /// <summary>
    /// 交互式查看排片功能。
    /// </summary>
    static void ViewSectionsInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 查看排片 ---");
        Console.Write("请输入查询开始日期 (YYYY-MM-DD，留空默认为今天): ");
        string startDateStr = Console.ReadLine();
        DateTime startDate = string.IsNullOrEmpty(startDateStr) ? DateTime.Today : (DateTime.TryParse(startDateStr, out DateTime sDate) ? sDate : DateTime.Today);

        Console.Write("请输入查询结束日期 (YYYY-MM-DD，留空默认为未来30天): ");
        string endDateStr = Console.ReadLine();
        DateTime endDate = string.IsNullOrEmpty(endDateStr) ? DateTime.Today.AddDays(30) : (DateTime.TryParse(endDateStr, out DateTime eDate) ? eDate : DateTime.Today.AddDays(30));

        List<Section> sections = _schedulingService.GetSectionsByDateRange(startDate, endDate);

        if (sections.Count == 0)
        {
            Console.WriteLine($"在 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 之间没有排片信息。");
        }
        else
        {
            Console.WriteLine($"\n以下是 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 的排片信息：");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-20} {5,-20} {6,-10}", "场次ID", "电影名称", "影厅号", "时段ID", "开始时间", "结束时间", "影厅类型");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            foreach (var section in sections)
            {
                Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-20:yyyy-MM-dd HH:mm} {5,-20:yyyy-MM-dd HH:mm} {6,-10}",
                                  section.SectionID,
                                  (section.FilmName.Length > 18 ? section.FilmName.Substring(0, 15) + "..." : section.FilmName), // 截断长电影名
                                  section.HallNo,
                                  section.TimeID,
                                  section.ScheduleStartTime,
                                  section.ScheduleEndTime,
                                  section.HallCategory);
            }
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
        }
    }

    /// <summary>
    /// 交互式删除排片功能。
    /// </summary>
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
