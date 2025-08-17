using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.Models;

namespace test.Repositories
{
    /// <summary>
    /// 评价数据仓储接口。
    /// </summary>
    public interface IRatingRepository
    {

        Rating GetRating(string ticketId);  // 在数据库中，通过ticketId直接找到Rating
         
        void AddOrUpdateRating(Rating rating);  // 添加影评

        void RemoveRating(string ticketId);    // 移除影评

        IEnumerable<Rating> GetRatingsByTicketIds(IEnumerable<string> ticketIds); // 批量查询

        IEnumerable<Rating> GetRatingsByFilmName(string filmName);        // 根据电影名查询
    }
}
