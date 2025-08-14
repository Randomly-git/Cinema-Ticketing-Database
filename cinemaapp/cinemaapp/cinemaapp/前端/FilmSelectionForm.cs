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
using test.Services;

namespace cinemaapp
{
    public partial class FilmSelectionForm : Form
    {
        private readonly IFilmService _filmService;
        private List<Film> _films;
        private readonly Customer _loggedInCustomer;
        private readonly MainForm _mainForm;
        public FilmSelectionForm(IFilmService filmService, Customer loggedInCustomer, MainForm mainForm)
        {
            InitializeComponent();
            _filmService = filmService;
            LoadFilms();
            _loggedInCustomer = loggedInCustomer;
            _mainForm = mainForm;
        }

        private void LoadFilms()
        {
            try
            {
                _films = _filmService.GetAvailableFilms();
                DisplayFilmThumbnails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载电影列表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayFilmThumbnails()
        {
            flowLayoutPanelFilms.Controls.Clear();

            foreach (var film in _films)
            {
                var filmPanel = new Panel
                {
                    Width = 150,
                    Height = 200,
                    Margin = new Padding(10),
                    Tag = film,
                    Cursor = Cursors.Hand
                };

                // 海报缩略图
                var pictureBox = new PictureBox
                {
                    Width = 130,
                    Height = 150,
                    Location = new Point(10, 10),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = System.Windows.Forms.BorderStyle.None
                };

                // 加载图片
                Image img = null;
                try
                {
                    if (!string.IsNullOrEmpty(film.ImagePath))
                    {
                        string imgPath = Path.Combine(Application.StartupPath, film.ImagePath);
                        if (File.Exists(imgPath))
                        {
                            // 加载原图后缩小为130x150做缩略图，保持高质量
                            using var original = Image.FromFile(imgPath);
                            img = ResizeImageHighQuality(original, 130, 195);
                        }
                    }
                }
                catch
                {
                    // 加载失败忽略
                }

                if (img == null)
                {
                    // 加载默认图片
                    string defaultImagePath = Path.Combine(Application.StartupPath, "images", "默认.jpg");
                    if (File.Exists(defaultImagePath))
                    {
                        using var defaultImg = Image.FromFile(defaultImagePath);
                        img = ResizeImageHighQuality(defaultImg, 130, 150);
                    }
                    else
                    {
                        // 创建纯色默认图片
                        img = new Bitmap(130, 150);
                        using (var g = Graphics.FromImage(img))
                        {
                            g.Clear(Color.LightGray);
                            using (var font = new Font("Arial", 10))
                            using (var brush = new SolidBrush(Color.Black))
                            {
                                g.DrawString("暂无海报", font, brush, new PointF(10, 60));
                            }
                        }
                    }
                }

                pictureBox.Image = img;

                // 电影名标签
                var lblName = new Label
                {
                    Text = film.FilmName,
                    Width = 130,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(10, 160),
                    Font = new Font("微软雅黑", 9, FontStyle.Bold)
                };

                filmPanel.Controls.Add(pictureBox);
                filmPanel.Controls.Add(lblName);

                // 点击事件
                filmPanel.Click += (sender, e) => DisplayFilmDetails(film);
                pictureBox.Click += (sender, e) => DisplayFilmDetails(film);
                lblName.Click += (sender, e) => DisplayFilmDetails(film);

                flowLayoutPanelFilms.Controls.Add(filmPanel);
            }
        }

        private void DisplayFilmDetails(Film film)
        {
            // 显示大图
            Image img = null;
            try
            {
                if (!string.IsNullOrEmpty(film.ImagePath))
                {
                    string imgPath = Path.Combine(Application.StartupPath, film.ImagePath);
                    if (File.Exists(imgPath))
                    {
                        // 加载原图，保持原始比例
                        using var original = Image.FromFile(imgPath);
                        img = new Bitmap(original);
                    }
                }
            }
            catch
            {
                // 加载失败忽略
            }

            if (img == null)
            {
                // 加载默认图片
                string defaultImagePath = Path.Combine(Application.StartupPath, "iamges", "默认.jpg");
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
            lblFilmName.Text = film.FilmName;
            lblGenre.Text = $"类型: {film.Genre}";
            lblDuration.Text = $"时长: {film.FilmLength}分钟";
            lblScore.Text = $"评分: {film.Score:F1} ({film.RatingNum}人评分)";
            lblBoxOffice.Text = $"票房: {film.BoxOffice:C}";
            lblAdmissions.Text = $"观影人次: {film.Admissions:N0}";
            lblPrice.Text = $"票价: {film.NormalPrice:C}";

            // 设置日期选择器范围
            DateTime today = DateTime.Today;
            DateTime minDate = today;

            // 如果电影有上映日期，并且上映日期在今天之后，则使用上映日期作为最小日期
            if (film.ReleaseDate.HasValue && film.ReleaseDate.Value.Date > today)
            {
                minDate = film.ReleaseDate.Value.Date;
            }

            // 设置日期选择器的最小日期（不能早于今天或上映日期）
            dateTimePicker.MinDate = minDate;

            // 设置日期选择器的最大日期（如果有撤档日期则使用，否则默认为1个月后）
            dateTimePicker.MaxDate = film.EndDate ?? today.AddMonths(1);

            // 设置默认选中日期为最小可用日期
            dateTimePicker.Value = minDate;
        }

        private void btnSearchSections_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblFilmName.Text))
            {
                MessageBox.Show("请先选择一部电影", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedFilm = _films.Find(f => f.FilmName == lblFilmName.Text);
            if (selectedFilm == null) return;

            var date = dateTimePicker.Value.Date;

            // 打开场次选择窗体
            var sectionForm = new SectionSelectionForm(selectedFilm, date, _loggedInCustomer,_mainForm);
            if (sectionForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("购票成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private static Image ResizeImageHighQuality(Image original, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}

