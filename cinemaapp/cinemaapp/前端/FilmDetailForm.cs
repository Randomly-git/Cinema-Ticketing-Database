using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{

    public partial class FilmDetailForm : Form
    {
        private readonly Film _selectedFilm;
        private readonly Customer _loggedInCustomer;
        private readonly MainForm _mainForm;
        private readonly DateTime _selectedDate;

        // 构造函数接收 Film 对象，以及其他可能需要的依赖
        public FilmDetailForm(Film selectedFilm, Customer loggedInCustomer, MainForm mainForm, DateTime selectedDate)
        {
            InitializeComponent();
            _selectedFilm = selectedFilm;
            _loggedInCustomer = loggedInCustomer;
            _mainForm = mainForm;
            _selectedDate = selectedDate;
            DisplayFilmDetails();
        }

        private void DisplayFilmDetails()
        {
            // 显示大图
            Image img = null;
            try
            {
                if (!string.IsNullOrEmpty(_selectedFilm.ImagePath))
                {
                    string imgPath = Path.Combine(Application.StartupPath, _selectedFilm.ImagePath);
                    if (File.Exists(imgPath))
                    {
                        // 加载原图，保持原始比例
                        using var original = Image.FromFile(imgPath);
                        img = new Bitmap(original);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载电影海报失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 加载失败忽略
            }

            if (img == null)
            {
                // 加载默认图片
                string defaultImagePath = Path.Combine(Application.StartupPath, "images", "默认.jpg");
                if (File.Exists(defaultImagePath))
                {
                    using var defaultImg = Image.FromFile(defaultImagePath);
                    img = new Bitmap(defaultImg);
                }
                else
                {
                    // 创建纯色默认图片
                    img = new Bitmap(pictureBoxPoster.Width, pictureBoxPoster.Height);
                    using (var g = Graphics.FromImage(img))
                    {
                        g.Clear(Color.LightGray);
                        using (var font = new Font("Arial", 12))
                        using (var brush = new SolidBrush(Color.Black))
                        {
                            g.DrawString("暂无海报", font, brush,
                                new PointF(pictureBoxPoster.Width / 2 - 40, pictureBoxPoster.Height / 2 - 10));
                        }
                    }
                }
            }
            pictureBoxPoster.Image = img;

            // 显示电影信息
            lblFilmName.Text = _selectedFilm.FilmName;
            lblGenre.Text = $"类型: {_selectedFilm.Genre}";
            lblDuration.Text = $"时长: {_selectedFilm.FilmLength}分钟";
            lblScore.Text = $"评分: {_selectedFilm.Score:F1} ({_selectedFilm.RatingNum}人评分)";
            lblBoxOffice.Text = $"票房: {_selectedFilm.BoxOffice:C}";
            lblAdmissions.Text = $"观影人次: {_selectedFilm.Admissions:N0}";
            lblPrice.Text = $"票价: {_selectedFilm.NormalPrice:C}";

            // 设置日期选择器范围
            DateTime today = DateTime.Today;
            DateTime minDate = today;

            if (_selectedFilm.ReleaseDate.HasValue && _selectedFilm.ReleaseDate.Value.Date > today)
            {
                minDate = _selectedFilm.ReleaseDate.Value.Date;
            }
        }

        /// <summary>
        /// “查询场次”按钮的点击事件，逻辑参照 FilmSelectionForm 中的 btnSearchSections_Click 方法。
        /// </summary>
        private void btnSearchSections_Click(object sender, EventArgs e)
        {
            var sectionForm = new SectionSelectionForm(_selectedFilm, _selectedDate, _loggedInCustomer, _mainForm);
            if (sectionForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("购票成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
