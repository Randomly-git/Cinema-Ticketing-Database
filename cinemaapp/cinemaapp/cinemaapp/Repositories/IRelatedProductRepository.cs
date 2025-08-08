using test.Models;
using System.Collections.Generic;

namespace test.Repositories
{
    /// <summary>
    /// 周边产品数据仓库接口。
    /// </summary>
    public interface IRelatedProductRepository
    {
        /// <summary>
        /// 获取所有周边产品。
        /// </summary>
        /// <returns>周边产品列表。</returns>
        List<RelatedProduct> GetAllProducts();

        /// <summary>
        /// 根据产品名称获取周边产品。
        /// </summary>
        /// <param name="productName">产品名称。</param>
        /// <returns>匹配的周边产品或 null。</returns>
        RelatedProduct GetProductByName(string productName);

        /// <summary>
        /// 更新产品库存。
        /// </summary>
        /// <param name="productName">产品名称。</param>
        /// <param name="quantityChange">库存变化量（购买时为负数）。</param>
        void UpdateProductStock(string productName, int quantityChange);
    }
}
