using System;
using System.Drawing;
using System.Windows.Forms;
using test.Services;

namespace cinemaapp
{
    public partial class ScheduleForm : Form
    {
        private readonly ISchedulingService _schedulingService;

        public ScheduleForm(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
            this.Text = "排片管理";
            this.Size = new Size(900, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            SetupUI();
        }

        private void SetupUI()
        {
            // 主容器
            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            this.Controls.Add(mainPanel);

            // 添加三个卡片
            mainPanel.Controls.Add(CreateCard(
                "🎬 手动排片",
                "选择电影、影厅、开始时间，手动创建单场排片。",
                (s, e) => AddSectionInteractive()
            ));

            mainPanel.Controls.Add(CreateCard(
                "📦 批量排片",
                "指定日期范围和最大场次数量，批量创建选定电影的排片。",
                (s, e) => BatchScheduleFilmInteractive()
            ));

            mainPanel.Controls.Add(CreateCard(
                "🤖 智能排片",
                "根据智能策略自动为所有影厅安排场次。",
                (s, e) => SmartAutoScheduleFilmInteractive()
            ));
        }

        private Panel CreateCard(string title, string description, EventHandler clickHandler)
        {
            var card = new Panel
            {
                Size = new Size(220, 180),
                Margin = new Padding(20),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblTitle);

            var lblDesc = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10, 5, 10, 5),
                TextAlign = ContentAlignment.TopCenter
            };
            card.Controls.Add(lblDesc);

            var btn = new Button
            {
                Text = "开始",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btn.Click += clickHandler;
            card.Controls.Add(btn);

            return card;
        }

        // 三个功能按钮事件
        private void AddSectionInteractive()
        {
            using (var form = new ManualScheduleForm(_schedulingService))
            {
                form.ShowDialog(this);
            }
        }

        private void BatchScheduleFilmInteractive()
        {
            var batchForm = new BatchScheduleForm(_schedulingService);
            batchForm.ShowDialog();
        }

        private void SmartAutoScheduleFilmInteractive()
        {
            var smartForm = new SmartAutoScheduleForm(_schedulingService);
            smartForm.ShowDialog();
        }
    }
}
