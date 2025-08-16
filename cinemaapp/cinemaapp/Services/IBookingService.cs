using System;
using test.Models; // 引用 Models 命名空间

namespace test.Services // 请替换为你的项目命名空间
{
    /// <summary>
    /// 购票业务服务接口。
    /// </summary>
    public interface IBookingService
    {
        public bool RefundTicket(int orderId, DateTime refundTime, out decimal refundFee, out decimal refundAmount, out string errorMessage); //退票方法

        bool TryGetRefundInfo(int bookingId, DateTime refundTime, out decimal fee, out decimal refundAmount, out string errorMessage);

        //一次性购买多张票
        List<OrderForTickets> PurchaseMultipleTickets(
        int sectionId,
        List<SeatHall> selectedSeats,
        string customerId,
        string paymentMethod,
        decimal pointsToUse = 0
    );

        decimal CalculateFinalTicketPrice(Section section, Customer customer, string lineNo);//计算总价方法

    }
}