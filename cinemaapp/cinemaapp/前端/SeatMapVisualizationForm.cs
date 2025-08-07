using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using test.Models;
using test.Services;

public partial class SeatMapVisualizationForm : Form
{
    private readonly ISchedulingService _schedulingService;
    private List<Section> _sections;

    private TextBox _txtScheduleList;
    private DateTimePicker _dtpStart;
    private DateTimePicker _dtpEnd;
    private Button _btnSearch;

    private Panel _schedulePanel;
    private DateTimePicker _datePicker;
    private Button _btnLoadSchedule;
    private ToolTip _toolTip;

    public SeatMapVisualizationForm(ISchedulingService schedulingService)
    {
        _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
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

        _toolTip = new ToolTip();
    }

    private void DrawScheduleGrid()
    {
        _schedulePanel.Controls.Clear();
        DateTime selectedDate = _datePicker.Value.Date;

        _sections = _schedulingService.GetSectionsByDateRange(selectedDate, selectedDate.AddDays(1));

        var hallList = _sections.Select(s => s.HallNo).Distinct().OrderBy(h => h).ToList();
        var startHour = 10;
        var endHour = 23;

        int cellWidth = 60;
        int cellHeight = 30;

        // 添加标题行
        for (int h = startHour; h <= endHour; h++)
        {
            var lbl = new Label
            {
                Text = $"{h}:00",
                Location = new Point((h - startHour + 1) * cellWidth, 0),
                Size = new Size(cellWidth, cellHeight),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            _schedulePanel.Controls.Add(lbl);
        }

        // 添加每一行
        for (int row = 0; row < hallList.Count; row++)
        {
            var hallNo = hallList[row];
            var lbl = new Label
            {
                Text = $"影厅 {hallNo}",
                Location = new Point(0, (row + 1) * cellHeight),
                Size = new Size(cellWidth, cellHeight),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            _schedulePanel.Controls.Add(lbl);

            // 获取该影厅的排片
            var hallSections = _sections.Where(s => s.HallNo == hallNo).ToList();

            foreach (var section in hallSections)
            {
                int start = section.ScheduleStartTime.Hour;
                int end = section.ScheduleEndTime.Hour;
                int left = (start - startHour + 1) * cellWidth;
                int top = (row + 1) * cellHeight;
                int width = Math.Max(1, end - start) * cellWidth;

                var box = new Panel
                {
                    BackColor = Color.LightBlue,
                    Location = new Point(left, top),
                    Size = new Size(width, cellHeight - 2),
                    BorderStyle = BorderStyle.FixedSingle
                };

                _toolTip.SetToolTip(box, $"{section.FilmName}\n{section.ScheduleStartTime:HH:mm} - {section.ScheduleEndTime:HH:mm}");
                _schedulePanel.Controls.Add(box);
            }
        }
    }



    private void LoadScheduleData()
    {
        DateTime start = _dtpStart.Value.Date;
        DateTime end = _dtpEnd.Value.Date.AddDays(1);  // 结束时间为次日 0 点

        if (start > end)
        {
            MessageBox.Show("起始日期不能晚于结束日期。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _sections = _schedulingService.GetSectionsByDateRange(start, end);
        ShowScheduleListText(start, end.AddDays(-1)); // 显示时减回一天，让文字更直观
    }

    private void ShowScheduleListText(DateTime start, DateTime end)
    {
        if (_sections == null || _sections.Count == 0)
        {
            _txtScheduleList.Text = $"[{start:yyyy-MM-dd}] 至 [{end:yyyy-MM-dd}] 无排片信息。";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[{start:yyyy-MM-dd}] 至 [{end:yyyy-MM-dd}] 排片信息：");
        sb.AppendLine("场次ID | 影厅号 | 电影名       | 开始时间           | 结束时间");
        sb.AppendLine("------------------------------------------------------------");

        foreach (var sec in _sections)
        {
            sb.AppendLine($"{sec.SectionID,6} | {sec.HallNo,6} | {sec.FilmName,-12} | {sec.ScheduleStartTime:yyyy-MM-dd HH:mm} | {sec.ScheduleEndTime:HH:mm}");
        }

        _txtScheduleList.Text = sb.ToString();
    }
}
