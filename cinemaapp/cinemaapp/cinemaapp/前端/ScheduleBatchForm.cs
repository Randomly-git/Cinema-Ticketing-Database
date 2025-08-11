using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class BatchScheduleForm : Form
    {
        private readonly ISchedulingService _schedulingService;

        private ComboBox cboFilms;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private TextBox txtMaxSessions;
        private Button btnConfirm;
        private Button btnCancel;

        public BatchScheduleForm(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));

            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "批量排片";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblFilm = new Label
            {
                Text = "电影：",
                Location = new Point(30, 30),
                AutoSize = true
            };
            this.Controls.Add(lblFilm);

            cboFilms = new ComboBox
            {
                Location = new Point(100, 25),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cboFilms);

            var lblStartDate = new Label
            {
                Text = "开始日期：",
                Location = new Point(30, 75),
                AutoSize = true
            };
            this.Controls.Add(lblStartDate);

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(100, 70),
                Width = 150,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = DateTime.Today
            };
            this.Controls.Add(dtpStartDate);

            var lblEndDate = new Label
            {
                Text = "结束日期：",
                Location = new Point(30, 115),
                AutoSize = true
            };
            this.Controls.Add(lblEndDate);

            dtpEndDate = new DateTimePicker
            {
                Location = new Point(100, 110),
                Width = 150,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = DateTime.Today.AddDays(7)
            };
            this.Controls.Add(dtpEndDate);

            var lblMaxSessions = new Label
            {
                Text = "最多场次数量：",
                Location = new Point(30, 155),
                AutoSize = true
            };
            this.Controls.Add(lblMaxSessions);

            txtMaxSessions = new TextBox
            {
                Location = new Point(130, 150),
                Width = 100,
                Text = "10" // 默认值
            };
            this.Controls.Add(txtMaxSessions);

            btnConfirm = new Button
            {
                Text = "确认",
                Location = new Point(100, 200),
                Width = 100
            };
            btnConfirm.Click += BtnConfirm_Click;
            this.Controls.Add(btnConfirm);

            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(220, 200),
                Width = 100
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void LoadData()
        {
            var films = _schedulingService.GetAllFilms();
            if (!films.Any())
            {
                MessageBox.Show("数据库中没有可用的电影，请先添加电影信息。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }
            cboFilms.DataSource = films.ToList();
            cboFilms.DisplayMember = "FilmName";
            cboFilms.ValueMember = "FilmName";
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (cboFilms.SelectedItem == null)
            {
                MessageBox.Show("请选择电影。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string filmName = ((Film)cboFilms.SelectedItem).FilmName;
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date;

            if (endDate < startDate)
            {
                MessageBox.Show("结束日期不能早于开始日期。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtMaxSessions.Text.Trim(), out int maxSessions) || maxSessions <= 0)
            {
                MessageBox.Show("请输入有效的最多场次数量（正整数）。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = _schedulingService.BatchScheduleFilm(filmName, startDate, endDate, maxSessions);
            MessageBox.Show(result.Message, "结果", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (result.Success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
