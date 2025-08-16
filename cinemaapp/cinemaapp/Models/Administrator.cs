using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.Models;

namespace test.Models
{
    public class Administrator
    {
        public string AdminID { get; set; }       // 管理员ID（主键）
        public string AdminName { get; set; }     // 管理员姓名
        public string PhoneNum { get; set; }      // 联系电话
        public string PasswordHash { get; set; }  // 密码哈希
        public string Salt { get; set; }          // 盐值

        // 运行时角色列表（管理员默认拥有管理员角色）
        public List<string> Roles { get; set; } = new List<string>();

        public Administrator()
        {
            // 初始化时添加管理员角色
            Roles.Add(UserRoles.Administrator);
        }

        /// 获取显示名称
        public virtual string GetDisplayName()
        {
            return AdminName;
        }

        /// 重写ToString方法，输出管理员基本信息
        public override string ToString()
        {
            return $"管理员ID: {AdminID}, 姓名: {AdminName}, 电话: {PhoneNum}";
        }



    }
}
