using test.Models;
using test.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test.Services
{
    /// <summary>
    /// 票务调度业务服务实现。
    /// </summary>
    public class ShowingService : IShowingService
    {
        private readonly IShowingRepository _showingRepository;
        private readonly IFilmRepository _filmRepository; // 需要 FilmRepository 来获取 MovieHall 和 TimeSlot 详情

        public ShowingService(IShowingRepository showingRepository, IFilmRepository filmRepository)
        {
            _showingRepository = showingRepository;
            _filmRepository = filmRepository;
        }

        /// <summary>
        /// 获取电影的场次列表，并填充关联的影厅和时段信息。
        /// </summary>
        public List<Section> GetFilmShowings(string filmName, DateTime? date = null)
        {
            var sections = _showingRepository.GetAvailableSections(filmName, date);
            return sections;
        }

        /// <summary>
        /// 获取指定场次的可用座位。
        /// 返回一个字典，键为行号，值为该行可用的列号列表。
        /// </summary>
        public Dictionary<string, List<string>> GetAvailableSeats(Section section)
        {
            var availableSeats = new Dictionary<string, List<string>>();

            // 1. 使用传入的场次信息，不再重新查询
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section), "传入的场次对象不能为空。");
            }

            int hallNo = section.HallNo;
            int sectionId = section.SectionID; // 获取场次ID用于后续查询已售票

            // 2. 获取影厅的完整座位布局
            var hallLayout = _showingRepository.GetHallSeatLayout(hallNo);
            if (!hallLayout.Any())
            {
                throw new InvalidOperationException($"影厅 {hallNo} 没有座位布局信息。请确保 SEATHALL 表中有此影厅的座位数据。");
            }

            // 3. 获取该场次已售出的座位
            var soldTickets = _showingRepository.GetSoldSeatsForSection(sectionId);
            var soldSeatStrings = new HashSet<string>(soldTickets.Select(t => $"{t.LineNo}{t.ColumnNo}"));

            // 4. 遍历所有座位，筛选出可用的座位
            foreach (var seat in hallLayout)
            {
                string seatString = $"{seat.LINENO}{seat.ColumnNo}";
                if (!soldSeatStrings.Contains(seatString))
                {
                    if (!availableSeats.ContainsKey(seat.LINENO))
                    {
                        availableSeats[seat.LINENO] = new List<string>();
                    }
                    availableSeats[seat.LINENO].Add(seat.ColumnNo.ToString());
                }
            }

            // 对每行的列号进行排序
            foreach (var row in availableSeats.Keys.ToList())
            {
                availableSeats[row] = availableSeats[row].OrderBy(col => int.Parse(col)).ToList();
            }

            return availableSeats;
        }

        public Dictionary<string, Dictionary<string, SeatStatus>> GetHallSeatStatus(Section section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section), "传入的场次对象不能为空。");
            }

            int hallNo = section.HallNo;
            int sectionId = section.SectionID;

            // 1. 获取影厅完整座位布局
            var hallLayout = _showingRepository.GetHallSeatLayout(hallNo);
            if (!hallLayout.Any())
            {
                throw new InvalidOperationException($"影厅 {hallNo} 没有座位布局信息。");
            }

            // 2. 获取已售座位
            var soldTickets = _showingRepository.GetSoldSeatsForSection(sectionId);
            var soldSeatSet = new HashSet<string>(soldTickets.Select(t => $"{t.LineNo}{t.ColumnNo}"));

            // 3. 构建完整座位状态表
            var seatStatusTable = new Dictionary<string, Dictionary<string, SeatStatus>>();

            foreach (var seat in hallLayout)
            {
                string lineNo = seat.LINENO;
                string columnNo = seat.ColumnNo.ToString();
                string seatKey = $"{lineNo}{columnNo}";

                // 初始化行字典
                if (!seatStatusTable.ContainsKey(lineNo))
                {
                    seatStatusTable[lineNo] = new Dictionary<string, SeatStatus>();
                }

                // 设置座位状态
                seatStatusTable[lineNo][columnNo] = soldSeatSet.Contains(seatKey)
                    ? SeatStatus.Sold
                    : SeatStatus.Available;
            }

            // 4. 按行号和列号排序
            var sortedTable = seatStatusTable
                .OrderBy(row => row.Key)
                .ToDictionary(
                    row => row.Key,
                    row => row.Value.OrderBy(col => int.Parse(col.Key))
                        .ToDictionary(col => col.Key, col => col.Value));

            return sortedTable;
        }

        /// </summary>
        /// 获取指定场次已售出的票
        /// </summary>
        public List<SeatHall> GetSoldSeatsForSection(int sectionId)
        {
            var tickets = _showingRepository.GetSoldSeatsForSection(sectionId);
            return tickets.Select(t => new SeatHall
            {
                LINENO = t.LineNo,
                ColumnNo = t.ColumnNo
            }).ToList();
        }


    }
}