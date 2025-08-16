
using System;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class LoginAdministrator : Form
    {
        private IAdministratorService _adminService;
        public Administrator LoggedInAdmin { get; private set; }

        private TextBox txtAdminId;
        private TextBox txtPassword;
        private Button btnLogin;

        public LoginAdministrator(IAdministratorService adminService)
        {
            _adminService = adminService;
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "管理员登录";
            this.Size = new System.Drawing.Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblAdminId = new Label() { Text = "管理员ID:", Location = new System.Drawing.Point(20, 30), AutoSize = true };
            txtAdminId = new TextBox() { Location = new System.Drawing.Point(100, 25), Width = 150 };

            Label lblPassword = new Label() { Text = "密码:", Location = new System.Drawing.Point(20, 70), AutoSize = true };
            txtPassword = new TextBox() { Location = new System.Drawing.Point(100, 65), Width = 150, UseSystemPasswordChar = true };

            btnLogin = new Button() { Text = "登录", Location = new System.Drawing.Point(100, 110), Width = 80 };
            btnLogin.Click += BtnLogin_Click;

            this.Controls.Add(lblAdminId);
            this.Controls.Add(txtAdminId);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string adminId = txtAdminId.Text.Trim();
            string password = txtPassword.Text;

            try
            {
                var admin = _adminService.AuthenticateAdministrator(adminId, password);
                if (admin != null)
                {
                    LoggedInAdmin = admin;
                    MessageBox.Show($"登录成功，欢迎管理员 {admin.AdminName ?? admin.AdminID}！");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("登录失败：管理员ID或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录异常: {ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
