using System;
using System.Collections.Generic;
using test.Models;

namespace test.Models 
{
    /// <summary>
    /// 对应数据库中的 order for tickets 表，存储电影票订单的信息。
    /// 字段：ORDERID, TICKETID, STATE, CUSTOMERID, DAY, PMETHOD, PRICE
    /// </summary>
    public class OrderForTickets
    {
        public int OrderID { get; set; }       // 订单号，PK
        public string TicketID { get; set; }   // 票号，FK 参考 ticket 表中的 ticketID
        public string State { get; set; }      // 订单状态（有效、失效）
        public string CustomerID { get; set; } // 顾客ID（即买家），FK 参考 customer 表中的 customerID
        public DateTime Day { get; set; }      // 订单日期
        public string PaymentMethod { get; set; } // 支付方式 (对应数据库 PMETHOD)
        public decimal TotalPrice { get; set; } // 支付金额 (对应数据库 PRICE)

        // 导航属性：方便获取关联的票和顾客信息
        public Ticket Ticket { get; set; }
        public Customer Customer { get; set; }

        public override string ToString()
        {
            return $"订单ID: {OrderID}, 票号: {TicketID}, 顾客ID: {CustomerID}, 状态: {State}, 支付方式: {PaymentMethod}, 金额: {TotalPrice:C}, 日期: {Day.ToShortDateString()}";
        }
    }
}
