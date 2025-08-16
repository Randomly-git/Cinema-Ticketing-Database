using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public class CustomerProfileForm : Form
    {
        private readonly Customer _customer;
        private readonly IRatingService _ratingService;

        private Label lblVipInfo;
        private Panel panelVipBar;
        private DataGridView dgvGenreImpression;
        private DataGridView dgvRecommendations;

        public CustomerProfileForm(Customer customer, IRatingService ratingService)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));

            InitializeUI();
            LoadProfile();
        }

        private void InitializeUI()
        {
            this.Text = "用户画像";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // VIP等级、积分、进度
            lblVipInfo = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(750, 30),
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblVipInfo);

            // 自定义VIP经验条
            panelVipBar = new Panel
            {
                Location = new Point(20, 50),
                Size = new Size(750, 25),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelVipBar);

            // 用户画像: 类型偏好
            Label lblGenre = new Label
            {
                Text = "电影类型偏好：",
                Location = new Point(20, 85),
                AutoSize = true
            };
            this.Controls.Add(lblGenre);

            dgvGenreImpression = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(350, 200),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };
            dgvGenreImpression.Columns.Add("Genre", "电影类型");
            dgvGenreImpression.Columns.Add("Score", "得分");
            this.Controls.Add(dgvGenreImpression);

            // 推荐电影
            Label lblRec = new Label
            {
                Text = "推荐电影（未来两个月）：",
                Location = new Point(400, 85),
                AutoSize = true
            };
            this.Controls.Add(lblRec);

            dgvRecommendations = new DataGridView
            {
                Location = new Point(400, 110),
                Size = new Size(370, 400),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };
            dgvRecommendations.Columns.Add("FilmName", "电影名称");
            dgvRecommendations.Columns.Add("Genres", "类型");
            dgvRecommendations.Columns.Add("NearestScreening", "最近场次");
            dgvRecommendations.Columns.Add("Score", "评分");
            dgvRecommendations.Columns.Add("RecommendationScore", "推荐指数");
            this.Controls.Add(dgvRecommendations);
        }

        private void LoadProfile()
        {
            DrawVipInfo();

            // --- 用户画像类型偏好 ---
            dgvGenreImpression.Rows.Clear();
            var genreImpression = _ratingService.GetUserGenreImpression(_customer.CustomerID)
                                               .OrderByDescending(kv => kv.Value)
                                               .ToList();
            foreach (var kv in genreImpression)
            {
                dgvGenreImpression.Rows.Add(kv.Key, kv.Value.ToString("F1"));
            }

            // --- 推荐电影 ---
            dgvRecommendations.Rows.Clear();
            var recommendations = _ratingService.GetMovieRecommendations(_customer.CustomerID);
            foreach (var movie in recommendations)
            {
                dgvRecommendations.Rows.Add(
                    movie.FilmName,
                    string.Join("/", movie.Genres),
                    movie.NearestScreening.ToString("yyyy-MM-dd HH:mm"),
                    movie.Score.ToString("F1"),
                    movie.RecommendationScore.ToString("F1")
                );
            }
        }

        private void DrawVipInfo()
        {
            panelVipBar.Invalidate(); // 先刷新

            if (_customer.VIPCard == null)
            {
                lblVipInfo.Text = "VIP等级: 0  |  积分: 0  | 升级进度: 0%";
                return;
            }

            int points = _customer.VIPCard.Points;
            int vipLevel = VipLevels.CalculateVipLevel(points);
            int nextLevelPoints = VipLevels.GetNextLevelPoints(vipLevel);

            double progress;
            string progressText;

            if (nextLevelPoints == -1) // 已经是最高等级
            {
                progress = 1.0;
                progressText = "已满级";
            }
            else
            {
                int currentLevelPoints = VipLevels.PointsRequired[vipLevel];
                int pointsInCurrentLevel = points - currentLevelPoints;
                int pointsToNextLevel = nextLevelPoints - currentLevelPoints;
                progress = Math.Min(1.0, Math.Max(0, (double)pointsInCurrentLevel / pointsToNextLevel));
                progressText = $"{(int)(progress * 100)}%";
            }

            lblVipInfo.Text = $"VIP等级: {vipLevel}  |  积分: {points}  | 升级进度: {progressText}";

            panelVipBar.Paint += (s, e) =>
            {
                e.Graphics.Clear(panelVipBar.BackColor);
                // 背景灰
                e.Graphics.FillRectangle(Brushes.LightGray, 0, 0, panelVipBar.Width, panelVipBar.Height);
                // 填充进度
                e.Graphics.FillRectangle(Brushes.CornflowerBlue, 0, 0, (int)(panelVipBar.Width * progress), panelVipBar.Height);
                // 边框
                e.Graphics.DrawRectangle(Pens.Black, 0, 0, panelVipBar.Width - 1, panelVipBar.Height - 1);
            };

            panelVipBar.Refresh();
        }
    }
}
