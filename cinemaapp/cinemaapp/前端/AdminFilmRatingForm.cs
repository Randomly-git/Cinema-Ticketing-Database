using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;

namespace cinemaapp
{
    public class AdminFilmRatingForm : Form
    {
        private readonly IFilmService _filmService;
        private readonly IRatingService _ratingService;

        private ComboBox cmbFilms;
        private DataGridView dgvRatings;

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
                Size = new Size(740, 480),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgvRatings);
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
        }
    }
}
