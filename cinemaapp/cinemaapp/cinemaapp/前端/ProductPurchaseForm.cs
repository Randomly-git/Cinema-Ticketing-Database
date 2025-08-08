using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;
using test.Repositories;

namespace cinemaapp
{
    public partial class ProductPurchaseForm : Form
    {
        private ComboBox cbPurchaseProducts;
        private NumericUpDown numPurchaseQty;
        private ComboBox cbPaymentMethod;  
        private Button btnPurchase;

        private ComboBox cbRewardProducts;
        private NumericUpDown numRewardQty;
        private Button btnRedeem;
        private Label lblCurrentPoints; // 当前积分显示标签

        // 这里假设这两个对象是你原来项目里的全局服务/登录用户
        private readonly IProductService _productService;
        private readonly Customer _loggedInCustomer;
        private readonly ICustomerRepository _customerRepository;

        public ProductPurchaseForm(IProductService productService, ICustomerRepository customerRepository , Customer loggedInCustomer)
        {
            _productService = productService;
            _customerRepository = customerRepository;
            _loggedInCustomer = loggedInCustomer;

            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "电影周边";
            this.Size = new Size(600, 400);

            var tabControl = new TabControl()
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(tabControl);

            // ------------------- Tab 1: 现金购买 -------------------
            var tabPurchase = new TabPage("现金购买");

            cbPurchaseProducts = new ComboBox()
            {
                Left = 30,
                Top = 40,
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed  // 开启自绘
            };
            cbPurchaseProducts.DrawItem += CbPurchaseProducts_DrawItem;
            tabPurchase.Controls.Add(new Label()
            {
                Text = "选择产品：",
                Left = 30,
                Top = 10
            });
            tabPurchase.Controls.Add(cbPurchaseProducts);

            tabPurchase.Controls.Add(new Label()
            {
                Text = "购买数量：",
                Left = 30,
                Top = 80
            });
            numPurchaseQty = new NumericUpDown()
            {
                Left = 150,
                Top = 78,
                Width = 100,
                Minimum = 1,
                Maximum = 100
            };
            tabPurchase.Controls.Add(numPurchaseQty);

            // 这里改成 ComboBox，代替 txtPaymentMethod
            tabPurchase.Controls.Add(new Label() { Text = "支付方式：", Left = 30, Top = 110 });
            cbPaymentMethod = new ComboBox()
            {
                Left = 150,
                Top = 108,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // 添加支付方式选项
            cbPaymentMethod.Items.AddRange(new string[] { "支付宝", "微信支付", "银行卡", "现金" });
            cbPaymentMethod.SelectedIndex = 0; // 默认选第一个
            tabPurchase.Controls.Add(cbPaymentMethod);

            btnPurchase = new Button()
            {
                Text = "确认购买",
                Left = 30,
                Top = 160,
                Width = 100
            };
            btnPurchase.Click += BtnPurchase_Click;
            tabPurchase.Controls.Add(btnPurchase);

            tabControl.TabPages.Add(tabPurchase);

            // ------------------- Tab 2: 积分兑换 -------------------
            var tabReward = new TabPage("积分兑换");

            // 当前积分显示 Label
            lblCurrentPoints = new Label()
            {
                Left = 30,
                Top = 5,
                Width = 300,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Text = "当前积分: 0" // 先占位，后面刷新覆盖
            };
            tabReward.Controls.Add(lblCurrentPoints);

            cbRewardProducts = new ComboBox()
            {
                Left = 30,
                Top = 40,
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            cbRewardProducts.DrawItem += CbRewardProducts_DrawItem;
            tabReward.Controls.Add(new Label()
            {
                Text = "选择兑换商品：",
                Left = 30,
                Top = 10
            });
            tabReward.Controls.Add(cbRewardProducts);

            tabReward.Controls.Add(new Label()
            {
                Text = "兑换数量：",
                Left = 30,
                Top = 80
            });
            numRewardQty = new NumericUpDown()
            {
                Left = 140,
                Top = 78,
                Width = 100,
                Minimum = 1,
                Maximum = 100
            };
            tabReward.Controls.Add(numRewardQty);

            btnRedeem = new Button()
            {
                Text = "确认兑换",
                Left = 30,
                Top = 120,
                Width = 100
            };
            btnRedeem.Click += BtnRedeem_Click;
            tabReward.Controls.Add(btnRedeem);

            tabControl.TabPages.Add(tabReward);

            // 初始化数据
            LoadProductLists();
            RefreshPointsDisplay();
        }
        // 现金购买 ComboBox 绘制事件
        private void CbPurchaseProducts_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0) return;

            var combo = sender as ComboBox;
            var product = combo.Items[e.Index] as RelatedProduct;

            string text = $"{product.ProductName}  价格: {product.Price:C}  库存: {product.ProductNumber}";

            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        // 积分兑换 ComboBox 绘制事件
        private void CbRewardProducts_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0) return;

            var combo = sender as ComboBox;
            var product = combo.Items[e.Index] as RelatedProduct;

            string text = $"{product.ProductName}  所需积分: {product.RequiredPoints}  库存: {product.ProductNumber}";

            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }
        private void LoadProductLists()
        {
            var products = _productService.GetAvailableProducts();
            cbPurchaseProducts.DataSource = null;
            cbPurchaseProducts.DataSource = products;
            cbPurchaseProducts.DisplayMember = "ProductName";

            var rewardProducts = products
                .Where(p => p.RequiredPoints > 0 && p.ProductNumber > 0)
                .ToList();
            cbRewardProducts.DataSource = null;
            cbRewardProducts.DataSource = rewardProducts;
            cbRewardProducts.DisplayMember = "ProductName";
        }


        // ------------------- 原 PurchaseProductMenu 逻辑迁移 -------------------
        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            if (_loggedInCustomer == null)
            {
                MessageBox.Show("请先登录才能购买周边产品。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var products = _productService.GetAvailableProducts();
                if (!products.Any())
                {
                    MessageBox.Show("当前没有可供购买的周边产品。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedProduct = cbPurchaseProducts.SelectedItem as RelatedProduct;
                int purchaseNum = (int)numPurchaseQty.Value;

                if (purchaseNum <= 0)
                {
                    MessageBox.Show("购买数量必须是大于0的整数。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string paymentMethod = cbPaymentMethod.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(paymentMethod))
                {
                    MessageBox.Show("请选择支付方式。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newOrder = _productService.PurchaseProduct(selectedProduct.ProductName, purchaseNum, _loggedInCustomer.CustomerID, paymentMethod);
                MessageBox.Show($"购买周边产品成功！\n订单信息：{newOrder}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"购买周边产品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadProductLists();       // 刷新产品库存显示
        }

        // ------------------- 原 RedeemReward 逻辑迁移 -------------------
        private void BtnRedeem_Click(object sender, EventArgs e)
        {
            if (_loggedInCustomer == null)
            {
                MessageBox.Show("请先登录才能兑换商品。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var products = _productService.GetAvailableProducts()
                .Where(p => p.RequiredPoints > 0 && p.ProductNumber > 0)
                .ToList();

            if (products.Count == 0)
            {
                MessageBox.Show("当前没有可兑换的商品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedProduct = cbRewardProducts.SelectedItem as RelatedProduct;
            int quantity = (int)numRewardQty.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("请输入有效的正数数量！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string result = _productService.RedeemProductWithPoints(selectedProduct.ProductName, quantity, _loggedInCustomer.CustomerID);
            MessageBox.Show(result, "兑换结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // 刷新积分显示
            RefreshPointsDisplay();
            LoadProductLists();       // 刷新产品库存显示
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

    }
}