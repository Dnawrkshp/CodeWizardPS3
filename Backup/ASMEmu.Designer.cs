namespace CodeWizardPS3
{
    partial class ASMEmu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ASMEmu));
            this.RegGroup = new System.Windows.Forms.GroupBox();
            this.FPregLab = new System.Windows.Forms.Label();
            this.GPregLab = new System.Windows.Forms.Label();
            this.SPregLab = new System.Windows.Forms.Label();
            this.MemoryBox = new System.Windows.Forms.GroupBox();
            this.StringBox = new System.Windows.Forms.ListBox();
            this.ASMBox = new System.Windows.Forms.ListBox();
            this.ValueBox = new System.Windows.Forms.ListBox();
            this.AddressBox = new System.Windows.Forms.ListBox();
            this.ControlsBox = new System.Windows.Forms.GroupBox();
            this.StepBack = new System.Windows.Forms.Button();
            this.debugBox = new System.Windows.Forms.TextBox();
            this.PBcycles = new System.Windows.Forms.PictureBox();
            this.CyclesBox = new System.Windows.Forms.TextBox();
            this.RunCycles = new System.Windows.Forms.Button();
            this.GotoAddrBox = new System.Windows.Forms.TextBox();
            this.GotoAddr = new System.Windows.Forms.Button();
            this.StepOver = new System.Windows.Forms.Button();
            this.StepInto = new System.Windows.Forms.Button();
            this.RegGroup.SuspendLayout();
            this.MemoryBox.SuspendLayout();
            this.ControlsBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBcycles)).BeginInit();
            this.SuspendLayout();
            // 
            // RegGroup
            // 
            this.RegGroup.Controls.Add(this.FPregLab);
            this.RegGroup.Controls.Add(this.GPregLab);
            this.RegGroup.Controls.Add(this.SPregLab);
            this.RegGroup.ForeColor = System.Drawing.Color.White;
            this.RegGroup.Location = new System.Drawing.Point(13, 13);
            this.RegGroup.Name = "RegGroup";
            this.RegGroup.Size = new System.Drawing.Size(384, 481);
            this.RegGroup.TabIndex = 0;
            this.RegGroup.TabStop = false;
            // 
            // FPregLab
            // 
            this.FPregLab.AutoSize = true;
            this.FPregLab.Location = new System.Drawing.Point(141, 0);
            this.FPregLab.Name = "FPregLab";
            this.FPregLab.Size = new System.Drawing.Size(105, 15);
            this.FPregLab.TabIndex = 2;
            this.FPregLab.Text = "Floating Point";
            this.FPregLab.Click += new System.EventHandler(this.FPregLab_Click);
            // 
            // GPregLab
            // 
            this.GPregLab.AutoSize = true;
            this.GPregLab.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.GPregLab.Location = new System.Drawing.Point(6, 0);
            this.GPregLab.Name = "GPregLab";
            this.GPregLab.Size = new System.Drawing.Size(114, 17);
            this.GPregLab.TabIndex = 1;
            this.GPregLab.Text = "General Purpose";
            this.GPregLab.Click += new System.EventHandler(this.GPregLab_Click);
            // 
            // SPregLab
            // 
            this.SPregLab.AutoSize = true;
            this.SPregLab.Location = new System.Drawing.Point(266, 0);
            this.SPregLab.Name = "SPregLab";
            this.SPregLab.Size = new System.Drawing.Size(112, 15);
            this.SPregLab.TabIndex = 0;
            this.SPregLab.Text = "Special Purpose";
            this.SPregLab.Click += new System.EventHandler(this.SPregLab_Click);
            // 
            // MemoryBox
            // 
            this.MemoryBox.Controls.Add(this.StringBox);
            this.MemoryBox.Controls.Add(this.ASMBox);
            this.MemoryBox.Controls.Add(this.ValueBox);
            this.MemoryBox.Controls.Add(this.AddressBox);
            this.MemoryBox.ForeColor = System.Drawing.Color.White;
            this.MemoryBox.Location = new System.Drawing.Point(403, 13);
            this.MemoryBox.Name = "MemoryBox";
            this.MemoryBox.Size = new System.Drawing.Size(384, 481);
            this.MemoryBox.TabIndex = 2;
            this.MemoryBox.TabStop = false;
            this.MemoryBox.Text = "Memory";
            // 
            // StringBox
            // 
            this.StringBox.BackColor = System.Drawing.Color.Black;
            this.StringBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.StringBox.ForeColor = System.Drawing.Color.White;
            this.StringBox.FormattingEnabled = true;
            this.StringBox.ItemHeight = 15;
            this.StringBox.Location = new System.Drawing.Point(175, 22);
            this.StringBox.Name = "StringBox";
            this.StringBox.Size = new System.Drawing.Size(78, 450);
            this.StringBox.TabIndex = 3;
            this.StringBox.SelectedIndexChanged += new System.EventHandler(this.StringBox_SelectedIndexChanged);
            this.StringBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StringBox_KeyDown);
            // 
            // ASMBox
            // 
            this.ASMBox.BackColor = System.Drawing.Color.Black;
            this.ASMBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ASMBox.ForeColor = System.Drawing.Color.White;
            this.ASMBox.FormattingEnabled = true;
            this.ASMBox.ItemHeight = 15;
            this.ASMBox.Location = new System.Drawing.Point(259, 22);
            this.ASMBox.Name = "ASMBox";
            this.ASMBox.Size = new System.Drawing.Size(119, 450);
            this.ASMBox.TabIndex = 2;
            this.ASMBox.SelectedIndexChanged += new System.EventHandler(this.ASMBox_SelectedIndexChanged);
            this.ASMBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ASMBox_KeyDown);
            // 
            // ValueBox
            // 
            this.ValueBox.BackColor = System.Drawing.Color.Black;
            this.ValueBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ValueBox.ForeColor = System.Drawing.Color.White;
            this.ValueBox.FormattingEnabled = true;
            this.ValueBox.ItemHeight = 15;
            this.ValueBox.Location = new System.Drawing.Point(91, 22);
            this.ValueBox.Name = "ValueBox";
            this.ValueBox.Size = new System.Drawing.Size(78, 450);
            this.ValueBox.TabIndex = 1;
            this.ValueBox.SelectedIndexChanged += new System.EventHandler(this.ValueBox_SelectedIndexChanged);
            this.ValueBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ValueBox_KeyDown);
            // 
            // AddressBox
            // 
            this.AddressBox.BackColor = System.Drawing.Color.Black;
            this.AddressBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AddressBox.ForeColor = System.Drawing.Color.White;
            this.AddressBox.FormattingEnabled = true;
            this.AddressBox.ItemHeight = 15;
            this.AddressBox.Location = new System.Drawing.Point(7, 22);
            this.AddressBox.Name = "AddressBox";
            this.AddressBox.Size = new System.Drawing.Size(78, 450);
            this.AddressBox.TabIndex = 0;
            this.AddressBox.SelectedIndexChanged += new System.EventHandler(this.AddressBox_SelectedIndexChanged);
            this.AddressBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddressBox_KeyDown);
            // 
            // ControlsBox
            // 
            this.ControlsBox.Controls.Add(this.StepBack);
            this.ControlsBox.Controls.Add(this.debugBox);
            this.ControlsBox.Controls.Add(this.PBcycles);
            this.ControlsBox.Controls.Add(this.CyclesBox);
            this.ControlsBox.Controls.Add(this.RunCycles);
            this.ControlsBox.Controls.Add(this.GotoAddrBox);
            this.ControlsBox.Controls.Add(this.GotoAddr);
            this.ControlsBox.Controls.Add(this.StepOver);
            this.ControlsBox.Controls.Add(this.StepInto);
            this.ControlsBox.ForeColor = System.Drawing.Color.White;
            this.ControlsBox.Location = new System.Drawing.Point(793, 12);
            this.ControlsBox.Name = "ControlsBox";
            this.ControlsBox.Size = new System.Drawing.Size(182, 481);
            this.ControlsBox.TabIndex = 3;
            this.ControlsBox.TabStop = false;
            this.ControlsBox.Text = "Controls";
            // 
            // StepBack
            // 
            this.StepBack.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.StepBack.Location = new System.Drawing.Point(6, 449);
            this.StepBack.Name = "StepBack";
            this.StepBack.Size = new System.Drawing.Size(170, 24);
            this.StepBack.TabIndex = 8;
            this.StepBack.Text = "Step Back";
            this.StepBack.UseVisualStyleBackColor = true;
            this.StepBack.Click += new System.EventHandler(this.StepBack_Click);
            // 
            // debugBox
            // 
            this.debugBox.BackColor = System.Drawing.Color.Black;
            this.debugBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.debugBox.ForeColor = System.Drawing.Color.White;
            this.debugBox.Location = new System.Drawing.Point(6, 23);
            this.debugBox.Multiline = true;
            this.debugBox.Name = "debugBox";
            this.debugBox.ReadOnly = true;
            this.debugBox.Size = new System.Drawing.Size(170, 243);
            this.debugBox.TabIndex = 7;
            this.debugBox.Text = "Debug:\r\n";
            // 
            // PBcycles
            // 
            this.PBcycles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PBcycles.Location = new System.Drawing.Point(51, 336);
            this.PBcycles.Name = "PBcycles";
            this.PBcycles.Size = new System.Drawing.Size(64, 23);
            this.PBcycles.TabIndex = 6;
            this.PBcycles.TabStop = false;
            this.PBcycles.Visible = false;
            this.PBcycles.Paint += new System.Windows.Forms.PaintEventHandler(this.PBcycles_Paint);
            // 
            // CyclesBox
            // 
            this.CyclesBox.BackColor = System.Drawing.Color.Black;
            this.CyclesBox.ForeColor = System.Drawing.Color.White;
            this.CyclesBox.Location = new System.Drawing.Point(51, 336);
            this.CyclesBox.MaxLength = 8;
            this.CyclesBox.Name = "CyclesBox";
            this.CyclesBox.Size = new System.Drawing.Size(64, 23);
            this.CyclesBox.TabIndex = 5;
            this.CyclesBox.Text = "0x1";
            this.CyclesBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // RunCycles
            // 
            this.RunCycles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.RunCycles.Location = new System.Drawing.Point(6, 335);
            this.RunCycles.Name = "RunCycles";
            this.RunCycles.Size = new System.Drawing.Size(170, 23);
            this.RunCycles.TabIndex = 4;
            this.RunCycles.Text = "Run             Cycles";
            this.RunCycles.UseVisualStyleBackColor = true;
            this.RunCycles.Click += new System.EventHandler(this.RunCycles_Click);
            // 
            // GotoAddrBox
            // 
            this.GotoAddrBox.BackColor = System.Drawing.Color.Black;
            this.GotoAddrBox.ForeColor = System.Drawing.Color.White;
            this.GotoAddrBox.Location = new System.Drawing.Point(112, 364);
            this.GotoAddrBox.MaxLength = 8;
            this.GotoAddrBox.Name = "GotoAddrBox";
            this.GotoAddrBox.Size = new System.Drawing.Size(64, 23);
            this.GotoAddrBox.TabIndex = 3;
            this.GotoAddrBox.Text = "00000000";
            this.GotoAddrBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // GotoAddr
            // 
            this.GotoAddr.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.GotoAddr.Location = new System.Drawing.Point(6, 364);
            this.GotoAddr.Name = "GotoAddr";
            this.GotoAddr.Size = new System.Drawing.Size(100, 23);
            this.GotoAddr.TabIndex = 2;
            this.GotoAddr.Text = "Goto Address";
            this.GotoAddr.UseVisualStyleBackColor = true;
            this.GotoAddr.Click += new System.EventHandler(this.GotoAddr_Click);
            // 
            // StepOver
            // 
            this.StepOver.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.StepOver.Location = new System.Drawing.Point(6, 421);
            this.StepOver.Name = "StepOver";
            this.StepOver.Size = new System.Drawing.Size(170, 24);
            this.StepOver.TabIndex = 1;
            this.StepOver.Text = "Step Over";
            this.StepOver.UseVisualStyleBackColor = true;
            this.StepOver.Click += new System.EventHandler(this.StepOver_Click);
            // 
            // StepInto
            // 
            this.StepInto.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.StepInto.Location = new System.Drawing.Point(6, 393);
            this.StepInto.Name = "StepInto";
            this.StepInto.Size = new System.Drawing.Size(170, 22);
            this.StepInto.TabIndex = 0;
            this.StepInto.Text = "Step Into";
            this.StepInto.UseVisualStyleBackColor = true;
            this.StepInto.Click += new System.EventHandler(this.StepInto_Click);
            // 
            // ASMEmu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(987, 506);
            this.Controls.Add(this.ControlsBox);
            this.Controls.Add(this.MemoryBox);
            this.Controls.Add(this.RegGroup);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ASMEmu";
            this.Text = "CodeWizard\'s PPC Emulator";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.ASMEmu_Load);
            this.Shown += new System.EventHandler(this.ASMEmu_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ASMEmu_FormClosing);
            this.Resize += new System.EventHandler(this.ASMEmu_Resize);
            this.RegGroup.ResumeLayout(false);
            this.RegGroup.PerformLayout();
            this.MemoryBox.ResumeLayout(false);
            this.ControlsBox.ResumeLayout(false);
            this.ControlsBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBcycles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox RegGroup;
        private System.Windows.Forms.Label GPregLab;
        private System.Windows.Forms.Label SPregLab;
        private System.Windows.Forms.GroupBox MemoryBox;
        private System.Windows.Forms.ListBox ValueBox;
        private System.Windows.Forms.ListBox AddressBox;
        private System.Windows.Forms.GroupBox ControlsBox;
        private System.Windows.Forms.ListBox ASMBox;
        private System.Windows.Forms.Button StepInto;
        private System.Windows.Forms.Button StepOver;
        private System.Windows.Forms.Button GotoAddr;
        private System.Windows.Forms.TextBox GotoAddrBox;
        private System.Windows.Forms.Button RunCycles;
        private System.Windows.Forms.PictureBox PBcycles;
        private System.Windows.Forms.TextBox debugBox;
        private System.Windows.Forms.TextBox CyclesBox;
        private System.Windows.Forms.Button StepBack;
        private System.Windows.Forms.ListBox StringBox;
        private System.Windows.Forms.Label FPregLab;
    }
}