namespace CodeWizardPS3
{
    partial class OptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.OkayButt = new System.Windows.Forms.Button();
            this.CancButt = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rad1BA = new System.Windows.Forms.RadioButton();
            this.rad1HSA = new System.Windows.Forms.RadioButton();
            this.rad1NC = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rboGTL = new System.Windows.Forms.RadioButton();
            this.rboLTG = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.MaxMemRange = new System.Windows.Forms.TextBox();
            this.MinMemRange = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.stackPointer = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.themeLight = new System.Windows.Forms.RadioButton();
            this.themeDark = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // OkayButt
            // 
            this.OkayButt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.OkayButt.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OkayButt.Location = new System.Drawing.Point(12, 177);
            this.OkayButt.Name = "OkayButt";
            this.OkayButt.Size = new System.Drawing.Size(103, 29);
            this.OkayButt.TabIndex = 1;
            this.OkayButt.Text = "Okay";
            this.OkayButt.UseVisualStyleBackColor = true;
            this.OkayButt.Click += new System.EventHandler(this.OkayButt_Click);
            // 
            // CancButt
            // 
            this.CancButt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.CancButt.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancButt.Location = new System.Drawing.Point(231, 177);
            this.CancButt.Name = "CancButt";
            this.CancButt.Size = new System.Drawing.Size(103, 29);
            this.CancButt.TabIndex = 2;
            this.CancButt.Text = "Cancel";
            this.CancButt.UseVisualStyleBackColor = true;
            this.CancButt.Click += new System.EventHandler(this.CancButt_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rad1BA);
            this.groupBox1.Controls.Add(this.rad1HSA);
            this.groupBox1.Controls.Add(this.rad1NC);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(322, 49);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output Type";
            // 
            // rad1BA
            // 
            this.rad1BA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.rad1BA.Location = new System.Drawing.Point(233, 21);
            this.rad1BA.Name = "rad1BA";
            this.rad1BA.Size = new System.Drawing.Size(75, 18);
            this.rad1BA.TabIndex = 6;
            this.rad1BA.Text = "Byte Array";
            this.rad1BA.UseVisualStyleBackColor = true;
            this.rad1BA.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // rad1HSA
            // 
            this.rad1HSA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.rad1HSA.Location = new System.Drawing.Point(112, 21);
            this.rad1HSA.Name = "rad1HSA";
            this.rad1HSA.Size = new System.Drawing.Size(106, 18);
            this.rad1HSA.TabIndex = 5;
            this.rad1HSA.Text = "Hex String Array";
            this.rad1HSA.UseVisualStyleBackColor = true;
            this.rad1HSA.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // rad1NC
            // 
            this.rad1NC.Checked = true;
            this.rad1NC.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.rad1NC.Location = new System.Drawing.Point(6, 21);
            this.rad1NC.Name = "rad1NC";
            this.rad1NC.Size = new System.Drawing.Size(93, 18);
            this.rad1NC.TabIndex = 4;
            this.rad1NC.TabStop = true;
            this.rad1NC.Text = "NetCheat PS3";
            this.rad1NC.UseVisualStyleBackColor = true;
            this.rad1NC.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rboGTL);
            this.groupBox2.Controls.Add(this.rboLTG);
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(12, 67);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(224, 49);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Register Order";
            // 
            // rboGTL
            // 
            this.rboGTL.Checked = true;
            this.rboGTL.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.rboGTL.Location = new System.Drawing.Point(127, 21);
            this.rboGTL.Name = "rboGTL";
            this.rboGTL.Size = new System.Drawing.Size(91, 18);
            this.rboGTL.TabIndex = 6;
            this.rboGTL.TabStop = true;
            this.rboGTL.Text = "Right To Left";
            this.rboGTL.UseVisualStyleBackColor = true;
            this.rboGTL.CheckedChanged += new System.EventHandler(this.rboGTL_CheckedChanged);
            // 
            // rboLTG
            // 
            this.rboLTG.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.rboLTG.Location = new System.Drawing.Point(6, 21);
            this.rboLTG.Name = "rboLTG";
            this.rboLTG.Size = new System.Drawing.Size(134, 18);
            this.rboLTG.TabIndex = 4;
            this.rboLTG.Text = "Left To Right";
            this.rboLTG.UseVisualStyleBackColor = true;
            this.rboLTG.CheckedChanged += new System.EventHandler(this.rboLTG_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.MaxMemRange);
            this.groupBox3.Controls.Add(this.MinMemRange);
            this.groupBox3.ForeColor = System.Drawing.Color.White;
            this.groupBox3.Location = new System.Drawing.Point(12, 122);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(131, 49);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Memory Range";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(60, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "-";
            // 
            // MaxMemRange
            // 
            this.MaxMemRange.BackColor = System.Drawing.Color.Black;
            this.MaxMemRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MaxMemRange.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxMemRange.ForeColor = System.Drawing.Color.White;
            this.MaxMemRange.Location = new System.Drawing.Point(73, 21);
            this.MaxMemRange.MaxLength = 8;
            this.MaxMemRange.Name = "MaxMemRange";
            this.MaxMemRange.Size = new System.Drawing.Size(52, 20);
            this.MaxMemRange.TabIndex = 1;
            this.MaxMemRange.Text = "02000000";
            // 
            // MinMemRange
            // 
            this.MinMemRange.BackColor = System.Drawing.Color.Black;
            this.MinMemRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MinMemRange.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinMemRange.ForeColor = System.Drawing.Color.White;
            this.MinMemRange.Location = new System.Drawing.Point(6, 21);
            this.MinMemRange.MaxLength = 8;
            this.MinMemRange.Name = "MinMemRange";
            this.MinMemRange.Size = new System.Drawing.Size(52, 20);
            this.MinMemRange.TabIndex = 0;
            this.MinMemRange.Text = "00000000";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.stackPointer);
            this.groupBox4.ForeColor = System.Drawing.Color.White;
            this.groupBox4.Location = new System.Drawing.Point(242, 67);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(92, 49);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Stack Pointer";
            // 
            // stackPointer
            // 
            this.stackPointer.BackColor = System.Drawing.Color.Black;
            this.stackPointer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stackPointer.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stackPointer.ForeColor = System.Drawing.Color.White;
            this.stackPointer.Location = new System.Drawing.Point(20, 21);
            this.stackPointer.MaxLength = 8;
            this.stackPointer.Name = "stackPointer";
            this.stackPointer.Size = new System.Drawing.Size(52, 20);
            this.stackPointer.TabIndex = 1;
            this.stackPointer.Text = "02000000";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.themeLight);
            this.groupBox5.Controls.Add(this.themeDark);
            this.groupBox5.ForeColor = System.Drawing.Color.White;
            this.groupBox5.Location = new System.Drawing.Point(149, 122);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(185, 49);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Theme";
            this.groupBox5.Visible = false;
            // 
            // themeLight
            // 
            this.themeLight.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.themeLight.Location = new System.Drawing.Point(93, 21);
            this.themeLight.Name = "themeLight";
            this.themeLight.Size = new System.Drawing.Size(86, 18);
            this.themeLight.TabIndex = 8;
            this.themeLight.Text = "Light Theme";
            this.themeLight.UseVisualStyleBackColor = true;
            // 
            // themeDark
            // 
            this.themeDark.Checked = true;
            this.themeDark.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.themeDark.Location = new System.Drawing.Point(6, 21);
            this.themeDark.Name = "themeDark";
            this.themeDark.Size = new System.Drawing.Size(85, 18);
            this.themeDark.TabIndex = 7;
            this.themeDark.TabStop = true;
            this.themeDark.Text = "Dark Theme";
            this.themeDark.UseVisualStyleBackColor = true;
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(346, 216);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CancButt);
            this.Controls.Add(this.OkayButt);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OptionsForm";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button OkayButt;
        private System.Windows.Forms.Button CancButt;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rad1BA;
        private System.Windows.Forms.RadioButton rad1HSA;
        private System.Windows.Forms.RadioButton rad1NC;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rboGTL;
        private System.Windows.Forms.RadioButton rboLTG;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox MaxMemRange;
        private System.Windows.Forms.TextBox MinMemRange;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox stackPointer;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton themeLight;
        private System.Windows.Forms.RadioButton themeDark;
    }
}