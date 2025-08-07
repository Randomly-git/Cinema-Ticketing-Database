using System;

namespace test.Models
{
    /// <summary>
    /// 表示 ORDERFORPRODUCTS 表中的周边产品订单信息。
    /// </summary>
    public class OrderForProduct
    {
        public long OrderID { get; set; } // 订单ID，使用 long 匹配 NUMBER(38,0)
        public string CustomerID { get; set; }
        public string ProductName { get; set; }
        public int PurchaseNum { get; set; } // 购买数量
        public DateTime Day { get; set; } // 订单日期
        public string State { get; set; } // 订单状态
        public string PMethod { get; set; } // 支付方式
        public decimal Price { get; set; } // 购买时的产品单价

        public override string ToString()
        {
            return $"订单ID: {OrderID}, 顾客ID: {CustomerID}, 产品: {ProductName}, 数量: {PurchaseNum}, 单价: {Price:C}, 总价: {(Price * PurchaseNum):C}, 日期: {Day.ToShortDateString()}, 状态: {State}, 支付方式: {PMethod}";
        }
    }
}
