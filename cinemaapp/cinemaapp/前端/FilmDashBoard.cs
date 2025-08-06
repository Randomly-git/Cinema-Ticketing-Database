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

        private ComboBox cmbOverviewFilms;
        private TextBox txtOverview;
        private TextBox txtCastResults;
        private TextBox txtCastSearch;
        private Button btnSearchCast;
        private ListBox lstCastResults;
        private ListBox lstCastMovies;

        private DataGridView dgvStatistics;
        private IFilmService _filmService;

        public FilmDashboard()
        {
            _filmService = Program._filmService;  // 赋值
            InitializeComponent();
            LoadShowingsFilms();
            LoadOverviewFilms();
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

            cmbShowingsFilms = new ComboBox
            {
                Location = new Point(20, 20),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbShowingsFilms.SelectedIndexChanged += (s, e) => LoadShowingsForSelectedFilm();

            dgvShowings = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(830, 450),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            tab.Controls.Add(cmbShowingsFilms);
            tab.Controls.Add(dgvShowings);
            return tab;
        }

        //
        private void LoadShowingsFilms()
        {
            try
            {
                var films = Program._filmService.GetAvailableFilms();
                cmbShowingsFilms.DataSource = films;
                cmbShowingsFilms.DisplayMember = "FilmName";
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载电影列表失败：" + ex.Message);
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
            var tab = new TabPage("影片概况");
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };

            cmbOverviewFilms = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOverviewFilms.SelectedIndexChanged += (s, e) => ShowFilmOverview();

            txtOverview = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Width = 800,
                Height = 400,
                ScrollBars = ScrollBars.Vertical
            };

            panel.Controls.Add(cmbOverviewFilms);
            panel.Controls.Add(txtOverview);
            tab.Controls.Add(panel);
            return tab;
        }

        private void LoadOverviewFilms()
        {
            try
            {
                var films = Program._filmService.GetAvailableFilms();
                cmbOverviewFilms.DataSource = films;
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

            txtOverview.Text = overview;
        }

        //查看演员演过的电影


        private DataGridView dgvCastResults;  // 成员变量

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
