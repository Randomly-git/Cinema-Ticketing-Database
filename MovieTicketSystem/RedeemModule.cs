using System;
using Oracle.ManagedDataAccess.Client;

namespace MovieTicketSystem
{
    public class RedeemModule
    {
        private readonly string connectionString;

        public RedeemModule(string connStr)
        {
            connectionString = connStr;
        }

        public void RedeemReward()
        {
            Console.WriteLine("\n=== 积分兑换 ===");

            Console.Write("请输入用户ID: ");
            string customerId = Console.ReadLine();

            using var conn = new OracleConnection(connectionString);
            conn.Open();

            using var tran = conn.BeginTransaction();
            try
            {
                // 查询用户当前积分
                using var getPointsCmd = new OracleCommand(
                    "SELECT POINTS FROM VIPCARD WHERE CUSTOMERID = :customerId FOR UPDATE", conn);
                getPointsCmd.Parameters.Add(new OracleParameter("customerId", customerId));
                getPointsCmd.Transaction = tran;

                var ptsObj = getPointsCmd.ExecuteScalar();
                if (ptsObj == null || ptsObj == DBNull.Value)
                {
                    tran.Rollback();
                    Console.WriteLine("未找到该用户或积分信息为空！");
                    return;
                }

                int points = Convert.ToInt32(ptsObj);
                Console.WriteLine($"当前积分：{points}");

                // 输出商品及所需积分表
                Console.WriteLine("\n可兑换商品列表（商品名称 - 所需积分）:");
                using var listProductsCmd = new OracleCommand(
                    "SELECT PRODUCTNAME, REQUIREDPOINTS FROM RELATEDPRODUCT", conn);
                using var reader = listProductsCmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    object pts = reader.IsDBNull(1) ? "未设置" : reader.GetInt32(1).ToString();
                    Console.WriteLine($"- {name}：{pts} 积分");
                }

                // 用户选择商品
                Console.Write("\n请输入要兑换的商品名称: ");
                string productName = Console.ReadLine();

                // 查询该商品所需积分
                using var getReqPointsCmd = new OracleCommand(
                    "SELECT REQUIREDPOINTS FROM RELATEDPRODUCT WHERE PRODUCTNAME = :productName", conn);
                getReqPointsCmd.Parameters.Add(new OracleParameter("productName", productName));
                getReqPointsCmd.Transaction = tran;

                var reqPtsObj = getReqPointsCmd.ExecuteScalar();
                if (reqPtsObj == null || reqPtsObj == DBNull.Value)
                {
                    tran.Rollback();
                    Console.WriteLine("未找到该商品，或所需积分未设置！");
                    return;
                }

                int requiredPoints = Convert.ToInt32(reqPtsObj);

                // 判断积分是否足够
                if (points < requiredPoints)
                {
                    tran.Rollback();
                    Console.WriteLine("积分不足，兑换失败！");
                    return;
                }

                // 检查库存并减库存
                using var updateInventory = new OracleCommand(
                    "UPDATE RELATEDPRODUCT SET PRODUCTNUMBER = PRODUCTNUMBER - 1 " +
                    "WHERE PRODUCTNAME = :productName AND PRODUCTNUMBER > 0",
                    conn);
                updateInventory.Parameters.Add(new OracleParameter("productName", productName));
                updateInventory.Transaction = tran;
                int updated = updateInventory.ExecuteNonQuery();
                if (updated == 0)
                {
                    tran.Rollback();
                    Console.WriteLine("库存不足，兑换失败！");
                    return;
                }

                // 扣除用户积分
                using var updatePoints = new OracleCommand(
                    "UPDATE VIPCARD SET POINTS = POINTS - :requiredPoints WHERE CUSTOMERID = :customerId",
                    conn);
                updatePoints.Parameters.Add(new OracleParameter("requiredPoints", requiredPoints));
                updatePoints.Parameters.Add(new OracleParameter("customerId", customerId));
                updatePoints.Transaction = tran;
                updatePoints.ExecuteNonQuery();

                // 查询兑换后剩余积分
                using var getNewPointsCmd = new OracleCommand(
                    "SELECT POINTS FROM VIPCARD WHERE CUSTOMERID = :customerId", conn);
                getNewPointsCmd.Parameters.Add(new OracleParameter("customerId", customerId));
                getNewPointsCmd.Transaction = tran;

                var newPtsObj = getNewPointsCmd.ExecuteScalar();
                int newPoints = Convert.ToInt32(newPtsObj);

                tran.Commit();
                Console.WriteLine("兑换成功！");
                Console.WriteLine($"兑换后积分余额：{newPoints}");
            }
            catch (Exception ex)
            {
                tran.Rollback();
                Console.WriteLine("发生异常：" + ex.Message);
            }
        }
    }
}
