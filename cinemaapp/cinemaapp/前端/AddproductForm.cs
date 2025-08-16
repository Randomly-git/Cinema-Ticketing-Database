using System;
using System.Drawing;
using System.Windows.Forms;
using test.Models; // 你的 RelatedProduct 模型所在命名空间

namespace cinemaapp
{
    public partial class AddProductForm : Form
    {
        private TextBox txtName;
        private TextBox txtPrice;
        private TextBox txtStock;
        private TextBox txtPoints;
        private Button btnAdd;
        private Button btnCancel;

        public AddProductForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "添加新周边产品";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblName = new Label { Text = "产品名称：", Location = new Point(30, 30), AutoSize = true };
            txtName = new TextBox { Location = new Point(150, 28), Width = 180 };

            Label lblPrice = new Label { Text = "产品价格：", Location = new Point(30, 70), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(150, 68), Width = 180 };

            Label lblStock = new Label { Text = "库存数量：", Location = new Point(30, 110), AutoSize = true };
            txtStock = new TextBox { Location = new Point(150, 108), Width = 180 };

            Label lblPoints = new Label { Text = "兑换积分：", Location = new Point(30, 150), AutoSize = true };
            txtPoints = new TextBox { Location = new Point(150, 148), Width = 180 };

            btnAdd = new Button { Text = "添加", Location = new Point(80, 200), Size = new Size(100, 30) };
            btnCancel = new Button { Text = "取消", Location = new Point(200, 200), Size = new Size(100, 30) };

            btnAdd.Click += BtnAdd_Click;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblPrice, txtPrice,
                lblStock, txtStock,
                lblPoints, txtPoints,
                btnAdd, btnCancel
            });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                RelatedProduct product = new RelatedProduct();

                // 产品名称
                product.ProductName = txtName.Text.Trim();
                if (string.IsNullOrEmpty(product.ProductName))
                    throw new ArgumentException("产品名称不能为空");

                // 价格
                if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price < 0)
                    throw new ArgumentException("价格必须是有效的非负数");
                product.Price = price;

                // 库存
                if (!int.TryParse(txtStock.Text.Trim(), out int stock) || stock < 0)
                    throw new ArgumentException("库存数量必须是有效的非负数");
                product.ProductNumber = stock;

                // 积分
                if (!int.TryParse(txtPoints.Text.Trim(), out int points) || points < 0)
                    throw new ArgumentException("积分必须是有效的非负数");
                product.RequiredPoints = points;

                // 保存到数据库
                Program._adminService.AddMerchandise(product);

                MessageBox.Show("添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
