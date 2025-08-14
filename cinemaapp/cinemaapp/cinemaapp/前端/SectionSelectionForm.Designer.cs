namespace cinemaapp
{
    partial class SectionSelectionForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView listViewSections;

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
            this.listViewSections = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // listViewSections
            // 
            listViewSections.Font = new Font("微软雅黑", 15); // 字体名称, 字号
            this.listViewSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSections.FullRowSelect = true;
            this.listViewSections.GridLines = true;
            this.listViewSections.HideSelection = false;
            this.listViewSections.Location = new System.Drawing.Point(0, 0);
            this.listViewSections.Name = "listViewSections";
            this.listViewSections.Size = new System.Drawing.Size(584, 361);
            this.listViewSections.TabIndex = 0;
            this.listViewSections.UseCompatibleStateImageBehavior = false;
            this.listViewSections.View = System.Windows.Forms.View.Details;
            // 
            // SectionSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.listViewSections);
            this.Name = "SectionSelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "场次选择";
            this.ResumeLayout(false);
        }
    }
}