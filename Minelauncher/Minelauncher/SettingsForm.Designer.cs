namespace Minelauncher
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.savebtn = new System.Windows.Forms.Button();
            this.ramupdown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.argumentBox = new System.Windows.Forms.TextBox();
            this.labelRamMb = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.javaBox = new System.Windows.Forms.TextBox();
            this.browseJavaBtn = new System.Windows.Forms.Button();
            this.logCheckbox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.ramupdown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(7, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Настройки";
            // 
            // savebtn
            // 
            this.savebtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.savebtn.Location = new System.Drawing.Point(12, 378);
            this.savebtn.Name = "savebtn";
            this.savebtn.Size = new System.Drawing.Size(111, 31);
            this.savebtn.TabIndex = 1;
            this.savebtn.Text = "Сохранить";
            this.savebtn.UseVisualStyleBackColor = true;
            // 
            // ramupdown
            // 
            this.ramupdown.Location = new System.Drawing.Point(12, 59);
            this.ramupdown.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.ramupdown.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.ramupdown.Name = "ramupdown";
            this.ramupdown.Size = new System.Drawing.Size(120, 20);
            this.ramupdown.TabIndex = 2;
            this.ramupdown.Value = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(9, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "ОЗУ для JVM";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(9, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Доп. аргументы";
            // 
            // argumentBox
            // 
            this.argumentBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.argumentBox.Location = new System.Drawing.Point(12, 162);
            this.argumentBox.Multiline = true;
            this.argumentBox.Name = "argumentBox";
            this.argumentBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.argumentBox.Size = new System.Drawing.Size(528, 60);
            this.argumentBox.TabIndex = 5;
            // 
            // labelRamMb
            // 
            this.labelRamMb.AutoSize = true;
            this.labelRamMb.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRamMb.ForeColor = System.Drawing.Color.White;
            this.labelRamMb.Location = new System.Drawing.Point(138, 61);
            this.labelRamMb.Name = "labelRamMb";
            this.labelRamMb.Size = new System.Drawing.Size(27, 17);
            this.labelRamMb.TabIndex = 6;
            this.labelRamMb.Text = "МБ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(9, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Путь к Java";
            // 
            // javaBox
            // 
            this.javaBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.javaBox.Location = new System.Drawing.Point(12, 110);
            this.javaBox.Name = "javaBox";
            this.javaBox.Size = new System.Drawing.Size(446, 20);
            this.javaBox.TabIndex = 8;
            // 
            // browseJavaBtn
            // 
            this.browseJavaBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseJavaBtn.Location = new System.Drawing.Point(464, 108);
            this.browseJavaBtn.Name = "browseJavaBtn";
            this.browseJavaBtn.Size = new System.Drawing.Size(76, 24);
            this.browseJavaBtn.TabIndex = 9;
            this.browseJavaBtn.Text = "Обзор";
            this.browseJavaBtn.UseVisualStyleBackColor = true;
            // 
            // logCheckbox
            // 
            this.logCheckbox.AutoSize = true;
            this.logCheckbox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logCheckbox.ForeColor = System.Drawing.Color.White;
            this.logCheckbox.Location = new System.Drawing.Point(12, 238);
            this.logCheckbox.Name = "logCheckbox";
            this.logCheckbox.Size = new System.Drawing.Size(170, 21);
            this.logCheckbox.TabIndex = 10;
            this.logCheckbox.Text = "Показывать окно логов";
            this.logCheckbox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(552, 421);
            this.Controls.Add(this.logCheckbox);
            this.Controls.Add(this.browseJavaBtn);
            this.Controls.Add(this.javaBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelRamMb);
            this.Controls.Add(this.argumentBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ramupdown);
            this.Controls.Add(this.savebtn);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Настройки";
            ((System.ComponentModel.ISupportInitialize)(this.ramupdown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button savebtn;
        private System.Windows.Forms.NumericUpDown ramupdown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox argumentBox;
        private System.Windows.Forms.Label labelRamMb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox javaBox;
        private System.Windows.Forms.Button browseJavaBtn;
        private System.Windows.Forms.CheckBox logCheckbox;
    }
}