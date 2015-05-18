namespace CodeWizardPS3
{
    partial class InsBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsBox));
            this.ListIns = new System.Windows.Forms.ListBox();
            this.TextIns = new System.Windows.Forms.RichTextBox();
            this.ListCom = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ListReg = new System.Windows.Forms.ListBox();
            this.ListTerm = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ListIns
            // 
            this.ListIns.BackColor = System.Drawing.Color.Black;
            this.ListIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListIns.ForeColor = System.Drawing.Color.White;
            this.ListIns.FormattingEnabled = true;
            this.ListIns.Location = new System.Drawing.Point(12, 25);
            this.ListIns.Name = "ListIns";
            this.ListIns.Size = new System.Drawing.Size(137, 67);
            this.ListIns.TabIndex = 0;
            this.ListIns.SelectedIndexChanged += new System.EventHandler(this.ListIns_SelectedIndexChanged);
            // 
            // TextIns
            // 
            this.TextIns.BackColor = System.Drawing.Color.Black;
            this.TextIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextIns.ForeColor = System.Drawing.Color.White;
            this.TextIns.Location = new System.Drawing.Point(155, 12);
            this.TextIns.Name = "TextIns";
            this.TextIns.ReadOnly = true;
            this.TextIns.Size = new System.Drawing.Size(251, 338);
            this.TextIns.TabIndex = 1;
            this.TextIns.Text = "The default output used in these examples are NetCheat PS3.\n\nIn the case of anoth" +
                "er output type (when assembling), the output would match that output type.\n\n";
            // 
            // ListCom
            // 
            this.ListCom.BackColor = System.Drawing.Color.Black;
            this.ListCom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListCom.ForeColor = System.Drawing.Color.White;
            this.ListCom.FormattingEnabled = true;
            this.ListCom.Location = new System.Drawing.Point(13, 111);
            this.ListCom.Name = "ListCom";
            this.ListCom.Size = new System.Drawing.Size(136, 67);
            this.ListCom.TabIndex = 2;
            this.ListCom.SelectedIndexChanged += new System.EventHandler(this.ListCom_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Instructions";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Commands";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 181);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Registers";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ListReg
            // 
            this.ListReg.BackColor = System.Drawing.Color.Black;
            this.ListReg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListReg.ForeColor = System.Drawing.Color.White;
            this.ListReg.FormattingEnabled = true;
            this.ListReg.Location = new System.Drawing.Point(13, 197);
            this.ListReg.Name = "ListReg";
            this.ListReg.Size = new System.Drawing.Size(136, 67);
            this.ListReg.TabIndex = 6;
            this.ListReg.SelectedIndexChanged += new System.EventHandler(this.ListReg_SelectedIndexChanged);
            // 
            // ListTerm
            // 
            this.ListTerm.BackColor = System.Drawing.Color.Black;
            this.ListTerm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListTerm.ForeColor = System.Drawing.Color.White;
            this.ListTerm.FormattingEnabled = true;
            this.ListTerm.Location = new System.Drawing.Point(13, 283);
            this.ListTerm.Name = "ListTerm";
            this.ListTerm.Size = new System.Drawing.Size(136, 67);
            this.ListTerm.TabIndex = 8;
            this.ListTerm.SelectedIndexChanged += new System.EventHandler(this.ListTerm_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 267);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Terms";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // InsBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(415, 360);
            this.Controls.Add(this.ListTerm);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ListReg);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ListCom);
            this.Controls.Add(this.TextIns);
            this.Controls.Add(this.ListIns);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InsBox";
            this.Text = "Supported Instructions And Registers";
            this.Load += new System.EventHandler(this.InsBox_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ListIns;
        private System.Windows.Forms.ListBox ListCom;
        private System.Windows.Forms.RichTextBox TextIns;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox ListReg;
        private System.Windows.Forms.ListBox ListTerm;
        private System.Windows.Forms.Label label4;
    }
}