using cinemaapp.前端;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;
using System.IO;

namespace cinemaapp
{
    public partial class MainForm : Form
    {
        private FlowLayoutPanel posterStrip; // 横向海报条
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
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            RefreshMenu();
        }

        // 根据当前登录状态刷新界面显示
        private void RefreshMenu()
        {
            this.Controls.Clear();


            LoadExitSystemImage(); // 加载退出按钮

            if (_loggedInCustomer == null && _loggedInAdmin == null)
            {
                AddButton("顾客注册", 70, RegisterCustomer);
                AddButton("顾客登录", 120, LoginCustomer);
                AddButton("管理员注册", 170, RegisterAdministrator);
                AddButton("管理员登录", 220, LoginAdministrator);
                //AddButton("退出系统", 270, () => this.Close());
            }
            else if (_loggedInCustomer != null)
            {
                lblUser = new Label()
                {
                    Location = new Point(10, 120),
                    AutoSize = true
                };
                this.Controls.Add(lblUser);

                UpdateUserInfoLabel(); // 初始化显示

                AddButtonCustomer("我的", 10, 150, OpenCustomerMineForm);
                AddButtonCustomer("更多电影信息", 200, 550, FilmDashBoard);
                AddButtonCustomer("我的电影票", 350, 550, ProcessTicketRefund);
                AddButtonCustomer("购买周边", 500, 550, PurchaseProduct);
                AddButtonCustomer("评价电影", 650, 550, RateFilm);  // 新增评价电影按钮
                AddButtonCustomer("删除我的账户", 800, 550, DeleteCustomerAccount);
                AddButtonCustomer("用户登出", 950,550, LogoutCustomer);
                //AddButtonCustomer("购票", 700, 600, PurchaseTicketMenu);
                //AddButtonCustomer("更新个人资料", 110, UpdateCustomerProfile);
                //AddButtonCustomer("我的所有有效订单", 230, DisplayCustomerPaidOrders);
                //AddButtonCustomer("查看用户画像", 390, ViewCustomerProfile);
                
                //AddButton("退出系统", 510, () => this.Close());

                // 加载头像图片
                LoadCustomerAvatar();
                AddHorizontalSeparator(170, 0, 700, Color.Black);

                //  创建并添加海报条
                posterStrip = BuildPosterStrip();//创建 posterStrip（注意：每次 RefreshMenu() 都清空控件，所以要重建）
                this.Controls.Add(posterStrip);// 添加到页面（这样滚动条会按实际宽度计算）

                //准备海报清单（片名 + 文件名）
                var films = new (string name, string fileName)[]
        {
            ("霸王别姬", "霸王别姬.jpg"),
            ("肖申克的救赎", "肖申克的救赎.jpg"),
            ("阿甘正传", "阿甘正传.jpg"),
            ("千与千寻", "千与千寻.jpg"),
            ("星际穿越", "星际穿越.jpg"),
            ("泰坦尼克号", "泰坦尼克号.jpg"),
            ("美丽人生", "美丽人生.jpg"),
            ("这个杀手不太冷", "这个杀手不太冷.jpg"),
            ("盗梦空间", "盗梦空间.jpg"),
            ("楚门的世界", "楚门的世界.jpg"),
        };

                // 4) 逐个添加到海报条（使用 images 目录）
                foreach (var f in films)
                {
                    string avatarPath = Path.Combine(Application.StartupPath, "images", f.fileName);
                    AddPosterItem(posterStrip, f.name, avatarPath);
                }




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
                //AddButton("退出系统", 460, () => this.Close());
            }


        }

        // 新增：实时刷新用户信息
        public void UpdateUserInfoLabel()
        {
            if (_loggedInCustomer == null) return;

            // 首先根据当前积分更新会员等级（数据库层面）
            Program._userService.UpdateMembershipLevel(_loggedInCustomer.CustomerID);

            // 获取更新后的客户信息
            var customer = Program._customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
            var vipCard = Program._customerRepository.GetVIPCardByCustomerID(_loggedInCustomer.CustomerID);
            int points = vipCard?.Points ?? 0;

            // 更新当前登录用户对象的属性
            _loggedInCustomer.VipLevel = customer.VipLevel;
            if (_loggedInCustomer.VIPCard != null)
            {
                _loggedInCustomer.VIPCard.Points = points;
            }

            // 更新UI显示
            lblUser.Text = $"Hi, {customer.Name} !  Lv: {customer.VipLevel} ";
            lblUser.Font = new Font("微软雅黑", 12, FontStyle.Regular);
        }

