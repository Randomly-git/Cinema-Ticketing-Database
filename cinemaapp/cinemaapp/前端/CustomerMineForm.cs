using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Label lblTitle = new Label()
            {
                Text = "我的账户",
                Font = new Font("微软雅黑", 16, FontStyle.Bold),
                Location = new Point(320, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // 添加按钮
            AddButton("更新个人资料", 70, UpdateCustomerProfile);
            AddButton("我的所有有效订单", 120, ViewAllOrders);
            AddButton("查看用户画像", 170, ViewCustomerProfile);
        }

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

        // 更新个人资料
        private void UpdateCustomerProfile()
        {
            using (var form = new UpdateCustomerProfile(_loggedInCustomer))
            {
                form.ShowDialog();
            }
        }

        // 查看所有有效订单
        private void ViewAllOrders()
        {
            var form = new CustomerOrdersForm(_loggedInCustomer, Program._orderRepository, Program._orderForProductRepository);
            form.ShowDialog();
        }

        // 查看用户画像
        private void ViewCustomerProfile()
        {
            var form = new CustomerProfileForm(_loggedInCustomer, Program._ratingService);
            form.ShowDialog();
        }











    }
}
