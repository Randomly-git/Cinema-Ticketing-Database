using System;
using System.Collections.Generic;
using System.Linq;
using System.Data; // 添加对System.Data的引用
using Oracle.ManagedDataAccess.Client; // 添加对Oracle.ManagedDataAccess的引用
using test.Models;
using test.Repositories; // 添加对仓储接口的引用

namespace test.Services
{
    public class RatingService : IRatingService
    {
        private readonly ISchedulingService _schedulingService;
        private readonly IRatingRepository _ratingRepository;
        private readonly IFilmRepository _filmRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly string _connectionString;

        /// <summary>
        /// 构造函数，注入依赖
        /// </summary>
        public RatingService(
            ISchedulingService schedulingService,
            IRatingRepository ratingRepository,
            IFilmRepository filmRepository,
            IOrderRepository orderRepository,
            string connectionString)
        {
            _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
            _filmRepository = filmRepository ?? throw new ArgumentNullException(nameof(filmRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private OracleConnection GetOpenConnection()
        {
            var conn = new OracleConnection(_connectionString);
            conn.Open();
            return conn;
        }



        /// <summary>
        /// 用户根据订单查到电影名(主要用于被其他服务调用，也可以直接服务于用户)
        /// </summary>
        public string GetFilmNamebyOrderId(int orderId)
        {
            try
            {
                // 获取订单以验证其存在
                var order = _orderRepository.GetOrderForTicketsById(orderId);
                if (order == null)
                {
                    throw new KeyNotFoundException($"找不到ID为{orderId}的订单");
                }

                // 开始从订单出发，获取一系列相关信息
                var ticket = _orderRepository.GetTicketById(order.TicketID);      // 导航到Ticket表
                var section = _orderRepository.GetSectionById(ticket.SectionID);   // 进一步获取Section，以为跳板获取FilmName
                var filmName = section.FilmName; // 最终获得电影名称

                if (string.IsNullOrEmpty(filmName))
                {
                    throw new KeyNotFoundException("无法从订单确定电影名称");
                }

                return filmName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"订单{orderId}获取电影名失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 用户查看订单是否已经被评论过
        /// </summary>
        public bool HasRated(int orderId)
        {
            var order = _orderRepository.GetOrderForTicketsById(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"找不到ID为{orderId}的订单");
            }
            var ticket = _orderRepository.GetTicketById(order.TicketID);
            return _ratingRepository.GetRating(ticket.TicketID) != null;
        }
        //通过orderID获取影评
        public Rating GetRating(int orderId)
        {
            var order = _orderRepository.GetOrderForTicketsById(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"找不到ID为{orderId}的订单");
            }
            var ticket = _orderRepository.GetTicketById(order.TicketID);
            return _ratingRepository.GetRating(ticket.TicketID);
        }

        /// <summary>
        /// 用户对电影票进行评分
        /// </summary>
        public void RateOrder(int orderId, string filmName, int score, string comment = null)
        {
            // 验证评分范围
            if (score < 0 || score > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(score), "评分必须在0到10之间");
            }

            using (var connection = GetOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var order = _orderRepository.GetOrderForTicketsById(orderId);
                    if (order == null)
                    {
                        throw new KeyNotFoundException($"找不到ID为{orderId}的订单");
                    }

                    var film = _filmRepository.GetFilmByName(filmName); // 获取电影
                    var ticketId = order.TicketID;  // 获取TicketID 

                    // 创建或更新评分
                    var rating = new Rating
                    {
                        TicketID = ticketId,
                        FilmName = filmName,
                        Score = score,
                        Comment = comment,
                        RatingDate = DateTime.Now,
                    };

                    _ratingRepository.AddOrUpdateRating(rating);

                    // 重新计算并更新电影的平均分
                    _filmRepository.UpdateAverageScore(film, score);

                    transaction.Commit();
                    Console.WriteLine($"订单{orderId}评分成功");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"订单{orderId}评分失败: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 用户撤销对电影票的评分
        /// </summary>
        public void CancelRating(int orderId, string filmName)
        {
            using (var connection = GetOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var order = _orderRepository.GetOrderForTicketsById(orderId);
                    if (order == null)
                    {
                        throw new KeyNotFoundException($"找不到ID为{orderId}的订单");
                    }

                    var film = _filmRepository.GetFilmByName(filmName); // 获取电影
                    var ticketId = order.TicketID;  // 获取TicketID
                    var rating = _ratingRepository.GetRating(ticketId); // 获取影评
                    if (rating == null)
                    {
                        // 评分不存在，无需操作
                        transaction.Commit();
                        Console.WriteLine($"订单{orderId}无评分记录，无需撤销");
                        return;
                    }
                    var score = rating.Score;

                    // 移除评分
                    _ratingRepository.RemoveRating(ticketId);
                    Console.WriteLine($"已移除订单{orderId}的评分");

                    // 重新计算并更新电影的平均分
                    _filmRepository.UpdateAverageScore(film, score, -1);

                    transaction.Commit();
                    Console.WriteLine($"订单{orderId}评分撤销成功");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"订单{orderId}评分撤销失败: {ex.Message}");
                    throw;
                }
            }
        }


        /// <summary>
        /// 获取电影的所有评分详情
        /// </summary>
        public IEnumerable<Rating> GetFilmRatingDetails(string filmName)
        {
            return _ratingRepository.GetRatingsByFilmName(filmName);
        }

        // 获取用户对于所有影片的印象分
        public Dictionary<string, decimal> GetUserGenreImpression(string customerId)
        {
            // 获取该用户的所有订单
            List<OrderForTickets> ordersOfCustomer = _orderRepository.GetOrdersForCustomer(customerId, true);
            var genreScores = new Dictionary<string, decimal>();

            foreach (var order in ordersOfCustomer)
            {
                var filmName = GetFilmNamebyOrderId(order.OrderID); // 获取电影名
                var film = _filmRepository.GetFilmByName(filmName); // 获取电影
                var filmGenres = GetFilmGenres(film.Genre);         // 获取电影类型
                if (filmGenres == null || filmGenres.Count() == 0)
                    continue;

                // 1. 计算基础分（主类型 +10，次类型 +5）
                string primaryGenre = filmGenres[0];
                string secondaryGenre = filmGenres.Count() > 1 ? filmGenres[1] : null;

                // 2. 计算时间衰减因子
                decimal timeFactor = CalculateTimeFactor(order.Day);

                var ticketId = order.TicketID;                      // 获取TicketID
                var rating = _ratingRepository.GetRating(ticketId); // 获取影评

                // 4. 若Rating存在，则要考虑影评中体现的印象分
                decimal ratingFactor = 1;                            // 将评分调整因子初始值置为1
                if (rating != null)
                {
                    ratingFactor = CalculateRatingFactor(rating.Score);
                }

                // 5. 计算主类型印象分
                decimal primaryScore = 10 * timeFactor * ratingFactor;
                AddOrUpdateGenreScore(genreScores, primaryGenre, primaryScore);

                // 6. 计算次类型印象分（如果有）
                if (secondaryGenre != null)
                {
                    decimal secondaryScore = 5 * timeFactor * ratingFactor;
                    AddOrUpdateGenreScore(genreScores, secondaryGenre, secondaryScore);
                }
            }

            return genreScores;
        }

        // 电影题材获取
        private List<string> GetFilmGenres(string genreString)
        {
            if (string.IsNullOrWhiteSpace(genreString))
                return new List<string>();

            // 1. 按 "/" 分割字符串，并去除空白字符
            var genres = genreString
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .ToList();

            // 2. 只取前两个类型（如果存在）
            return genres.Take(2).ToList();
        }

        // 计算时间衰减因子
        private decimal CalculateTimeFactor(DateTime? Date)
        {
            TimeSpan timeSinceRating = DateTime.Now - Date.Value;

            if (timeSinceRating < TimeSpan.Zero)
                return 1.5M; // 未看完
            else if (timeSinceRating.TotalDays <= 365)
                return 1.0M; // 1 年内
            else if (timeSinceRating.TotalDays <= 365 * 3)
                return 0.5M; // 1-3 年
            else
                return 0.3M; // 3 年以上
        }

        // 计算评分调整因子
        private decimal CalculateRatingFactor(int score)
        {
            if (score == 0 || score == 1)
                return -1.0M; // 负面印象
            else if (score >= 9)
                return 2.25M; // 高分加成
            else if (score >= 7)
                return 1.5M;
            else if (score >= 5)
                return 1.0M;
            else
                return 0.0M; // 评分为（2、3、4），相当于没看过
        }

        // 更新类型印象分
        private void AddOrUpdateGenreScore(Dictionary<string, decimal> genreScores, string genre, decimal score)
        {
            if (genreScores.ContainsKey(genre))
                genreScores[genre] += score;
            else
                genreScores.Add(genre, score);
        }

        // 获取推荐的top5电影
        public IEnumerable<MovieRecommendation> GetMovieRecommendations(string customerId)
        {
            // 1. 获取用户画像数据
            var userGenreScores = GetUserGenreImpression(customerId);
            if (!userGenreScores.Any())
            {
                return Enumerable.Empty<MovieRecommendation>(); // 无画像数据时返回空
            }

            // 2. 获取用户已观看的电影名称集合
            var watchedFilms = _orderRepository.GetOrdersForCustomer(customerId)
                .AsParallel()
                .Select(order => GetFilmNamebyOrderId(order.OrderID))
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet();

            // 3. 获取未来两个月的排片场次
            var tomorrow = DateTime.Today.AddDays(1);
            var twoMonthsLater = tomorrow.AddMonths(2);
            var upcomingSections = _schedulingService.GetSectionsByDateRange(tomorrow, twoMonthsLater);

            // 4. 构建可推荐电影字典（电影名 -> 最早场次）
            var candidateFilms = upcomingSections
                .GroupBy(s => s.FilmName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Min(s => s.ScheduleStartTime));


            // 5.计算推荐指数
            var recommendations = new List<MovieRecommendation>();
            foreach (var filmName in candidateFilms.Keys)
            {
                if (watchedFilms.Contains(filmName)) continue;   // 已经购票过的电影，不再推荐

                var film = _filmRepository.GetFilmByName(filmName);
                if (film == null) continue;

                var genres = GetFilmGenres(film.Genre);
                if (genres.Count() == 0) continue;

                // 计算类型匹配得分：第一类型占0.65权重，第二类型占0.35权重
                decimal genreScore = 0;
                if (genres.Count() >= 1 && userGenreScores.TryGetValue(genres[0], out var primaryScore))
                    genreScore += primaryScore * 0.65M;
                if (genres.Count() >= 2 && userGenreScores.TryGetValue(genres[1], out var secondaryScore))
                    genreScore += secondaryScore * 0.35M;

                // 计算最终推荐得分
                decimal recommendationScore;
                if (film.RatingNum == 0)
                {
                    // 情况1：电影未上映（无评分数据）
                    recommendationScore = Math.Round(genreScore, 1);
                }
                else
                {
                    // 情况2：已上映电影（有评分数据）
                    decimal normalizedRating = film.Score / 10;
                    recommendationScore = Math.Round(genreScore * normalizedRating, 1);
                }

                recommendations.Add(new MovieRecommendation
                {
                    FilmName = filmName,
                    Genres = genres,
                    NearestScreening = candidateFilms[filmName],
                    Score = film.RatingNum,
                    RecommendationScore = recommendationScore
                });
            }

            return recommendations
                .OrderByDescending(r => r.RecommendationScore)
                .Take(5);
        }
    }
}