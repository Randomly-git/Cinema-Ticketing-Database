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

        public ProductService(IRelatedProductRepository relatedProductRepository, IOrderForProductRepository orderForProductRepository)
        {
            _relatedProductRepository = relatedProductRepository;
            _orderForProductRepository = orderForProductRepository;
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
    }
}
