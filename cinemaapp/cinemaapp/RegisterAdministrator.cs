using System;
using System.Drawing;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class RegisterAdministrator : Form
    {
        private TextBox txtAdminID;
        private TextBox txtName;
        private TextBox txtPhone;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;

        public RegisterAdministrator()
        {
            InitializeComponent();
            InitializeFormControls();
        }

        private void InitializeFormControls()
        {
            this.Text = "管理员注册";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 标签和文本框
            Label lblID = new Label { Text = "管理员 ID:", Location = new Point(30, 30), AutoSize = true };
            txtAdminID = new TextBox { Location = new Point(120, 25), Width = 200 };

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

            // 添加控件
            this.Controls.AddRange(new Control[]
            {
                lblID, txtAdminID,
                lblName, txtName,
                lblPhone, txtPhone,
                lblPassword, txtPassword,
                lblConfirm, txtConfirmPassword,
                btnRegister
            });
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string adminId = txtAdminID.Text.Trim();
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
                Administrator newAdmin = new Administrator
                {
                    AdminID = adminId,
                    AdminName = name,
                    PhoneNum = phone
                };

                Program._adminService.RegisterAdministrator(newAdmin, password);

                MessageBox.Show($"管理员 {name} 注册成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"注册失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