        private void AddButton(string text, int top, Action onClick)
        {
            var btn = new Button()
            {
                Text = text,
                Location = new Point(520, top),
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


        private void AddButtonCustomer(string text, int length, int top, Action onClick)
        {
            var btn = new Button()
            {
                Text = text,
                Location = new Point(length, top),
                Size = new Size(100, 50),
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(76, 175, 80),   // 绿色主题
                ForeColor = Color.White,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(67, 160, 71);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(56, 142, 60);

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

        //购票
        private void PurchaseTicketMenu()
        {
            var form = new FilmSelectionForm(Program._filmService, Program._loggedInCustomer, this);
            form.ShowDialog(); // 模态窗口，用户必须先操作完这个窗体才能回主界面
        }

        private void PurchaseProduct()
        {
            var form = new ProductPurchaseForm(Program._productService, Program._customerRepository, Program._loggedInCustomer, this);
            form.ShowDialog(); // 模态窗口，用户必须先操作完这个窗体才能回主界面
        }







        //查看订单
        private void ViewAllOrders()
        {
            var form = new ViewAllOrders();
            form.ShowDialog();
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

        // 打开 CustomerMineForm 窗体
        private void OpenCustomerMineForm()
        {
            var customerMineForm = new CustomerMineForm(_loggedInCustomer);
            customerMineForm.ShowDialog();
            RefreshMenu();
        }


        // 加载并显示顾客头像
        private void LoadCustomerAvatar()
        {
            var avatarPictureBox = new PictureBox
            {
                Width = 100, // 设置宽度
                Height = 100, // 设置高度
                Location = new Point(10, 10), // 设置图片位置：左上角
                SizeMode = PictureBoxSizeMode.StretchImage, // 设置图片拉伸方式，填充控件区域
                //BorderStyle = BorderStyle.None // 可选，去掉边框
            };

            string avatarPath = Path.Combine(Application.StartupPath, "images", "#顾客头像.png");// 构建图片路径

            if (File.Exists(avatarPath))
            {
                avatarPictureBox.Image = Image.FromFile(avatarPath); // 加载图片
            }
            else
            {
                // 如果图片不存在，显示默认的头像或其他默认图片
                avatarPictureBox.Image = CreateDefaultAvatar();
            }

            // 将头像图片控件添加到窗体中
            this.Controls.Add(avatarPictureBox);
        }
        private Image CreateDefaultAvatar()
        {

            var img = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(img))
            {
                g.Clear(Color.Gray); // 设置背景颜色
                using (var font = new Font("Arial", 12))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString("无头像", font, brush, new PointF(10, 10));
                }
            }
            return img;
        }

        private void LoadExitSystemImage()
        {
            var exitPictureBox = new PictureBox
            {
                Width = 50, // 设置图片宽度
                Height = 50, // 设置图片高度
                Location = new Point(10, this.ClientSize.Height - 60), // 设置图片位置：左下角
                SizeMode = PictureBoxSizeMode.StretchImage, // 设置图片拉伸方式，填充控件区域
                BorderStyle = BorderStyle.None // 可选，去掉边框
            };

            string exitImagePath = Path.Combine(Application.StartupPath, "images", "#退出系统标识.png");

            if (File.Exists(exitImagePath))
            {
                exitPictureBox.Image = Image.FromFile(exitImagePath); // 加载图片
            }
            else
            {
                // 如果没有图片，使用默认图片或创建一个默认图片
                exitPictureBox.Image = CreateDefaultExitImage();
            }

            // 添加点击事件，点击图片时关闭窗体
            exitPictureBox.Click += (sender, e) => this.Close();

            // 将图片控件添加到窗体
            this.Controls.Add(exitPictureBox);
        }

        private Image CreateDefaultExitImage()
        {
            // 创建一个默认的退出图标（当图片加载失败时）
            var img = new Bitmap(50, 50);
            using (var g = Graphics.FromImage(img))
            {
                g.Clear(Color.Gray); // 设置背景颜色
                using (var font = new Font("Arial", 12))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString("退出", font, brush, new PointF(10, 10)); // 在图片上绘制文字
                }
            }
            return img;
        }

        //海报展示
        private FlowLayoutPanel BuildPosterStrip()
        {
            var flp = new FlowLayoutPanel
            {
                WrapContents = false,              // 单行横向
                FlowDirection = FlowDirection.LeftToRight, // 左→右
                AutoScroll = true,                 // 超出显示水平滚动条
                Dock = DockStyle.None,
                Location = new Point(200, 40),
                Size = new Size(this.ClientSize.Width - 40, 450), // 设置大小：预留两边空白（40px）// 150(海报) + 30(片名) + 上下余量
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            return flp;
        }

        // ——————————— 添加一个海报项 ———————————
        private void AddPosterItem(FlowLayoutPanel target, string filmName, string absoluteImagePath)
        {
            var itemPanel = new Panel
            {
                Width = 300,
                Height = 400,
                Margin = new Padding(10) // 相邻项水平总空隙 20px（左右各10）
            };

            var pic = new PictureBox
            {
                Width = 280,
                Height = 350,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                Image = LoadPosterThumbnail(absoluteImagePath, 280, 350)
            };
            // 为海报添加点击事件
            pic.Click += (s, e) =>
            {
                // 确保用户已登录，否则无法查看电影详情
                if (_loggedInCustomer == null)
                {
                    MessageBox.Show("请先登录才能查看电影详情！", "未登录", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var film = Program._filmService.GetAvailableFilms().FirstOrDefault(f => f.FilmName == filmName);
                if (film == null)
                {
                    MessageBox.Show("未找到该电影的信息。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var detailForm = new FilmDetailForm(film, _loggedInCustomer, this);
                detailForm.ShowDialog();
            };

            var name = new Label
            {
                Text = string.IsNullOrWhiteSpace(filmName) ? "未命名" : filmName,
                Width = 280,
                Height = 40,
                AutoSize = false,
                Location = new Point(10, 360),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微软雅黑", 9, FontStyle.Bold)
            };

            itemPanel.Controls.Add(pic);
            itemPanel.Controls.Add(name);
            target.Controls.Add(itemPanel);
        }

        // ——————————— 加载缩略图（本地路径 → 130×150） ———————————
        private Image LoadPosterThumbnail(string absolutePath, int w, int h)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(absolutePath) && File.Exists(absolutePath))
                {
                    using var original = Image.FromFile(absolutePath);
                    return ResizeImageHighQuality(original, w, h);
                }
            }
            catch { /* 忽略，走默认图或占位 */ }

            // 默认图：{StartupPath}\images\默认.jpg
            try
            {
                string fallback = Path.Combine(Application.StartupPath, "images", "默认.jpg");
                if (File.Exists(fallback))
                {
                    using var def = Image.FromFile(fallback);
                    return ResizeImageHighQuality(def, w, h);
                }
            }
            catch { }

            // 灰色占位
            var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.LightGray);
            using var font = new Font("Arial", 10);
            using var brush = new SolidBrush(Color.Black);
            var text = "暂无海报";
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, brush, (w - size.Width) / 2f, (h - size.Height) / 2f);
            return bmp;
        }

        // ——————————— 高质量缩放 ———————————
        private static Image ResizeImageHighQuality(Image original, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using var wrapMode = new System.Drawing.Imaging.ImageAttributes();
                wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height,
                  GraphicsUnit.Pixel, wrapMode);
            }
            return destImage;
        }

        private void AddHorizontalSeparator(int x, int y, int hight, Color color)
        {
            var separator = new Panel()
            {
                Height = hight,                      // 线的厚度
                Width = 2,                   // 线的长度
                BackColor = color,               // 线的颜色
                Location = new Point(x, y)       // 线的位置
            };
            this.Controls.Add(separator);
        }



        /* 以下功能更改为在CustomerMineForm中实现
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
        // 查看用户画像
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
*/


    }

}
