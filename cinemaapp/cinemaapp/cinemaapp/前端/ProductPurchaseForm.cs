using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using test.Models;
using test.Services;
using test.Repositories;

namespace cinemaapp
{
    public partial class ProductPurchaseForm : Form
    {
        // 控件声明
        private ListView lvProducts;
        private ImageList imageListProducts;
        private PictureBox pbProductImage;
        private Label lblProductName, lblProductDesc, lblProductPrice, lblProductStock, lblCurrentPoints;
        private NumericUpDown numQty;
        private RadioButton rbCashPurchase, rbPointsRedeem;
        private ComboBox cbPaymentMethod;
        private Button btnPurchase;

        private readonly IProductService _productService;
        private readonly Customer _loggedInCustomer;
        private readonly ICustomerRepository _customerRepository;
        private MainForm _mainForm;

        public ProductPurchaseForm(IProductService productService, ICustomerRepository customerRepository, Customer loggedInCustomer, MainForm mainForm)
        {
            _productService = productService;
            _customerRepository = customerRepository;
            _loggedInCustomer = loggedInCustomer;
            _mainForm = mainForm;

            InitializeComponent();
            SetupUI();
            LoadProductList();
            RefreshPointsDisplay();
        }

        private void SetupUI()
        {
            this.Text = "电影周边购买";
            this.Size = new Size(1600, 1000);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 当前积分显示
            lblCurrentPoints = new Label()
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
            };
            this.Controls.Add(lblCurrentPoints);

            // 产品列表和图片列表
            imageListProducts = new ImageList() { ImageSize = new Size(128, 128) };
            lvProducts = new ListView()
            {
                Location = new Point(20, 60),
                Size = new Size(600, 900),
                View = View.LargeIcon,
                LargeImageList = imageListProducts,
                MultiSelect = false,
                Font = new Font(FontFamily.GenericSansSerif, 12),
            };
            lvProducts.SelectedIndexChanged += LvProducts_SelectedIndexChanged;
            this.Controls.Add(lvProducts);

            // 右侧产品详情图片框，放大为400x400
            pbProductImage = new PictureBox()
            {
                Location = new Point(650, 60),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(pbProductImage);

            lblProductName = new Label()
            {
                Location = new Point(650, 480),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold)
            };
            this.Controls.Add(lblProductName);

            lblProductDesc = new Label()
            {
                Location = new Point(650, 520),
                Size = new Size(900, 120),
                AutoEllipsis = true,
                Font = new Font(FontFamily.GenericSansSerif, 12)
            };
            this.Controls.Add(lblProductDesc);

