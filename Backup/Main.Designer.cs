namespace CodeWizardPS3
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.ASMBox = new System.Windows.Forms.RichTextBox();
            this.AssembleASM = new System.Windows.Forms.Button();
            this.DisassembleASM = new System.Windows.Forms.Button();
            this.CopyCode = new System.Windows.Forms.Button();
            this.OpenCWP3 = new System.Windows.Forms.Button();
            this.SaveCWP3 = new System.Windows.Forms.Button();
            this.SaveAsCWP3 = new System.Windows.Forms.Button();
            this.CodeBox = new System.Windows.Forms.TextBox();
            this.Form1MenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assemblerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assembleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disassembleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emulateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byteArrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.conversionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pinstructionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.instructionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItemBox = new System.Windows.Forms.ToolStripMenuItem();
            this.codeDefBox = new System.Windows.Forms.RichTextBox();
            this.Form1MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ASMBox
            // 
            this.ASMBox.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ASMBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ASMBox.ForeColor = System.Drawing.Color.White;
            this.ASMBox.Location = new System.Drawing.Point(12, 27);
            this.ASMBox.Name = "ASMBox";
            this.ASMBox.Size = new System.Drawing.Size(415, 301);
            this.ASMBox.TabIndex = 4;
            this.ASMBox.Text = "";
            this.ASMBox.WordWrap = false;
            this.ASMBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ASMBox_KeyDown);
            this.ASMBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ASMBox_KeyUp);
            this.ASMBox.TextChanged += new System.EventHandler(this.ASMBox_TextChanged);
            // 
            // AssembleASM
            // 
            this.AssembleASM.BackColor = System.Drawing.Color.Black;
            this.AssembleASM.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.AssembleASM.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AssembleASM.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AssembleASM.ForeColor = System.Drawing.Color.White;
            this.AssembleASM.Location = new System.Drawing.Point(433, 421);
            this.AssembleASM.Name = "AssembleASM";
            this.AssembleASM.Size = new System.Drawing.Size(98, 27);
            this.AssembleASM.TabIndex = 1;
            this.AssembleASM.Text = "Assemble";
            this.AssembleASM.UseVisualStyleBackColor = false;
            this.AssembleASM.Click += new System.EventHandler(this.AssembleASM_Click);
            // 
            // DisassembleASM
            // 
            this.DisassembleASM.BackColor = System.Drawing.Color.Black;
            this.DisassembleASM.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.DisassembleASM.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.DisassembleASM.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DisassembleASM.ForeColor = System.Drawing.Color.White;
            this.DisassembleASM.Location = new System.Drawing.Point(433, 454);
            this.DisassembleASM.Name = "DisassembleASM";
            this.DisassembleASM.Size = new System.Drawing.Size(98, 27);
            this.DisassembleASM.TabIndex = 3;
            this.DisassembleASM.Text = "Disassemble";
            this.DisassembleASM.UseVisualStyleBackColor = false;
            this.DisassembleASM.Click += new System.EventHandler(this.DisassembleASM_Click);
            // 
            // CopyCode
            // 
            this.CopyCode.BackColor = System.Drawing.Color.Black;
            this.CopyCode.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.CopyCode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.CopyCode.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CopyCode.ForeColor = System.Drawing.Color.White;
            this.CopyCode.Location = new System.Drawing.Point(433, 487);
            this.CopyCode.Name = "CopyCode";
            this.CopyCode.Size = new System.Drawing.Size(98, 27);
            this.CopyCode.TabIndex = 4;
            this.CopyCode.Text = "Copy";
            this.CopyCode.UseVisualStyleBackColor = false;
            this.CopyCode.Click += new System.EventHandler(this.CopyCode_Click);
            // 
            // OpenCWP3
            // 
            this.OpenCWP3.BackColor = System.Drawing.Color.Black;
            this.OpenCWP3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.OpenCWP3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.OpenCWP3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenCWP3.ForeColor = System.Drawing.Color.White;
            this.OpenCWP3.Location = new System.Drawing.Point(534, 421);
            this.OpenCWP3.Name = "OpenCWP3";
            this.OpenCWP3.Size = new System.Drawing.Size(92, 27);
            this.OpenCWP3.TabIndex = 5;
            this.OpenCWP3.Text = "Open";
            this.OpenCWP3.UseVisualStyleBackColor = false;
            this.OpenCWP3.Click += new System.EventHandler(this.OpenCWP3_Click);
            // 
            // SaveCWP3
            // 
            this.SaveCWP3.BackColor = System.Drawing.Color.Black;
            this.SaveCWP3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.SaveCWP3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SaveCWP3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveCWP3.ForeColor = System.Drawing.Color.White;
            this.SaveCWP3.Location = new System.Drawing.Point(534, 454);
            this.SaveCWP3.Name = "SaveCWP3";
            this.SaveCWP3.Size = new System.Drawing.Size(92, 27);
            this.SaveCWP3.TabIndex = 6;
            this.SaveCWP3.Text = "Save";
            this.SaveCWP3.UseVisualStyleBackColor = false;
            this.SaveCWP3.Click += new System.EventHandler(this.SaveCWP3_Click);
            // 
            // SaveAsCWP3
            // 
            this.SaveAsCWP3.BackColor = System.Drawing.Color.Black;
            this.SaveAsCWP3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.SaveAsCWP3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SaveAsCWP3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveAsCWP3.ForeColor = System.Drawing.Color.White;
            this.SaveAsCWP3.Location = new System.Drawing.Point(534, 487);
            this.SaveAsCWP3.Name = "SaveAsCWP3";
            this.SaveAsCWP3.Size = new System.Drawing.Size(92, 27);
            this.SaveAsCWP3.TabIndex = 7;
            this.SaveAsCWP3.Text = "Save As";
            this.SaveAsCWP3.UseVisualStyleBackColor = false;
            this.SaveAsCWP3.Click += new System.EventHandler(this.SaveAsCWP3_Click);
            // 
            // CodeBox
            // 
            this.CodeBox.BackColor = System.Drawing.Color.Black;
            this.CodeBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CodeBox.ForeColor = System.Drawing.Color.White;
            this.CodeBox.Location = new System.Drawing.Point(433, 27);
            this.CodeBox.Multiline = true;
            this.CodeBox.Name = "CodeBox";
            this.CodeBox.Size = new System.Drawing.Size(193, 388);
            this.CodeBox.TabIndex = 8;
            this.CodeBox.TextChanged += new System.EventHandler(this.CodeBox_TextChanged);
            this.CodeBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CodeBox_KeyUp);
            // 
            // Form1MenuStrip
            // 
            this.Form1MenuStrip.BackColor = System.Drawing.Color.Black;
            this.Form1MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.assemblerToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.Form1MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.Form1MenuStrip.Name = "Form1MenuStrip";
            this.Form1MenuStrip.Size = new System.Drawing.Size(638, 24);
            this.Form1MenuStrip.TabIndex = 9;
            this.Form1MenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.AccessibleDescription = "";
            this.fileToolStripMenuItem.AccessibleName = "";
            this.fileToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.fileToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.openToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.saveToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.saveAsToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.closeToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // assemblerToolStripMenuItem
            // 
            this.assemblerToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.assemblerToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.assemblerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.assembleToolStripMenuItem,
            this.disassembleToolStripMenuItem,
            this.emulateToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.assemblerToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.assemblerToolStripMenuItem.Name = "assemblerToolStripMenuItem";
            this.assemblerToolStripMenuItem.Size = new System.Drawing.Size(74, 20);
            this.assemblerToolStripMenuItem.Text = "Assembler";
            // 
            // assembleToolStripMenuItem
            // 
            this.assembleToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.assembleToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.assembleToolStripMenuItem.Name = "assembleToolStripMenuItem";
            this.assembleToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.assembleToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.assembleToolStripMenuItem.Text = "Assemble";
            this.assembleToolStripMenuItem.Click += new System.EventHandler(this.assembleToolStripMenuItem_Click);
            // 
            // disassembleToolStripMenuItem
            // 
            this.disassembleToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.disassembleToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.disassembleToolStripMenuItem.Name = "disassembleToolStripMenuItem";
            this.disassembleToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.disassembleToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.disassembleToolStripMenuItem.Text = "Disassemble";
            this.disassembleToolStripMenuItem.Click += new System.EventHandler(this.disassembleToolStripMenuItem_Click);
            // 
            // emulateToolStripMenuItem
            // 
            this.emulateToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.emulateToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.emulateToolStripMenuItem.Name = "emulateToolStripMenuItem";
            this.emulateToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F7;
            this.emulateToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.emulateToolStripMenuItem.Text = "Emulate";
            this.emulateToolStripMenuItem.Click += new System.EventHandler(this.simulateToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.optionsToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.exportToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cToolStripMenuItem,
            this.byteArrayToolStripMenuItem});
            this.exportToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Visible = false;
            // 
            // cToolStripMenuItem
            // 
            this.cToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.cToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.cToolStripMenuItem.Name = "cToolStripMenuItem";
            this.cToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.cToolStripMenuItem.Text = ".C";
            this.cToolStripMenuItem.Click += new System.EventHandler(this.cToolStripMenuItem_Click);
            // 
            // byteArrayToolStripMenuItem
            // 
            this.byteArrayToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.byteArrayToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.byteArrayToolStripMenuItem.Name = "byteArrayToolStripMenuItem";
            this.byteArrayToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.byteArrayToolStripMenuItem.Text = "Byte Array (C#)";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.conversionToolStripMenuItem,
            this.pinstructionToolStripMenuItem});
            this.toolsToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // conversionToolStripMenuItem
            // 
            this.conversionToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.conversionToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.conversionToolStripMenuItem.Name = "conversionToolStripMenuItem";
            this.conversionToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.conversionToolStripMenuItem.Text = "Conversion";
            this.conversionToolStripMenuItem.Click += new System.EventHandler(this.conversionToolStripMenuItem_Click);
            // 
            // pinstructionToolStripMenuItem
            // 
            this.pinstructionToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.pinstructionToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.pinstructionToolStripMenuItem.Name = "pinstructionToolStripMenuItem";
            this.pinstructionToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.pinstructionToolStripMenuItem.Text = "Pseudo Instruction";
            this.pinstructionToolStripMenuItem.Click += new System.EventHandler(this.pinstructionToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.instructionsToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.helpToolStripMenuItemBox});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // instructionsToolStripMenuItem
            // 
            this.instructionsToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.instructionsToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.instructionsToolStripMenuItem.Name = "instructionsToolStripMenuItem";
            this.instructionsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.instructionsToolStripMenuItem.Text = "Assembly Help";
            this.instructionsToolStripMenuItem.Click += new System.EventHandler(this.instructionsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItemBox
            // 
            this.helpToolStripMenuItemBox.BackColor = System.Drawing.Color.Black;
            this.helpToolStripMenuItemBox.ForeColor = System.Drawing.Color.White;
            this.helpToolStripMenuItemBox.Name = "helpToolStripMenuItemBox";
            this.helpToolStripMenuItemBox.Size = new System.Drawing.Size(153, 22);
            this.helpToolStripMenuItemBox.Text = "Help";
            this.helpToolStripMenuItemBox.Click += new System.EventHandler(this.helpToolStripMenuItemBox_Click);
            // 
            // codeDefBox
            // 
            this.codeDefBox.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.codeDefBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.codeDefBox.ForeColor = System.Drawing.Color.White;
            this.codeDefBox.Location = new System.Drawing.Point(12, 334);
            this.codeDefBox.Name = "codeDefBox";
            this.codeDefBox.Size = new System.Drawing.Size(415, 180);
            this.codeDefBox.TabIndex = 10;
            this.codeDefBox.Text = "// Declarations\n";
            this.codeDefBox.WordWrap = false;
            this.codeDefBox.TextChanged += new System.EventHandler(this.codeDefBox_TextChanged);
            // 
            // Main
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(638, 526);
            this.Controls.Add(this.codeDefBox);
            this.Controls.Add(this.CodeBox);
            this.Controls.Add(this.SaveAsCWP3);
            this.Controls.Add(this.SaveCWP3);
            this.Controls.Add(this.OpenCWP3);
            this.Controls.Add(this.CopyCode);
            this.Controls.Add(this.DisassembleASM);
            this.Controls.Add(this.AssembleASM);
            this.Controls.Add(this.ASMBox);
            this.Controls.Add(this.Form1MenuStrip);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "CodeWizard PS3";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Main_Load);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.Form1MenuStrip.ResumeLayout(false);
            this.Form1MenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox ASMBox;
        private System.Windows.Forms.Button AssembleASM;
        private System.Windows.Forms.Button DisassembleASM;
        private System.Windows.Forms.Button CopyCode;
        private System.Windows.Forms.Button OpenCWP3;
        private System.Windows.Forms.Button SaveCWP3;
        private System.Windows.Forms.Button SaveAsCWP3;
        private System.Windows.Forms.TextBox CodeBox;
        private System.Windows.Forms.MenuStrip Form1MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assemblerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assembleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disassembleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emulateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem byteArrayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem instructionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem conversionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItemBox;
        private System.Windows.Forms.ToolStripMenuItem pinstructionToolStripMenuItem;
        private System.Windows.Forms.RichTextBox codeDefBox;
    }
}

