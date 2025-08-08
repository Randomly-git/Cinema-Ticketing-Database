using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;

namespace cinemaapp
{
    public partial class ViewAllOrders : Form
    {
        private DataGridView dgvOrders;
        private Button btnClose;

        public ViewAllOrders()
        {
            BuildUI();
            LoadOrders();
        }

        private void BuildUI()
        {
            this.Text = "查看所有订单";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            dgvOrders = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(740, 370),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnClose = new Button
            {
                Text = "关闭",
                Location = new Point(350, 410),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(dgvOrders);
            this.Controls.Add(btnClose);
        }

        private void LoadOrders()
        {
            try
            {
                List<OrderForTickets> orders = Program._adminService.GetAllOrders();
                if (orders == null || orders.Count == 0)
                {
                    MessageBox.Show("当前没有订单记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var sortedOrders = orders.OrderByDescending(o => o.Day).ThenByDescending(o => o.OrderID).ToList();

                dgvOrders.DataSource = sortedOrders.Select(o => new
                {
                    订单编号 = o.OrderID,
                    票号 = o.TicketID,
                    顾客ID = o.CustomerID,
                    状态 = o.State,
                    金额 = o.TotalPrice,
                    日期 = o.Day.ToShortDateString(),
                    支付方式 = o.PaymentMethod
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
