using System;
using System.Drawing;
using System.Windows.Forms;
using test.Services;
using test.Models;

public partial class SeatMapForm : Form
{
    private IShowingService _showingService;
    private Section _section;

    public SeatMapForm(IShowingService showingService, Section section)
    {
        _showingService = showingService;
        _section = section;

        // 不调用 InitializeComponent，纯代码初始化
        this.Load += SeatMapForm_Load;
    }

    private void SeatMapForm_Load(object sender, EventArgs e)
    {
        this.Text = $"影厅 {_section.HallNo} 电影 {_section.FilmName} 座位图";
        this.Size = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;

        RenderSeats();
    }

    private void RenderSeats()
    {
        var availableSeats = _showingService.GetAvailableSeats(_section);

        // 假设固定行列，你可改成动态读取影厅行列数
        int rows = 10;
        int cols = 15;
        int seatSize = 30;
        int startX = 20;
        int startY = 20;

        // 清理之前的控件（防止重复渲染）
        this.Controls.Clear();

        for (int r = 0; r < rows; r++)
        {
            string rowLabel = ((char)('A' + r)).ToString();
            for (int c = 1; c <= cols; c++)
            {
                bool isAvailable = availableSeats.ContainsKey(rowLabel) && availableSeats[rowLabel].Contains(c.ToString());

                Button seatBtn = new Button
                {
                    Location = new Point(startX + (c - 1) * seatSize, startY + r * seatSize),
                    Size = new Size(seatSize - 2, seatSize - 2),
                    Text = isAvailable ? "√" : "X",
                    BackColor = isAvailable ? Color.LightGreen : Color.LightCoral,
                    Enabled = isAvailable,
                    Font = new Font("微软雅黑", 10, FontStyle.Bold),
                    Tag = $"{rowLabel}{c}"
                };

                // 你可以给按钮绑定点击事件，比如显示座位信息
                seatBtn.Click += SeatBtn_Click;

                this.Controls.Add(seatBtn);
            }
        }
    }

    private void SeatBtn_Click(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.Tag is string seat)
        {
            MessageBox.Show($"你点击了座位 {seat}", "座位信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
