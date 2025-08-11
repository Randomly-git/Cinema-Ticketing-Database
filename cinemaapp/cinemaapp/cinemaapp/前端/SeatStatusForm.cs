using System;
using System.Drawing;
using System.Windows.Forms;
using test.Models;
using test.Services;
using System.Collections.Generic;

public class SeatStatusForm : Form
{
    private readonly Section _section;
    private readonly IShowingService _showingService;
    private readonly ISchedulingService _schedulingService;

    public SeatStatusForm(Section section, IShowingService showingService, ISchedulingService schedulingService)
    {
        if (section == null)
            throw new ArgumentNullException(nameof(section));
        if (showingService == null)
            throw new ArgumentNullException(nameof(showingService));

        if (section.ScheduleStartTime == default || section.ScheduleEndTime == default)
            throw new ArgumentException("无效的场次时间");

        _section = section;
        _showingService = showingService;
        _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));

        // 初始化窗体基础属性
        this.Text = $"电影：{_section.FilmName} | 日期：{_section.ScheduleStartTime:yyyy-MM-dd} | 时间：{_section.ScheduleStartTime:HH:mm} - {_section.ScheduleEndTime:HH:mm} | 影厅：{_section.HallNo}";
        this.Size = new Size(900, 700);
        this.AutoScroll = true;

        // 顶部工具栏
        var btnDelete = new Button()
        {
            Text = "删除排片",
            Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.IndianRed,
            AutoSize = true,
            Location = new Point(10, 10)
        };
        btnDelete.Click += BtnDelete_Click;
        this.Controls.Add(btnDelete);

        try
        {
            InitializeSeatLayout();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"初始化座位图时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        // 检查是否有已售座位
        var seatStatus = _showingService.GetHallSeatStatus(_section);
        bool hasSoldSeats = seatStatus.Any(row => row.Value.Values.Contains(SeatStatus.Sold));

        if (hasSoldSeats)
        {
            MessageBox.Show("该排片已有售出的座位，无法删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 二次确认
        var confirmResult = MessageBox.Show(
            "确定要删除该排片吗？此操作不可恢复！",
            "确认删除",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2 // 默认选中“否”
        );

        if (confirmResult != DialogResult.Yes)
        {
            return; // 用户取消删除
        }

        // 调用删除方法
        var result = _schedulingService.DeleteSection(_section.SectionID);
        if (result.Success)
        {
            MessageBox.Show("排片已删除。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 让父窗体收到信号刷新
            this.DialogResult = DialogResult.OK;

            // 关闭当前窗体
            this.Close();
        }
        else
        {
            MessageBox.Show($"删除失败：{result.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    private void InitializeSeatLayout()
    {
        this.Text = $"电影：{_section.FilmName} | 日期：{_section.ScheduleStartTime:yyyy-MM-dd} | 时间：{_section.ScheduleStartTime:HH:mm} - {_section.ScheduleEndTime:HH:mm} | 影厅：{_section.HallNo}";
        this.Size = new Size(900, 700); // 稍微加宽和加高，给图例和行列号留空间
        this.AutoScroll = true;

        var seatStatus = _showingService.GetHallSeatStatus(_section);

        if (seatStatus == null || seatStatus.Count == 0)
        {
            MessageBox.Show("未能获取座位状态数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int btnSize = 40;
        int padding = 10;
        int startX = 80; // 给左边留空间显示行号
        int startY = 80; // 给上边留空间显示列号

        // --- 先绘制列号 ---
        // 取所有列号，合并成一个列集合
        var allCols = new SortedSet<int>();
        foreach (var row in seatStatus)
        {
            foreach (var col in row.Value)
            {
                if (int.TryParse(col.Key, out int colIndex))
                    allCols.Add(colIndex);
            }
        }

        foreach (var colIndex in allCols)
        {
            var colLabel = new Label
            {
                Text = colIndex.ToString(),
                Width = btnSize,
                Height = btnSize,
                Left = startX + (colIndex - 1) * (btnSize + padding),
                Top = startY - btnSize - 5,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(colLabel);
        }

        // --- 再绘制行号和座位 ---
        foreach (var row in seatStatus)
        {
            string lineNo = row.Key;
            if (!int.TryParse(lineNo, out int rowIndex))
                continue;

            // 行号显示在左侧
            var rowLabel = new Label
            {
                Text = lineNo,
                Width = startX - 20,
                Height = btnSize,
                Left = 10,
                Top = startY + (rowIndex - 1) * (btnSize + padding),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(rowLabel);

            foreach (var col in row.Value)
            {
                string colNo = col.Key;
                var status = col.Value;

                if (!int.TryParse(colNo, out int colIndex))
                    continue;

                var seatBtn = new Label
                {
                    Width = btnSize,
                    Height = btnSize,
                    Left = startX + (colIndex - 1) * (btnSize + padding),
                    Top = startY + (rowIndex - 1) * (btnSize + padding),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = status == SeatStatus.Sold ? Color.LightGray : Color.LightGreen,
                    Tag = $"{lineNo}{colNo}"
                };

                var toolTip = new ToolTip();
                toolTip.SetToolTip(seatBtn, $"座位 {lineNo}排{colNo}列");

                this.Controls.Add(seatBtn);
            }
        }

        // --- 最后添加图例说明 Panel ---
        var legendPanel = new Panel
        {
            Width = 300,
            Height = 50,
            Left = 10,
            Top = this.ClientSize.Height - 60,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = Color.Transparent
        };

        // 售出座位
        var soldBox = new Panel
        {
            BackColor = Color.LightGray,
            Width = 20,
            Height = 20,
            Left = 10,
            Top = 15
        };
        var soldLabel = new Label
        {
            Text = "已售出",
            Left = soldBox.Right + 5,
            Top = 15,
            AutoSize = true
        };
        legendPanel.Controls.Add(soldBox);
        legendPanel.Controls.Add(soldLabel);

        // 可售座位
        var availableBox = new Panel
        {
            BackColor = Color.LightGreen,
            Width = 20,
            Height = 20,
            Left = soldLabel.Right + 30,
            Top = 15
        };
        var availableLabel = new Label
        {
            Text = "可售",
            Left = availableBox.Right + 5,
            Top = 15,
            AutoSize = true
        };
        legendPanel.Controls.Add(availableBox);
        legendPanel.Controls.Add(availableLabel);

        this.Controls.Add(legendPanel);
    }



}
