
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;

namespace cinemaapp
{
    public partial class MainForm : Form
    {
        private Customer _loggedInCustomer = null;
        private Administrator _loggedInAdmin = null;

        public MainForm()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "电影院管理系统";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            RefreshMenu();
        }

        // 根据当前登录状态刷新界面显示
        private void RefreshMenu()
        {
            this.Controls.Clear();

            Label lblTitle = new Label()
            {
                Text = "电影院管理系统主菜单",
                Font = new Font("微软雅黑", 16, FontStyle.Bold),
                Location = new Point(140, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            if (_loggedInCustomer == null && _loggedInAdmin == null)
            {
                AddButton("顾客注册", 70, RegisterCustomer);
                AddButton("顾客登录", 120, LoginCustomer);
                AddButton("管理员注册", 170, RegisterAdministrator);
                AddButton("管理员登录", 220, LoginAdministrator);
                AddButton("退出系统", 270, () => this.Close());
            }
            else if (_loggedInCustomer != null)
            {
                Label lblUser = new Label()
                {
                    Text = $"当前用户: {_loggedInCustomer.Name} (等级: {_loggedInCustomer.VipLevel}, 积分: {_loggedInCustomer.VIPCard?.Points ?? 0})",
                    Location = new Point(30, 70),
                    AutoSize = true
                };
                this.Controls.Add(lblUser);

                AddButton("更新个人资料", 110, UpdateCustomerProfile);
                AddButton("查看电影排挡", 160, ViewFilmShowings);
                AddButton("购票", 210, PurchaseTicketMenu);
                AddButton("购买周边 (未实现)", 260, () => MessageBox.Show("功能未实现"));
                AddButton("删除我的账户", 310, DeleteCustomerAccount);
                AddButton("用户登出", 360, LogoutCustomer);
                AddButton("退出系统", 410, () => this.Close());
            }
            else if (_loggedInAdmin != null)
            {
                Label lblAdmin = new Label()
                {
                    Text = $"当前管理员: {_loggedInAdmin.AdminName}",
                    Location = new Point(30, 70),
                    AutoSize = true
                };
                this.Controls.Add(lblAdmin);

                AddButton("电影管理 (增/改)", 110, ManageFilmsMenu);
                AddButton("查看所有订单", 160, ViewAllOrders);
                AddButton("管理员登出", 210, LogoutAdministrator);
                AddButton("退出系统", 260, () => this.Close());
            }
        }

        private void AddButton(string text, int top, Action onClick)
        {
            var btn = new Button()
            {
                Text = text,
                Location = new Point(170, top),
                Size = new Size(150, 40),
            };
            btn.Click += (s, e) =>
            {
                try
                {
                    onClick();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("操作失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            this.Controls.Add(btn);
        }

        //顾客注册
        private void RegisterCustomer()
        {
            RegisterCustomer registerForm = new RegisterCustomer();
            registerForm.ShowDialog(); // 模态打开
        }

        //顾客登录
        private void LoginCustomer()
        {
            using (var loginForm = new LoginCustomer(Program._userService, Program._customerRepository))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    Program._loggedInCustomer = loginForm.LoggedInCustomer;
                    _loggedInCustomer = loginForm.LoggedInCustomer;
                    RefreshMenu(); // 根据登录状态刷新界面
                }
            }
        }

        //顾客登出
        private void LogoutCustomer()
        {
            _loggedInCustomer = null;
            MessageBox.Show("用户已登出");
            RefreshMenu();
        }

        //管理员注册
        private void RegisterAdministrator()
        {
            var form = new RegisterAdministrator();
            form.ShowDialog();
        }
        
        //管理员登录
        private void LoginAdministrator()
        {
            using (var loginForm = new LoginAdministrator(Program._adminService))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    Program._loggedInAdmin = loginForm.LoggedInAdmin;
                    _loggedInAdmin = loginForm.LoggedInAdmin;
                    RefreshMenu(); // 或根据需要显示管理员功能
                }
            }
        }
        
        //管理员登出
        private void LogoutAdministrator()
        {
            _loggedInAdmin = null;
            MessageBox.Show("管理员已登出");
            RefreshMenu();
        }

        // 更新个人资料
        private void UpdateCustomerProfile()
        {
            if (Program._loggedInCustomer == null)
            {
                MessageBox.Show("请先登录后再更新资料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var updateForm = new UpdateCustomerProfile(Program._loggedInCustomer))
            {
                if (updateForm.ShowDialog() == DialogResult.OK)
                {
                    // 更新成功后刷新界面（如果你有显示姓名或手机号之类信息）
                    RefreshMenu();
                }
            }
        }


        //查看电影排挡
        private void ViewFilmShowings()
        {
            MessageBox.Show("查看电影排挡 - 功能未实现");
        }

        //购票
        private void PurchaseTicketMenu()
        {
            MessageBox.Show("购票 - 功能未实现");
        }

        //删除账户
        private void DeleteCustomerAccount()
        {
            if (Program._loggedInCustomer == null)
            {
                MessageBox.Show("请先登录再删除账户", "未登录", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var deleteForm = new DeleteCustomerAccount();
            var result = deleteForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                Program._loggedInCustomer = null;
                RefreshMenu(); // 若你界面中有菜单或按钮状态刷新
            }
        }





        //电影管理
        private void ManageFilmsMenu()
        {
            MessageBox.Show("电影管理 - 功能未实现");
        }

        //查看订单
        private void ViewAllOrders()
        {
            MessageBox.Show("查看订单 - 功能未实现");
        }

    }
}
