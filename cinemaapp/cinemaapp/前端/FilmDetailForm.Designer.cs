namespace cinemaapp
{
    partial class FilmDetailForm
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
            pictureBoxPoster = new PictureBox();
            lblFilmName = new Label();
            lblGenre = new Label();
            lblDuration = new Label();
            lblScore = new Label();
            lblBoxOffice = new Label();
            lblAdmissions = new Label();
            lblPrice = new Label();
            btnSearchSections = new Button();
            lb1 = new Label();
            rtbComments = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPoster).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxPoster
            // 
            pictureBoxPoster.Location = new Point(55, 60);
            pictureBoxPoster.Margin = new Padding(6);
            pictureBoxPoster.Name = "pictureBoxPoster";
            pictureBoxPoster.Size = new Size(458, 700);
            pictureBoxPoster.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxPoster.TabIndex = 0;
            pictureBoxPoster.TabStop = false;
            // 
            // lblFilmName
            // 
            lblFilmName.AutoSize = true;
            lblFilmName.Font = new Font("微软雅黑", 18F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lblFilmName.Location = new Point(550, 60);
            lblFilmName.Margin = new Padding(6, 0, 6, 0);
            lblFilmName.Name = "lblFilmName";
            lblFilmName.Size = new Size(164, 47);
            lblFilmName.TabIndex = 1;
            lblFilmName.Text = "电影名称";
            // 
            // lblGenre
            // 
            lblGenre.AutoSize = true;
            lblGenre.Font = new Font("微软雅黑", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblGenre.Location = new Point(550, 160);
            lblGenre.Margin = new Padding(6, 0, 6, 0);
            lblGenre.Name = "lblGenre";
            lblGenre.Size = new Size(86, 31);
            lblGenre.TabIndex = 2;
            lblGenre.Text = "类型：";
            // 
            // lblDuration
            // 
            lblDuration.AutoSize = true;
            lblDuration.Font = new Font("微软雅黑", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblDuration.Location = new Point(550, 220);
            lblDuration.Margin = new Padding(6, 0, 6, 0);
            lblDuration.Name = "lblDuration";
            lblDuration.Size = new Size(86, 31);
            lblDuration.TabIndex = 3;
            lblDuration.Text = "时长：";
            // 
            // lblScore
            // 
            lblScore.AutoSize = true;
            lblScore.Font = new Font("微软雅黑", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblScore.Location = new Point(550, 280);
            lblScore.Margin = new Padding(6, 0, 6, 0);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(86, 31);
            lblScore.TabIndex = 4;
            lblScore.Text = "评分：";
            // 
            // lblBoxOffice
            // 
            lblBoxOffice.AutoSize = true;
            lblBoxOffice.Font = new Font("微软雅黑", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblBoxOffice.Location = new Point(550, 340);
            lblBoxOffice.Margin = new Padding(6, 0, 6, 0);
            lblBoxOffice.Name = "lblBoxOffice";
            lblBoxOffice.Size = new Size(86, 31);
            lblBoxOffice.TabIndex = 5;
            lblBoxOffice.Text = "票房：";
            // 
            // lblAdmissions
            // 
            lblAdmissions.AutoSize = true;
            lblAdmissions.Font = new Font("微软雅黑", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblAdmissions.Location = new Point(550, 400);
            lblAdmissions.Margin = new Padding(6, 0, 6, 0);
            lblAdmissions.Name = "lblAdmissions";
            lblAdmissions.Size = new Size(134, 31);
            lblAdmissions.TabIndex = 6;
            lblAdmissions.Text = "观影人次：";
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("微软雅黑", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblPrice.Location = new Point(550, 460);
            lblPrice.Margin = new Padding(6, 0, 6, 0);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(86, 31);
            lblPrice.TabIndex = 7;
            lblPrice.Text = "票价：";
            // 
            // btnSearchSections
            // 
            btnSearchSections.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSearchSections.Location = new Point(440, 800);
            btnSearchSections.Margin = new Padding(6);
            btnSearchSections.Name = "btnSearchSections";
            btnSearchSections.Size = new Size(183, 46);
            btnSearchSections.TabIndex = 9;
            btnSearchSections.Text = "查询场次";
            btnSearchSections.UseVisualStyleBackColor = true;
            btnSearchSections.Click += btnSearchSections_Click;
            // 
            // lb1
            // 
            lb1.AutoSize = true;
            lb1.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lb1.Location = new Point(1078, 154);
            lb1.Name = "lb1";
            lb1.Size = new Size(101, 37);
            lb1.TabIndex = 10;
            lb1.Text = "评论区";
            lb1.Click += lb1_Click;
            // 
            // rtbComments
            // 
            rtbComments.Location = new Point(935, 220);
            rtbComments.Name = "rtbComments";
            rtbComments.ReadOnly = true;
            rtbComments.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbComments.Size = new Size(386, 415);
            rtbComments.TabIndex = 11;
            rtbComments.Text = "";
            // 
            // FilmDetailForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1467, 1200);
            Controls.Add(rtbComments);
            Controls.Add(lb1);
            Controls.Add(btnSearchSections);
            Controls.Add(lblPrice);
            Controls.Add(lblAdmissions);
            Controls.Add(lblBoxOffice);
            Controls.Add(lblScore);
            Controls.Add(lblDuration);
            Controls.Add(lblGenre);
            Controls.Add(lblFilmName);
            Controls.Add(pictureBoxPoster);
            Margin = new Padding(6);
            Name = "FilmDetailForm";
            ShowInTaskbar = false;
            Text = "电影详情";
            ((System.ComponentModel.ISupportInitialize)pictureBoxPoster).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxPoster;
        private System.Windows.Forms.Label lblFilmName;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Label lblBoxOffice;
        private System.Windows.Forms.Label lblAdmissions;
        private System.Windows.Forms.Label lblPrice;
        //private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.Button btnSearchSections;
        private Label lb1;
        private RichTextBox rtbComments;
    }
}
