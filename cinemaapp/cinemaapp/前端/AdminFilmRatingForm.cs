using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public class AdminFilmRatingForm : Form
    {
        private readonly IFilmService _filmService;
        private readonly IRatingService _ratingService;

        private ComboBox cmbFilms;
        private DataGridView dgvRatings;
        private Label lblFullComment;
        private TextBox txtFullComment;

        public AdminFilmRatingForm(IFilmService filmService, IRatingService ratingService)
        {
            _filmService = filmService ?? throw new ArgumentNullException(nameof(filmService));
            _ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));

            InitUI();
            LoadFilmList();
        }

        private void InitUI()
        {
            this.Text = "管理员查看电影评论";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblSelectFilm = new Label { Text = "选择电影:", Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblSelectFilm);

            cmbFilms = new ComboBox
            {
                Location = new Point(100, 18),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilms.SelectedIndexChanged += CmbFilms_SelectedIndexChanged;
            this.Controls.Add(cmbFilms);

            dgvRatings = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(740, 400),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvRatings.CellDoubleClick += DgvRatings_CellDoubleClick;
            this.Controls.Add(dgvRatings);

            // 完整评论标签
            lblFullComment = new Label
            {
                Text = "完整评论显示：",
                Location = new Point(20, 470),
                AutoSize = true
            };
            this.Controls.Add(lblFullComment);

            // 完整评论框
            txtFullComment = new TextBox
            {
                Location = new Point(20, 500),
                Size = new Size(740, 60),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White
            };
            this.Controls.Add(txtFullComment);
        }

        private void LoadFilmList()
        {
            var films = Program._filmRepository.GetAllFilms();
            cmbFilms.Items.Clear();
            foreach (var film in films)
            {
                cmbFilms.Items.Add(film.FilmName);
            }

            if (cmbFilms.Items.Count > 0)
                cmbFilms.SelectedIndex = 0; // 默认选中第一部电影
        }

        private void CmbFilms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFilms.SelectedItem == null)
                return;

            string filmName = cmbFilms.SelectedItem.ToString();
            LoadRatings(filmName);
        }

        private void LoadRatings(string filmName)
        {
            var ratings = _ratingService.GetFilmRatingDetails(filmName).ToList();

            var ratingData = ratings.Select(r => new
            {
                r.TicketID,
                r.Score,
                r.Comment,
                RatingDate = r.RatingDate.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
           
            dgvRatings.DataSource = ratingData;
            NarrowScoreColumn(); // <- 收窄评分列
        }

        // 绑定完成后调用：把“评分”列收窄
        private void NarrowScoreColumn()
        {
            // 先按 DataPropertyName，再按 Name，最后按 HeaderText（中文“评分”）
            var scoreCol = dgvRatings.Columns.Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.DataPropertyName, "Score", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, "Score", StringComparison.OrdinalIgnoreCase) ||
                    c.HeaderText.Trim().Equals("评分", StringComparison.OrdinalIgnoreCase));

            if (scoreCol != null)
            {
                scoreCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁止随 Fill 撑开
                scoreCol.Width = 50; // 想更窄可设 40~60
            }
        }


        private void DgvRatings_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvRatings.Rows[e.RowIndex];
                var comment = row.Cells["Comment"].Value?.ToString();

                if (!string.IsNullOrEmpty(comment))
                {
                    txtFullComment.Text = comment;
                }
                else
                {
                    txtFullComment.Text = "（无评论内容）";
                }
            }
        }
    }
}
