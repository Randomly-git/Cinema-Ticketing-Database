using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.Models
{
    /// <summary>
    /// 对应数据库中的 RATING 表，存储影评信息。
    /// 字段：TICKETID, SCORE, COMMENT, RATINGDATE
    /// </summary>
    public class Rating
    {
        public string TicketID { get; set; }      // 对应的电影票ID（主键 & 外键关联 TICKET 表）
        public int Score { get; set; }            // 用户评分（整数，0-10）
        public string Comment { get; set; }       // 用户评论（可选）
        public DateTime RatingDate { get; set; }  // 评分时间

        // 导航属性（关联查询 TICKET 及后续电影信息）
        public Ticket Ticket { get; set; }        // 关联的电影票

        public override string ToString()
        {
            return $"票ID: {TicketID}, 评分: {Score}, 评论: {Comment}, 时间: {RatingDate.ToShortDateString()}";
        }
    }
}