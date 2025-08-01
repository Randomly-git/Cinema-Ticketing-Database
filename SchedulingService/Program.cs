using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

class Program
{
    private static SchedulingService _schedulingService;
    private static QueryService _queryService;
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
        _queryService = new QueryService(_connectionString);

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
            Console.Clear();
            Console.WriteLine("--- 电影院购票管理系统 (控制台版) ---");
            Console.WriteLine("1. 排片管理");
            Console.WriteLine("2. 影片信息查询与统计");
            Console.WriteLine("3. 退出");
            Console.Write("请选择一个操作 (1-3): ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SchedulingManagementMenu();
                    break;
                case "2":
                    QueryFunctionsMenu();
                    break;
                case "3":
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
    /// 排片管理子菜单。
    /// </summary>
    static void SchedulingManagementMenu()
    {
        bool inMenu = true;
        while (inMenu)
        {
            Console.Clear();
            Console.WriteLine("--- 排片管理菜单 ---");
            Console.WriteLine("1. 添加新排片");
            Console.WriteLine("2. 查看排片");
            Console.WriteLine("3. 删除排片");
            Console.WriteLine("B. 返回主菜单");
            Console.Write("请选择一个操作: ");
            string choice = Console.ReadLine();
            switch (choice.ToUpper())
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
                case "B":
                    inMenu = false;
                    break;
                default:
                    Console.WriteLine("无效的选择，请重新输入。");
                    break;
            }
            if (inMenu && choice.ToUpper() != "B")
            {
                Console.WriteLine("\n按任意键继续...");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// 交互式添加排片功能。
    /// </summary>
    static void AddSectionInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 添加新排片 ---");

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
        Film selectedFilm = films[filmIndex - 1];

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

        Console.Write("请输入排片开始时间 (YYYY-MM-DD HH:mm): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime scheduleStartTime))
        {
            Console.WriteLine("无效的日期时间格式。");
            return;
        }

        var result = _schedulingService.AddSection(selectedFilm.FilmName, hallNo, scheduleStartTime);
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

        Console.Write("请输入查询结束日期 (YYYY-MM-DD，留空默认为未来7天): ");
        string endDateStr = Console.ReadLine();
        DateTime endDate = string.IsNullOrEmpty(endDateStr) ? DateTime.Today.AddDays(7) : (DateTime.TryParse(endDateStr, out DateTime eDate) ? eDate : DateTime.Today.AddDays(7));

        List<Section> sections = _schedulingService.GetSectionsByDateRange(startDate, endDate);

        if (sections.Count == 0)
        {
            Console.WriteLine($"在 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 之间没有排片信息。");
        }
        else
        {
            Console.WriteLine($"\n以下是 {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd} 的排片信息：");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-20} {5,-20}", "场次ID", "电影名称", "影厅号", "时段ID", "开始时间", "结束时间");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            foreach (var section in sections)
            {
                Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10} {4,-20:yyyy-MM-dd HH:mm} {5,-20:yyyy-MM-dd HH:mm}",
                                  section.SectionID,
                                  (section.FilmName.Length > 18 ? section.FilmName.Substring(0, 15) + "..." : section.FilmName),
                                  section.HallNo,
                                  section.TimeID,
                                  section.ScheduleStartTime,
                                  section.ScheduleEndTime);
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

    /// <summary>
    /// 处理查询功能的子菜单。
    /// </summary>
    static void QueryFunctionsMenu()
    {
        bool inMenu = true;
        while (inMenu)
        {
            Console.Clear();
            Console.WriteLine("--- 影片信息查询与统计菜单 ---");
            Console.WriteLine("1. 影片排档、撤档信息");
            Console.WriteLine("2. 影片概况查询");
            Console.WriteLine("3. 演职人员查询");
            Console.WriteLine("4. 电影数据统计");
            Console.WriteLine("B. 返回主菜单");
            Console.Write("请选择查询功能: ");

            string queryOption = Console.ReadLine();
            switch (queryOption.ToUpper())
            {
                case "1":
                    GetMovieSchedulingInfoInteractive();
                    break;
                case "2":
                    GetMovieOverviewInteractive();
                    break;
                case "3":
                    GetCastCrewDetailsInteractive();
                    break;
                case "4":
                    GetMovieStatisticsInteractive();
                    break;
                case "B":
                    inMenu = false;
                    break;
                default:
                    Console.WriteLine("无效的选项，请重试。");
                    break;
            }
            if (inMenu && queryOption.ToUpper() != "B")
            {
                Console.WriteLine("\n按任意键继续...");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// 交互式获取影片排档信息。
    /// </summary>
    static void GetMovieSchedulingInfoInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 影片排档、撤档信息 ---");
        Console.Write("请输入电影名称: ");
        string filmName = Console.ReadLine();
        var (film, sessions) = _queryService.GetMovieSchedulingInfo(filmName);

        if (film != null)
        {
            Console.WriteLine($"\n影片排档信息:");
            Console.WriteLine($"电影名称: {film.FilmName}");
            Console.WriteLine($"上映日期: {film.ReleaseDate?.ToShortDateString() ?? "N/A"}");
            Console.WriteLine($"撤档日期: {film.EndDate?.ToShortDateString() ?? "N/A"}");
            Console.WriteLine("--- 场次信息 ---");
            if (sessions.Any())
            {
                foreach (var session in sessions)
                {
                    Console.WriteLine($"- 场次ID: {session.SectionID}, 影厅号: {session.HallNo}, 开始时间: {session.ScheduleStartTime:yyyy-MM-dd HH:mm}, 结束时间: {session.ScheduleEndTime:yyyy-MM-dd HH:mm}");
                }
            }
            else
            {
                Console.WriteLine("该影片暂无排片信息。");
            }
        }
        else
        {
            Console.WriteLine($"未找到电影 '{filmName}' 的排档信息。");
        }
    }

    /// <summary>
    /// 交互式获取影片概况。
    /// </summary>
    static void GetMovieOverviewInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 影片概况查询 ---");
        Console.Write("请输入电影名称: ");
        string filmName = Console.ReadLine();
        var overview = _queryService.GetMovieOverview(filmName);

        if (overview != null)
        {
            Console.WriteLine($"\n影片概况信息:");
            Console.WriteLine($"电影名称: {overview.FilmName}");
            Console.WriteLine($"类型: {overview.Genre}");
            Console.WriteLine($"时长: {overview.FilmLength} 分钟");
            Console.WriteLine($"标准票价: {overview.NormalPrice:C}");
            Console.WriteLine($"本影院总票房: {overview.BoxOffice:C}");
            Console.WriteLine($"观影人次: {overview.Admissions}");
            Console.WriteLine($"评分: {overview.Score:N1}");
        }
        else
        {
            Console.WriteLine($"未找到电影 '{filmName}' 的概况信息。");
        }
    }

    /// <summary>
    /// 交互式查询演职人员。
    /// </summary>
    static void GetCastCrewDetailsInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 演职人员查询 ---");
        Console.Write("请输入演职人员姓名: ");
        string memberName = Console.ReadLine();
        var castDetails = _queryService.GetCastCrewDetails(memberName);

        if (castDetails.Any())
        {
            Console.WriteLine($"\n演职人员 '{memberName}' 的参演电影:");
            foreach (var detail in castDetails)
            {
                Console.WriteLine($"- 电影名称: {detail.FilmName}, 担任角色: {detail.Role}");
            }
        }
        else
        {
            Console.WriteLine($"未找到演职人员 '{memberName}' 的信息。");
        }
    }

    /// <summary>
    /// 交互式进行电影数据统计。
    /// </summary>
    static void GetMovieStatisticsInteractive()
    {
        Console.Clear();
        Console.WriteLine("--- 电影数据统计 ---");
        Console.Write("请输入电影名称: ");
        string filmName = Console.ReadLine();
        var stats = _queryService.GetMovieStatistics(filmName);
        var film = _queryService.GetMovieOverview(filmName);

        if (film != null)
        {
            Console.WriteLine($"\n电影数据统计:");
            Console.WriteLine($"电影名称: {film.FilmName}");
            Console.WriteLine($"标准票价: {film.NormalPrice:C}");
            Console.WriteLine($"本影院总票房: {stats.BoxOffice:C}");
            Console.WriteLine($"已售票数: {stats.TicketsSold}");
            Console.WriteLine($"上座率: {stats.OccupancyRate:P2}");
        }
        else
        {
            Console.WriteLine($"未找到电影 '{filmName}' 的数据统计信息。");
        }
    }
}
