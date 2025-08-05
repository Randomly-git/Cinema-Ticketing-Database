using System;
using System.Drawing;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class RegisterCustomer : Form
    {
        // 控件定义
        private TextBox txtCustomerID;
        private TextBox txtName;
        private TextBox txtPhone;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;

        public RegisterCustomer()
        {
            InitializeComponent();
            InitializeFormControls(); // 控件初始化
        }

        private void InitializeFormControls()
        {
            this.Text = "顾客注册";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 标签 + 文本框
            Label lblID = new Label { Text = "用户 ID:", Location = new Point(30, 30), AutoSize = true };
            txtCustomerID = new TextBox { Location = new Point(120, 25), Width = 200 };

            Label lblName = new Label { Text = "姓名:", Location = new Point(30, 70), AutoSize = true };
            txtName = new TextBox { Location = new Point(120, 65), Width = 200 };

            Label lblPhone = new Label { Text = "手机号:", Location = new Point(30, 110), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(120, 105), Width = 200 };

            Label lblPassword = new Label { Text = "密码:", Location = new Point(30, 150), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(120, 145), Width = 200, PasswordChar = '*' };

            Label lblConfirm = new Label { Text = "确认密码:", Location = new Point(30, 190), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(120, 185), Width = 200, PasswordChar = '*' };

            btnRegister = new Button
            {
                Text = "注册",
                Location = new Point(140, 240),
                Size = new Size(100, 35)
            };
            btnRegister.Click += btnRegister_Click;

            // 加入控件
            this.Controls.AddRange(new Control[]
            {
                lblID, txtCustomerID,
                lblName, txtName,
                lblPhone, txtPhone,
                lblPassword, txtPassword,
                lblConfirm, txtConfirmPassword,
                btnRegister
            });
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string customerId = txtCustomerID.Text.Trim();
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (password != confirmPassword)
            {
                MessageBox.Show("两次密码输入不一致，请重新输入！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Customer newCustomer = new Customer
                {
                    CustomerID = customerId,
                    Username = customerId,
                    Name = name,
                    PhoneNum = phone,
                    VipLevel = 0
                };

                Program._userService.RegisterCustomer(newCustomer, password);

                MessageBox.Show($"注册成功，欢迎你 {name}！", "注册成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // 注册成功后关闭窗口
            }
            catch (Exception ex)
            {
                MessageBox.Show($"注册失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
