
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class MainForm : Form
    {
        private Customer _loggedInCustomer = null;
        private Administrator _loggedInAdmin = null;
        private ISchedulingService _schedulingService;  
        private IShowingService _showingService;

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
                    Text = $"当前用户: {_loggedInCustomer.Name} (ID: {_loggedInCustomer.CustomerID}, 等级: {_loggedInCustomer.VipLevel}, 积分: {_loggedInCustomer.VIPCard?.Points ?? 0})",
                    Location = new Point(30, 70),
                    AutoSize = true
                };
                this.Controls.Add(lblUser);

                AddButton("1. 更新个人资料", 110, UpdateCustomerProfile);
                AddButton("2. 查看电影相关信息", 150, FilmDashBoard);
                AddButton("3. 购票", 190, PurchaseTicketMenu);
                AddButton("4. 查看所有有效订单", 230, DisplayCustomerPaidOrders);
                AddButton("5. 退票", 270, ProcessTicketRefund);
                AddButton("6. 购买周边 (未实现)", 310, () => MessageBox.Show("功能尚未实现"));
                AddButton("7. 删除我的账户", 350, DeleteCustomerAccount);
                AddButton("8. 用户登出", 390, LogoutCustomer);
                AddButton("0. 退出系统", 430, () => this.Close());
            }
            else if (_loggedInAdmin != null)
            {
                Label lblAdmin = new Label()
                {
                    Text = $"当前管理员: {_loggedInAdmin.AdminName} (ID: {_loggedInAdmin.AdminID})",
                    Location = new Point(30, 70),
                    AutoSize = true
                };
                this.Controls.Add(lblAdmin);

                AddButton("1. 电影管理 (增/改)", 110, ManageFilmsMenu);
                AddButton("2. 查看所有订单", 160, ViewAllOrders);
                AddButton("3. 添加新排片", 210, AddSectionInteractive);
                AddButton("4. 排片和座位图可视化", 260, ShowCinemaScheduleAndSeatMap);
                AddButton("5. 删除排片", 310, DeleteSectionInteractive);
                AddButton("6. 管理员登出", 360, LogoutAdministrator);
                AddButton("0. 退出系统", 410, () => this.Close());
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
        /// <summary>
        /// 登录与账户信息
        /// </summary>
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




        //查看电影排挡
        private void FilmDashBoard()
        {
            using (var form = new FilmDashboard())
            {
                form.ShowDialog();
            }
        }


        private void ShowCinemaScheduleAndSeatMap()
        {
            var form = new SeatMapVisualizationForm(cinemaapp.Program._schedulingService);
            form.ShowDialog();
        }






        //购票
        private void PurchaseTicketMenu()
        {
            MessageBox.Show("购票 - 功能未实现");
        }







        //电影管理
        private void ManageFilmsMenu()
        {
            MessageBox.Show("电影管理 - 功能未实现");
        }

        //查看订单
        private void ViewAllOrders()
        {
            var form = new ViewAllOrders();
            form.ShowDialog();
        }








        // 4. 查看所有有效订单
        private void DisplayCustomerPaidOrders()
        {
            MessageBox.Show("查看所有有效订单 - 功能未实现");
        }

        // 5. 退票
        private void ProcessTicketRefund()
        {
            MessageBox.Show("退票 - 功能未实现");
        }









        // 2. 查看所有订单（已实现或正在实现）

        // 3. 添加新排片
        private void AddSectionInteractive()
        {
            MessageBox.Show("添加新排片 - 功能未实现");
        }

        // 4. 查看排片
        private void ViewSectionsInteractive()
        {
            MessageBox.Show("查看排片 - 功能未实现");
        }

        // 5. 删除排片
        private void DeleteSectionInteractive()
        {
            MessageBox.Show("删除排片 - 功能未实现");
        }






    }
}
