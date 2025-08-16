
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;

namespace cinemaapp
{
    public partial class MainForm : Form
    {
        private Customer _loggedInCustomer = null;
        private Administrator _loggedInAdmin = null;
        private Label lblUser;  // 提升为类字段

        public MainForm()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "电影院管理系统";
            this.Size = new Size(500, 600);
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
                lblUser = new Label()
                {
                    Location = new Point(30, 70),
                    AutoSize = true
                };
                this.Controls.Add(lblUser);

                UpdateUserInfoLabel(); // 初始化显示

                AddButton("更新个人资料", 110, UpdateCustomerProfile);
                AddButton("查看电影相关信息", 150, FilmDashBoard);
                AddButton("购票", 190, PurchaseTicketMenu);
                AddButton("我的所有有效订单", 230, DisplayCustomerPaidOrders);
                AddButton("我的电影票", 270, ProcessTicketRefund);
                AddButton("购买周边", 310, PurchaseProduct);
                AddButton("评价电影", 350, RateFilm);  // 新增评价电影按钮
                AddButton("查看用户画像", 390, ViewCustomerProfile);
                AddButton("删除我的账户", 430, DeleteCustomerAccount);
                AddButton("用户登出", 470, LogoutCustomer);
                AddButton("退出系统", 510, () => this.Close());
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

                AddButton("电影管理 (增/改)", 110, ManageFilmsMenu);
                AddButton("查看所有订单", 160, ViewAllOrders);
                AddButton("添加新排片", 210, AddSectionInteractive);
                AddButton("添加新的周边产品", 260, AddProducts);
                AddButton("排片和座位图可视化", 310, ShowCinemaScheduleAndSeatMap);
                AddButton("查看电影评价", 360, ShowFilmRatings);
                AddButton("管理员登出", 410, LogoutAdministrator);
                AddButton("退出系统", 460, () => this.Close());
            }
        }

        // 新增：实时刷新用户信息
        public void UpdateUserInfoLabel()
        {
            if (_loggedInCustomer == null) return;

            // First update the membership level based on current points
            Program._userService.UpdateMembershipLevel(_loggedInCustomer.CustomerID);

            // Then get the updated customer information
            var customer = Program._customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
            var vipCard = Program._customerRepository.GetVIPCardByCustomerID(_loggedInCustomer.CustomerID);
            int points = vipCard?.Points ?? 0;

            lblUser.Text = $"当前用户: {customer.Name} (ID: {customer.CustomerID}, 等级: {customer.VipLevel}, 积分: {points})";
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


        private void ShowFilmRatings()
        {
            // 调用管理员电影评价窗体
            var ratingForm = new AdminFilmRatingForm(Program._filmService, Program._ratingService);
            ratingForm.ShowDialog(); // 弹窗显示
        }

        private void ManageFilmsMenu()
        {
            // 创建并显示电影管理窗体
            var filmManagementForm = new FilmManagementForm(Program._adminService, Program._filmService);
            filmManagementForm.ShowDialog(); // 使用 ShowDialog 使窗体模态显示
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
            var form = new SeatMapVisualizationForm(
                cinemaapp.Program._schedulingService,
                cinemaapp.Program._showingService 
            );
            form.ShowDialog();
        }



        private void ViewCustomerProfile()
        {
            if (_loggedInCustomer == null)
            {
                MessageBox.Show("请先登录才能查看用户画像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form = new CustomerProfileForm(_loggedInCustomer, Program._ratingService);
            form.ShowDialog();
        }




        //购票
        private void PurchaseTicketMenu()
        {
            var form = new FilmSelectionForm(Program._filmService, Program._loggedInCustomer,this);
            form.ShowDialog(); // 模态窗口，用户必须先操作完这个窗体才能回主界面
        }

        private void PurchaseProduct()
        {
            var form = new ProductPurchaseForm(Program._productService, Program._customerRepository ,Program._loggedInCustomer, this);
            form.ShowDialog(); // 模态窗口，用户必须先操作完这个窗体才能回主界面
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
            if (_loggedInCustomer == null)
            {
                MessageBox.Show("请先登录才能查看订单。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; 
            }

            var form = new CustomerOrdersForm(_loggedInCustomer, Program._orderRepository, Program._orderForProductRepository);
            form.ShowDialog(); // 或 Show()，看你的窗口逻辑
        }


        // 5. 退票

        private void ProcessTicketRefund()
        {
            try
            {
                // 1. 验证用户登录状态
                if (_loggedInCustomer == null)
                {
                    MessageBox.Show("请先登录才能退票", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. 验证服务实例
                if (Program._orderRepository == null || Program._bookingService == null)
                {
                    MessageBox.Show("系统服务未初始化，无法办理退票", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. 创建并显示退票窗体
                using (var refundForm = new TicketRefundForm(
                    _loggedInCustomer,
                    Program._orderRepository,
                    Program._bookingService,
                    Program._ticketService))
                {
                    // 4. 处理窗体结果
                    var result = refundForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // 可添加退票成功后的处理逻辑
                        MessageBox.Show("退票操作已完成", "成功",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // 5. 捕获全局异常
                MessageBox.Show($"退票过程中发生错误：{ex.Message}", "系统错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 记录日志（实际项目中应使用日志框架）
                Debug.WriteLine($"退票错误：{ex.ToString()}");
            }
        }



        private void RateFilm()
        {
            if (_loggedInCustomer == null)
            {
                MessageBox.Show("请先登录才能评价电影", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new FilmRatingForm(
                _loggedInCustomer,
                Program._orderRepository,
                Program._ratingService))
            {
                form.ShowDialog();
            }
        }





        // 2. 查看所有订单（已实现或正在实现）

        // 3. 添加新排片
        private void AddSectionInteractive()
        {
            using (var form = new ScheduleForm(Program._schedulingService)) // ScheduleOptionsForm 就是我帮你写的卡片窗体类
            {
                form.ShowDialog(this); // 模态方式打开
            }
        }

        // 6. 添加新的周边产品
        private void AddProducts()
        {
            var form = new AddProductForm();
            form.ShowDialog();
        }

    }
}
