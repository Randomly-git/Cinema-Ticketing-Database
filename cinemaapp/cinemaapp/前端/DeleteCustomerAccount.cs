using System;
using System.Drawing;
using System.Windows.Forms;
using test.Services;
using test.Models;

namespace cinemaapp
{
    public partial class DeleteCustomerAccount : Form
    {
        private Label lblPrompt;
        private TextBox txtPassword;
        private Button btnDelete;
        private Button btnCancel;

        public DeleteCustomerAccount()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "删除账户确认";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblPrompt = new Label();
            lblPrompt.Text = "请输入密码以确认删除账户：";
            lblPrompt.AutoSize = true;
            lblPrompt.Location = new Point(30, 30);
            this.Controls.Add(lblPrompt);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(30, 60);
            txtPassword.Width = 300;
            txtPassword.PasswordChar = '*';
            this.Controls.Add(txtPassword);

            btnDelete = new Button();
            btnDelete.Text = "删除";
            btnDelete.Size = new Size(80, 30);
            btnDelete.Location = new Point(70, 110);
            btnDelete.Click += btnDelete_Click;
            this.Controls.Add(btnDelete);

            btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Size = new Size(80, 30);
            btnCancel.Location = new Point(200, 110);
            btnCancel.Click += btnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入密码以确认删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var customer = Program._userService.AuthenticateCustomer(Program._loggedInCustomer.CustomerID, password);
                if (customer == null)
                {
                    MessageBox.Show("密码错误，无法删除账户", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Program._userService.DeleteCustomerAccount(Program._loggedInCustomer.CustomerID);
                MessageBox.Show("账户删除成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
