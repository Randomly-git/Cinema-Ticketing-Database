using System;
using test.Models;

namespace test.Models 
{
    /// <summary>
    /// 对应数据库中的 ticket 表，存储电影票的信息。
    /// 根据文档字段：TICKETID, PRICE, RATING, SECTIONID, LINENO, COLUMNNO, STATE
    /// 注意：TicketID 不再直接关联 CustomerID，而是通过 OrderForTickets 表关联。
    /// </summary>
    public class Ticket
    {
        public string TicketID { get; set; }   // 票号，PK
        public decimal Price { get; set; }     // 票价（实际售价）
        public int Rating { get; set; }        // 评分（单次观影的评分）
        public int SectionID { get; set; }     // 场次号，FK 参考 section 表中的 sectionID
        public string LineNo { get; set; }     // 座位行号
        public int ColumnNo { get; set; }      // 座位列号
        public string State { get; set; }      // 状态（已售出、未售出）

        // 导航属性：方便获取关联的场次信息 (OrderForTickets 的关联在 OrderForTickets 模型中处理)
        public Section Section { get; set; }

        public override string ToString()
        {
            return $"票号: {TicketID}, 场次: {SectionID}, 座位: {LineNo}{ColumnNo}, 价格: {Price:C}, 状态: {State}";
        }
    }
}
