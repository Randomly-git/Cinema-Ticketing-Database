using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public class FilmDashboard : Form
    {
        private TabControl tabControl;

        private ComboBox cmbShowingsFilms;
        private DataGridView dgvShowings;
        private TextBox txtFilmSearch;  // 搜索框
        private List<Film> allShowingsFilms = new(); // 用于搜索过滤
        private TextBox txtShowingsSearch; // 搜索框

        private List<Film> allFilms;  // 全部电影列表缓存

        private ComboBox cmbOverviewFilms;
        private TextBox txtOverview;
        private TextBox txtCastResults;
        private TextBox txtCastSearch;
        private Button btnSearchCast;
        private ListBox lstCastResults;
        private ListBox lstCastMovies;
        private DataGridView dgvCastResults;  

        private DataGridView dgvStatistics;
        private IFilmService _filmService;
        private IRatingService _ratingService;

        public FilmDashboard(string? filmname = null)
        {
            _filmService = Program._filmService;  // 赋值
            _ratingService = Program._ratingService;  // 赋值
            InitializeComponent();
            LoadShowingsFilms();
            LoadOverviewFilms();
            AttachStatisticsRowClickEvent();  // 绑定事件

            // 如果传入了电影名称，填充搜索框并触发搜索
            if (!string.IsNullOrEmpty(filmname))
            {
                // 填充影片概况页的搜索框
                txtFilmSearch.Text = filmname;
                // 触发搜索事件
                TxtFilmSearch_TextChanged(this, EventArgs.Empty);
                var matchedFilm = cmbOverviewFilms.Items.Cast<Film>().FirstOrDefault(f => f.FilmName.Equals(filmname, StringComparison.OrdinalIgnoreCase));

                if (matchedFilm != null)
                {
                    cmbOverviewFilms.SelectedItem = matchedFilm;
                    // 手动触发详情显示
                    ShowFilmOverview();
                }


                // 可选：同时填充排挡页的搜索框
                txtShowingsSearch.Text = filmname;
                TxtShowingsSearch_TextChanged(this, EventArgs.Empty);
                var matchedShowingsFilm = cmbShowingsFilms.Items.Cast<Film>().FirstOrDefault(f => f.FilmName.Equals(filmname, StringComparison.OrdinalIgnoreCase));
                if (matchedShowingsFilm != null)
                {
                    cmbShowingsFilms.SelectedItem = matchedShowingsFilm;
                    LoadShowingsForSelectedFilm();
                }
            }
        }

        private void AttachStatisticsRowClickEvent()
        {
            dgvStatistics.CellDoubleClick += DgvStatistics_CellDoubleClick;
        }

        private void DgvStatistics_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // 忽略标题行等无效点击

            // 取出点击行的电影名，假设数据源里列名是 FilmName
            var filmName = dgvStatistics.Rows[e.RowIndex].Cells["FilmName"].Value?.ToString();
            if (string.IsNullOrEmpty(filmName)) return;

            // 找到影片概况页签索引
            var overviewTabIndex = tabControl.TabPages.IndexOfKey("影片概况");
            if (overviewTabIndex < 0) return;

            tabControl.SelectedIndex = overviewTabIndex;

            JumpToFilmOverview(filmName);

        }
        private void InitializeComponent()
        {
            this.Text = "影片综合管理";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            tabControl = new TabControl { Dock = DockStyle.Fill };

            tabControl.TabPages.Add(CreateShowingsTab());
            tabControl.TabPages.Add(CreateFilmOverviewTab());
            tabControl.TabPages.Add(CreateCastTab());
            tabControl.TabPages.Add(CreateStatisticsTab());

            this.Controls.Add(tabControl);
        }

        //查看排挡
        private TabPage CreateShowingsTab()
        {
            var tab = new TabPage("查看排挡");

            // 搜索框
            txtShowingsSearch = new TextBox
            {
                Location = new Point(680, 20), // 靠右对齐
                Width = 170,
                PlaceholderText = "搜索影片..."
            };
            txtShowingsSearch.TextChanged += TxtShowingsSearch_TextChanged;

            // 影片下拉框
            cmbShowingsFilms = new ComboBox
            {
                Location = new Point(20, 20),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbShowingsFilms.SelectedIndexChanged += (s, e) => LoadShowingsForSelectedFilm();

            // 排挡表格
            dgvShowings = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(830, 450),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            tab.Controls.Add(txtShowingsSearch);
            tab.Controls.Add(cmbShowingsFilms);
            tab.Controls.Add(dgvShowings);
            return tab;
        }


        //
        private void LoadShowingsFilms()
        {
            try
            {
                allShowingsFilms = Program._filmService.GetAvailableFilms();
                cmbShowingsFilms.DataSource = allShowingsFilms;
                cmbShowingsFilms.DisplayMember = "FilmName";
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载电影列表失败：" + ex.Message);
            }
        }

        private void TxtShowingsSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtShowingsSearch.Text.Trim().ToLower();

            var filtered = string.IsNullOrEmpty(keyword)
                ? allShowingsFilms
                : allShowingsFilms.Where(f => f.FilmName.ToLower().Contains(keyword)).ToList();

            cmbShowingsFilms.DataSource = filtered;
            cmbShowingsFilms.DisplayMember = "FilmName";

            // 自动选择第一项（如果存在）
            if (filtered.Any())
            {
                cmbShowingsFilms.SelectedIndex = 0;
            }
        }


        private void LoadShowingsForSelectedFilm()
        {
            if (cmbShowingsFilms.SelectedItem is not Film film) return;

            try
            {
                var sections = Program._showingService.GetFilmShowings(film.FilmName);
                dgvShowings.DataSource = sections.Select(s => new
                {
                    场次ID = s.SectionID,
                    日期 = s.TimeSlot.StartTime.ToString("yyyy-MM-dd dddd"),
                    时段 = $"{s.TimeSlot.StartTime:HH:mm} - {s.TimeSlot.EndTime:HH:mm}",
                    影厅 = $"{s.MovieHall.HallNo}号厅 ({s.MovieHall.Category})"
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载排挡失败：" + ex.Message);
            }
        }


        //影片概况
        private TabPage CreateFilmOverviewTab()
        {
            var tab = new TabPage("影片概况") { Name = "影片概况" };

            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // 搜索框（右上角）
            txtFilmSearch = new TextBox
            {
                Width = 300,
                PlaceholderText = "输入电影名搜索...",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panel.Width - 310, 10)
            };
            txtFilmSearch.TextChanged += TxtFilmSearch_TextChanged;

            // 影片下拉框（左上角）
            cmbOverviewFilms = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(10, 10)
            };
            cmbOverviewFilms.SelectedIndexChanged += (s, e) => ShowFilmOverview();

            // 创建PictureBox用于显示海报
            PictureBox pictureBoxPoster = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 300,
                Height = 450,
                Location = new Point(10, 50),
                BorderStyle = BorderStyle.None
            };

            // 文本框显示影片信息
            txtOverview = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Width = 550,
                Height = 450,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(320, 50)
            };

            // 添加"查看评论"按钮
            Button btnViewRatings = new Button
            {
                Text = "查看评论",
                Location = new Point(320, 10),
                Size = new Size(100, 30)
            };
            btnViewRatings.Click += BtnViewRatings_Click;

            // 添加控件
            panel.Controls.Add(txtFilmSearch);
            panel.Controls.Add(cmbOverviewFilms);
            panel.Controls.Add(pictureBoxPoster);
            panel.Controls.Add(txtOverview);
            panel.Controls.Add(btnViewRatings);
            tab.Controls.Add(panel);

            panel.Resize += (s, e) =>
            {
                txtFilmSearch.Location = new Point(panel.Width - txtFilmSearch.Width - 10, 10);
            };

            return tab;
        }

        private void BtnViewRatings_Click(object sender, EventArgs e)
        {
            if (cmbOverviewFilms.SelectedItem is not Film selectedFilm)
            {
                MessageBox.Show("请先选择一部电影");
                return;
            }

            try
            {
                var ratings = _ratingService.GetFilmRatingDetails(selectedFilm.FilmName);

                if (!ratings.Any())
                {
                    MessageBox.Show("该电影暂无评论");
                    return;
                }

                // 创建评论显示窗体
                Form ratingForm = new Form
                {
                    Text = $"《{selectedFilm.FilmName}》的影评",
                    Size = new Size(600, 520), // 留出空间给下方评论区
                    StartPosition = FormStartPosition.CenterParent
                };

                // 创建 DataGridView
                DataGridView dgvRatings = new DataGridView
                {
                    Dock = DockStyle.Top,
                    Height = 350, // 上方表格高度
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToResizeRows = false,
                    DefaultCellStyle = { WrapMode = DataGridViewTriState.True },
                    AutoGenerateColumns = false
                };

                // 创建列
                dgvRatings.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    HeaderText = "评分",
                    DataPropertyName = "Score",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells // 根据内容自动调整
                });

                dgvRatings.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    HeaderText = "评论",
                    DataPropertyName = "Comment",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
                });

                dgvRatings.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    HeaderText = "评价时间",
                    DataPropertyName = "RatingDate",
                    Width = 120
                });

                // 准备显示数据
                var displayData = ratings.Select(r => new
                {
                    Score = r.Score,
                    Comment = string.IsNullOrEmpty(r.Comment) ? "无文字评论" : r.Comment,
                    RatingDate = r.RatingDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                dgvRatings.DataSource = displayData;

                // 创建下方评论显示区 Panel
                Panel commentPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 130 // Label(30) + TextBox(100)
                };

                // 创建标题标签
                Label lblFullComment = new Label
                {
                    Text = "完整评论显示：",
                    Dock = DockStyle.Top,
                    Height = 20,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("微软雅黑", 10, FontStyle.Bold),
                    Padding = new Padding(5, 0, 0, 0)
                };

                // 创建下方评论显示区
                TextBox txtFullComment = new TextBox
                {
                    Dock = DockStyle.Bottom,
                    Height = 100, // 评论显示区域高度
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("微软雅黑", 10),
                    BackColor = Color.White
                };

                // 把 Label 和 TextBox 放进 Panel
                commentPanel.Controls.Add(txtFullComment);
                commentPanel.Controls.Add(lblFullComment);

                // 双击行显示完整评论
                dgvRatings.CellDoubleClick += (s, ev) =>
                {
                    if (ev.RowIndex >= 0 && ev.RowIndex < displayData.Count)
                    {
                        string comment = displayData[ev.RowIndex].Comment;
                        txtFullComment.Text = comment;
                    }
                };

                // 添加控件
                ratingForm.Controls.Add(dgvRatings);
                ratingForm.Controls.Add(commentPanel);

                ratingForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取评论失败: " + ex.Message);
            }
        }





        private void LoadOverviewFilms()
        {
            try
            {
                allFilms = Program._filmService.GetAvailableFilms().ToList();
                cmbOverviewFilms.DataSource = allFilms;
                cmbOverviewFilms.DisplayMember = "FilmName";
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载影片列表失败：" + ex.Message);
            }
        }


        private void ShowFilmOverview()
        {
            if (cmbOverviewFilms.SelectedItem is not Film film) return;

            // 查找PictureBox控件
            PictureBox pictureBoxPoster = null;
            foreach (Control control in tabControl.TabPages["影片概况"].Controls[0].Controls)
            {
                if (control is PictureBox)
                {
                    pictureBoxPoster = (PictureBox)control;
                    break;
                }
            }

            // 加载海报图片
            if (pictureBoxPoster != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(film.ImagePath))
                    {
                        string imagePath = Path.Combine(Application.StartupPath, film.ImagePath);
                        if (File.Exists(imagePath))
                        {
                            // 使用using确保及时释放资源
                            using (var tempImage = Image.FromFile(imagePath))
                            {
                                pictureBoxPoster.Image = new Bitmap(tempImage);
                            }
                        }
                        else
                        {
                            // 文件不存在，设置空白图片
                            pictureBoxPoster.Image = null;
                        }
                    }
                    else
                    {
                        // 没有图片路径，设置空白图片
                        pictureBoxPoster.Image = null;
                    }
                }
                catch (Exception)
                {
                    // 加载失败，设置空白图片
                    pictureBoxPoster.Image = null;
                }
            }

            var overview = $"影片名称: {film.FilmName}\r\n" +
                           $"类型: {film.Genre}\r\n" +
                           $"时长: {film.FilmLength} 分钟\r\n" +
                           $"票价: {film.NormalPrice:C}\r\n" +
                           $"上映日期: {film.ReleaseDate?.ToShortDateString() ?? "未定"}\r\n" +
                           $"评分: {film.Score}\r\n" +
                           $"票房: {film.BoxOffice}\r\n" +
                           $"观影人次: {film.Admissions}\r\n";

            // 取出演员列表
            var castList = Program._filmRepository.GetCastByFilmName(film.FilmName);

            if (castList.Any())
            {
                overview += "\r\n参演演员及角色:\r\n";
                foreach (var cast in castList)
                {
                    overview += $"{cast.MemberName} 饰 {cast.Role}\r\n";
                }
            }
            else
            {
                overview += "\r\n暂无演员信息。\r\n";
            }

            txtOverview.Text = overview;
        }

        private void TxtFilmSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtFilmSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                // 显示全部电影
                cmbOverviewFilms.DataSource = allFilms;
            }
            else
            {
                var filtered = allFilms
                    .Where(f => f.FilmName.ToLower().Contains(keyword))
                    .ToList();

                cmbOverviewFilms.DataSource = filtered;
            }

            cmbOverviewFilms.DisplayMember = "FilmName";

            // 如果搜索结果中只有一个电影，可以自动选中
            if (cmbOverviewFilms.Items.Count == 1)
            {
                cmbOverviewFilms.SelectedIndex = 0;
            }
        }

  



        //查看演员演过的电影
        private TabPage CreateCastTab()
        {
            var tab = new TabPage("演职人员");

            txtCastSearch = new TextBox { Location = new Point(20, 20), Width = 200 };
            btnSearchCast = new Button { Text = "查询", Location = new Point(230, 18), Size = new Size(70, 26) };
            btnSearchCast.Click += BtnSearchCast_Click;

            dgvCastResults = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(830, 450),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // 定义列
            dgvCastResults.Columns.Add("MemberName", "演员姓名");
            dgvCastResults.Columns.Add("FilmName", "电影名称");
            dgvCastResults.Columns.Add("Role", "饰演角色");

            tab.Controls.Add(txtCastSearch);
            tab.Controls.Add(btnSearchCast);
            tab.Controls.Add(dgvCastResults);

            return tab;
        }

        private void BtnSearchCast_Click(object sender, EventArgs e)
        {
            dgvCastResults.Rows.Clear();

            var keyword = txtCastSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("请输入演职人员姓名关键字。");
                return;
            }

            try
            {
                var castDetails = Program._filmRepository.GetCastCrewDetails(keyword);

                if (!castDetails.Any())
                {
                    MessageBox.Show("未找到相关演职人员。");
                    return;
                }

                foreach (var c in castDetails)
                {
                    dgvCastResults.Rows.Add(c.MemberName, c.FilmName, c.Role);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询演职人员失败：" + ex.Message);
            }
        }

        private void JumpToFilmOverview(string filmName)
        {
            // 清除搜索框，避免过滤
            txtFilmSearch.TextChanged -= TxtFilmSearch_TextChanged;
            txtFilmSearch.Text = "";
            txtFilmSearch.TextChanged += TxtFilmSearch_TextChanged;

            // 重新加载所有电影数据到下拉框
            cmbOverviewFilms.DataSource = allFilms;
            cmbOverviewFilms.DisplayMember = "FilmName";

            // 查找目标影片
            for (int i = 0; i < cmbOverviewFilms.Items.Count; i++)
            {
                if (cmbOverviewFilms.Items[i] is Film film && film.FilmName == filmName)
                {
                    cmbOverviewFilms.SelectedIndex = i;
                    break;
                }
            }

            // 切换到“影片概况”Tab
            tabControl.SelectedTab = tabControl.TabPages["影片概况"];

            // 强制刷新影片信息
            ShowFilmOverview();
        }



        //排行榜
        private TabPage CreateStatisticsTab()
        {
            var tab = new TabPage("数据统计");

            dgvStatistics = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(830, 450),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var comboBoxSort = new ComboBox
            {
                Location = new Point(700, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150
            };
            comboBoxSort.Items.AddRange(new string[] { "票房", "评分", "人次", "电影时长", "上映日期", "票价" });
            comboBoxSort.SelectedIndex = 0;

            comboBoxSort.SelectedIndexChanged += (s, e) =>
            {
                string orderBy = comboBoxSort.SelectedItem.ToString();
                string orderKey = orderBy switch
                {
                    "评分" => "score",
                    "人次" => "admissions",
                    "电影时长" => "filmLength",
                    "上映日期" => "releaseDate",
                    "票价" => "normalPrice",
                    _ => "boxOffice"
                };


                var stats = _filmService.GetFilmStatistics(orderKey);
                dgvStatistics.DataSource = stats;
            };

            tab.Controls.Add(comboBoxSort);
            tab.Controls.Add(dgvStatistics);

            // 初始加载数据
            var stats = _filmService.GetFilmStatistics("boxOffice");
            dgvStatistics.DataSource = stats;

            return tab;
        }

    }
}
