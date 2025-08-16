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
    public partial class PaymentForm : Form
    {
        private readonly Film _film;
        private readonly Section _section;
        private readonly List<SeatSelection> _selectedSeats;
        private decimal _totalPrice;
        private readonly IBookingService _bookingService = Program._bookingService;
        private readonly Customer _loggedInCustomer;
        private MainForm _mainForm;

        public PaymentForm(Film film, Section section, List<SeatSelection> selectedSeats,Customer loggedInCustomer,MainForm mainForm)
        {
            InitializeComponent();
            _film = film;
            _section = section;
            _selectedSeats = selectedSeats;
            _loggedInCustomer = loggedInCustomer;
            _mainForm = mainForm;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "支付订单";

            // 计算总价
            _totalPrice = _selectedSeats.Sum(s =>
                _bookingService.CalculateFinalTicketPrice(_section, _loggedInCustomer, s.LineNo));

            // 显示订单信息
            lblFilmName.Text = _film.FilmName;
            lblShowtime.Text = $"{_section.TimeSlot.StartTime:HH\\:mm} 影厅{_section.MovieHall.HallNo}";
            lblSeats.Text = string.Join(", ", _selectedSeats.Select(s => $"{s.LineNo}{s.ColumnNo}"));
            lblTotalPrice.Text = _totalPrice.ToString("C");
            lblVIPPoints.Text = $"当前积分: {_loggedInCustomer.VIPCard?.Points ?? 0}";

            // 设置支付方式
            cmbPaymentMethod.Items.AddRange(new object[] { "微信支付", "支付宝", "银行卡", "现金" });
            cmbPaymentMethod.SelectedIndex = 0;
        }

        private void btnCompletePayment_Click(object sender, EventArgs e)
        {
            if (cmbPaymentMethod.SelectedItem == null)
            {
                MessageBox.Show("请选择支付方式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string paymentMethod = cmbPaymentMethod.SelectedItem.ToString();

            try
            {
                // 创建订单
                var orders = _bookingService.PurchaseMultipleTickets(
                    _section.SectionID,
                    _selectedSeats.Select(s => new SeatHall { LINENO = s.LineNo, ColumnNo = s.ColumnNo }).ToList(),
                    _loggedInCustomer.CustomerID,
                    paymentMethod,
                    0 // 这里可以添加积分支付逻辑
                );
                //刷新主界面的用户信息标签
                _mainForm.UpdateUserInfoLabel();

                MessageBox.Show($"购票成功! 订单号: {string.Join(", ", orders.Select(o => o.OrderID))}",
                    "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"购票失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
