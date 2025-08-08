using System.Collections.Generic;
using test.Models;

namespace test.Services
{
    /// <summary>
    /// 周边产品业务服务接口。
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// 获取所有可用的周边产品。
        /// </summary>
        /// <returns>产品列表。</returns>
        List<RelatedProduct> GetAvailableProducts();

        /// <summary>
        /// 购买周边产品。
        /// </summary>
        /// <param name="productName">产品名称。</param>
        /// <param name="purchaseNum">购买数量。</param>
        /// <param name="customerId">顾客ID。</param>
        /// <param name="paymentMethod">支付方式。</param>
        /// <returns>生成的订单。</returns>
        OrderForProduct PurchaseProduct(string productName, int purchaseNum, string customerId, string paymentMethod);
        /// <summary>
        /// 使用积分兑换周边产品
        /// </summary>
        /// <param name="productName">产品名称</param>
        /// <param name="quantity">兑换数量</param>
        /// <param name="customerId">顾客ID</param>
        /// <returns>兑换结果信息</returns>
        string RedeemProductWithPoints(string productName, int quantity, string customerId);
    }
}
