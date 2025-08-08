using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using test.Models;
using test.Repositories;

namespace test.Repositories
{
    /// <summary>
    /// Oracle 数据库的周边产品数据仓库实现。
    /// </summary>
    public class OracleRelatedProductRepository : IRelatedProductRepository
    {
        private readonly string _connectionString;

        public OracleRelatedProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 获取所有周边产品。
        /// </summary>
        public List<RelatedProduct> GetAllProducts()
        {
            List<RelatedProduct> products = new List<RelatedProduct>();
            // 添加 REQUIREDPOINTS 到查询字段
            string sql = "SELECT PRODUCTNAME, PRICE, PRODUCTNUMBER, REQUIREDPOINTS FROM RELATEDPRODUCT";

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new RelatedProduct
                            {
                                ProductName = reader.GetString(0),
                                Price = reader.GetDecimal(1),
                                ProductNumber = reader.GetInt32(2),
                                RequiredPoints = reader.GetInt32(3) // 添加 REQUIREDPOINTS 字段
                            });
                        }
                    }
                }
            }
            return products;
        }

        /// <summary>
        /// 根据产品名称获取周边产品。
        /// </summary>
        public RelatedProduct GetProductByName(string productName)
        {
            RelatedProduct product = null;
            // 添加 REQUIREDPOINTS 到查询字段
            string sql = "SELECT PRODUCTNAME, PRICE, PRODUCTNUMBER, REQUIREDPOINTS FROM RELATEDPRODUCT WHERE PRODUCTNAME = :productName";

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("productName", productName));
                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new RelatedProduct
                            {
                                ProductName = reader.GetString(0),
                                Price = reader.GetDecimal(1),
                                ProductNumber = reader.GetInt32(2),
                                RequiredPoints = reader.IsDBNull(3) ? 0 : reader.GetInt32(3) // 安全获取积分值
                            };
                        }
                    }
                }
            }
            return product;
        }

        /// <summary>
        /// 更新产品库存。
        /// </summary>
        public void UpdateProductStock(string productName, int quantityChange)
        {
            string sql = "UPDATE RELATEDPRODUCT SET PRODUCTNUMBER = PRODUCTNUMBER + :quantityChange WHERE PRODUCTNAME = :productName";

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("quantityChange", quantityChange));
                    command.Parameters.Add(new OracleParameter("productName", productName));
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException($"更新产品 '{productName}' 库存失败，可能产品不存在。");
                    }
                }
            }
        }
    }
}
