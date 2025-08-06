using System;

namespace MovieTicketSystem
{
    class Program
    {
        static void Main()
        {
            string connectionString = "User Id=cbc;Password=123456;Data Source=8.148.76.54:1524/orclpdb1";

            while (true)
            {
                Console.WriteLine("\n========= 电影票系统扩展业务 =========");
                Console.WriteLine("1. 积分兑换");
                Console.WriteLine("2. 周边商品购买");
                Console.WriteLine("0. 退出系统");
                Console.WriteLine("=======================================");

                Console.Write("请输入你要执行的功能: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        var redeem = new RedeemModule(connectionString);
                        redeem.RedeemReward();
                        break;
                    case "2":
                        var purchase = new PurchaseModule(connectionString);
                        purchase.PurchaseProduct();
                        break;
                    case "0":
                        Console.WriteLine("已退出系统。");
                        return;
                    default:
                        Console.WriteLine("无效输入，请重新选择。");
                        break;
                }
            }
        }
    }
}
