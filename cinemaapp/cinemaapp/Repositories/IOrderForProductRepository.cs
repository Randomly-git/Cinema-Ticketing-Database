using System;
using System.Collections.Generic;
using test.Models;

namespace test.Repositories
{
    /// <summary>
    /// 周边产品订单数据仓库接口。
    /// </summary>
    public interface IOrderForProductRepository
    {
        /// <summary>
        /// 添加新的周边产品订单。
        /// </summary>
        /// <param name="order">要添加的订单对象。</param>
        void AddOrderForProduct(OrderForProduct order);

        /// <summary>
        /// 根据顾客ID获取其所有周边产品订单。
        /// </summary>
        /// <param name="customerId">顾客ID。</param>
        /// <returns>周边产品订单列表。</returns>
        List<OrderForProduct> GetOrdersByCustomerId(string customerId);

        // 可以根据需要添加其他方法如：
        // OrderForProduct GetOrderById(long orderId);
        // List<OrderForProduct> GetAllOrders();
    }
}
