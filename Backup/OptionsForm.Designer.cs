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
            this.colCommand = new System.Windows.Forms.CheckBox();
            this.colCom = new System.Windows.Forms.CheckBox();
            this.colLab = new System.Windows.Forms.CheckBox();
            this.colReg = new System.Windows.Forms.CheckBox();
            this.colIns = new System.Windows.Forms.CheckBox();
            this.colEnabled = new System.Windows.Forms.CheckBox();
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
            this.OkayButt.Location = new System.Drawing.Point(12, 276);
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
            this.CancButt.Location = new System.Drawing.Point(231, 276);
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
            this.rboGTL.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.rboGTL.Location = new System.Drawing.Point(127, 21);
            this.rboGTL.Name = "rboGTL";
            this.rboGTL.Size = new System.Drawing.Size(91, 18);
            this.rboGTL.TabIndex = 6;
            this.rboGTL.Text = "Right To Left";
            this.rboGTL.UseVisualStyleBackColor = true;
            this.rboGTL.CheckedChanged += new System.EventHandler(this.rboGTL_CheckedChanged);
            // 
            // rboLTG
            // 
            this.rboLTG.Checked = true;
            this.rboLTG.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.rboLTG.Location = new System.Drawing.Point(6, 21);
            this.rboLTG.Name = "rboLTG";
            this.rboLTG.Size = new System.Drawing.Size(134, 18);
            this.rboLTG.TabIndex = 4;
            this.rboLTG.TabStop = true;
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
            this.groupBox3.Location = new System.Drawing.Point(102, 221);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(140, 49);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Memory Range";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 23);
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
            this.MaxMemRange.Location = new System.Drawing.Point(82, 21);
            this.MaxMemRange.MaxLength = 8;
            this.MaxMemRange.Name = "MaxMemRange";
            this.MaxMemRange.Size = new System.Drawing.Size(52, 20);
            this.MaxMemRange.TabIndex = 1;
            this.MaxMemRange.Text = "01BB0000";
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
            this.stackPointer.Text = "01BB0000";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.colCommand);
            this.groupBox5.Controls.Add(this.colCom);
            this.groupBox5.Controls.Add(this.colLab);
            this.groupBox5.Controls.Add(this.colReg);
            this.groupBox5.Controls.Add(this.colIns);
            this.groupBox5.Controls.Add(this.colEnabled);
            this.groupBox5.ForeColor = System.Drawing.Color.White;
            this.groupBox5.Location = new System.Drawing.Point(12, 122);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(322, 93);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Colorizer";
            // 
            // colCommand
            // 
            this.colCommand.AutoSize = true;
            this.colCommand.Checked = true;
            this.colCommand.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colCommand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colCommand.Location = new System.Drawing.Point(199, 45);
            this.colCommand.Name = "colCommand";
            this.colCommand.Size = new System.Drawing.Size(112, 17);
            this.colCommand.TabIndex = 5;
            this.colCommand.Text = "Color Commands";
            this.colCommand.UseVisualStyleBackColor = true;
            // 
            // colCom
            // 
            this.colCom.AutoSize = true;
            this.colCom.Checked = true;
            this.colCom.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colCom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colCom.Location = new System.Drawing.Point(199, 21);
            this.colCom.Name = "colCom";
            this.colCom.Size = new System.Drawing.Size(109, 17);
            this.colCom.TabIndex = 4;
            this.colCom.Text = "Color Comments";
            this.colCom.UseVisualStyleBackColor = true;
            // 
            // colLab
            // 
            this.colLab.AutoSize = true;
            this.colLab.Checked = true;
            this.colLab.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colLab.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colLab.Location = new System.Drawing.Point(199, 68);
            this.colLab.Name = "colLab";
            this.colLab.Size = new System.Drawing.Size(87, 17);
            this.colLab.TabIndex = 3;
            this.colLab.Text = "Color Labels";
            this.colLab.UseVisualStyleBackColor = true;
            // 
            // colReg
            // 
            this.colReg.AutoSize = true;
            this.colReg.Checked = true;
            this.colReg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colReg.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colReg.Location = new System.Drawing.Point(7, 68);
            this.colReg.Name = "colReg";
            this.colReg.Size = new System.Drawing.Size(102, 17);
            this.colReg.TabIndex = 2;
            this.colReg.Text = "Color Registers";
            this.colReg.UseVisualStyleBackColor = true;
            // 
            // colIns
            // 
            this.colIns.AutoSize = true;
            this.colIns.Checked = true;
            this.colIns.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colIns.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colIns.Location = new System.Drawing.Point(7, 45);
            this.colIns.Name = "colIns";
            this.colIns.Size = new System.Drawing.Size(116, 17);
            this.colIns.TabIndex = 1;
            this.colIns.Text = "Color Instructions";
            this.colIns.UseVisualStyleBackColor = true;
            // 
            // colEnabled
            // 
            this.colEnabled.AutoSize = true;
            this.colEnabled.Checked = true;
            this.colEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colEnabled.Location = new System.Drawing.Point(7, 22);
            this.colEnabled.Name = "colEnabled";
            this.colEnabled.Size = new System.Drawing.Size(66, 17);
            this.colEnabled.TabIndex = 0;
            this.colEnabled.Text = "Enabled";
            this.colEnabled.UseVisualStyleBackColor = true;
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(346, 321);
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
            this.groupBox5.PerformLayout();
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
        private System.Windows.Forms.CheckBox colIns;
        private System.Windows.Forms.CheckBox colEnabled;
        private System.Windows.Forms.CheckBox colCommand;
        private System.Windows.Forms.CheckBox colCom;
        private System.Windows.Forms.CheckBox colLab;
        private System.Windows.Forms.CheckBox colReg;
    }
}