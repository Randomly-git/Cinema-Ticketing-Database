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
            //加载评论
            LoadCommentsWithFormat();

            // 设置日期选择器范围
            DateTime today = DateTime.Today;
            DateTime minDate = today;

            if (_selectedFilm.ReleaseDate.HasValue && _selectedFilm.ReleaseDate.Value.Date > today)
            {
                minDate = _selectedFilm.ReleaseDate.Value.Date;
            }
        }
        //显示评论
        private void LoadCommentsWithFormat()
        {
            try
            {
                // 1. 从服务获取当前电影的所有评论（需确保IRatingService实现正确）
                List<Rating> filmComments = Program._ratingService.GetFilmRatingDetails(_selectedFilm.FilmName)
                                                          .ToList(); // 复用现有获取评论的方法

                // 2. 清空RichTextBox原有内容
                rtbComments.Clear();

                // 3. 处理“无评论”场景
                if (filmComments.Count == 0)
                {
                    // 设置“暂无评论”为灰色斜体，居中显示
                    rtbComments.SelectionFont = new Font("微软雅黑",10f, FontStyle.Italic);
                    rtbComments.SelectionColor = Color.Gray;
                    // 居中对齐（需先设置对齐方式，再追加文本）
                    rtbComments.SelectionAlignment = HorizontalAlignment.Center;
                    rtbComments.AppendText("暂无评论");
                    return;
                }

                // 4. 处理“有评论”场景：差异化字体展示CustomerId和评论文本
                foreach (var comment in filmComments)
                {
                    // 格式1：CustomerId（加粗、深蓝色、10号字）
                    rtbComments.SelectionFont = new Font("微软雅黑", 10f, FontStyle.Bold);
                    rtbComments.SelectionColor = Color.DarkBlue;
                    rtbComments.SelectionAlignment = HorizontalAlignment.Left; // 左对齐
                    // 拼接CustomerId文本（处理空值，避免显示“null”）
                    string customerid=Program._orderRepository.GetCustomerIDByTicketID(comment.TicketID);

                    string customerIdText = $"用户ID：{customerid ?? "未知用户"}";
                    rtbComments.AppendText(customerIdText);

                    rtbComments.SelectionFont = new Font("微软雅黑", 10f, FontStyle.Bold);
                    rtbComments.SelectionColor = Color.Red;
                    rtbComments.AppendText($"  评分：{comment.Score}/10");


                    // 格式2：评论文本（常规、黑色、9号字）+ 换行分隔
                    rtbComments.SelectionFont = new Font("微软雅黑", 9f, FontStyle.Regular);
                    rtbComments.SelectionColor = Color.Black;
                    // 处理空评论（显示“无评论文字”）
                    string commentText = comment.Comment ?? "无评论文字";
                    // 追加评论文本 + 两次换行（每条评论间空一行分隔）
                    rtbComments.AppendText($"\n{commentText}");

                    // 格式化评论时间（例如：2023-10-01 15:30）
                    string timeText = comment.RatingDate != DateTime.MinValue
                        ? comment.RatingDate.ToString("yyyy-MM-dd HH:mm")
                        : "未知时间";

                    // 单独设置时间格式（灰色、斜体）
                    rtbComments.SelectionFont = new Font("微软雅黑", 8f, FontStyle.Italic);
                    rtbComments.SelectionColor = Color.Gray;
                    rtbComments.AppendText($" （{timeText}）\n\n");
                }

                // 5. 重置光标位置到顶部（方便用户从第一条开始看）
                rtbComments.SelectionStart = 0;
                rtbComments.ScrollToCaret();
            }
            catch (Exception ex)
            {
                // 异常处理：显示“加载失败”提示
                rtbComments.Clear();
                rtbComments.SelectionFont = new Font("微软雅黑", 10f, FontStyle.Italic);
                rtbComments.SelectionColor = Color.Red;
                rtbComments.SelectionAlignment = HorizontalAlignment.Center;
                rtbComments.AppendText($"评论加载失败：{ex.Message}");
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

        private void lb1_Click(object sender, EventArgs e)
        {

        }
    }
}
