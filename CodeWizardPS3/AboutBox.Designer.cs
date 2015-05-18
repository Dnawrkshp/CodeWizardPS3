namespace CodeWizardPS3
{
    partial class AboutBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.FontLink = new System.Windows.Forms.Button();
            this.SourceLink = new System.Windows.Forms.Button();
            this.CloseBox = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FontLink
            // 
            this.FontLink.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.FontLink.Image = ((System.Drawing.Image)(resources.GetObject("FontLink.Image")));
            this.FontLink.Location = new System.Drawing.Point(12, 287);
            this.FontLink.Name = "FontLink";
            this.FontLink.Size = new System.Drawing.Size(454, 50);
            this.FontLink.TabIndex = 0;
            this.FontLink.UseVisualStyleBackColor = true;
            this.FontLink.Click += new System.EventHandler(this.FontLink_Click);
            // 
            // SourceLink
            // 
            this.SourceLink.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SourceLink.Image = ((System.Drawing.Image)(resources.GetObject("SourceLink.Image")));
            this.SourceLink.Location = new System.Drawing.Point(12, 343);
            this.SourceLink.Name = "SourceLink";
            this.SourceLink.Size = new System.Drawing.Size(454, 50);
            this.SourceLink.TabIndex = 1;
            this.SourceLink.UseVisualStyleBackColor = true;
            this.SourceLink.Click += new System.EventHandler(this.SourceLink_Click);
            // 
            // CloseBox
            // 
            this.CloseBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.CloseBox.Image = ((System.Drawing.Image)(resources.GetObject("CloseBox.Image")));
            this.CloseBox.Location = new System.Drawing.Point(12, 399);
            this.CloseBox.Name = "CloseBox";
            this.CloseBox.Size = new System.Drawing.Size(454, 50);
            this.CloseBox.TabIndex = 2;
            this.CloseBox.UseVisualStyleBackColor = true;
            this.CloseBox.Click += new System.EventHandler(this.Close_Click);
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(478, 458);
            this.ControlBox = false;
            this.Controls.Add(this.CloseBox);
            this.Controls.Add(this.SourceLink);
            this.Controls.Add(this.FontLink);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AboutBox";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button FontLink;
        private System.Windows.Forms.Button SourceLink;
        private System.Windows.Forms.Button CloseBox;
    }
}