namespace OpenMobile.Controls
{
    /// <summary>
    /// Allows selection of semi-transparent colors
    /// </summary>
    partial class TransparentColorPicker
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.domainUpDown3 = new System.Windows.Forms.DomainUpDown();
            this.domainUpDown2 = new System.Windows.Forms.DomainUpDown();
            this.domainUpDown1 = new System.Windows.Forms.DomainUpDown();
            this.luminositySlider1 = new OpenMobile.Framework.Controls.LuminositySlider();
            this.panel1 = new System.Windows.Forms.Panel();
            this.customColor1 = new OpenMobile.Controls.CustomColor();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(172, 176);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.TabStop = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.listBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(164, 150);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Web";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Items.AddRange(new object[] {
            "Black",
            "Green"});
            this.listBox1.Location = new System.Drawing.Point(2, 2);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(159, 147);
            this.listBox1.TabIndex = 0;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDown);
            this.listBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyDown);
            this.listBox1.Click += new System.EventHandler(this.listBox1_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.domainUpDown3);
            this.tabPage2.Controls.Add(this.domainUpDown2);
            this.tabPage2.Controls.Add(this.domainUpDown1);
            this.tabPage2.Controls.Add(this.luminositySlider1);
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Controls.Add(this.customColor1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(164, 150);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Custom";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // domainUpDown3
            // 
            this.domainUpDown3.Location = new System.Drawing.Point(90, 126);
            this.domainUpDown3.Name = "domainUpDown3";
            this.domainUpDown3.Size = new System.Drawing.Size(38, 20);
            this.domainUpDown3.TabIndex = 5;
            this.domainUpDown3.Text = "000";
            this.domainUpDown3.TextChanged += new System.EventHandler(this.domainUpDown_TextChanged);
            this.domainUpDown3.SelectedItemChanged += new System.EventHandler(this.domainUpDown_SelectedItemChanged);
            // 
            // domainUpDown2
            // 
            this.domainUpDown2.Location = new System.Drawing.Point(48, 126);
            this.domainUpDown2.Name = "domainUpDown2";
            this.domainUpDown2.Size = new System.Drawing.Size(38, 20);
            this.domainUpDown2.TabIndex = 4;
            this.domainUpDown2.Text = "000";
            this.domainUpDown2.TextChanged += new System.EventHandler(this.domainUpDown_TextChanged);
            this.domainUpDown2.SelectedItemChanged += new System.EventHandler(this.domainUpDown_SelectedItemChanged);
            // 
            // domainUpDown1
            // 
            this.domainUpDown1.Location = new System.Drawing.Point(6, 126);
            this.domainUpDown1.Name = "domainUpDown1";
            this.domainUpDown1.Size = new System.Drawing.Size(38, 20);
            this.domainUpDown1.TabIndex = 3;
            this.domainUpDown1.Text = "000";
            this.domainUpDown1.TextChanged += new System.EventHandler(this.domainUpDown_TextChanged);
            this.domainUpDown1.SelectedItemChanged += new System.EventHandler(this.domainUpDown_SelectedItemChanged);
            // 
            // luminositySlider1
            // 
            this.luminositySlider1.ForeColor = System.Drawing.Color.Lime;
            this.luminositySlider1.Location = new System.Drawing.Point(131, 2);
            this.luminositySlider1.Name = "luminositySlider1";
            this.luminositySlider1.Size = new System.Drawing.Size(31, 119);
            this.luminositySlider1.TabIndex = 2;
            this.luminositySlider1.Value = 50;
            this.luminositySlider1.SelectedColorChanged += new OpenMobile.Framework.Controls.LuminositySlider.ColorChanged(this.luminositySlider1_SelectedColorChanged);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(131, 124);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(22, 23);
            this.panel1.TabIndex = 1;
            // 
            // customColor1
            // 
            this.customColor1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.customColor1.Location = new System.Drawing.Point(5, 4);
            this.customColor1.Name = "customColor1";
            this.customColor1.Size = new System.Drawing.Size(120, 118);
            this.customColor1.TabIndex = 0;
            this.customColor1.SelectedColorChanged += new OpenMobile.Controls.CustomColor.ColorChanged(this.customColor1_SelectedColorChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.textBox1);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.trackBar1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(164, 150);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Transparency";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.tabPage3.Paint += new System.Windows.Forms.PaintEventHandler(this.tabPage3_Paint);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(21, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Opacity:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(72, 119);
            this.textBox1.MaxLength = 3;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(59, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "0";
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(135, 122);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "%";
            // 
            // trackBar1
            // 
            this.trackBar1.BackColor = System.Drawing.SystemColors.Window;
            this.trackBar1.Location = new System.Drawing.Point(3, 82);
            this.trackBar1.Margin = new System.Windows.Forms.Padding(1);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(158, 45);
            this.trackBar1.TabIndex = 0;
            this.trackBar1.TabStop = false;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // TransparentColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "TransparentColorPicker";
            this.Size = new System.Drawing.Size(173, 175);
            this.Resize += new System.EventHandler(this.TransparentColorPicker_Resize);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TabPage tabPage3;
        private CustomColor customColor1;
        private System.Windows.Forms.Panel panel1;
        private OpenMobile.Framework.Controls.LuminositySlider luminositySlider1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DomainUpDown domainUpDown3;
        private System.Windows.Forms.DomainUpDown domainUpDown2;
        private System.Windows.Forms.DomainUpDown domainUpDown1;
    }
}
