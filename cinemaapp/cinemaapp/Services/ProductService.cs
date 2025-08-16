using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using test.Models;
using test.Repositories;
using test.Services;

namespace test.Services
{
    /// <summary>
    /// 周边产品业务服务实现。
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IRelatedProductRepository _relatedProductRepository;
        private readonly IOrderForProductRepository _orderForProductRepository;
        private readonly string _connectionString;
        public ProductService(IRelatedProductRepository relatedProductRepository, IOrderForProductRepository orderForProductRepository, string connectionString)
        {
            _relatedProductRepository = relatedProductRepository;
            _orderForProductRepository = orderForProductRepository;
            _connectionString = connectionString;

        }

        /// <summary>
        /// 获取所有可用的周边产品。
        /// </summary>
        public List<RelatedProduct> GetAvailableProducts()
        {
            return _relatedProductRepository.GetAllProducts();
        }

        /// <summary>
        /// 购买周边产品。
        /// </summary>
        public OrderForProduct PurchaseProduct(string productName, int purchaseNum, string customerId, string paymentMethod)
        {
            if (purchaseNum <= 0)
            {
                throw new ArgumentException("购买数量必须大于0。");
            }

            RelatedProduct product = _relatedProductRepository.GetProductByName(productName);
            if (product == null)
            {
                throw new InvalidOperationException($"产品 '{productName}' 不存在。");
            }

            if (product.ProductNumber < purchaseNum)
            {
                throw new InvalidOperationException($"产品 '{productName}' 库存不足。当前库存: {product.ProductNumber}, 您需要: {purchaseNum}");
            }

            // 创建订单
            OrderForProduct newOrder = new OrderForProduct
            {
                CustomerID = customerId,
                ProductName = productName,
                PurchaseNum = purchaseNum,
                Day = DateTime.Now, // 订单日期为当前时间
                State = "已完成",   // 假设直接完成
                PMethod = paymentMethod,
                Price = product.Price // 记录购买时的产品单价
            };

            // 原子性事务：更新库存和添加订单
            try
            {
                // 先添加订单，订单ID会在仓库中生成
                _orderForProductRepository.AddOrderForProduct(newOrder);

                // 然后更新产品库存 (减少库存，所以 quantityChange 是负数)
                _relatedProductRepository.UpdateProductStock(productName, -purchaseNum);

                return newOrder;
            }
            catch (Exception ex)
            {
                // 事务回滚
                throw new InvalidOperationException($"购买产品失败，请重试。错误详情: {ex.Message}", ex);
            }
        }
        public string RedeemProductWithPoints(string productName, int quantity, string customerId)
        {
            if (quantity <= 0)
            {
                return "兑换数量必须大于0！";
            }

            var conn = new OracleConnection(_connectionString);
            conn.Open();

            var tran = conn.BeginTransaction();
            try
            {
                // 1. 检查用户积分
                var getPointsCmd = new OracleCommand(
                    "SELECT POINTS FROM VIPCARD WHERE CUSTOMERID = :customerId FOR UPDATE", conn);
                getPointsCmd.Parameters.Add(new OracleParameter("customerId", customerId));
                getPointsCmd.Transaction = tran;

                var ptsObj = getPointsCmd.ExecuteScalar();
                if (ptsObj == null || ptsObj == DBNull.Value)
                {
                    tran.Rollback();
                    return "未找到该用户或积分信息为空！";
                }

                int points = Convert.ToInt32(ptsObj);

                // 2. 获取商品信息
                RelatedProduct product = _relatedProductRepository.GetProductByName(productName);
                if (product == null)
                {
                    throw new InvalidOperationException($"产品 '{productName}' 不存在。");
                }
                if (product.ProductNumber < quantity)
                {
                    throw new InvalidOperationException($"产品 '{productName}' 库存不足。当前库存: {product.ProductNumber}, 您需要: {quantity}");
                }


                int totalRequiredPoints = product.RequiredPoints * quantity;

                // 3. 检查积分是否足够
                if (points < totalRequiredPoints)
                {
                    tran.Rollback();
                    return $"积分不足，需要 {totalRequiredPoints} 积分，当前只有 {points} 积分！";
                }

                // 4. 兑换商品（减少库存）
                _relatedProductRepository.UpdateProductStock(productName, -quantity);

                // 5. 扣除积分
                var updatePoints = new OracleCommand(
                    "UPDATE VIPCARD SET POINTS = POINTS - :totalRequiredPoints WHERE CUSTOMERID = :customerId",
                    conn);
                updatePoints.Parameters.Add(new OracleParameter("totalRequiredPoints", totalRequiredPoints));
                updatePoints.Parameters.Add(new OracleParameter("customerId", customerId));
                updatePoints.Transaction = tran;
                updatePoints.ExecuteNonQuery();

                // 6. 创建订单
                var order = new OrderForProduct
                {
                    CustomerID = customerId,
                    ProductName = productName,
                    PurchaseNum = quantity,
                    Day = DateTime.Now,
                    State = "已完成", // 假设兑换订单状态为"已完成"
                    PMethod = "积分",
                    Price = product.Price
                };

                _orderForProductRepository.AddOrderForProduct(order);

                // 7. 查询剩余积分
                var newPtsObj = getPointsCmd.ExecuteScalar();
                int newPoints = Convert.ToInt32(newPtsObj);

                tran.Commit();
                return $"成功兑换 {quantity} 个 {productName}！剩余积分：{newPoints}";
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return $"兑换失败：{ex.Message}";
            }
        }
    }
}
