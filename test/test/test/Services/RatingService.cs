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
        private readonly IRatingRepository _ratingRepository;
        private readonly IFilmRepository _filmRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly string _connectionString;

        /// <summary>
        /// 构造函数，注入依赖
        /// </summary>
        public RatingService(
            IRatingRepository ratingRepository,
            IFilmRepository filmRepository,
            IOrderRepository orderRepository,
            string connectionString)
        {
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

                Console.WriteLine($"订单{orderId}获取电影名成功");
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

        /// <summary>
        /// 用户对电影票进行评分
        /// </summary>
        public void RateOrder(int orderId, int score, string comment = null)
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

                    var filmName = GetFilmNamebyOrderId(orderId); // 获取电影名
                    var film = _filmRepository.GetFilmByName(filmName); // 获取电影
                    var ticketId = order.TicketID;  // 获取TicketID

                    // 创建或更新评分
                    var rating = new Rating
                    {
                        TicketID = ticketId,
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
        public void CancelRating(int orderId)
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

                    var filmName = GetFilmNamebyOrderId(orderId); // 获取电影名
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
/*
        /// <summary>
        /// 获取电影的平均评分
        /// </summary>
        public float GetAverageRating(string filmName)
        {
            var film = _filmRepository.GetFilmByName(filmName);
            if (film == null)
            {
                throw new KeyNotFoundException($"找不到电影'{filmName}'");
            }

            Console.WriteLine($"获取电影{filmName}的平均评分为: {film.AverageScore ?? 0f}");
            return film.AverageScore ?? 0f;
        }

        /// <summary>
        /// 获取电影的所有评分详情
        /// </summary>
        public IEnumerable<RatingDetail> GetFilmRatingDetails(string filmName)
        {
            // 获取该电影的所有评分
            var ratings = _ratingRepository.GetRatingsByFilmName(filmName);
            if (ratings == null || !ratings.Any())
            {
                Console.WriteLine($"电影{filmName}暂无评分记录");
                return Enumerable.Empty<RatingDetail>();
            }

            // 获取这些评分对应的所有订单以获取用户信息
            var ticketIds = ratings.Select(r => r.TicketId).ToList();
            var orders = _orderRepository.GetOrdersByTicketIds(ticketIds);

            // 组合成RatingDetail对象
            var result = ratings.Select(r =>
            {
                var order = orders.FirstOrDefault(o => o.Id.ToString() == r.TicketId);
                return new RatingDetail
                {
                    TicketId = r.TicketId,
                    Score = r.Score,
                    Comment = r.Comment,
                    RatingDate = r.RatingDate,
                    CustomerId = r.CustomerId,
                    CustomerName = order?.CustomerName
                };
            }).ToList();

            Console.WriteLine($"获取电影{filmName}的{result.Count}条评分详情");
            return result;
        }

        /// <summary>
        /// 获取用户的所有评分记录
        /// </summary>
        public IEnumerable<UserRating> GetUserRatings(string customerId)
        {
            // 获取该用户的所有评分
            var ratings = _ratingRepository.GetRatingsByCustomerId(customerId);
            if (ratings == null || !ratings.Any())
            {
                Console.WriteLine($"用户{customerId}暂无评分记录");
                return Enumerable.Empty<UserRating>();
            }

            // 获取这些评分对应的所有订单以获取电影信息
            var ticketIds = ratings.Select(r => r.TicketId).ToList();
            var orders = _orderRepository.GetOrdersByTicketIds(ticketIds);

            // 组合成UserRating对象
            var result = ratings.Select(r =>
            {
                var order = orders.FirstOrDefault(o => o.Id.ToString() == r.TicketId);
                return new UserRating
                {
                    TicketId = r.TicketId,
                    FilmName = r.FilmName,
                    Score = r.Score,
                    Comment = r.Comment,
                    RatingDate = r.RatingDate,
                    WatchDate = order?.OrderDate ?? DateTime.MinValue
                };
            }).ToList();

            Console.WriteLine($"获取用户{customerId}的{result.Count}条评分记录");
            return result;
        }
*/
    }
}