using System;
using System.Collections.Generic;
using test.Models; // 引用 Models 命名空间

namespace test.Services // 请替换为你的项目命名空间
{
    /// <summary>
    /// 购票业务服务接口。
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// 购买电影票。
        /// </summary>
        /// <param name="sectionId">场次ID。</param>
        /// <param name="lineNo">座位行号。</param>
        /// <param name="columnNo">座位列号。</param>
        /// <param name="customerId">顾客ID。</param>
        /// <returns>购买成功的票对象。</returns>
        OrderForTickets PurchaseTicket(int sectionId, string lineNo, int columnNo, string customerId, string paymentMethod); //购票方法

        bool RefundTicket(int orderId, DateTime refundTime, out decimal refundFee, out decimal refundAmount); //退票方法

        //一次性购买多张票
        List<OrderForTickets> PurchaseMultipleTickets(
        int sectionId,
        List<SeatHall> selectedSeats,
        string customerId,
        string paymentMethod
    );

        decimal CalculateFinalTicketPrice(Section section, Customer customer, string lineNo);//计算总价方法
    }
}