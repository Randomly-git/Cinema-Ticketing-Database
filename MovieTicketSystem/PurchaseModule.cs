using System;
using Oracle.ManagedDataAccess.Client;

namespace MovieTicketSystem
{
    public class PurchaseModule
    {
        private readonly string connectionString;

        public PurchaseModule(string connStr)
        {
            connectionString = connStr;
        }

        public void PurchaseProduct()
        {
            Console.WriteLine("\n=== 周边购买 ===");
            Console.Write("请输入您的ID: ");
            string customerId = Console.ReadLine();

            using var conn = new OracleConnection(connectionString);
            conn.Open();

            // 显示商品列表
            using var listCmd = new OracleCommand("SELECT PRODUCTNAME, PRICE FROM RELATEDPRODUCT", conn);
            using var reader = listCmd.ExecuteReader();
            Console.WriteLine("\n商品名      价格");
            while (reader.Read())
            {
                Console.WriteLine($"{reader.GetString(0)}    {reader.GetDecimal(1)}元");
            }

            // 用户输入购买信息
            Console.Write("\n请输入您要购买的商品名称: ");
            string productName = Console.ReadLine();
            Console.Write("请输入购买数量: ");
            int quantity = int.Parse(Console.ReadLine());
            Console.Write("请输入支付方式(微信支付/支付宝/银行卡): ");
            string paymentMethod = Console.ReadLine();

            using var tran = conn.BeginTransaction();
            try
            {
                // 查询库存和价格
                using var checkCmd = new OracleCommand(
                    "SELECT PRODUCTNUMBER, PRICE FROM RELATEDPRODUCT WHERE PRODUCTNAME = :pname FOR UPDATE", conn);
                checkCmd.Parameters.Add(new OracleParameter("pname", productName));
                checkCmd.Transaction = tran;

                using var r = checkCmd.ExecuteReader();
                if (!r.Read())
                {
                    tran.Rollback();
                    Console.WriteLine("未找到该商品！");
                    return;
                }

                int inventory = r.GetInt32(0);
                decimal price = r.GetDecimal(1);
                r.Close();

                if (inventory < quantity)
                {
                    tran.Rollback();
                    Console.WriteLine("库存不足！");
                    return;
                }

                // 减库存
                using var updateInv = new OracleCommand(
                    "UPDATE RELATEDPRODUCT SET PRODUCTNUMBER = PRODUCTNUMBER - :qty WHERE PRODUCTNAME = :pname",
                    conn);
                updateInv.Parameters.Add(new OracleParameter("qty", quantity));
                updateInv.Parameters.Add(new OracleParameter("pname", productName));
                updateInv.Transaction = tran;
                updateInv.ExecuteNonQuery();

                // 插入新订单
                using var insertCmd = new OracleCommand(
                    @"INSERT INTO ORDERFORPRODUCTS 
                    (ORDERID, CUSTOMERID, PRODUCTNAME, PURCHASENUM, DAY, STATE, PMETHOD, PRICE) 
                    VALUES 
                    (ORDERFORPRODUCTS_seq.NEXTVAL, :cid, :pname, :qty, SYSDATE, '有效', :payMethod, :total)",
                    conn);
                insertCmd.Parameters.Add(new OracleParameter("cid", customerId));
                insertCmd.Parameters.Add(new OracleParameter("pname", productName));
                insertCmd.Parameters.Add(new OracleParameter("qty", quantity));
                insertCmd.Parameters.Add(new OracleParameter("payMethod", paymentMethod));
                insertCmd.Parameters.Add(new OracleParameter("total", price * quantity));
                insertCmd.Transaction = tran;
                insertCmd.ExecuteNonQuery();

                tran.Commit();
                Console.WriteLine("购买成功！(跳转逻辑之后在网页实现)");
            }
            catch (Exception ex)
            {
                tran.Rollback();
                Console.WriteLine("发生异常：" + ex.Message);
            }
        }
    }
}
