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
    public partial class SeatSelectionForm : Form
    {
        private readonly Section _section;
        private readonly Film _film;
        private readonly Dictionary<string, Button> _seatButtons = new Dictionary<string, Button>();
        private readonly List<SeatSelection> _selectedSeats = new List<SeatSelection>();
        private readonly IShowingService _showingService = Program._showingService;
        private readonly IBookingService _bookingService = Program._bookingService;
        private readonly Customer _loggedInCustomer;
        private MainForm _mainForm;
        public List<SeatSelection> SelectedSeats => _selectedSeats;

        public SeatSelectionForm(Section section, Film film, Customer loggedInCustomer,MainForm mainForm)
        {
            InitializeComponent();
            _section = section;
            _film = film;
            _loggedInCustomer = loggedInCustomer;
            _mainForm = mainForm;

            InitializeUI();
            LoadSeatMap();
        }

        private void InitializeUI()
        {
            Text = $"{_film.FilmName} - {_section.TimeSlot.StartTime:HH\\:mm} 影厅{_section.MovieHall.HallNo}";
            lblFilmInfo.Text = $"{_film.FilmName} | {_section.TimeSlot.StartTime:HH\\:mm}-{_section.TimeSlot.EndTime:HH\\:mm}";
            lblHallInfo.Text = $"影厅: {_section.MovieHall.HallNo}";

            btnConfirm.Enabled = false;
        }

        private void LoadSeatMap()
        {
            panelSeats.Controls.Clear();
            _seatButtons.Clear();

            // 获取座位状态
            var seatStatus = _showingService.GetHallSeatStatus(_section);

            // 创建座位按钮
            int startX = 50;
            int startY = 50;
            int buttonSize = 30;
            int spacing = 5;

            // 添加屏幕标识
            var lblScreen = new Label
            {
                Text = "—————— 银 幕 ——————",
                Location = new Point(startX + (_section.MovieHall.ColumnsCount * (buttonSize + spacing)) / 2 - 100, startY),
                AutoSize = true,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                ForeColor = Color.Blue
            };
            panelSeats.Controls.Add(lblScreen);
            startY += 40;

            // 创建列号标签
            for (int col = 1; col <= _section.MovieHall.ColumnsCount; col++)
            {
                var lblCol = new Label
                {
                    Text = col.ToString(),
                    Location = new Point(startX + (col - 1) * (buttonSize + spacing) + buttonSize / 2 - 5, startY),
                    AutoSize = true,
                    Font = new Font("微软雅黑", 8)
                };
                panelSeats.Controls.Add(lblCol);
            }
            startY += 20;

            // 创建行座位
            foreach (var row in seatStatus.OrderBy(r => r.Key))
            {
                string lineNo = row.Key;

                // 添加行号标签
                var lblRow = new Label
                {
                    Text = lineNo,
                    Location = new Point(20, startY + (buttonSize / 2) - 8),
                    AutoSize = true,
                    Font = new Font("微软雅黑", 9, FontStyle.Bold)
                };
                panelSeats.Controls.Add(lblRow);

                // 创建该行座位
                int col = 1;
                foreach (var seat in row.Value.OrderBy(s => int.Parse(s.Key)))
                {
                    var btnSeat = new Button
                    {
                        //Text = "○",
                        Tag = new SeatSelection { LineNo = lineNo, ColumnNo = col },
                        Location = new Point(startX + (col - 1) * (buttonSize + spacing), startY),
                        Size = new Size(buttonSize, buttonSize),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("微软雅黑", 10)
                    };

                    // 设置座位状态
                    if (seat.Value == SeatStatus.Sold)
                    {
                        //btnSeat.Text = "×";
                        btnSeat.ForeColor = Color.Red;
                        btnSeat.Enabled = false;
                        btnSeat.FlatAppearance.BorderColor = Color.Red;
                    }
                    else
                    {
                        btnSeat.ForeColor = Color.Green;
                        btnSeat.Click += SeatButton_Click;
                        btnSeat.FlatAppearance.BorderColor = Color.Green;
                    }

                    panelSeats.Controls.Add(btnSeat);
                    _seatButtons.Add($"{lineNo}{col}", btnSeat);
                    col++;
                }
                startY += buttonSize + spacing;
            }

            // 添加图例说明
            var lblLegend = new Label
            {
                Text = "可用(绿色)  已售(红色)",
                Location = new Point(startX, startY + 20),
                AutoSize = true,
                Font = new Font("微软雅黑", 9)
            };
            panelSeats.Controls.Add(lblLegend);
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var seat = (SeatSelection)button.Tag;

            if (_selectedSeats.Any(s => s.LineNo == seat.LineNo && s.ColumnNo == seat.ColumnNo))
            {
                // 取消选择
                _selectedSeats.RemoveAll(s => s.LineNo == seat.LineNo && s.ColumnNo == seat.ColumnNo);
                button.Text = " ";
                button.ForeColor = Color.Green;
                button.FlatAppearance.BorderColor = Color.Green;
            }
            else
            {
                // 选择座位
                _selectedSeats.Add(seat);
                button.Text = "✓";
                button.ForeColor = Color.Blue;
                button.FlatAppearance.BorderColor = Color.Blue;
            }

            UpdateSelectionInfo();
        }

        private void UpdateSelectionInfo()
        {
            lblSelectedSeats.Text = $"已选座位: {string.Join(", ", _selectedSeats.Select(s => $"{s.LineNo}{s.ColumnNo}"))}";
            lblTotalPrice.Text = $"总价: {_selectedSeats.Sum(s => _bookingService.CalculateFinalTicketPrice(_section, _loggedInCustomer, s.LineNo)):C}";

            btnConfirm.Enabled = _selectedSeats.Any();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!_selectedSeats.Any())
            {
                MessageBox.Show("请至少选择一个座位", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

           // 打开支付窗体
           var paymentForm = new PaymentForm(_film, _section, _selectedSeats,_loggedInCustomer,_mainForm);
           if (paymentForm.ShowDialog() == DialogResult.OK)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }

    public class SeatSelection
    {
        public string LineNo { get; set; }
        public int ColumnNo { get; set; }
    }
}
