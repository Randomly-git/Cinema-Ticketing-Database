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
        void RateOrder(int orderId, int score, string comment = null);

       // 用户按照订单删除影评
        void CancelRating(int orderId);

       // 获取某部电影的全部影评情况（每个评分+评论+日期）
        IEnumerable<Rating> GetFilmRatingDetails(string filmName);

        // 获取某用户的全部影评情况（包括电影名+评分+日期），便于给用户画像
        //IEnumerable<UserRating> GetUserRatings(string customerId);
    
    }

    /// <summary>
    /// 用户评分DTO(包含电影信息)
    /// </summary>
    public class UserRating
    {
        public string FilmName { get; set; }
        public int Score { get; set; }
        public DateTime RatingDate { get; set; }
    }

}