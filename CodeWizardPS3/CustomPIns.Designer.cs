namespace CodeWizardPS3
{
    partial class CustomPIns
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomPIns));
            this.listCPI = new System.Windows.Forms.ListBox();
            this.namePCI = new System.Windows.Forms.TextBox();
            this.labPCI1 = new System.Windows.Forms.Label();
            this.labPCI2 = new System.Windows.Forms.Label();
            this.formatPCI = new System.Windows.Forms.TextBox();
            this.labPCI3 = new System.Windows.Forms.Label();
            this.savePCI = new System.Windows.Forms.Button();
            this.exitPCI = new System.Windows.Forms.Button();
            this.asmPCI = new System.Windows.Forms.RichTextBox();
            this.openDir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listCPI
            // 
            this.listCPI.BackColor = System.Drawing.Color.Black;
            this.listCPI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listCPI.ForeColor = System.Drawing.Color.White;
            this.listCPI.FormattingEnabled = true;
            this.listCPI.ItemHeight = 15;
            this.listCPI.Location = new System.Drawing.Point(12, 12);
            this.listCPI.Name = "listCPI";
            this.listCPI.Size = new System.Drawing.Size(216, 452);
            this.listCPI.TabIndex = 0;
            this.listCPI.SelectedIndexChanged += new System.EventHandler(this.listCPI_SelectedIndexChanged);
            this.listCPI.DoubleClick += new System.EventHandler(this.listCPI_DoubleClick);
            this.listCPI.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listCPI_KeyUp);
            // 
            // namePCI
            // 
            this.namePCI.BackColor = System.Drawing.Color.Black;
            this.namePCI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.namePCI.ForeColor = System.Drawing.Color.White;
            this.namePCI.Location = new System.Drawing.Point(235, 32);
            this.namePCI.Name = "namePCI";
            this.namePCI.Size = new System.Drawing.Size(369, 23);
            this.namePCI.TabIndex = 1;
            this.namePCI.TextChanged += new System.EventHandler(this.namePCI_TextChanged);
            // 
            // labPCI1
            // 
            this.labPCI1.Location = new System.Drawing.Point(235, 12);
            this.labPCI1.Name = "labPCI1";
            this.labPCI1.Size = new System.Drawing.Size(369, 14);
            this.labPCI1.TabIndex = 2;
            this.labPCI1.Text = "Name Of Pseudo Instruction";
            this.labPCI1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labPCI2
            // 
            this.labPCI2.Location = new System.Drawing.Point(235, 58);
            this.labPCI2.Name = "labPCI2";
            this.labPCI2.Size = new System.Drawing.Size(369, 14);
            this.labPCI2.TabIndex = 4;
            this.labPCI2.Text = "Format";
            this.labPCI2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // formatPCI
            // 
            this.formatPCI.BackColor = System.Drawing.Color.Black;
            this.formatPCI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.formatPCI.ForeColor = System.Drawing.Color.White;
            this.formatPCI.Location = new System.Drawing.Point(235, 78);
            this.formatPCI.Name = "formatPCI";
            this.formatPCI.Size = new System.Drawing.Size(369, 23);
            this.formatPCI.TabIndex = 3;
            this.formatPCI.TextChanged += new System.EventHandler(this.formatPCI_TextChanged);
            // 
            // labPCI3
            // 
            this.labPCI3.Location = new System.Drawing.Point(234, 104);
            this.labPCI3.Name = "labPCI3";
            this.labPCI3.Size = new System.Drawing.Size(370, 14);
            this.labPCI3.TabIndex = 6;
            this.labPCI3.Text = "Assembly";
            this.labPCI3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // savePCI
            // 
            this.savePCI.BackColor = System.Drawing.Color.Black;
            this.savePCI.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.savePCI.Location = new System.Drawing.Point(234, 456);
            this.savePCI.Name = "savePCI";
            this.savePCI.Size = new System.Drawing.Size(107, 23);
            this.savePCI.TabIndex = 7;
            this.savePCI.Text = "Save";
            this.savePCI.UseVisualStyleBackColor = false;
            this.savePCI.Click += new System.EventHandler(this.savePCI_Click);
            // 
            // exitPCI
            // 
            this.exitPCI.BackColor = System.Drawing.Color.Black;
            this.exitPCI.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.exitPCI.Location = new System.Drawing.Point(497, 455);
            this.exitPCI.Name = "exitPCI";
            this.exitPCI.Size = new System.Drawing.Size(107, 23);
            this.exitPCI.TabIndex = 8;
            this.exitPCI.Text = "Exit";
            this.exitPCI.UseVisualStyleBackColor = false;
            this.exitPCI.Click += new System.EventHandler(this.exitPCI_Click);
            // 
            // asmPCI
            // 
            this.asmPCI.BackColor = System.Drawing.Color.Black;
            this.asmPCI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.asmPCI.ForeColor = System.Drawing.Color.White;
            this.asmPCI.Location = new System.Drawing.Point(235, 122);
            this.asmPCI.Name = "asmPCI";
            this.asmPCI.Size = new System.Drawing.Size(369, 327);
            this.asmPCI.TabIndex = 9;
            this.asmPCI.Text = "";
            this.asmPCI.TextChanged += new System.EventHandler(this.asmPCI_TextChanged);
            // 
            // openDir
            // 
            this.openDir.BackColor = System.Drawing.Color.Black;
            this.openDir.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.openDir.Location = new System.Drawing.Point(347, 456);
            this.openDir.Name = "openDir";
            this.openDir.Size = new System.Drawing.Size(144, 23);
            this.openDir.TabIndex = 10;
            this.openDir.Text = "Open Directory";
            this.openDir.UseVisualStyleBackColor = false;
            this.openDir.Click += new System.EventHandler(this.openDir_Click);
            // 
            // CustomPIns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(616, 487);
            this.Controls.Add(this.openDir);
            this.Controls.Add(this.asmPCI);
            this.Controls.Add(this.exitPCI);
            this.Controls.Add(this.savePCI);
            this.Controls.Add(this.labPCI3);
            this.Controls.Add(this.labPCI2);
            this.Controls.Add(this.formatPCI);
            this.Controls.Add(this.labPCI1);
            this.Controls.Add(this.namePCI);
            this.Controls.Add(this.listCPI);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(632, 521);
            this.MinimumSize = new System.Drawing.Size(632, 521);
            this.Name = "CustomPIns";
            this.Text = "Custom Pseudo Instructions";
            this.Load += new System.EventHandler(this.CustomPIns_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listCPI;
        private System.Windows.Forms.TextBox namePCI;
        private System.Windows.Forms.Label labPCI1;
        private System.Windows.Forms.Label labPCI2;
        private System.Windows.Forms.TextBox formatPCI;
        private System.Windows.Forms.Label labPCI3;
        private System.Windows.Forms.Button savePCI;
        private System.Windows.Forms.Button exitPCI;
        private System.Windows.Forms.RichTextBox asmPCI;
        private System.Windows.Forms.Button openDir;
    }
}