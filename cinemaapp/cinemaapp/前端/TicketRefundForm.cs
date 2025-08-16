using cinemaapp.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;

namespace cinemaapp
{
    public partial class TicketRefundForm : Form
    {
        private readonly Customer _loggedInCustomer;
        private readonly IOrderRepository _orderRepository;
        private readonly IBookingService _bookingService;
        private readonly ITicketService _ticketService;

        public TicketRefundForm(Customer loggedInCustomer,
                                IOrderRepository orderRepository,
                                IBookingService bookingService,
                                ITicketService ticketService) // 新增参数
        {
            InitializeComponent();
            _loggedInCustomer = loggedInCustomer;
            _orderRepository = orderRepository;
            _bookingService = bookingService;
            _ticketService = ticketService; // 初始化
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "电影票退票";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(600, 450);

            // 添加DataGridView显示订单
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 添加按钮
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            var btnRefund = new Button { Text = "退票", Dock = DockStyle.Right, Width = 100 };
            var btnCancel = new Button { Text = "取消", Dock = DockStyle.Right, Width = 100 };

            btnRefund.Click += BtnRefund_Click;
            btnCancel.Click += (s, e) => this.Close();
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;

            panel.Controls.Add(btnRefund);
            panel.Controls.Add(btnCancel);
            this.Controls.Add(dataGridView1);
            this.Controls.Add(panel);

            LoadOrders();
        }

        private void LoadOrders()
        {
            // 1. 优先检查控件是否存在
            if (dataGridView1 == null || dataGridView1.IsDisposed)
            {
                MessageBox.Show("界面初始化失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 2. 严格空值检查（使用Debug.Assert辅助调试）
                Debug.Assert(_loggedInCustomer != null, "用户未登录");
                Debug.Assert(_orderRepository != null, "订单服务未初始化");

                if (_loggedInCustomer == null || _orderRepository == null)
                {
                    MessageBox.Show("系统未正确初始化", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. 安全获取数据
                var validOrders = _orderRepository.GetOrdersForCustomer(
                    _loggedInCustomer.CustomerID,
                    onlyValid: true) ?? Enumerable.Empty<OrderForTickets>(); // 确保不为null

                // 4. 检查有效数据
                if (!validOrders.Any())
                {
                    MessageBox.Show("没有可退款的订单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 5. 安全数据绑定
                this.SafeDataBind(validOrders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载订单失败: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SafeDataBind(IEnumerable<OrderForTickets> orders)
        {
            // 1. 双重检查控件状态
            if (this.IsDisposed || dataGridView1 == null || dataGridView1.IsDisposed)
                return;

            // 2. 确保在 UI 线程执行
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.BeginInvoke(new Action(() => SafeDataBind(orders)));
                return;
            }

            // 3. 转换绑定数据
            var displayData = orders.Select((order, index) => new
            {
                序号 = index + 1,
                订单ID = order.OrderID,
                电影票ID = order.TicketID,
                订单日期 = order.Day.ToString("yyyy-MM-dd"),
                支付金额 = order.TotalPrice.ToString("C")
            }).ToList();

            // 4. 清空并设置手动列
            dataGridView1.SuspendLayout();
            dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = false;

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "序号列",
                HeaderText = "序号",
                DataPropertyName = "序号",
                Width = 50,
                ReadOnly = true
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "订单ID列",
                HeaderText = "订单ID",
                DataPropertyName = "订单ID",
                Width = 100,
                ReadOnly = true
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "电影票ID列",
                HeaderText = "电影票ID",
                DataPropertyName = "电影票ID",
                Width = 100,
                ReadOnly = true
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "订单日期列",
                HeaderText = "订单日期",
                DataPropertyName = "订单日期",
                Width = 120,
                ReadOnly = true
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "支付金额列",
                HeaderText = "支付金额",
                DataPropertyName = "支付金额",
                Width = 100,
                ReadOnly = true
            });

            // 5. 设置数据源
            dataGridView1.DataSource = displayData;
            dataGridView1.ResumeLayout();
        }


        private void BtnRefund_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要退票的订单", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedOrderId = (int)dataGridView1.SelectedRows[0].Cells["订单ID列"].Value;

            // ✳️ 第一步：尝试计算手续费和退款金额
            if (!_bookingService.TryGetRefundInfo(
                selectedOrderId,
                DateTime.Now,
                out decimal fee,
                out decimal refundAmount,
                out string errMsg))
            {
                MessageBox.Show(errMsg, "无法退票", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✳️ 第二步：提示用户详细的退款信息
            string confirmMsg = $"确定要退票吗？\n\n" +
                                $"票价: {refundAmount + fee} 元\n" +
                                $"手续费: {fee:F2} 元\n" +
                                $"实际可退: {refundAmount} 元";

            if (MessageBox.Show(confirmMsg, "确认退票", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            // ✳️ 第三步：正式调用 RefundTicket 执行退票
            try
            {
                decimal refundFee;
                decimal actualRefundAmount;

                bool success = _bookingService.RefundTicket(
                    selectedOrderId,
                    DateTime.Now,
                    out refundFee,
                    out actualRefundAmount,
                    out string errorMsg);

                if (success)
                {
                    string message = $"退票成功！\n\n";

                    MessageBox.Show(message, "退票成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadOrders(); // 刷新订单列表
                }
                else
                {
                    MessageBox.Show("退票失败：" + errorMsg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"退票过程中发生错误: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dataGridView1.Rows[e.RowIndex];
            string ticketId = row.Cells["电影票ID列"].Value?.ToString();

            if (string.IsNullOrEmpty(ticketId)) return;

            var ticket = _ticketService.GetTicketWithSection(ticketId);
            if (ticket == null || ticket.Section == null)
            {
                MessageBox.Show("未找到该票的详细排片信息", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sec = ticket.Section;
            string msg = $"🎬 电影：{sec.FilmName}\n" +
                         $"🏟️ 影厅：{sec.HallNo}（{sec.HallCategory}）\n" +
                         $"🕒 时间：{sec.ScheduleStartTime:yyyy-MM-dd HH:mm} ~ {sec.ScheduleEndTime:HH:mm}\n" +
                         $"📍 座位：{ticket.LineNo}排{ticket.ColumnNo}座\n" +
                         $"💰 价格：{ticket.Price:C}\n";

            MessageBox.Show(msg, "排片详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



    }
}
