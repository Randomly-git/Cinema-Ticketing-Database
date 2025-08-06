using System;
using System.Windows.Forms;
using test.Models;
using test.Repositories;
using test.Services;

namespace cinemaapp
{
    public partial class LoginCustomer : Form
    {
        private IUserService _userService;
        private ICustomerRepository _customerRepository;

        public Customer LoggedInCustomer { get; private set; }

        private TextBox txtCustomerId;
        private TextBox txtPassword;
        private Button btnLogin;

        public LoginCustomer(IUserService userService, ICustomerRepository customerRepository)
        {
            _userService = userService;
            _customerRepository = customerRepository;

            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "顾客登录";
            this.Size = new System.Drawing.Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblCustomerId = new Label() { Text = "用户ID:", Location = new System.Drawing.Point(20, 30), AutoSize = true };
            txtCustomerId = new TextBox() { Location = new System.Drawing.Point(100, 25), Width = 150 };

            Label lblPassword = new Label() { Text = "密码:", Location = new System.Drawing.Point(20, 70), AutoSize = true };
            txtPassword = new TextBox() { Location = new System.Drawing.Point(100, 65), Width = 150, UseSystemPasswordChar = true };

            btnLogin = new Button() { Text = "登录", Location = new System.Drawing.Point(100, 110), Width = 80 };
            btnLogin.Click += BtnLogin_Click;

            this.Controls.Add(lblCustomerId);
            this.Controls.Add(txtCustomerId);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string customerId = txtCustomerId.Text.Trim();
            string password = txtPassword.Text;

            try
            {
                var customer = _userService.AuthenticateCustomer(customerId, password);
                if (customer != null)
                {
                    // 登录成功，获取完整顾客信息（包括积分/等级等）
                    LoggedInCustomer = _customerRepository.GetCustomerById(customer.CustomerID);
                    MessageBox.Show($"登录成功！欢迎您，{LoggedInCustomer.Name}！");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("登录失败：用户ID或密码不正确。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录失败：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
