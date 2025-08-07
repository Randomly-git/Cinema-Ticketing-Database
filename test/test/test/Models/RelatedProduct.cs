using Oracle.ManagedDataAccess.Types;
using System;

namespace test.Models
{
    /// <summary>
    /// 表示 RELATEDPRODUCT 表中的周边产品信息。
    /// </summary>
    public class RelatedProduct
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int ProductNumber { get; set; } // 库存数量

        public int RequiredPoints { get; set; } // 兑换所需积分
        public override string ToString()
        {
            return $"产品名称: {ProductName}, 价格: {Price:C}, 库存: {ProductNumber}";
        }
    }
}
