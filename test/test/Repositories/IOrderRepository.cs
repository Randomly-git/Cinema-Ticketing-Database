using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using test.Models;

namespace test.Repositories 
{
    /// <summary>
    /// 订单数据仓储接口。
    /// </summary>
    public interface IOrderRepository
    {
        void AddOrderForTickets(OrderForTickets order, OracleTransaction transaction = null); // 添加电影票订单
        OrderForTickets GetOrderForTicketsById(int orderId); // 根据订单ID获取电影票订单
        List<OrderForTickets> GetOrdersForCustomer(string customerId); // 获取某个顾客的所有电影票订单
        // 可以添加更新订单状态、退票等方法
        
        List<OrderForTickets> GetAllOrders(DateTime? startDate = null, DateTime? endDate = null);//管理员特权方法：获取所有订单
    }
}
