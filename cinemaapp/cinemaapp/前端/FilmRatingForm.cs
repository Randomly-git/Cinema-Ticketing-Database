using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;

namespace cinemaapp
{
    public class OrderDisplay
    {
        public int OrderID { get; set; }
        public string FilmName { get; set; }
        public string Rated { get; set; }  // "已评分" / "未评分"
    }
    public class FilmRatingForm : Form
    {
        private readonly Customer _customer;
        private readonly IOrderRepository _orderRepository;
        private readonly IRatingService _ratingService;

        private DataGridView dgvOrders;
        private NumericUpDown nudScore;
        private TextBox txtComment;
        private Button btnSubmit;
        private Button btnCancelRating;

        public FilmRatingForm(Customer customer, IOrderRepository orderRepository, IRatingService ratingService)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));

            InitUI();
            LoadOrders();
        }

        private void InitUI()
        {
            this.Text = "评价电影";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // 订单列表
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 200,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false // ← 禁止自动新增行
            };
            this.Controls.Add(dgvOrders);

            // 评分输入
            Label lblScore = new Label { Text = "评分 (0-10):", Location = new Point(20, 220), AutoSize = true };
            this.Controls.Add(lblScore);

            nudScore = new NumericUpDown
            {
                Location = new Point(100, 215),
                Minimum = 0,
                Maximum = 10,
                DecimalPlaces = 0
            };
            this.Controls.Add(nudScore);

            // 评论输入
            Label lblComment = new Label { Text = "评论 (最多200字):", Location = new Point(20, 260), AutoSize = true };
            this.Controls.Add(lblComment);

            txtComment = new TextBox
            {
                Location = new Point(20, 285),
                Size = new Size(640, 100),
                Multiline = true,
                MaxLength = 200
            };
            this.Controls.Add(txtComment);

            // 提交按钮
            btnSubmit = new Button
            {
                Text = "提交评分",
                Location = new Point(20, 400),
                Size = new Size(100, 30)
            };
            btnSubmit.Click += BtnSubmit_Click;
            this.Controls.Add(btnSubmit);

            // 撤销评分按钮
            btnCancelRating = new Button
            {
                Text = "撤销评分",
                Location = new Point(140, 400),
                Size = new Size(100, 30)
            };
            btnCancelRating.Click += BtnCancelRating_Click;
            this.Controls.Add(btnCancelRating);

            // 添加DataGridView选择改变事件
            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;
        }

        private BindingList<OrderDisplay> finishedOrders; // 类成员

        private void LoadOrders()
        {
            var validOrders = _orderRepository
                .GetOrdersForCustomer(_customer.CustomerID, true)
                .ToList();

            finishedOrders = new BindingList<OrderDisplay>();

            foreach (var order in validOrders)
            {
                var ticket = _orderRepository.GetTicketById(order.TicketID);
                if (ticket == null) continue;

                var section = _orderRepository.GetSectionById(ticket.SectionID);
                if (section?.TimeSlot == null) continue;

                if (DateTime.Now >= section.TimeSlot.EndTime)
                {
                    finishedOrders.Add(new OrderDisplay
                    {
                        OrderID = order.OrderID,
                        FilmName = section.FilmName,
                        Rated = _ratingService.HasRated(order.OrderID) ? "已评分" : "未评分"
                    });
                }
            }

            dgvOrders.DataSource = finishedOrders;

            nudScore.Value = 0;
            txtComment.Text = string.Empty;
        }


        private void DgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                btnSubmit.Text = "提交评分";
                btnCancelRating.Enabled = false;
                return;
            }

            var orderId = (int)dgvOrders.SelectedRows[0].Cells["OrderID"].Value;
            bool hasRated = _ratingService.HasRated(orderId);

            btnSubmit.Text = hasRated ? "修改评论" : "提交评分";
            btnCancelRating.Enabled = hasRated;

            // 获取并显示评分和评论
            if (hasRated)
            {
                var rating = _ratingService.GetRating(orderId);
                nudScore.Value = rating.Score;
                txtComment.Text = rating.Comment;
            }
            else
            {
                nudScore.Value = 0;
                txtComment.Text = string.Empty;
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要评价的电影", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dgvOrders.SelectedRows[0];
            var orderDisplay = (OrderDisplay)selectedRow.DataBoundItem;  // 获取绑定对象

            var orderId = orderDisplay.OrderID;
            var score = (int)nudScore.Value;
            var comment = txtComment.Text.Trim();
            var filmName = orderDisplay.FilmName;

            bool wasRated = _ratingService.HasRated(orderId);

            try
            {
                if (wasRated)
                {
                    _ratingService.CancelRating(orderId, filmName);
                }

                _ratingService.RateOrder(orderId, filmName, score, comment);

                // 修改绑定对象属性，UI 会自动刷新
                orderDisplay.Rated = "已评分";
                btnSubmit.Text = "修改评论";
                btnCancelRating.Enabled = true;

                MessageBox.Show(wasRated ? "评论已修改！" : "评分成功！", "成功",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void BtnCancelRating_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要撤销的电影评分", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dgvOrders.SelectedRows[0];
            var orderDisplay = (OrderDisplay)selectedRow.DataBoundItem; // 获取绑定对象

            var orderId = orderDisplay.OrderID;
            var filmName = orderDisplay.FilmName;

            try
            {
                if (_ratingService.HasRated(orderId))
                {
                    _ratingService.CancelRating(orderId, filmName);

                    // 修改绑定对象属性，UI 自动刷新
                    orderDisplay.Rated = "未评分";
                    btnSubmit.Text = "提交评分";
                    btnCancelRating.Enabled = false;
                    nudScore.Value = 0;
                    txtComment.Text = string.Empty;

                    MessageBox.Show("评分已撤销！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("该电影尚未评分，无法撤销", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("撤销评分失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
