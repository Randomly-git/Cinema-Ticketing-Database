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
            this.pictureBoxPoster = new System.Windows.Forms.PictureBox();
            this.lblFilmName = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblBoxOffice = new System.Windows.Forms.Label();
            this.lblAdmissions = new System.Windows.Forms.Label();
            this.lblPrice = new System.Windows.Forms.Label();
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.btnSearchSections = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPoster)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxPoster
            // 
            this.pictureBoxPoster.Location = new System.Drawing.Point(30, 30);
            this.pictureBoxPoster.Name = "pictureBoxPoster";
            this.pictureBoxPoster.Size = new System.Drawing.Size(250, 350);
            this.pictureBoxPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPoster.TabIndex = 0;
            this.pictureBoxPoster.TabStop = false;
            // 
            // lblFilmName
            // 
            this.lblFilmName.AutoSize = true;
            this.lblFilmName.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFilmName.Location = new System.Drawing.Point(300, 30);
            this.lblFilmName.Name = "lblFilmName";
            this.lblFilmName.Size = new System.Drawing.Size(110, 31);
            this.lblFilmName.TabIndex = 1;
            this.lblFilmName.Text = "电影名称";
            // 
            // lblGenre
            // 
            this.lblGenre.AutoSize = true;
            this.lblGenre.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblGenre.Location = new System.Drawing.Point(300, 80);
            this.lblGenre.Name = "lblGenre";
            this.lblGenre.Size = new System.Drawing.Size(58, 21);
            this.lblGenre.TabIndex = 2;
            this.lblGenre.Text = "类型：";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDuration.Location = new System.Drawing.Point(300, 110);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(58, 21);
            this.lblDuration.TabIndex = 3;
            this.lblDuration.Text = "时长：";
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblScore.Location = new System.Drawing.Point(300, 140);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(58, 21);
            this.lblScore.TabIndex = 4;
            this.lblScore.Text = "评分：";
            // 
            // lblBoxOffice
            // 
            this.lblBoxOffice.AutoSize = true;
            this.lblBoxOffice.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBoxOffice.Location = new System.Drawing.Point(300, 170);
            this.lblBoxOffice.Name = "lblBoxOffice";
            this.lblBoxOffice.Size = new System.Drawing.Size(58, 21);
            this.lblBoxOffice.TabIndex = 5;
            this.lblBoxOffice.Text = "票房：";
            // 
            // lblAdmissions
            // 
            this.lblAdmissions.AutoSize = true;
            this.lblAdmissions.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAdmissions.Location = new System.Drawing.Point(300, 200);
            this.lblAdmissions.Name = "lblAdmissions";
            this.lblAdmissions.Size = new System.Drawing.Size(90, 21);
            this.lblAdmissions.TabIndex = 6;
            this.lblAdmissions.Text = "观影人次：";
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPrice.Location = new System.Drawing.Point(300, 230);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(58, 21);
            this.lblPrice.TabIndex = 7;
            this.lblPrice.Text = "票价：";
            // 
            // dateTimePicker
            // 
            this.dateTimePicker.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePicker.Location = new System.Drawing.Point(30, 400);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.Size = new System.Drawing.Size(200, 23);
            this.dateTimePicker.TabIndex = 8;
            // 
            // btnSearchSections
            // 
            this.btnSearchSections.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSearchSections.Location = new System.Drawing.Point(240, 400);
            this.btnSearchSections.Name = "btnSearchSections";
            this.btnSearchSections.Size = new System.Drawing.Size(100, 23);
            this.btnSearchSections.TabIndex = 9;
            this.btnSearchSections.Text = "查询场次";
            this.btnSearchSections.UseVisualStyleBackColor = true;
            this.btnSearchSections.Click += new System.EventHandler(this.btnSearchSections_Click);
            // 
            // FilmDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.btnSearchSections);
            this.Controls.Add(this.dateTimePicker);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.lblAdmissions);
            this.Controls.Add(this.lblBoxOffice);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.lblGenre);
            this.Controls.Add(this.lblFilmName);
            this.Controls.Add(this.pictureBoxPoster);
            this.Name = "FilmDetailForm";
            this.Text = "电影详情";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPoster)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.Button btnSearchSections;
    }
}
