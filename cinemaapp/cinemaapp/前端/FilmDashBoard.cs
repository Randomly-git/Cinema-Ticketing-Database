using System;
using System.Collections.Generic;
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

        public FilmDashboard()
        {
            _filmService = Program._filmService;  // 赋值
            InitializeComponent();
            LoadShowingsFilms();
            LoadOverviewFilms();
            AttachStatisticsRowClickEvent();  // 绑定事件
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
                Location = new Point(panel.Width - 310, 10)  // 右边预留10像素边距
            };
            txtFilmSearch.TextChanged += TxtFilmSearch_TextChanged;

            // 影片下拉框（左上角）
            cmbOverviewFilms = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(10, 10)  // 左边预留10像素
            };
            cmbOverviewFilms.SelectedIndexChanged += (s, e) => ShowFilmOverview();

            // 文本框显示影片信息（居中偏下）
            txtOverview = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Width = 800,
                Height = 400,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 50)
            };

            // 添加控件
            panel.Controls.Add(txtFilmSearch);
            panel.Controls.Add(cmbOverviewFilms);
            panel.Controls.Add(txtOverview);
            tab.Controls.Add(panel);

            // 响应 Panel Resize（动态更新搜索框位置）
            panel.Resize += (s, e) =>
            {
                txtFilmSearch.Location = new Point(panel.Width - txtFilmSearch.Width - 10, 10);
            };

            return tab;
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

            var overview = $"影片名称: {film.FilmName}\r\n" +
                           $"类型: {film.Genre}\r\n" +
                           $"时长: {film.FilmLength} 分钟\r\n" +
                           $"票价: {film.NormalPrice:C}\r\n" +
                           $"上映日期: {film.ReleaseDate?.ToShortDateString() ?? "未定"}\r\n" +
                           $"评分: {film.Score}\r\n" +
                           $"票房: {film.BoxOffice}\r\n" +
                           $"观影人次: {film.Admissions}\r\n";

            // 取出演员列表（假设方法在Program._filmRepository里）
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
