namespace cinemaapp
{
    partial class PaymentForm
    {
        // 必需的组件变量
        private System.ComponentModel.IContainer components = null;

        // 清理所有正在使用的资源
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        // 设计器支持所需的方法 - 不要修改
        // 使用代码编辑器修改此方法的内容
        private void InitializeComponent()
        {
            // 电影名称标签
            this.lblFilmTitle = new System.Windows.Forms.Label();
            this.lblFilmName = new System.Windows.Forms.Label();

            // 场次信息标签
            this.lblShowtimeTitle = new System.Windows.Forms.Label();
            this.lblShowtime = new System.Windows.Forms.Label();

            // 座位信息标签
            this.lblSeatsTitle = new System.Windows.Forms.Label();
            this.lblSeats = new System.Windows.Forms.Label();

            // 价格信息标签
            this.lblTotalPriceTitle = new System.Windows.Forms.Label();
            this.lblTotalPrice = new System.Windows.Forms.Label();

            // VIP积分标签
            this.lblVIPPoints = new System.Windows.Forms.Label();

            // 支付方式选择
            this.lblPaymentMethod = new System.Windows.Forms.Label();
            this.cmbPaymentMethod = new System.Windows.Forms.ComboBox();

            // 支付按钮
            this.btnCompletePayment = new System.Windows.Forms.Button();

            // 分隔线
            this.panelDivider = new System.Windows.Forms.Panel();

            this.SuspendLayout();

            // 
            // lblFilmTitle (电影名称标题)
            // 
            this.lblFilmTitle.AutoSize = true;
            this.lblFilmTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblFilmTitle.Location = new System.Drawing.Point(30, 30);
            this.lblFilmTitle.Name = "lblFilmTitle";
            this.lblFilmTitle.Size = new System.Drawing.Size(65, 19);
            this.lblFilmTitle.TabIndex = 0;
            this.lblFilmTitle.Text = "电影名称:";

            // 
            // lblFilmName (电影名称值)
            // 
            this.lblFilmName.AutoSize = true;
            this.lblFilmName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblFilmName.Location = new System.Drawing.Point(100, 30);
            this.lblFilmName.Name = "lblFilmName";
            this.lblFilmName.Size = new System.Drawing.Size(43, 19);
            this.lblFilmName.TabIndex = 1;
            this.lblFilmName.Text = "label1";

            // 
            // lblShowtimeTitle (场次标题)
            // 
            this.lblShowtimeTitle.AutoSize = true;
            this.lblShowtimeTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblShowtimeTitle.Location = new System.Drawing.Point(30, 60);
            this.lblShowtimeTitle.Name = "lblShowtimeTitle";
            this.lblShowtimeTitle.Size = new System.Drawing.Size(65, 19);
            this.lblShowtimeTitle.TabIndex = 2;
            this.lblShowtimeTitle.Text = "放映场次:";

            // 
            // lblShowtime (场次值)
            // 
            this.lblShowtime.AutoSize = true;
            this.lblShowtime.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblShowtime.Location = new System.Drawing.Point(100, 60);
            this.lblShowtime.Name = "lblShowtime";
            this.lblShowtime.Size = new System.Drawing.Size(43, 19);
            this.lblShowtime.TabIndex = 3;
            this.lblShowtime.Text = "label2";

            // 
            // lblSeatsTitle (座位标题)
            // 
            this.lblSeatsTitle.AutoSize = true;
            this.lblSeatsTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblSeatsTitle.Location = new System.Drawing.Point(30, 90);
            this.lblSeatsTitle.Name = "lblSeatsTitle";
            this.lblSeatsTitle.Size = new System.Drawing.Size(65, 19);
            this.lblSeatsTitle.TabIndex = 4;
            this.lblSeatsTitle.Text = "已选座位:";

            // 
            // lblSeats (座位值)
            // 
            this.lblSeats.AutoSize = true;
            this.lblSeats.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblSeats.Location = new System.Drawing.Point(100, 90);
            this.lblSeats.Name = "lblSeats";
            this.lblSeats.Size = new System.Drawing.Size(43, 19);
            this.lblSeats.TabIndex = 5;
            this.lblSeats.Text = "label3";

            // 
            // lblTotalPriceTitle (总价标题)
            // 
            this.lblTotalPriceTitle.AutoSize = true;
            this.lblTotalPriceTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalPriceTitle.Location = new System.Drawing.Point(30, 120);
            this.lblTotalPriceTitle.Name = "lblTotalPriceTitle";
            this.lblTotalPriceTitle.Size = new System.Drawing.Size(65, 19);
            this.lblTotalPriceTitle.TabIndex = 6;
            this.lblTotalPriceTitle.Text = "应付金额:";

            // 
            // lblTotalPrice (总价值)
            // 
            this.lblTotalPrice.AutoSize = true;
            this.lblTotalPrice.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotalPrice.ForeColor = System.Drawing.Color.Red;
            this.lblTotalPrice.Location = new System.Drawing.Point(100, 120);
            this.lblTotalPrice.Name = "lblTotalPrice";
            this.lblTotalPrice.Size = new System.Drawing.Size(59, 21);
            this.lblTotalPrice.TabIndex = 7;
            this.lblTotalPrice.Text = "￥0.00";

            // 
            // lblVIPPoints (VIP积分)
            // 
            this.lblVIPPoints.AutoSize = true;
            this.lblVIPPoints.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblVIPPoints.Location = new System.Drawing.Point(30, 150);
            this.lblVIPPoints.Name = "lblVIPPoints";
            this.lblVIPPoints.Size = new System.Drawing.Size(80, 17);
            this.lblVIPPoints.TabIndex = 8;
            this.lblVIPPoints.Text = "当前积分: 0";

            // 
            // panelDivider (分隔线)
            // 
            this.panelDivider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDivider.Location = new System.Drawing.Point(30, 180);
            this.panelDivider.Name = "panelDivider";
            this.panelDivider.Size = new System.Drawing.Size(340, 1);
            this.panelDivider.TabIndex = 9;

            // 
            // lblPaymentMethod (支付方式标题)
            // 
            this.lblPaymentMethod.AutoSize = true;
            this.lblPaymentMethod.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblPaymentMethod.Location = new System.Drawing.Point(30, 200);
            this.lblPaymentMethod.Name = "lblPaymentMethod";
            this.lblPaymentMethod.Size = new System.Drawing.Size(65, 19);
            this.lblPaymentMethod.TabIndex = 10;
            this.lblPaymentMethod.Text = "支付方式:";

            // 
            // cmbPaymentMethod (支付方式下拉框)
            // 
            this.cmbPaymentMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPaymentMethod.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbPaymentMethod.FormattingEnabled = true;
            this.cmbPaymentMethod.Location = new System.Drawing.Point(100, 200);
            this.cmbPaymentMethod.Name = "cmbPaymentMethod";
            this.cmbPaymentMethod.Size = new System.Drawing.Size(150, 27);
            this.cmbPaymentMethod.TabIndex = 11;

            // 
            // btnCompletePayment (完成支付按钮)
            // 
            this.btnCompletePayment.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnCompletePayment.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btnCompletePayment.ForeColor = System.Drawing.Color.White;
            this.btnCompletePayment.Location = new System.Drawing.Point(100, 250);
            this.btnCompletePayment.Name = "btnCompletePayment";
            this.btnCompletePayment.Size = new System.Drawing.Size(180, 40);
            this.btnCompletePayment.TabIndex = 12;
            this.btnCompletePayment.Text = "确认支付";
            this.btnCompletePayment.UseVisualStyleBackColor = false;
            this.btnCompletePayment.Click += new System.EventHandler(this.btnCompletePayment_Click);

            // 
            // PaymentForm (支付窗体)
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 320);
            this.Controls.Add(this.btnCompletePayment);
            this.Controls.Add(this.cmbPaymentMethod);
            this.Controls.Add(this.lblPaymentMethod);
            this.Controls.Add(this.panelDivider);
            this.Controls.Add(this.lblVIPPoints);
            this.Controls.Add(this.lblTotalPrice);
            this.Controls.Add(this.lblTotalPriceTitle);
            this.Controls.Add(this.lblSeats);
            this.Controls.Add(this.lblSeatsTitle);
            this.Controls.Add(this.lblShowtime);
            this.Controls.Add(this.lblShowtimeTitle);
            this.Controls.Add(this.lblFilmName);
            this.Controls.Add(this.lblFilmTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaymentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "支付订单";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // 定义所有控件变量
        private System.Windows.Forms.Label lblFilmTitle;
        private System.Windows.Forms.Label lblFilmName;
        private System.Windows.Forms.Label lblShowtimeTitle;
        private System.Windows.Forms.Label lblShowtime;
        private System.Windows.Forms.Label lblSeatsTitle;
        private System.Windows.Forms.Label lblSeats;
        private System.Windows.Forms.Label lblTotalPriceTitle;
        private System.Windows.Forms.Label lblTotalPrice;
        private System.Windows.Forms.Label lblVIPPoints;
        private System.Windows.Forms.Label lblPaymentMethod;
        private System.Windows.Forms.ComboBox cmbPaymentMethod;
        private System.Windows.Forms.Button btnCompletePayment;
        private System.Windows.Forms.Panel panelDivider;
    }
}