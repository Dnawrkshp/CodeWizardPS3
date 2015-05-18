namespace CodeWizardPS3
{
    partial class PPCReg
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NameLab = new System.Windows.Forms.Label();
            this.RangeLab1 = new System.Windows.Forms.Label();
            this.RangeLab2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // NameLab
            // 
            this.NameLab.AutoSize = true;
            this.NameLab.Location = new System.Drawing.Point(3, 4);
            this.NameLab.Name = "NameLab";
            this.NameLab.Size = new System.Drawing.Size(35, 15);
            this.NameLab.TabIndex = 0;
            this.NameLab.Text = "NAME";
            // 
            // RangeLab1
            // 
            this.RangeLab1.AutoSize = true;
            this.RangeLab1.Location = new System.Drawing.Point(65, 4);
            this.RangeLab1.Name = "RangeLab1";
            this.RangeLab1.Size = new System.Drawing.Size(14, 15);
            this.RangeLab1.TabIndex = 1;
            this.RangeLab1.Text = "0";
            // 
            // RangeLab2
            // 
            this.RangeLab2.AutoSize = true;
            this.RangeLab2.Location = new System.Drawing.Point(337, 4);
            this.RangeLab2.Name = "RangeLab2";
            this.RangeLab2.Size = new System.Drawing.Size(14, 15);
            this.RangeLab2.TabIndex = 2;
            this.RangeLab2.Text = "4";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.Black;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(85, 4);
            this.textBox1.MaxLength = 8;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(60, 16);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "00000000";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.Color.Black;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.ForeColor = System.Drawing.Color.White;
            this.textBox2.Location = new System.Drawing.Point(148, 4);
            this.textBox2.MaxLength = 8;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(60, 16);
            this.textBox2.TabIndex = 4;
            this.textBox2.Text = "00000000";
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.Color.Black;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.ForeColor = System.Drawing.Color.White;
            this.textBox3.Location = new System.Drawing.Point(211, 4);
            this.textBox3.MaxLength = 8;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(60, 16);
            this.textBox3.TabIndex = 5;
            this.textBox3.Text = "00000000";
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.Color.Black;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.ForeColor = System.Drawing.Color.White;
            this.textBox4.Location = new System.Drawing.Point(274, 4);
            this.textBox4.MaxLength = 8;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(60, 16);
            this.textBox4.TabIndex = 6;
            this.textBox4.Text = "00000000";
            // 
            // PPCReg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.RangeLab2);
            this.Controls.Add(this.RangeLab1);
            this.Controls.Add(this.NameLab);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "PPCReg";
            this.Size = new System.Drawing.Size(367, 25);
            this.Load += new System.EventHandler(this.PPCReg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NameLab;
        private System.Windows.Forms.Label RangeLab1;
        private System.Windows.Forms.Label RangeLab2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
    }
}
