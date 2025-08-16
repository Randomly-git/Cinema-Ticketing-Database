namespace cinemaapp
{
    partial class FilmManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageAdd = new System.Windows.Forms.TabPage();
            this.btnAddFilm = new System.Windows.Forms.Button();
            this.dtpReleaseDate = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.txtNormalPrice = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFilmLength = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtGenre = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFilmName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPageUpdate = new System.Windows.Forms.TabPage();
            this.label15 = new System.Windows.Forms.Label();
            this.cmbUpdateFilms = new System.Windows.Forms.ComboBox();
            this.btnClearUpdateFields = new System.Windows.Forms.Button();
            this.btnUpdateFilm = new System.Windows.Forms.Button();
            this.txtUpdateAdmissions = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtUpdateBoxOffice = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtUpdateScore = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.dtpUpdateEndDate = new System.Windows.Forms.DateTimePicker();
            this.label11 = new System.Windows.Forms.Label();
            this.dtpUpdateReleaseDate = new System.Windows.Forms.DateTimePicker();
            this.label10 = new System.Windows.Forms.Label();
            this.txtUpdateNormalPrice = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtUpdateFilmLength = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtUpdateGenre = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtUpdateFilmName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPageAdd.SuspendLayout();
            this.tabPageUpdate.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageAdd);
            this.tabControl1.Controls.Add(this.tabPageUpdate);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(500, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageAdd
            // 
            this.tabPageAdd.Controls.Add(this.btnAddFilm);
            this.tabPageAdd.Controls.Add(this.dtpReleaseDate);
            this.tabPageAdd.Controls.Add(this.label5);
            this.tabPageAdd.Controls.Add(this.txtNormalPrice);
            this.tabPageAdd.Controls.Add(this.label4);
            this.tabPageAdd.Controls.Add(this.txtFilmLength);
            this.tabPageAdd.Controls.Add(this.label3);
            this.tabPageAdd.Controls.Add(this.txtGenre);
            this.tabPageAdd.Controls.Add(this.label2);
            this.tabPageAdd.Controls.Add(this.txtFilmName);
            this.tabPageAdd.Controls.Add(this.label1);
            this.tabPageAdd.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdd.Name = "tabPageAdd";
            this.tabPageAdd.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAdd.Size = new System.Drawing.Size(492, 424);
            this.tabPageAdd.TabIndex = 0;
            this.tabPageAdd.Text = "添加电影";
            this.tabPageAdd.UseVisualStyleBackColor = true;
            // 
            // btnAddFilm
            // 
            this.btnAddFilm.Location = new System.Drawing.Point(180, 280);
            this.btnAddFilm.Name = "btnAddFilm";
            this.btnAddFilm.Size = new System.Drawing.Size(120, 40);
            this.btnAddFilm.TabIndex = 10;
            this.btnAddFilm.Text = "添加电影";
            this.btnAddFilm.UseVisualStyleBackColor = true;
            this.btnAddFilm.Click += new System.EventHandler(this.btnAddFilm_Click);
            // 
            // dtpReleaseDate
            // 
            this.dtpReleaseDate.Location = new System.Drawing.Point(120, 220);
            this.dtpReleaseDate.Name = "dtpReleaseDate";
            this.dtpReleaseDate.Size = new System.Drawing.Size(200, 21);
            this.dtpReleaseDate.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(50, 225);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "上映日期：";
            // 
            // txtNormalPrice
            // 
            this.txtNormalPrice.Location = new System.Drawing.Point(120, 180);
            this.txtNormalPrice.Name = "txtNormalPrice";
            this.txtNormalPrice.Size = new System.Drawing.Size(100, 21);
            this.txtNormalPrice.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(50, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "普通票价：";
            // 
            // txtFilmLength
            // 
            this.txtFilmLength.Location = new System.Drawing.Point(120, 140);
            this.txtFilmLength.Name = "txtFilmLength";
            this.txtFilmLength.Size = new System.Drawing.Size(100, 21);
            this.txtFilmLength.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(50, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "电影时长：";
            // 
            // txtGenre
            // 
            this.txtGenre.Location = new System.Drawing.Point(120, 100);
            this.txtGenre.Name = "txtGenre";
            this.txtGenre.Size = new System.Drawing.Size(200, 21);
            this.txtGenre.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "电影类型：";
            // 
            // txtFilmName
            // 
            this.txtFilmName.Location = new System.Drawing.Point(120, 60);
            this.txtFilmName.Name = "txtFilmName";
            this.txtFilmName.Size = new System.Drawing.Size(200, 21);
            this.txtFilmName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "电影名称：";
            // 
            // tabPageUpdate
            // 
            this.tabPageUpdate.Controls.Add(this.label15);
            this.tabPageUpdate.Controls.Add(this.cmbUpdateFilms);
            this.tabPageUpdate.Controls.Add(this.btnClearUpdateFields);
            this.tabPageUpdate.Controls.Add(this.btnUpdateFilm);
            this.tabPageUpdate.Controls.Add(this.txtUpdateAdmissions);
            this.tabPageUpdate.Controls.Add(this.label14);
            this.tabPageUpdate.Controls.Add(this.txtUpdateBoxOffice);
            this.tabPageUpdate.Controls.Add(this.label13);
            this.tabPageUpdate.Controls.Add(this.txtUpdateScore);
            this.tabPageUpdate.Controls.Add(this.label12);
            this.tabPageUpdate.Controls.Add(this.dtpUpdateEndDate);
            this.tabPageUpdate.Controls.Add(this.label11);
            this.tabPageUpdate.Controls.Add(this.dtpUpdateReleaseDate);
            this.tabPageUpdate.Controls.Add(this.label10);
            this.tabPageUpdate.Controls.Add(this.txtUpdateNormalPrice);
            this.tabPageUpdate.Controls.Add(this.label9);
            this.tabPageUpdate.Controls.Add(this.txtUpdateFilmLength);
            this.tabPageUpdate.Controls.Add(this.label8);
            this.tabPageUpdate.Controls.Add(this.txtUpdateGenre);
            this.tabPageUpdate.Controls.Add(this.label7);
            this.tabPageUpdate.Controls.Add(this.txtUpdateFilmName);
            this.tabPageUpdate.Controls.Add(this.label6);
            this.tabPageUpdate.Location = new System.Drawing.Point(4, 22);
            this.tabPageUpdate.Name = "tabPageUpdate";
            this.tabPageUpdate.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUpdate.Size = new System.Drawing.Size(492, 424);
            this.tabPageUpdate.TabIndex = 1;
            this.tabPageUpdate.Text = "更新电影";
            this.tabPageUpdate.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(50, 25);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(65, 12);
            this.label15.TabIndex = 21;
            this.label15.Text = "选择电影：";
            // 
            // cmbUpdateFilms
            // 
            this.cmbUpdateFilms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUpdateFilms.FormattingEnabled = true;
            this.cmbUpdateFilms.Location = new System.Drawing.Point(120, 20);
            this.cmbUpdateFilms.Name = "cmbUpdateFilms";
            this.cmbUpdateFilms.Size = new System.Drawing.Size(200, 20);
            this.cmbUpdateFilms.TabIndex = 20;
            this.cmbUpdateFilms.SelectedIndexChanged += new System.EventHandler(this.cmbUpdateFilms_SelectedIndexChanged);
            // 
            // btnClearUpdateFields
            // 
            this.btnClearUpdateFields.Location = new System.Drawing.Point(260, 380);
            this.btnClearUpdateFields.Name = "btnClearUpdateFields";
            this.btnClearUpdateFields.Size = new System.Drawing.Size(120, 30);
            this.btnClearUpdateFields.TabIndex = 19;
            this.btnClearUpdateFields.Text = "清空";
            this.btnClearUpdateFields.UseVisualStyleBackColor = true;
            this.btnClearUpdateFields.Click += new System.EventHandler(this.btnClearUpdateFields_Click);
            // 
            // btnUpdateFilm
            // 
            this.btnUpdateFilm.Location = new System.Drawing.Point(120, 380);
            this.btnUpdateFilm.Name = "btnUpdateFilm";
            this.btnUpdateFilm.Size = new System.Drawing.Size(120, 30);
            this.btnUpdateFilm.TabIndex = 18;
            this.btnUpdateFilm.Text = "更新电影";
            this.btnUpdateFilm.UseVisualStyleBackColor = true;
            this.btnUpdateFilm.Click += new System.EventHandler(this.btnUpdateFilm_Click);
            // 
            // txtUpdateAdmissions
            // 
            this.txtUpdateAdmissions.Location = new System.Drawing.Point(120, 340);
            this.txtUpdateAdmissions.Name = "txtUpdateAdmissions";
            this.txtUpdateAdmissions.Size = new System.Drawing.Size(100, 21);
            this.txtUpdateAdmissions.TabIndex = 17;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(50, 345);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 12);
            this.label14.TabIndex = 16;
            this.label14.Text = "观影人次：";
            // 
            // txtUpdateBoxOffice
            // 
            this.txtUpdateBoxOffice.Location = new System.Drawing.Point(120, 300);
            this.txtUpdateBoxOffice.Name = "txtUpdateBoxOffice";
            this.txtUpdateBoxOffice.Size = new System.Drawing.Size(100, 21);
            this.txtUpdateBoxOffice.TabIndex = 15;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(50, 305);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(41, 12);
            this.label13.TabIndex = 14;
            this.label13.Text = "票房：";
            // 
            // txtUpdateScore
            // 
            this.txtUpdateScore.Location = new System.Drawing.Point(120, 260);
            this.txtUpdateScore.Name = "txtUpdateScore";
            this.txtUpdateScore.Size = new System.Drawing.Size(100, 21);
            this.txtUpdateScore.TabIndex = 13;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(50, 265);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(41, 12);
            this.label12.TabIndex = 12;
            this.label12.Text = "评分：";
            // 
            // dtpUpdateEndDate
            // 
            this.dtpUpdateEndDate.Checked = false;
            this.dtpUpdateEndDate.Location = new System.Drawing.Point(120, 220);
            this.dtpUpdateEndDate.Name = "dtpUpdateEndDate";
            this.dtpUpdateEndDate.ShowCheckBox = true;
            this.dtpUpdateEndDate.Size = new System.Drawing.Size(200, 21);
            this.dtpUpdateEndDate.TabIndex = 11;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(50, 225);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 10;
            this.label11.Text = "下映日期：";
            // 
            // dtpUpdateReleaseDate
            // 
            this.dtpUpdateReleaseDate.Location = new System.Drawing.Point(120, 180);
            this.dtpUpdateReleaseDate.Name = "dtpUpdateReleaseDate";
            this.dtpUpdateReleaseDate.Size = new System.Drawing.Size(200, 21);
            this.dtpUpdateReleaseDate.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(50, 185);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 8;
            this.label10.Text = "上映日期：";
            // 
            // txtUpdateNormalPrice
            // 
            this.txtUpdateNormalPrice.Location = new System.Drawing.Point(120, 140);
            this.txtUpdateNormalPrice.Name = "txtUpdateNormalPrice";
            this.txtUpdateNormalPrice.Size = new System.Drawing.Size(100, 21);
            this.txtUpdateNormalPrice.TabIndex = 7;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(50, 145);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 12);
            this.label9.TabIndex = 6;
            this.label9.Text = "普通票价：";
            // 
            // txtUpdateFilmLength
            // 
            this.txtUpdateFilmLength.Location = new System.Drawing.Point(120, 100);
            this.txtUpdateFilmLength.Name = "txtUpdateFilmLength";
            this.txtUpdateFilmLength.Size = new System.Drawing.Size(100, 21);
            this.txtUpdateFilmLength.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(50, 105);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 4;
            this.label8.Text = "电影时长：";
            // 
            // txtUpdateGenre
            // 
            this.txtUpdateGenre.Location = new System.Drawing.Point(120, 60);
            this.txtUpdateGenre.Name = "txtUpdateGenre";
            this.txtUpdateGenre.Size = new System.Drawing.Size(200, 21);
            this.txtUpdateGenre.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(50, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 2;
            this.label7.Text = "电影类型：";
            // 
            // txtUpdateFilmName
            // 
            this.txtUpdateFilmName.Location = new System.Drawing.Point(120, 20);
            this.txtUpdateFilmName.Name = "txtUpdateFilmName";
            this.txtUpdateFilmName.Size = new System.Drawing.Size(200, 21);
            this.txtUpdateFilmName.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(50, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "电影名称：";
            // 
            // FilmManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "FilmManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "电影管理";
            this.Load += new System.EventHandler(this.FilmManagementForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageAdd.ResumeLayout(false);
            this.tabPageAdd.PerformLayout();
            this.tabPageUpdate.ResumeLayout(false);
            this.tabPageUpdate.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageAdd;
        private System.Windows.Forms.Button btnAddFilm;
        private System.Windows.Forms.DateTimePicker dtpReleaseDate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtNormalPrice;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFilmLength;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtGenre;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFilmName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPageUpdate;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox cmbUpdateFilms;
        private System.Windows.Forms.Button btnClearUpdateFields;
        private System.Windows.Forms.Button btnUpdateFilm;
        private System.Windows.Forms.TextBox txtUpdateAdmissions;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtUpdateBoxOffice;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtUpdateScore;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.DateTimePicker dtpUpdateEndDate;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.DateTimePicker dtpUpdateReleaseDate;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtUpdateNormalPrice;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtUpdateFilmLength;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtUpdateGenre;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtUpdateFilmName;
        private System.Windows.Forms.Label label6;
    }
}