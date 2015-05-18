namespace CodeWizardPS3
{
    partial class ConversionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConversionForm));
            this.label1 = new System.Windows.Forms.Label();
            this.InputTBox = new System.Windows.Forms.TextBox();
            this.InputCBox = new System.Windows.Forms.ComboBox();
            this.OutputCBox = new System.Windows.Forms.ComboBox();
            this.OutputTBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.stateNormal = new System.Windows.Forms.PictureBox();
            this.stateMouseDown = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.extraTBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.stateNormal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stateMouseDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // InputTBox
            // 
            this.InputTBox.BackColor = System.Drawing.Color.Black;
            this.InputTBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputTBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputTBox.ForeColor = System.Drawing.Color.White;
            this.InputTBox.Location = new System.Drawing.Point(15, 56);
            this.InputTBox.Multiline = true;
            this.InputTBox.Name = "InputTBox";
            this.InputTBox.Size = new System.Drawing.Size(152, 253);
            this.InputTBox.TabIndex = 1;
            // 
            // InputCBox
            // 
            this.InputCBox.BackColor = System.Drawing.Color.Black;
            this.InputCBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InputCBox.ForeColor = System.Drawing.Color.White;
            this.InputCBox.FormattingEnabled = true;
            this.InputCBox.Items.AddRange(new object[] {
            "NetCheat PS3",
            "Hex String Array",
            "Byte Array"});
            this.InputCBox.Location = new System.Drawing.Point(15, 29);
            this.InputCBox.Name = "InputCBox";
            this.InputCBox.Size = new System.Drawing.Size(152, 21);
            this.InputCBox.TabIndex = 2;
            // 
            // OutputCBox
            // 
            this.OutputCBox.BackColor = System.Drawing.Color.Black;
            this.OutputCBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.OutputCBox.ForeColor = System.Drawing.Color.White;
            this.OutputCBox.FormattingEnabled = true;
            this.OutputCBox.Items.AddRange(new object[] {
            "NetCheat PS3",
            "Hex String Array",
            "Byte Array"});
            this.OutputCBox.Location = new System.Drawing.Point(221, 29);
            this.OutputCBox.Name = "OutputCBox";
            this.OutputCBox.Size = new System.Drawing.Size(152, 21);
            this.OutputCBox.TabIndex = 5;
            this.OutputCBox.SelectedIndexChanged += new System.EventHandler(this.OutputCBox_SelectedIndexChanged);
            // 
            // OutputTBox
            // 
            this.OutputTBox.BackColor = System.Drawing.Color.Black;
            this.OutputTBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OutputTBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OutputTBox.ForeColor = System.Drawing.Color.White;
            this.OutputTBox.Location = new System.Drawing.Point(221, 56);
            this.OutputTBox.Multiline = true;
            this.OutputTBox.Name = "OutputTBox";
            this.OutputTBox.Size = new System.Drawing.Size(152, 253);
            this.OutputTBox.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(218, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(155, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // stateNormal
            // 
            this.stateNormal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.stateNormal.Image = ((System.Drawing.Image)(resources.GetObject("stateNormal.Image")));
            this.stateNormal.Location = new System.Drawing.Point(173, 144);
            this.stateNormal.Name = "stateNormal";
            this.stateNormal.Size = new System.Drawing.Size(42, 44);
            this.stateNormal.TabIndex = 6;
            this.stateNormal.TabStop = false;
            this.stateNormal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.stateNormal_MouseDown);
            this.stateNormal.MouseUp += new System.Windows.Forms.MouseEventHandler(this.stateNormal_MouseUp);
            // 
            // stateMouseDown
            // 
            this.stateMouseDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.stateMouseDown.ErrorImage = null;
            this.stateMouseDown.Image = ((System.Drawing.Image)(resources.GetObject("stateMouseDown.Image")));
            this.stateMouseDown.Location = new System.Drawing.Point(173, 194);
            this.stateMouseDown.Name = "stateMouseDown";
            this.stateMouseDown.Size = new System.Drawing.Size(42, 44);
            this.stateMouseDown.TabIndex = 7;
            this.stateMouseDown.TabStop = false;
            this.stateMouseDown.Visible = false;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(15, 312);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(358, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Start Address";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // extraTBox
            // 
            this.extraTBox.BackColor = System.Drawing.Color.Black;
            this.extraTBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extraTBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.extraTBox.ForeColor = System.Drawing.Color.White;
            this.extraTBox.Location = new System.Drawing.Point(12, 331);
            this.extraTBox.Name = "extraTBox";
            this.extraTBox.Size = new System.Drawing.Size(361, 22);
            this.extraTBox.TabIndex = 9;
            // 
            // ConversionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(389, 367);
            this.Controls.Add(this.extraTBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.stateMouseDown);
            this.Controls.Add(this.stateNormal);
            this.Controls.Add(this.OutputCBox);
            this.Controls.Add(this.OutputTBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.InputCBox);
            this.Controls.Add(this.InputTBox);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(395, 395);
            this.MinimumSize = new System.Drawing.Size(395, 350);
            this.Name = "ConversionForm";
            this.Text = "Format Conversion";
            this.Load += new System.EventHandler(this.ConversionForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.stateNormal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stateMouseDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox InputTBox;
        private System.Windows.Forms.ComboBox InputCBox;
        private System.Windows.Forms.ComboBox OutputCBox;
        private System.Windows.Forms.TextBox OutputTBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox stateNormal;
        private System.Windows.Forms.PictureBox stateMouseDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox extraTBox;
    }
}