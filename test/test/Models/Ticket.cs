using System;
using test.Models;

namespace test.Models 
{
    /// <summary>
    /// 对应数据库中的 ticket 表，存储票务信息（已售出座位）。
    /// </summary>
    public class Ticket
    {
        public string TicketID { get; set; }   // 票ID，PK
        public int SectionID { get; set; }     // 场次号，FK
        public string LineNo { get; set; }     // 座位行号，对应数据库 LINENO
        public int ColumnNo { get; set; }      // 座位列号，对应数据库 COLUMNNO
        public string State { get; set; }      // 票状态（例如：已售出，已退票），对应数据库 STATE
        public decimal Price { get; set; }     // 票价，对应数据库 PRICE
        public int Rating { get; set; }        // 评分（观影后评分），对应数据库 RATING

        // 导航属性：方便获取关联的场次和顾客信息
        public Section Section { get; set; }
        public Customer Customer { get; set; }

        public override string ToString()
        {
            return $"票ID: {TicketID}, 场次: {SectionID}, 座位: {LineNo}{ColumnNo}, 价格: {Price:C}, 状态: {State}";
        }
    }
}
