using System;
using System.Drawing;
using System.Windows.Forms;
using test.Services;

namespace cinemaapp
{
    public partial class SmartAutoScheduleForm : Form
    {
        private readonly ISchedulingService _schedulingService;

        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private TextBox txtTargetSessions;
        private Button btnConfirm;
        private Button btnCancel;
        private Label lblStatus;  // 状态标签

        public SmartAutoScheduleForm(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));

            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "智能自动排片";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblStartDate = new Label
            {
                Text = "开始日期：",
                Location = new Point(30, 30),
                AutoSize = true
            };
            this.Controls.Add(lblStartDate);

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(120, 25),
                Width = 200,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = DateTime.Today
            };
            this.Controls.Add(dtpStartDate);

            var lblEndDate = new Label
            {
                Text = "结束日期：",
                Location = new Point(30, 70),
                AutoSize = true
            };
            this.Controls.Add(lblEndDate);

            dtpEndDate = new DateTimePicker
            {
                Location = new Point(120, 65),
                Width = 200,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = DateTime.Today.AddDays(7)
            };
            this.Controls.Add(dtpEndDate);

            var lblTargetSessions = new Label
            {
                Text = "每天每个影厅的目标场次数：",
                Location = new Point(30, 110),
                AutoSize = true
            };
            this.Controls.Add(lblTargetSessions);

            txtTargetSessions = new TextBox
            {
                Location = new Point(200, 105),
                Width = 100,
                Text = "3"
            };
            this.Controls.Add(txtTargetSessions);

            lblStatus = new Label
            {
                Text = "状态：等待操作",
                Location = new Point(30, 145),
                AutoSize = true,
                ForeColor = Color.Blue
            };
            this.Controls.Add(lblStatus);

            btnConfirm = new Button
            {
                Text = "确认",
                Location = new Point(100, 180),
                Width = 100
            };
            btnConfirm.Click += BtnConfirm_Click;
            this.Controls.Add(btnConfirm);

            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(220, 180),
                Width = 100
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private async void BtnConfirm_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date;

            if (endDate < startDate)
            {
                MessageBox.Show("结束日期不能早于开始日期。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtTargetSessions.Text.Trim(), out int targetSessions) || targetSessions <= 0)
            {
                MessageBox.Show("请输入有效的每天目标场次数（正整数）。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnConfirm.Enabled = false;
            lblStatus.Text = "状态：正在排片，请稍候...";
            lblStatus.ForeColor = Color.Orange;

            try
            {
                var result = await System.Threading.Tasks.Task.Run(() =>
                    // _schedulingService.SmartAutoScheduleFilm(startDate, endDate, targetSessions)
                    _schedulingService.ImprovedSmartAutoScheduleFilm(startDate, endDate, targetSessions)
                );

                lblStatus.Text = "状态：操作完成";
                lblStatus.ForeColor = result.Success ? Color.Green : Color.Red;
                MessageBox.Show(result.Message, "结果", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (result.Success)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "状态：操作失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("排片过程中发生错误：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConfirm.Enabled = true;
            }
        }
    }
}
