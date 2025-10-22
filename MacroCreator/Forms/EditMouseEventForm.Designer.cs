namespace MacroCreator.Forms
{
    partial class EditMouseEventForm
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
            labelEventName = new Label();
            textBoxEventName = new TextBox();
            labelAction = new Label();
            comboBoxAction = new ComboBox();
            labelX = new Label();
            numericUpDownX = new NumericUpDown();
            labelY = new Label();
            numericUpDownY = new NumericUpDown();
            labelWheelDelta = new Label();
            numericUpDownWheelDelta = new NumericUpDown();
            groupBoxCoordinates = new GroupBox();
            groupBoxDelay = new GroupBox();
            comboBoxDelayUnit = new ComboBox();
            numericUpDownDelay = new NumericUpDown();
            labelDelayUnit = new Label();
            labelDelay = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            chkPairedEvents = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDownX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownWheelDelta).BeginInit();
            groupBoxCoordinates.SuspendLayout();
            groupBoxDelay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDelay).BeginInit();
            SuspendLayout();
            // 
            // labelEventName
            // 
            labelEventName.AutoSize = true;
            labelEventName.Location = new Point(12, 9);
            labelEventName.Name = "labelEventName";
            labelEventName.Size = new Size(56, 17);
            labelEventName.TabIndex = 0;
            labelEventName.Text = "事件名称";
            // 
            // textBoxEventName
            // 
            textBoxEventName.Location = new Point(12, 29);
            textBoxEventName.Name = "textBoxEventName";
            textBoxEventName.Size = new Size(360, 23);
            textBoxEventName.TabIndex = 1;
            // 
            // labelAction
            // 
            labelAction.AutoSize = true;
            labelAction.Location = new Point(12, 65);
            labelAction.Name = "labelAction";
            labelAction.Size = new Size(56, 17);
            labelAction.TabIndex = 0;
            labelAction.Text = "鼠标动作";
            // 
            // comboBoxAction
            // 
            comboBoxAction.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxAction.FormattingEnabled = true;
            comboBoxAction.Items.AddRange(new object[] { "移动(MouseMove)", "左键按下(LeftDown)", "左键释放(LeftUp)", "右键按下(RightDown)", "右键释放(RightUp)", "中键按下(MiddleDown)", "中键释放(MiddleUp)", "滚轮滚动(WheelScroll)" });
            comboBoxAction.Location = new Point(12, 85);
            comboBoxAction.Name = "comboBoxAction";
            comboBoxAction.Size = new Size(360, 25);
            comboBoxAction.TabIndex = 2;
            comboBoxAction.SelectedIndexChanged += ComboBoxAction_SelectedIndexChanged;
            // 
            // labelX
            // 
            labelX.AutoSize = true;
            labelX.Location = new Point(15, 25);
            labelX.Name = "labelX";
            labelX.Size = new Size(44, 17);
            labelX.TabIndex = 0;
            labelX.Text = "X 坐标";
            // 
            // numericUpDownX
            // 
            numericUpDownX.Location = new Point(15, 45);
            numericUpDownX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDownX.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            numericUpDownX.Name = "numericUpDownX";
            numericUpDownX.Size = new Size(150, 23);
            numericUpDownX.TabIndex = 3;
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(188, 25);
            labelY.Name = "labelY";
            labelY.Size = new Size(43, 17);
            labelY.TabIndex = 0;
            labelY.Text = "Y 坐标";
            // 
            // numericUpDownY
            // 
            numericUpDownY.Location = new Point(188, 45);
            numericUpDownY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDownY.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            numericUpDownY.Name = "numericUpDownY";
            numericUpDownY.Size = new Size(150, 23);
            numericUpDownY.TabIndex = 4;
            // 
            // labelWheelDelta
            // 
            labelWheelDelta.AutoSize = true;
            labelWheelDelta.Location = new Point(15, 81);
            labelWheelDelta.Name = "labelWheelDelta";
            labelWheelDelta.Size = new Size(44, 17);
            labelWheelDelta.TabIndex = 0;
            labelWheelDelta.Text = "滚动量";
            labelWheelDelta.Visible = false;
            // 
            // numericUpDownWheelDelta
            // 
            numericUpDownWheelDelta.Location = new Point(15, 101);
            numericUpDownWheelDelta.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numericUpDownWheelDelta.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numericUpDownWheelDelta.Name = "numericUpDownWheelDelta";
            numericUpDownWheelDelta.Size = new Size(150, 23);
            numericUpDownWheelDelta.TabIndex = 5;
            numericUpDownWheelDelta.Visible = false;
            // 
            // groupBoxCoordinates
            // 
            groupBoxCoordinates.Controls.Add(labelX);
            groupBoxCoordinates.Controls.Add(numericUpDownWheelDelta);
            groupBoxCoordinates.Controls.Add(numericUpDownX);
            groupBoxCoordinates.Controls.Add(labelWheelDelta);
            groupBoxCoordinates.Controls.Add(labelY);
            groupBoxCoordinates.Controls.Add(numericUpDownY);
            groupBoxCoordinates.Location = new Point(12, 125);
            groupBoxCoordinates.Name = "groupBoxCoordinates";
            groupBoxCoordinates.Size = new Size(360, 145);
            groupBoxCoordinates.TabIndex = 5;
            groupBoxCoordinates.TabStop = false;
            groupBoxCoordinates.Text = "坐标与参数";
            // 
            // groupBoxDelay
            // 
            groupBoxDelay.Controls.Add(comboBoxDelayUnit);
            groupBoxDelay.Controls.Add(numericUpDownDelay);
            groupBoxDelay.Controls.Add(labelDelayUnit);
            groupBoxDelay.Controls.Add(labelDelay);
            groupBoxDelay.Location = new Point(12, 285);
            groupBoxDelay.Name = "groupBoxDelay";
            groupBoxDelay.Size = new Size(360, 95);
            groupBoxDelay.TabIndex = 6;
            groupBoxDelay.TabStop = false;
            groupBoxDelay.Text = "延迟时间";
            // 
            // comboBoxDelayUnit
            // 
            comboBoxDelayUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxDelayUnit.FormattingEnabled = true;
            comboBoxDelayUnit.Items.AddRange(new object[] { "毫秒", "秒", "分钟" });
            comboBoxDelayUnit.Location = new Point(188, 45);
            comboBoxDelayUnit.Name = "comboBoxDelayUnit";
            comboBoxDelayUnit.Size = new Size(150, 25);
            comboBoxDelayUnit.TabIndex = 8;
            // 
            // numericUpDownDelay
            // 
            numericUpDownDelay.DecimalPlaces = 2;
            numericUpDownDelay.Location = new Point(15, 45);
            numericUpDownDelay.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numericUpDownDelay.Name = "numericUpDownDelay";
            numericUpDownDelay.Size = new Size(150, 23);
            numericUpDownDelay.TabIndex = 7;
            // 
            // labelDelayUnit
            // 
            labelDelayUnit.AutoSize = true;
            labelDelayUnit.Location = new Point(188, 25);
            labelDelayUnit.Name = "labelDelayUnit";
            labelDelayUnit.Size = new Size(32, 17);
            labelDelayUnit.TabIndex = 0;
            labelDelayUnit.Text = "单位";
            // 
            // labelDelay
            // 
            labelDelay.AutoSize = true;
            labelDelay.Location = new Point(15, 25);
            labelDelay.Name = "labelDelay";
            labelDelay.Size = new Size(32, 17);
            labelDelay.TabIndex = 0;
            labelDelay.Text = "延迟";
            // 
            // btnOk
            // 
            btnOk.Location = new Point(208, 400);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 31);
            btnOk.TabIndex = 9;
            btnOk.Text = "确认(&O)";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(297, 400);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 31);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "取消(&C)";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // chkPairedEvents
            // 
            chkPairedEvents.AutoSize = true;
            chkPairedEvents.Checked = true;
            chkPairedEvents.CheckState = CheckState.Checked;
            chkPairedEvents.Location = new Point(100, 406);
            chkPairedEvents.Name = "chkPairedEvents";
            chkPairedEvents.Size = new Size(102, 21);
            chkPairedEvents.TabIndex = 12;
            chkPairedEvents.Text = "创建事件对(&P)";
            chkPairedEvents.UseVisualStyleBackColor = true;
            // 
            // EditMouseEventForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(384, 441);
            Controls.Add(chkPairedEvents);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(groupBoxDelay);
            Controls.Add(groupBoxCoordinates);
            Controls.Add(comboBoxAction);
            Controls.Add(labelAction);
            Controls.Add(textBoxEventName);
            Controls.Add(labelEventName);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "EditMouseEventForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "编辑鼠标事件";
            Load += EditMouseEventForm_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDownX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownWheelDelta).EndInit();
            groupBoxCoordinates.ResumeLayout(false);
            groupBoxCoordinates.PerformLayout();
            groupBoxDelay.ResumeLayout(false);
            groupBoxDelay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDelay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelEventName;
        private TextBox textBoxEventName;
        private Label labelAction;
        private ComboBox comboBoxAction;
        private Label labelX;
        private NumericUpDown numericUpDownX;
        private Label labelY;
        private NumericUpDown numericUpDownY;
        private Label labelWheelDelta;
        private NumericUpDown numericUpDownWheelDelta;
        private GroupBox groupBoxCoordinates;
        private GroupBox groupBoxDelay;
        private ComboBox comboBoxDelayUnit;
        private NumericUpDown numericUpDownDelay;
        private Label labelDelayUnit;
        private Label labelDelay;
        private Button btnOk;
        private Button btnCancel;
        private CheckBox chkPairedEvents;
    }
}
