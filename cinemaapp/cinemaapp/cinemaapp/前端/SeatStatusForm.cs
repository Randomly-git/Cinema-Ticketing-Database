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
            Font = new Font("Microsoft YaHei", 15, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.IndianRed,
            AutoSize = true,
            Location = new Point(450, 10)
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
        this.Size = new Size(950, 750); // 稍微扩大窗体以适应边距
        this.AutoScroll = true;

        var seatStatus = _showingService.GetHallSeatStatus(_section);
        if (seatStatus == null || seatStatus.Count == 0)
        {
            MessageBox.Show("未能获取座位状态数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // ===== 1. 定义布局参数 =====
        int btnSize = 40;               // 座位按钮大小
        int padding = 10;               // 座位间距
        int borderPadding = 25;         // 四周留白（左/右/上/下统一）
        int legendHeight = 40;          // 图例区域高度

        // 计算起始坐标（考虑左边距和上图例）
        int startX = 50;
        int startY = borderPadding + legendHeight + 20; // 图例下方留20px空隙

        // ===== 2. 顶部图例 =====
        var legendPanel = new Panel
        {
            Width = this.ClientSize.Width - 2 * borderPadding,
            Height = legendHeight,
            Left = borderPadding,
            Top = borderPadding,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right // 随窗体拉伸
        };

        // 售出座位图例
        var soldBox = new Panel
        {
            BackColor = Color.LightGray,
            Width = 20,
            Height = 20,
            Left = 10,
            Top = 10,
            BorderStyle = BorderStyle.FixedSingle
        };
        var soldLabel = new Label
        {
            Text = "已售出",
            Left = soldBox.Right + 5,
            Top = 10,
            AutoSize = true,
            Font = new Font("Arial", 10)
        };

        // 可售座位图例
        var availableBox = new Panel
        {
            BackColor = Color.LightGreen,
            Width = 20,
            Height = 20,
            Left = soldLabel.Right + 30,
            Top = 10,
            BorderStyle = BorderStyle.FixedSingle
        };
        var availableLabel = new Label
        {
            Text = "可售",
            Left = availableBox.Right + 5,
            Top = 10,
            AutoSize = true,
            Font = new Font("Arial", 10)
        };

        legendPanel.Controls.AddRange(new Control[] { soldBox, soldLabel, availableBox, availableLabel });
        this.Controls.Add(legendPanel);

        // ===== 3. 绘制列号（顶部） =====
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
                Height = 20, // 列号标签高度减小
                Left = startX + (colIndex - 1) * (btnSize + padding),
                Top = startY - 25, // 上移列号标签
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Bold) // 字体略小
            };
            this.Controls.Add(colLabel);
        }

        // ===== 4. 绘制行号（左侧）和座位 =====
        int maxRowHeight = 0; // 记录最大行高度（用于下边距计算）

        foreach (var row in seatStatus)
        {
            string lineNo = row.Key;
            int rowPosY = startY + (lineNo[0] - 'A') * (btnSize + padding);
            maxRowHeight = Math.Max(maxRowHeight, rowPosY + btnSize);

            // 行号标签（左侧）
            var rowLabel = new Label
            {
                Text = lineNo,
                Width = 30, // 行号宽度
                Height = btnSize,
                Left = 0,
                Top = rowPosY,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            this.Controls.Add(rowLabel);

            // 每个座位
            foreach (var col in row.Value)
            {
                if (!int.TryParse(col.Key, out int colIndex)) continue;

                var seatBtn = new Label
                {
                    Width = btnSize,
                    Height = btnSize,
                    Left = startX + (colIndex - 1) * (btnSize + padding),
                    Top = rowPosY,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = col.Value == SeatStatus.Sold ? Color.LightGray : Color.LightGreen,
                    Tag = $"{lineNo}{col.Key}"
                };

                new ToolTip().SetToolTip(seatBtn, $"座位 {lineNo}排{col.Key}列");
                this.Controls.Add(seatBtn);
            }
        }

        // ===== 5. 下边距处理 =====
        this.AutoScrollMinSize = new Size(
            startX + allCols.Count * (btnSize + padding) + borderPadding, // 宽度
            maxRowHeight + borderPadding // 高度
        );
    }



}
