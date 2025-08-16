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
    public partial class FilmManagementForm : Form
    {
        private readonly IAdministratorService _adminService;
        private readonly IFilmService _filmService;

        public FilmManagementForm(IAdministratorService adminService, IFilmService filmService)
        {
            InitializeComponent();
            _adminService = adminService;
            _filmService = filmService;
        }

        private void FilmManagementForm_Load(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0; // 默认选中添加电影标签页
            LoadFilmsForUpdate(); // 加载电影列表到更新页面的下拉框
        }

        // 加载所有电影到更新页面的下拉框
        private void LoadFilmsForUpdate()
        {
            try
            {
                cmbUpdateFilms.Items.Clear();
                var films = _filmService.GetAvailableFilms();

                if (films == null || films.Count == 0)
                {
                    MessageBox.Show("当前没有可修改的电影，请先添加电影。", "提示",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (var film in films)
                {
                    cmbUpdateFilms.Items.Add(film.FilmName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载电影列表失败: {ex.Message}", "错误",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 添加电影按钮点击事件
        private void btnAddFilm_Click(object sender, EventArgs e)
        {
            if (!ValidateAddFilmInputs())
                return;

            var newFilm = new Film
            {
                FilmName = txtFilmName.Text,
                Genre = txtGenre.Text,
                FilmLength = int.Parse(txtFilmLength.Text),
                NormalPrice = decimal.Parse(txtNormalPrice.Text),
                ReleaseDate = dtpReleaseDate.Value,
                Admissions = 0,
                BoxOffice = 0,
                Score = 0,
                RatingNum = 0
            };

            try
            {
                _adminService.AddFilm(newFilm);
                MessageBox.Show($"电影《{newFilm.FilmName}》添加成功！", "成功",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearAddFilmFields();
                LoadFilmsForUpdate(); // 刷新更新页面的电影列表
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加电影失败: {ex.Message}", "错误",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 更新电影选择变化事件
        private void cmbUpdateFilms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUpdateFilms.SelectedItem == null)
                return;

            string selectedFilm = cmbUpdateFilms.SelectedItem.ToString();

            try
            {
                var film = _filmService.GetFilmDetails(selectedFilm);

                if (film != null)
                {
                    // 填充电影信息到表单
                    txtUpdateFilmName.Text = film.FilmName;
                    txtUpdateGenre.Text = film.Genre;
                    txtUpdateFilmLength.Text = film.FilmLength.ToString();
                    txtUpdateNormalPrice.Text = film.NormalPrice.ToString("F2");

                    // 设置日期控件
                    dtpUpdateReleaseDate.Value = film.ReleaseDate ?? DateTime.Now;

                    // 下映日期处理（使用ShowCheckBox表示可选）
                    dtpUpdateEndDate.Checked = film.EndDate.HasValue;
                    if (film.EndDate.HasValue)
                    {
                        dtpUpdateEndDate.Value = film.EndDate.Value;
                    }

                    // 其他信息
                    txtUpdateScore.Text = film.Score.ToString();
                    txtUpdateBoxOffice.Text = film.BoxOffice.ToString();
                    txtUpdateAdmissions.Text = film.Admissions.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取电影信息失败: {ex.Message}", "错误",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 更新电影按钮点击事件
        private void btnUpdateFilm_Click(object sender, EventArgs e)
        {
            if (cmbUpdateFilms.SelectedItem == null)
            {
                MessageBox.Show("请先选择要更新的电影", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateUpdateFilmInputs())
                return;

            var updatedFilm = new Film
            {
                FilmName = txtUpdateFilmName.Text,
                Genre = txtUpdateGenre.Text,
                FilmLength = int.Parse(txtUpdateFilmLength.Text),
                NormalPrice = decimal.Parse(txtUpdateNormalPrice.Text),
                ReleaseDate = dtpUpdateReleaseDate.Value,
                EndDate = dtpUpdateEndDate.Checked ? dtpUpdateEndDate.Value : (DateTime?)null,
                Score = decimal.Parse(txtUpdateScore.Text),
                BoxOffice = decimal.Parse(txtUpdateBoxOffice.Text),
                Admissions = int.Parse(txtUpdateAdmissions.Text)
            };

            try
            {
                _adminService.UpdateFilm(updatedFilm);
                MessageBox.Show($"电影《{updatedFilm.FilmName}》更新成功！", "成功",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFilmsForUpdate(); // 刷新电影列表
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新电影失败: {ex.Message}", "错误",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 清空更新电影字段
        private void btnClearUpdateFields_Click(object sender, EventArgs e)
        {
            cmbUpdateFilms.SelectedIndex = -1;
            ClearUpdateFilmFields();
        }

        // 清空添加电影字段
        private void ClearAddFilmFields()
        {
            txtFilmName.Text = "";
            txtGenre.Text = "";
            txtFilmLength.Text = "";
            txtNormalPrice.Text = "";
            dtpReleaseDate.Value = DateTime.Now;
        }

        // 清空更新电影字段
        private void ClearUpdateFilmFields()
        {
            txtUpdateFilmName.Text = "";
            txtUpdateGenre.Text = "";
            txtUpdateFilmLength.Text = "";
            txtUpdateNormalPrice.Text = "";
            dtpUpdateReleaseDate.Value = DateTime.Now;
            dtpUpdateEndDate.Value = DateTime.Now;
            dtpUpdateEndDate.Checked = false;
            txtUpdateScore.Text = "0";
            txtUpdateBoxOffice.Text = "0";
            txtUpdateAdmissions.Text = "0";
        }

        // 验证添加电影输入
        private bool ValidateAddFilmInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFilmName.Text))
            {
                MessageBox.Show("请输入电影名称", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtGenre.Text))
            {
                MessageBox.Show("请输入电影类型", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtFilmLength.Text, out _) || int.Parse(txtFilmLength.Text) <= 0)
            {
                MessageBox.Show("请输入有效的电影时长(正整数)", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtNormalPrice.Text, out _) || decimal.Parse(txtNormalPrice.Text) <= 0)
            {
                MessageBox.Show("请输入有效的票价(正数)", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // 验证更新电影输入
        private bool ValidateUpdateFilmInputs()
        {
            if (string.IsNullOrWhiteSpace(txtUpdateFilmName.Text))
            {
                MessageBox.Show("电影名称不能为空", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUpdateGenre.Text))
            {
                MessageBox.Show("电影类型不能为空", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtUpdateFilmLength.Text, out _) || int.Parse(txtUpdateFilmLength.Text) <= 0)
            {
                MessageBox.Show("请输入有效的电影时长(正整数)", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtUpdateNormalPrice.Text, out _) || decimal.Parse(txtUpdateNormalPrice.Text) <= 0)
            {
                MessageBox.Show("请输入有效的票价(正数)", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 验证评分输入（允许小数）
            if (!decimal.TryParse(txtUpdateScore.Text, out decimal score) || score < 0 || score > 10)
            {
                MessageBox.Show("评分必须是0-10之间的数字", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtUpdateBoxOffice.Text, out _) || decimal.Parse(txtUpdateBoxOffice.Text) < 0)
            {
                MessageBox.Show("票房必须是非负数", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtUpdateAdmissions.Text, out _) || int.Parse(txtUpdateAdmissions.Text) < 0)
            {
                MessageBox.Show("观影人次必须是非负整数", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 新增日期验证
            if (dtpUpdateEndDate.Checked && dtpUpdateReleaseDate.Value > dtpUpdateEndDate.Value)
            {
                MessageBox.Show("上映日期不能晚于下映日期", "提示",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
