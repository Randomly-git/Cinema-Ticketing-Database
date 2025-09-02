using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;

namespace cinemaapp.前端
{
    public partial class CustomerMineForm : Form
    {
        private Customer _loggedInCustomer; // 当前登录的顾客对象

        public CustomerMineForm(Customer loggedInCustomer)
        {
            _loggedInCustomer = loggedInCustomer;
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "我的账户";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 标题
            Label lblTitle = new Label()
            {
                Text = "我的账户",
                Font = new Font("微软雅黑", 16, FontStyle.Bold),
                Location = new Point(320, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // 用户信息
            RefreshUserInfo();

            // 按钮区
            AddButton("更新个人资料", 170, UpdateCustomerProfile);
            AddButton("我的所有有效订单", 220, ViewAllOrders);
            AddButton("删除此账号", 270, DeleteCustomerAccount);
        }

        /// <summary>
        /// 刷新用户信息显示
        /// </summary>
        private void RefreshUserInfo()
        {
            // 移除旧的用户信息 Label
            var oldLabel = this.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblUserInfo");
            if (oldLabel != null)
                this.Controls.Remove(oldLabel);

            // 获取最新用户数据
            var customer = Program._customerRepository.GetCustomerById(_loggedInCustomer.CustomerID);
            var vipCard = Program._customerRepository.GetVIPCardByCustomerID(_loggedInCustomer.CustomerID);
            int points = vipCard?.Points ?? 0;

            // 创建新的 Label 显示用户信息
            Label lblUser = new Label()
            {
                Name = "lblUserInfo", // 必须设置 Name，方便刷新
                Location = new Point(320, 70),
                AutoSize = true,
                Text = $"name: {customer.Name}   Lv: {customer.VipLevel}\nID: {customer.CustomerID}, 当前积分: {points}",
                Font = new Font("微软雅黑", 12, FontStyle.Regular),
            };
            this.Controls.Add(lblUser);
        }

        /// <summary>
        /// 添加按钮
        /// </summary>
        private void AddButton(string text, int top, Action onClick)
        {
            var btn = new Button()
            {
                Text = text,
                Location = new Point(320, top),
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
        /// 更新个人资料
        /// </summary>
        private void UpdateCustomerProfile()
        {
            using (var form = new UpdateCustomerProfile(_loggedInCustomer))
            {
                form.ShowDialog();
            }

            // 更新完成后刷新用户信息
            RefreshUserInfo();
        }

        /// <summary>
        /// 查看所有有效订单
        /// </summary>
        private void ViewAllOrders()
        {
            using (var form = new CustomerOrdersForm(_loggedInCustomer, Program._orderRepository, Program._orderForProductRepository))
            {
                form.ShowDialog();
            }
        }

        /// <summary>
        /// 删除账户
        /// </summary>
        private void DeleteCustomerAccount()
        {
            if (Program._loggedInCustomer == null)
            {
                MessageBox.Show("请先登录再删除账户", "未登录", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 确认删除
            var confirmResult = MessageBox.Show("确定要删除账户吗？此操作不可恢复！",
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes)
                return;

            try
            {
                // 执行删除操作
                var deleteForm = new DeleteCustomerAccount();
                var result = deleteForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // 设置对话框结果并关闭当前窗体
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除账户失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
