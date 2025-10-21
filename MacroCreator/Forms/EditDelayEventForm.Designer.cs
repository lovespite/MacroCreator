namespace MacroCreator.Forms
{
    partial class EditDelayEventForm
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
            label1 = new Label();
            textBox1 = new TextBox();
            label2 = new Label();
            numericUpDown1 = new NumericUpDown();
            comboBox1 = new ComboBox();
            label3 = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 0;
            label1.Text = "事件名称";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 29);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(312, 23);
            textBox1.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 65);
            label2.Name = "label2";
            label2.Size = new Size(32, 17);
            label2.TabIndex = 0;
            label2.Text = "延迟";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(12, 85);
            numericUpDown1.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(173, 23);
            numericUpDown1.TabIndex = 2;
            numericUpDown1.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "毫秒", "秒", "分钟" });
            comboBox1.Location = new Point(203, 85);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 25);
            comboBox1.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(203, 65);
            label3.Name = "label3";
            label3.Size = new Size(32, 17);
            label3.TabIndex = 0;
            label3.Text = "单位";
            // 
            // btnOk
            // 
            btnOk.Location = new Point(160, 137);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 31);
            btnOk.TabIndex = 5;
            btnOk.Text = "确认(&O)";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(249, 137);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 31);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消(&C)";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // InsertDelayForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(345, 180);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(comboBox1);
            Controls.Add(numericUpDown1);
            Controls.Add(textBox1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "InsertDelayForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "插入延迟事件";
            Load += InsertDelayForm_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Label label2;
        private NumericUpDown numericUpDown1;
        private ComboBox comboBox1;
        private Label label3;
        private Button btnOk;
        private Button btnCancel;
    }
}