namespace CodeWizardPS3
{
    partial class DebugPopup
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
            this.DebugBox = new System.Windows.Forms.TextBox();
            this.OkayButt = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DebugBox
            // 
            this.DebugBox.BackColor = System.Drawing.Color.Black;
            this.DebugBox.ForeColor = System.Drawing.Color.White;
            this.DebugBox.Location = new System.Drawing.Point(12, 22);
            this.DebugBox.Multiline = true;
            this.DebugBox.Name = "DebugBox";
            this.DebugBox.Size = new System.Drawing.Size(260, 173);
            this.DebugBox.TabIndex = 0;
            this.DebugBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DebugBox.TextChanged += new System.EventHandler(this.DebugBox_TextChanged);
            // 
            // OkayButt
            // 
            this.OkayButt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OkayButt.ForeColor = System.Drawing.Color.White;
            this.OkayButt.Location = new System.Drawing.Point(12, 201);
            this.OkayButt.Name = "OkayButt";
            this.OkayButt.Size = new System.Drawing.Size(260, 25);
            this.OkayButt.TabIndex = 1;
            this.OkayButt.Text = "Okay";
            this.OkayButt.UseVisualStyleBackColor = true;
            this.OkayButt.Click += new System.EventHandler(this.OkayButt_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(12, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(260, 19);
            this.label1.TabIndex = 2;
            this.label1.Text = "Compile Error Output";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // DebugPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(284, 236);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OkayButt);
            this.Controls.Add(this.DebugBox);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DebugPopup";
            this.Load += new System.EventHandler(this.DebugPopup_Load);
            this.Shown += new System.EventHandler(this.DebugPopup_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox DebugBox;
        private System.Windows.Forms.Button OkayButt;
        private System.Windows.Forms.Label label1;
    }
}