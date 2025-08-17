using System;
using System.Collections.Generic;
using test.Models;

namespace test.Services
{
    /// <summary>
    /// 电影评分服务接口
    /// </summary>
    public interface IRatingService
    {
        // 根据订单号直接查询电影名称（辅助函数）
        string GetFilmNamebyOrderId(int orderId);

        // 用户查询某个订单是否已经被自己评论过
        bool HasRated(int orderId);

       // 用户按照订单对电影进行影评（订单能够从用户直接查到）
        void RateOrder(int orderId, string filmName,int score, string comment = null);

       // 用户按照订单删除影评
        void CancelRating(int orderId, string filmName);

       // 获取某部电影的全部影评情况（每个评分+评论+日期）
        IEnumerable<Rating> GetFilmRatingDetails(string filmName);

        // 用户画像：根据用户的订单和现有的评分，得到每种题材给用户的印象分
        Dictionary<string, decimal> GetUserGenreImpression(string customerId);

        // 获取推荐的电影
        IEnumerable<MovieRecommendation> GetMovieRecommendations(string customerId);
    }

    public class MovieRecommendation
    {
        public string FilmName { get; set; }       // 电影名称
        public List<string> Genres { get; set; }   // 电影类型（主+次）
        public DateTime NearestScreening { get; set; } // 未来最近的排片日期
        public decimal Score { get; set; }     // 电影总评分
        public decimal RecommendationScore { get; set; } // 推荐指数（计算得分）
    }

}