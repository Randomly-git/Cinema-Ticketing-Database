using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

public partial class SeatMapVisualizationForm : Form
{
    private readonly ISchedulingService _schedulingService;
    private readonly IShowingService _showingService;
    private List<Section> _sections;

    private TextBox _txtScheduleList;
    private DateTimePicker _dtpStart;
    private DateTimePicker _dtpEnd;
    private Button _btnSearch;

    private Panel _schedulePanel;
    private DateTimePicker _datePicker;
    private Button _btnLoadSchedule;
    private ToolTip _toolTip;

    public SeatMapVisualizationForm(ISchedulingService schedulingService, IShowingService showingService)
    {
        _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
        _showingService = showingService ?? throw new ArgumentNullException(nameof(showingService));
        Load += SeatMapVisualizationForm_Load;
    }


    private void SeatMapVisualizationForm_Load(object sender, EventArgs e)
    {
        Text = "影院排片网格";
        Size = new Size(1000, 600);
        StartPosition = FormStartPosition.CenterParent;

        _datePicker = new DateTimePicker
        {
            Location = new Point(20, 20),
            Width = 200
        };
        Controls.Add(_datePicker);

        _btnLoadSchedule = new Button
        {
            Text = "加载排片",
            Location = new Point(230, 20)
        };
        _btnLoadSchedule.Click += (s, e) => DrawScheduleGrid();
        Controls.Add(_btnLoadSchedule);

        _schedulePanel = new Panel
        {
            Location = new Point(20, 60),
            AutoScroll = true,
            Size = new Size(940, 480),
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(_schedulePanel);

        // 初始化 ToolTip 并设置属性
        _toolTip = new ToolTip();
        _toolTip.InitialDelay = 100;
        _toolTip.ReshowDelay = 100;
        _toolTip.AutoPopDelay = 10000;
        _toolTip.ShowAlways = true;
    }

    private void DrawScheduleGrid()
    {
        _schedulePanel.Controls.Clear();
        DateTime selectedDate = _datePicker.Value.Date;

        _sections = _schedulingService.GetSectionsByDateRange(selectedDate, selectedDate.AddDays(1));

        var hallList = _sections.Select(s => s.HallNo).Distinct().OrderBy(h => h).ToList();

        DateTime startDateTime = selectedDate.AddHours(9);
        DateTime endDateTime = selectedDate.AddDays(1).AddHours(2);
        int totalMinutes = (int)(endDateTime - startDateTime).TotalMinutes;

        int cellWidth = 10;
        int cellHeight = 30;
        int hallLabelWidth = 70;

        var tooltipCache = new Dictionary<Section, string>();
        foreach (var section in _sections)
        {
            tooltipCache[section] = $"{section.ScheduleStartTime:HH:mm} - {section.ScheduleEndTime:HH:mm}";
        }

        // 添加时间轴标题行
        for (int i = 0; i < 17; i++)
        {
            DateTime hourTime = startDateTime.AddHours(i);
            var lbl = new Label
            {
                Text = $"{hourTime.Hour}:00",
                Location = new Point(hallLabelWidth + i * 6 * cellWidth, 0),
                Size = new Size(6 * cellWidth, cellHeight),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            _schedulePanel.Controls.Add(lbl);
        }

        // 添加每行影厅号及排片条
        for (int row = 0; row < hallList.Count; row++)
        {
            var hallNo = hallList[row];
            var lbl = new Label
            {
                Text = $"影厅 {hallNo}",
                Location = new Point(0, (row + 1) * cellHeight),
                Size = new Size(hallLabelWidth, cellHeight),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            _schedulePanel.Controls.Add(lbl);

            var hallSections = _sections.Where(s => s.HallNo == hallNo).ToList();

            foreach (var section in hallSections)
            {
                int startTotalMinutes = (int)(section.ScheduleStartTime - startDateTime).TotalMinutes;
                int endTotalMinutes = (int)(section.ScheduleEndTime - startDateTime).TotalMinutes;

                if (endTotalMinutes <= 0 || startTotalMinutes >= totalMinutes)
                    continue;

                startTotalMinutes = Math.Max(startTotalMinutes, 0);
                endTotalMinutes = Math.Min(endTotalMinutes, totalMinutes);

                int left = hallLabelWidth + (int)(startTotalMinutes * (cellWidth / 10.0));
                int width = (int)((endTotalMinutes - startTotalMinutes) * (cellWidth / 10.0));
                width = Math.Max(width, 5);

                int top = (row + 1) * cellHeight;

                var box = new Panel
                {
                    BackColor = Color.LightBlue,
                    Location = new Point(left, top),
                    Size = new Size(width, cellHeight - 2),
                    BorderStyle = BorderStyle.FixedSingle,
                    Cursor = Cursors.Hand,
                    Tag = section
                };

                var filmNameLabel = new Label
                {
                    Text = section.FilmName,
                    ForeColor = Color.Black,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    AutoEllipsis = true,
                    Font = new Font("Segoe UI", 8, FontStyle.Regular),
                    Cursor = Cursors.Hand // 让 label 可点击
                };

                // 调用 HandleBoxClick
                box.Click += (s, e) =>
                {
                    var clickedPanel = s as Panel;
                    var sec = clickedPanel?.Tag as Section;

                    if (sec != null)
                    {
                        HandleBoxClick(sec);
                    }
                };

                // 调用 HandleBoxClick，使用闭包保存的 section
                filmNameLabel.Click += (s, e) =>
                {
                    HandleBoxClick(section);
                };


                box.Controls.Add(filmNameLabel);

                _toolTip.SetToolTip(box, tooltipCache[section]);
                _toolTip.SetToolTip(filmNameLabel, tooltipCache[section]);

                _schedulePanel.Controls.Add(box);
            }
        }
    }

    private void HandleBoxClick(Section section)
    {
        var seatForm = new SeatStatusForm(section, _showingService);
        seatForm.ShowDialog();
    }


}
