using System;

namespace ConsoleApp1.Models
{
    // RelatedProduct 类用于映射 related product 表
    public class RelatedProduct
    {
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int ProductNumber { get; set; }

        // 默认构造函数
        public RelatedProduct() { }

        // 带参数的构造函数
        public RelatedProduct(string productName, int price, int productnumber)
        {
            ProductName = productName;
            Price = price;
            ProductNumber = productnumber;
        }

        // 打印商品信息的方法
        public void DisplayProductInfo()
        {
            Console.WriteLine($"商品名: {ProductName}");
            Console.WriteLine($"价格: {Price}");
            Console.WriteLine($"库存数量: {ProductNumber}");
        }
    }
}