            lblProductPrice = new Label()
            {
                Location = new Point(650, 650),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            this.Controls.Add(lblProductPrice);

            lblProductStock = new Label()
            {
                Location = new Point(650, 690),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            this.Controls.Add(lblProductStock);

            // 数量选择控件
            var lblQty = new Label()
            {
                Location = new Point(650, 730),
                Text = "购买数量：",
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            this.Controls.Add(lblQty);

            numQty = new NumericUpDown()
            {
                Location = new Point(760, 728),
                Width = 120,
                Minimum = 1,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            this.Controls.Add(numQty);

            // 购买方式选择
            rbCashPurchase = new RadioButton()
            {
                Text = "现金购买",
                Location = new Point(650, 770),
                Checked = true,
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            rbPointsRedeem = new RadioButton()
            {
                Text = "积分兑换",
                Location = new Point(780, 770),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            rbCashPurchase.CheckedChanged += PurchaseModeChanged;
            rbPointsRedeem.CheckedChanged += PurchaseModeChanged;
            this.Controls.Add(rbCashPurchase);
            this.Controls.Add(rbPointsRedeem);

            // 支付方式选择（仅现金购买启用）
            var lblPay = new Label()
            {
                Location = new Point(650, 810),
                Text = "支付方式：",
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            this.Controls.Add(lblPay);

            cbPaymentMethod = new ComboBox()
            {
                Location = new Point(760, 808),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            cbPaymentMethod.Items.AddRange(new string[] { "支付宝", "微信支付", "银行卡", "现金" });
            cbPaymentMethod.SelectedIndex = 0;
            this.Controls.Add(cbPaymentMethod);

            // 购买按钮
            btnPurchase = new Button()
            {
                Text = "确认购买",
                Location = new Point(950, 800),
                Width = 150,
                Height = 50,
                Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold)
            };
            btnPurchase.Click += BtnPurchase_Click;
            this.Controls.Add(btnPurchase);
        }


        // 加载产品列表（缩略图64x64）
        private void LoadProductList()
        {
            var products = _productService.GetAvailableProducts();

            imageListProducts.Images.Clear();
            lvProducts.Items.Clear();

            int idx = 0;
            foreach (var product in products)
            {
                Image img = null;
                try
                {
                    if (!string.IsNullOrEmpty(product.ImagePath))
                    {
                        string imgPath = Path.Combine(Application.StartupPath, product.ImagePath);
                        if (File.Exists(imgPath))
                        {
                            // 加载原图后缩小为64x64做缩略图，保持清晰
                            using var original = Image.FromFile(imgPath);
                            img = ResizeImageHighQuality(original, 64, 64);
                        }
                    }
                }
                catch
                {
                    // 加载失败忽略
                }

                if (img == null)
                {
                    string defaultImagePath = Path.Combine(Application.StartupPath, "images", "默认.jpg");
                    if (File.Exists(defaultImagePath))
                    {
                        using var defaultImg = Image.FromFile(defaultImagePath);
                        img = ResizeImageHighQuality(defaultImg, 64, 64);
                    }
                    else
                    {
                        img = new Bitmap(64, 64);
                    }
                }

                imageListProducts.Images.Add(img);

                var item = new ListViewItem(product.ProductName, idx);
                item.Tag = product;
                lvProducts.Items.Add(item);
                idx++;
            }

            if (lvProducts.Items.Count > 0)
                lvProducts.Items[0].Selected = true;
        }

        // 产品选中，显示大图和详情
        private void LvProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvProducts.SelectedItems.Count == 0)
            {
                ClearProductDetails();
                return;
            }

            var product = lvProducts.SelectedItems[0].Tag as RelatedProduct;

            lblProductName.Text = $"名称: {product.ProductName}";
            lblProductDesc.Text = "这里填写产品描述（可扩展模型）";
            lblProductPrice.Text = $"价格: {product.Price:C}";
            lblProductStock.Text = $"库存: {product.ProductNumber}  所需积分: {product.RequiredPoints}";

            // 加载原图，并高质量缩放到PictureBox大小
            try
            {
                string imgPath = Path.Combine(Application.StartupPath, product.ImagePath);
                Image img;

                if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(imgPath))
                {
                    using var original = Image.FromFile(imgPath);
                    img = ResizeImageHighQuality(original, pbProductImage.Width, pbProductImage.Height);
                }
                else
                {
                    string defaultImagePath = Path.Combine(Application.StartupPath, "images", "默认.jpg");
                    using var defaultImg = Image.FromFile(defaultImagePath);
                    img = ResizeImageHighQuality(defaultImg, pbProductImage.Width, pbProductImage.Height);
                }

                pbProductImage.Image?.Dispose();
                pbProductImage.Image = img;
            }
            catch
            {
                pbProductImage.Image?.Dispose();
                pbProductImage.Image = null;
            }

            numQty.Value = 1;
            //numQty.Maximum = Math.Max(1, product.ProductNumber);
        }

        private void ClearProductDetails()
        {
            lblProductName.Text = "";
            lblProductDesc.Text = "";
            lblProductPrice.Text = "";
            lblProductStock.Text = "";
            pbProductImage.Image?.Dispose();
            pbProductImage.Image = null;
            numQty.Value = 1;
        }

        private void PurchaseModeChanged(object sender, EventArgs e)
        {
            cbPaymentMethod.Enabled = rbCashPurchase.Checked;
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            if (_loggedInCustomer == null)
            {
                MessageBox.Show("请先登录才能购买。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lvProducts.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一个产品。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var product = lvProducts.SelectedItems[0].Tag as RelatedProduct;
            int qty = (int)numQty.Value;

            if (qty <= 0)
            {
                MessageBox.Show("购买数量必须大于0。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (qty > product.ProductNumber)
            {
                MessageBox.Show($"库存不足，最多可购买 {product.ProductNumber} 个。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (rbCashPurchase.Checked)
            {
                string payMethod = cbPaymentMethod.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(payMethod))
                {
                    MessageBox.Show("请选择支付方式。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    var order = _productService.PurchaseProduct(product.ProductName, qty, _loggedInCustomer.CustomerID, payMethod);
                    MessageBox.Show($"购买成功！订单信息：\n{order}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"购买失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (rbPointsRedeem.Checked)
            {
                var vipCard = _customerRepository.GetVIPCardByCustomerID(_loggedInCustomer.CustomerID);
                int points = vipCard?.Points ?? 0;
                int neededPoints = product.RequiredPoints * qty;

                if (points < neededPoints)
                {
                    MessageBox.Show($"积分不足，需 {neededPoints} 积分，当前积分 {points}。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    string result = _productService.RedeemProductWithPoints(product.ProductName, qty, _loggedInCustomer.CustomerID);
                    MessageBox.Show(result, "兑换结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshPointsDisplay();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"兑换失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // 购买后刷新界面
            UpdateProductStockAndUI(product.ProductName);
            RefreshPointsDisplay();
            //刷新主界面的用户信息标签
            _mainForm.UpdateUserInfoLabel();
        }

        private void RefreshPointsDisplay()
        {
            var vipCard = _customerRepository.GetVIPCardByCustomerID(_loggedInCustomer.CustomerID);
            int points = vipCard?.Points ?? 0;

            if (_loggedInCustomer.VIPCard != null)
            {
                _loggedInCustomer.VIPCard.Points = points;
            }
            else
            {
                _loggedInCustomer.VIPCard = vipCard;
            }

            lblCurrentPoints.Text = $"当前积分: {points}";
        }

        // 高质量缩放图片函数
        private Image ResizeImageHighQuality(Image img, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        /// <summary>
        /// 根据产品名刷新库存显示和数量最大限制，不重载图片和列表项
        /// </summary>
        private void UpdateProductStockAndUI(string productName)
        {
            // 从服务拿最新库存
            var updatedProduct = _productService.GetAvailableProducts()
                                    .FirstOrDefault(p => p.ProductName == productName);
            if (updatedProduct == null) return;

            // 在列表中找到对应项，更新库存字段
            foreach (ListViewItem item in lvProducts.Items)
            {
                if (item.Text == productName)
                {
                    var product = item.Tag as RelatedProduct;
                    if (product != null)
                    {
                        product.ProductNumber = updatedProduct.ProductNumber;

                        // 更新列表项文本（如果显示库存）
                        item.Text = product.ProductName; // 你可以加库存数之类的显示格式，自己定
                        item.Tag = product;
                    }
                    break;
                }
            }

            // 如果当前选中的是这个产品，更新详情库存显示和数量最大限制
            if (lvProducts.SelectedItems.Count > 0 && lvProducts.SelectedItems[0].Text == productName)
            {
                var product = lvProducts.SelectedItems[0].Tag as RelatedProduct;
                lblProductStock.Text = $"库存: {product.ProductNumber}  所需积分: {product.RequiredPoints}";
            }
        }
    }
}
