namespace cinemaapp
{
    partial class FilmSelectionForm
    {
        private System.ComponentModel.IContainer components = null;

        private SplitContainer splitContainer1;
        private FlowLayoutPanel flowLayoutPanelFilms;
        private Panel panelDetails;
        private PictureBox pictureBoxPoster;
        private Label lblFilmName;
        private Label lblGenre;
        private Label lblDuration;
        private Label lblScore;
        private Label lblBoxOffice;
        private Label lblAdmissions;
        private Label lblPrice;
        private DateTimePicker dateTimePicker;
        private Button btnSearchSections;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanelFilms = new System.Windows.Forms.FlowLayoutPanel();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.btnSearchSections = new System.Windows.Forms.Button();
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.lblPrice = new System.Windows.Forms.Label();
            this.lblAdmissions = new System.Windows.Forms.Label();
            this.lblBoxOffice = new System.Windows.Forms.Label();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblFilmName = new System.Windows.Forms.Label();
            this.pictureBoxPoster = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPoster)).BeginInit();
            this.SuspendLayout();

            // splitContainer1
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";

            // splitContainer1.Panel1 (Film List)
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanelFilms);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Panel1MinSize = 600;  // Increased minimum width for film list

            // splitContainer1.Panel2 (Details)
            this.splitContainer1.Panel2.Controls.Add(this.panelDetails);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Panel2MinSize = 500;  // Increased minimum width for details

            this.splitContainer1.Size = new System.Drawing.Size(1400, 700); // Wider and shorter form
            this.splitContainer1.SplitterDistance = 850;  // Allocate more width to film list
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 0;

            // flowLayoutPanelFilms
            this.flowLayoutPanelFilms.AutoScroll = true;
            this.flowLayoutPanelFilms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelFilms.Location = new System.Drawing.Point(5, 5);
            this.flowLayoutPanelFilms.Name = "flowLayoutPanelFilms";
            this.flowLayoutPanelFilms.Size = new System.Drawing.Size(840, 690);
            this.flowLayoutPanelFilms.TabIndex = 0;

            // panelDetails
            this.panelDetails.Controls.Add(this.btnSearchSections);
            this.panelDetails.Controls.Add(this.dateTimePicker);
            this.panelDetails.Controls.Add(this.lblPrice);
            this.panelDetails.Controls.Add(this.lblAdmissions);
            this.panelDetails.Controls.Add(this.lblBoxOffice);
            this.panelDetails.Controls.Add(this.lblScore);
            this.panelDetails.Controls.Add(this.lblDuration);
            this.panelDetails.Controls.Add(this.lblGenre);
            this.panelDetails.Controls.Add(this.lblFilmName);
            this.panelDetails.Controls.Add(this.pictureBoxPoster);
            this.panelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDetails.Location = new System.Drawing.Point(5, 5);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(532, 690);
            this.panelDetails.TabIndex = 0;

            // btnSearchSections
            this.btnSearchSections.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearchSections.Location = new System.Drawing.Point(400, 650);
            this.btnSearchSections.Name = "btnSearchSections";
            this.btnSearchSections.Size = new System.Drawing.Size(120, 30);
            this.btnSearchSections.TabIndex = 9;
            this.btnSearchSections.Text = "查询场次";
            this.btnSearchSections.UseVisualStyleBackColor = true;
            this.btnSearchSections.Click += new System.EventHandler(this.btnSearchSections_Click);

            // dateTimePicker
            this.dateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dateTimePicker.Location = new System.Drawing.Point(20, 655);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker.TabIndex = 8;

            // lblPrice
            this.lblPrice.AutoSize = true;
            this.lblPrice.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblPrice.Location = new System.Drawing.Point(350, 300);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(51, 20);
            this.lblPrice.TabIndex = 7;
            this.lblPrice.Text = "票价: ";

            // lblAdmissions
            this.lblAdmissions.AutoSize = true;
            this.lblAdmissions.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblAdmissions.Location = new System.Drawing.Point(350, 260);
            this.lblAdmissions.Name = "lblAdmissions";
            this.lblAdmissions.Size = new System.Drawing.Size(79, 20);
            this.lblAdmissions.TabIndex = 6;
            this.lblAdmissions.Text = "观影人次: ";

            // lblBoxOffice
            this.lblBoxOffice.AutoSize = true;
            this.lblBoxOffice.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblBoxOffice.Location = new System.Drawing.Point(350, 220);
            this.lblBoxOffice.Name = "lblBoxOffice";
            this.lblBoxOffice.Size = new System.Drawing.Size(51, 20);
            this.lblBoxOffice.TabIndex = 5;
            this.lblBoxOffice.Text = "票房: ";

            // lblScore
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblScore.Location = new System.Drawing.Point(350, 180);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(51, 20);
            this.lblScore.TabIndex = 4;
            this.lblScore.Text = "评分: ";

            // lblDuration
            this.lblDuration.AutoSize = true;
            this.lblDuration.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblDuration.Location = new System.Drawing.Point(350, 140);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(51, 20);
            this.lblDuration.TabIndex = 3;
            this.lblDuration.Text = "时长: ";

            // lblGenre
            this.lblGenre.AutoSize = true;
            this.lblGenre.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblGenre.Location = new System.Drawing.Point(350, 100);
            this.lblGenre.Name = "lblGenre";
            this.lblGenre.Size = new System.Drawing.Size(51, 20);
            this.lblGenre.TabIndex = 2;
            this.lblGenre.Text = "类型: ";

            // lblFilmName
            this.lblFilmName.AutoSize = true;
            this.lblFilmName.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.lblFilmName.Location = new System.Drawing.Point(350, 50);
            this.lblFilmName.Name = "lblFilmName";
            this.lblFilmName.Size = new System.Drawing.Size(69, 26);
            this.lblFilmName.TabIndex = 1;
            this.lblFilmName.Text = "电影名";

            // pictureBoxPoster - Removed border and increased size
            this.pictureBoxPoster.Location = new System.Drawing.Point(20, 0);
            this.pictureBoxPoster.Name = "pictureBoxPoster";
            this.pictureBoxPoster.Size = new System.Drawing.Size(300, 450); // Larger size
            this.pictureBoxPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPoster.TabIndex = 0;
            this.pictureBoxPoster.TabStop = false;

            // FilmSelectionForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 600); // Wider and shorter form
            this.Controls.Add(this.splitContainer1);
            this.Name = "FilmSelectionForm";
            this.Text = "电影选择";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPoster)).EndInit();
            this.ResumeLayout(false);
        }
    }
}