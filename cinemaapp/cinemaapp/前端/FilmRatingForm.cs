using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;

namespace cinemaapp
{
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
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
        }

        private void LoadOrders()
        {
            var finishedOrders = _orderRepository
                .GetOrdersForCustomer(_customer.CustomerID, true)
                .Where(o => o.State == "有效")
                .Select(o => new
                {
                    o.OrderID,
                    FilmName = _ratingService.GetFilmNamebyOrderId(o.OrderID),
                    Rated = _ratingService.HasRated(o.OrderID) ? "已评分" : "未评分"
                })
                .ToList();

            dgvOrders.DataSource = finishedOrders;

            // 每次加载后绑定选择改变事件，更新按钮文字
            dgvOrders.SelectionChanged -= DgvOrders_SelectionChanged;
            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;
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
        }


        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要评价的电影", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var orderId = (int)dgvOrders.SelectedRows[0].Cells["OrderID"].Value;
            var score = (int)nudScore.Value;
            var comment = txtComment.Text.Trim();

            try
            {
                _ratingService.RateOrder(orderId, score, comment);
                MessageBox.Show(_ratingService.HasRated(orderId) ? "评论已修改！" : "评分成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 更新当前行的 Rated 列
                dgvOrders.SelectedRows[0].Cells["Rated"].Value = "已评分";

                // 更新按钮文字
                DgvOrders_SelectionChanged(null, null);
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

            var orderId = (int)dgvOrders.SelectedRows[0].Cells["OrderID"].Value;

            try
            {
                if (_ratingService.HasRated(orderId))
                {
                    _ratingService.CancelRating(orderId);
                    MessageBox.Show("评分已撤销！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadOrders();
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


