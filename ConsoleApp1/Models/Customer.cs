using System;

namespace ConsoleApp1.Models
{
    // Customer 类用于映射顾客表
    public class Customer
    {
        public string CustomerID { get; set; }
        public string Name { get; set; }
        public string PhoneNum { get; set; }
        public int VipLevel { get; set; }

        // 默认构造函数
        public Customer()
        {
        }

        // 带参数的构造函数
        public Customer(string customerID, string name, string phoneNum, int vipLevel)
        {
            CustomerID = customerID;
            Name = name;
            PhoneNum = phoneNum;
            VipLevel = vipLevel;
        }

        // 打印顾客信息的方法
        public void DisplayCustomerInfo()
        {
            Console.WriteLine($"顾客ID: {CustomerID}");
            Console.WriteLine($"顾客姓名: {Name}");
            Console.WriteLine($"联系电话: {PhoneNum}");
            Console.WriteLine($"会员等级: {VipLevel}");
        }
    }
}
