namespace cinemaapp
{
    partial class SeatSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelSeats = new System.Windows.Forms.Panel();
            this.lblFilmInfo = new System.Windows.Forms.Label();
            this.lblHallInfo = new System.Windows.Forms.Label();
            this.lblSelectedSeats = new System.Windows.Forms.Label();
            this.lblTotalPrice = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelSeats
            // 
            this.panelSeats.AutoScroll = true;
            this.panelSeats.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelSeats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSeats.Location = new System.Drawing.Point(20, 80);
            this.panelSeats.Name = "panelSeats";
            this.panelSeats.Size = new System.Drawing.Size(750, 400);
            this.panelSeats.TabIndex = 0;
            // 
            // lblFilmInfo
            // 
            this.lblFilmInfo.AutoSize = true;
            this.lblFilmInfo.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.lblFilmInfo.Location = new System.Drawing.Point(20, 20);
            this.lblFilmInfo.Name = "lblFilmInfo";
            this.lblFilmInfo.Size = new System.Drawing.Size(52, 19);
            this.lblFilmInfo.TabIndex = 1;
            this.lblFilmInfo.Text = "label1";
            // 
            // lblHallInfo
            // 
            this.lblHallInfo.AutoSize = true;
            this.lblHallInfo.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblHallInfo.Location = new System.Drawing.Point(20, 45);
            this.lblHallInfo.Name = "lblHallInfo";
            this.lblHallInfo.Size = new System.Drawing.Size(43, 17);
            this.lblHallInfo.TabIndex = 2;
            this.lblHallInfo.Text = "label2";
            // 
            // lblSelectedSeats
            // 
            this.lblSelectedSeats.AutoSize = true;
            this.lblSelectedSeats.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.lblSelectedSeats.Location = new System.Drawing.Point(20, 500);
            this.lblSelectedSeats.Name = "lblSelectedSeats";
            this.lblSelectedSeats.Size = new System.Drawing.Size(107, 17);
            this.lblSelectedSeats.TabIndex = 3;
            this.lblSelectedSeats.Text = "已选座位: 无";
            // 
            // lblTotalPrice
            // 
            this.lblTotalPrice.AutoSize = true;
            this.lblTotalPrice.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalPrice.Location = new System.Drawing.Point(20, 530);
            this.lblTotalPrice.Name = "lblTotalPrice";
            this.lblTotalPrice.Size = new System.Drawing.Size(56, 17);
            this.lblTotalPrice.TabIndex = 4;
            this.lblTotalPrice.Text = "总价: ￥0";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.Location = new System.Drawing.Point(650, 520);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(120, 35);
            this.btnConfirm.TabIndex = 5;
            this.btnConfirm.Text = "确认选座";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // SeatSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 570);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.lblTotalPrice);
            this.Controls.Add(this.lblSelectedSeats);
            this.Controls.Add(this.lblHallInfo);
            this.Controls.Add(this.lblFilmInfo);
            this.Controls.Add(this.panelSeats);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SeatSelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "座位选择";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelSeats;
        private System.Windows.Forms.Label lblFilmInfo;
        private System.Windows.Forms.Label lblHallInfo;
        private System.Windows.Forms.Label lblSelectedSeats;
        private System.Windows.Forms.Label lblTotalPrice;
        private System.Windows.Forms.Button btnConfirm;
    }
}