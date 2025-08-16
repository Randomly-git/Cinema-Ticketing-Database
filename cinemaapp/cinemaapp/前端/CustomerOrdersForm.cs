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
using test.Repositories;

namespace cinemaapp
{
    public class CustomerOrdersForm : Form
    {
        private readonly Customer _loggedInCustomer;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderForProductRepository _orderForProductRepository;

        private DataGridView dgvTicketOrders;
        private DataGridView dgvProductOrders;

        public CustomerOrdersForm(Customer loggedInCustomer,
                                  IOrderRepository orderRepository,
                                  IOrderForProductRepository orderForProductRepository)
        {
            _loggedInCustomer = loggedInCustomer ?? throw new ArgumentNullException(nameof(loggedInCustomer));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _orderForProductRepository = orderForProductRepository ?? throw new ArgumentNullException(nameof(orderForProductRepository));

            InitializeUI();
            LoadCustomerOrders();
        }

        private void InitializeUI()
        {
            this.Text = "我的订单";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var tabControl = new TabControl { Dock = DockStyle.Fill };

            // === 电影票订单页面 ===
            var tabTicketOrders = new TabPage("电影票订单");
            dgvTicketOrders = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            tabTicketOrders.Controls.Add(dgvTicketOrders);
            tabControl.TabPages.Add(tabTicketOrders);

            // === 周边产品订单页面 ===
            var tabProductOrders = new TabPage("周边产品订单");
            dgvProductOrders = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            tabProductOrders.Controls.Add(dgvProductOrders);
            tabControl.TabPages.Add(tabProductOrders);

            this.Controls.Add(tabControl);
        }

        private void LoadCustomerOrders()
        {
            try
            {
                var paidTicketOrders = _orderRepository.GetOrdersForCustomer(_loggedInCustomer.CustomerID, true)
                                                       .Where(o => o.State == "有效")
                                                       .OrderByDescending(o => o.Day)
                                                       .Select(o => new
                                                       {
                                                           o.OrderID,
                                                           订单日期 = o.Day.ToString("yyyy-MM-dd HH:mm"),
                                                           o.TicketID,
                                                           支付方式 = o.PaymentMethod,
                                                           金额 = o.TotalPrice,
                                                           状态 = o.State
                                                       })
                                                       .ToList();

                dgvTicketOrders.DataSource = paidTicketOrders;

                var paidProductOrders = _orderForProductRepository.GetOrdersByCustomerId(_loggedInCustomer.CustomerID)
                                                                  .Where(o => o.State == "已完成")
                                                                  .OrderByDescending(o => o.Day)
                                                                  .Select(o => new
                                                                  {
                                                                      o.OrderID,
                                                                      订单日期 = o.Day.ToString("yyyy-MM-dd HH:mm"),
                                                                      产品名称 = o.ProductName,
                                                                      购买数量 = o.PurchaseNum,
                                                                      支付方式 = o.PMethod,
                                                                      金额 = o.Price * o.PurchaseNum,
                                                                      状态 = o.State
                                                                  })
                                                                  .ToList();

                dgvProductOrders.DataSource = paidProductOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载订单失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
