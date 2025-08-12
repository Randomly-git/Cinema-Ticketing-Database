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
        private TabControl tabControl;
        private DataGridView dgvTicketOrders;
        private DataGridView dgvProductOrders;
        private Button btnClose;

        public ViewAllOrders()
        {
            BuildUI();
            LoadTicketOrders();
            LoadProductOrders();
        }

        private void BuildUI()
        {
            this.Text = "查看所有订单";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;

            tabControl = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 550
            };
            this.Controls.Add(tabControl);

            // Tab 1: 电影票订单
            var tabTickets = new TabPage("🎬 电影票订单");
            dgvTicketOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            tabTickets.Controls.Add(dgvTicketOrders);
            tabControl.TabPages.Add(tabTickets);

            // Tab 2: 周边商品订单
            var tabProducts = new TabPage("🛍 周边商品订单");
            dgvProductOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            tabProducts.Controls.Add(dgvProductOrders);
            tabControl.TabPages.Add(tabProducts);

            // 关闭按钮
            btnClose = new Button
            {
                Text = "关闭",
                Size = new Size(100, 30),
                Location = new Point((this.ClientSize.Width - 100) / 2, 560),
                Anchor = AnchorStyles.Bottom
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void LoadTicketOrders()
        {
            try
            {
                List<OrderForTickets> orders = Program._adminService.GetAllOrders();
                if (orders == null || orders.Count == 0)
                {
                    dgvTicketOrders.DataSource = null;
                    return;
                }

                var sortedOrders = orders.OrderByDescending(o => o.Day)
                                         .ThenByDescending(o => o.OrderID)
                                         .ToList();

                dgvTicketOrders.DataSource = sortedOrders.Select(o => new
                {
                    订单编号 = o.OrderID,
                    票号 = o.TicketID,
                    顾客ID = o.CustomerID,
                    状态 = o.State,
                    金额 = o.TotalPrice,
                    日期 = o.Day.ToString("yyyy-MM-dd HH:mm"),
                    支付方式 = o.PaymentMethod
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载电影票订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductOrders()
        {
            try
            {
                List<OrderForProduct> productOrders = Program._adminService.GetProductOrders();
                if (productOrders == null || productOrders.Count == 0)
                {
                    dgvProductOrders.DataSource = null;
                    return;
                }

                var sortedOrders = productOrders.OrderByDescending(o => o.Day)
                                                .ThenByDescending(o => o.OrderID)
                                                .ToList();

                dgvProductOrders.DataSource = sortedOrders.Select(o => new
                {
                    订单编号 = o.OrderID,
                    顾客ID = o.CustomerID,
                    商品名称 = o.ProductName,
                    购买数量 = o.PurchaseNum,
                    状态 = o.State,
                    单价 = o.Price,
                    总价 = o.Price * o.PurchaseNum,
                    日期 = o.Day.ToString("yyyy-MM-dd HH:mm"),
                    支付方式 = o.PMethod
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载周边订单失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
