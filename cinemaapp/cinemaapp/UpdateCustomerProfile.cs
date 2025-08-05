using System;
using System.Drawing;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class UpdateCustomerProfile : Form
    {
        private Customer _customer;

        private Label lblName;
        private TextBox txtName;

        private Label lblPhone;
        private TextBox txtPhone;

        private Button btnSave;
        private Button btnCancel;

        public UpdateCustomerProfile(Customer customer)
        {
            _customer = customer;
            InitializeComponent();
            BuildUI();
            LoadCustomerData();
        }

        private void BuildUI()
        {
            this.Text = "更新个人资料";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblName = new Label() { Text = "姓名:", Location = new Point(30, 30), AutoSize = true };
            txtName = new TextBox() { Location = new Point(100, 25), Width = 220 };

            lblPhone = new Label() { Text = "手机号:", Location = new Point(30, 80), AutoSize = true };
            txtPhone = new TextBox() { Location = new Point(100, 75), Width = 220 };

            btnSave = new Button() { Text = "保存", Location = new Point(80, 140), Size = new Size(80, 30) };
            btnCancel = new Button() { Text = "取消", Location = new Point(200, 140), Size = new Size(80, 30) };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblPhone);
            Controls.Add(txtPhone);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
        }

        private void LoadCustomerData()
        {
            txtName.Text = _customer.Name;
            txtPhone.Text = _customer.PhoneNum;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string newName = txtName.Text.Trim();
            string newPhone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newPhone))
            {
                MessageBox.Show("姓名和手机号不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _customer.Name = newName;
            _customer.PhoneNum = newPhone;

            try
            {
                // 更新数据库
                Program._userService.UpdateCustomerProfile(_customer);

                // 重新从数据库获取最新用户数据（刷新全局引用）
                Program._loggedInCustomer = Program._customerRepository.GetCustomerById(_customer.CustomerID);

                MessageBox.Show("资料更新成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    
    }
}

