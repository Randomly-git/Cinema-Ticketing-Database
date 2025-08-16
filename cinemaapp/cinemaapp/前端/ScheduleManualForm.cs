using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class ManualScheduleForm : Form
    {
        private readonly ISchedulingService _schedulingService;
        private readonly DateTime? _defaultDate; // 保存外部传入的默认日期

        private ComboBox cboFilms;
        private ComboBox cboHalls;
        private DateTimePicker dtpDate;
        private DateTimePicker dtpTime;
        private Button btnConfirm;
        private Button btnCancel;

        public ManualScheduleForm(ISchedulingService schedulingService, DateTime? defaultDate = null)
        {
            _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
            _defaultDate = defaultDate?.Date; // 只保留日期部分

            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "手动排片";
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
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cboFilms);

            var lblHall = new Label
            {
                Text = "影厅：",
                Location = new Point(30, 80),
                AutoSize = true
            };
            this.Controls.Add(lblHall);

            cboHalls = new ComboBox
            {
                Location = new Point(100, 75),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cboHalls);

            var lblTime = new Label
            {
                Text = "开始时间：",
                Location = new Point(30, 130),
                AutoSize = true
            };
            this.Controls.Add(lblTime);

            dtpDate = new DateTimePicker
            {
                Location = new Point(100, 130),
                Width = 150,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                ShowUpDown = false,
                Value = _defaultDate ?? DateTime.Today, // 初始化时直接用默认日期
                Enabled = !_defaultDate.HasValue        // 如果有默认日期就禁用
            };
            this.Controls.Add(dtpDate);

            dtpTime = new DateTimePicker
            {
                Location = new Point(260, 130),
                Width = 100,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true
            };
            this.Controls.Add(dtpTime);

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
                Location = new Point(240, 200),
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
            cboFilms.DataSource = films;
            cboFilms.DisplayMember = "FilmName";
            cboFilms.ValueMember = "FilmName";

            var halls = _schedulingService.GetAllMovieHalls();
            if (!halls.Any())
            {
                MessageBox.Show("数据库中没有可用的影厅，请先添加影厅信息。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }
            cboHalls.DataSource = halls;
            cboHalls.DisplayMember = "HallNo";
            cboHalls.ValueMember = "HallNo";
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (cboFilms.SelectedItem == null || cboHalls.SelectedItem == null)
            {
                MessageBox.Show("请选择电影和影厅。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string filmName = ((Film)cboFilms.SelectedItem).FilmName;
            int hallNo = ((MovieHall)cboHalls.SelectedItem).HallNo;

            DateTime selectedDate = dtpDate.Value.Date;
            TimeSpan selectedTime = dtpTime.Value.TimeOfDay;
            DateTime scheduleStartTime = selectedDate.Add(selectedTime);

            var result = _schedulingService.AddSection(filmName, hallNo, scheduleStartTime);
            MessageBox.Show(result.Message, "结果", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (result.Success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
