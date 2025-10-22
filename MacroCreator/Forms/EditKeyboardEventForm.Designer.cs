namespace MacroCreator.Forms
{
  partial class EditKeyboardEventForm
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
      labelKey = new Label();
      comboBoxKey = new ComboBox();
      groupBoxKeyInfo = new GroupBox();
      textBoxKeyCode = new TextBox();
      labelKeyCode = new Label();
      groupBoxDelay = new GroupBox();
      comboBoxDelayUnit = new ComboBox();
      numericUpDownDelay = new NumericUpDown();
      labelDelayUnit = new Label();
      labelDelay = new Label();
      btnOk = new Button();
      btnCancel = new Button();
      groupBoxKeyInfo.SuspendLayout();
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
      labelAction.Text = "键盘动作";
      // 
      // comboBoxAction
      // 
      comboBoxAction.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxAction.FormattingEnabled = true;
      comboBoxAction.Items.AddRange(new object[] { "按下(KeyDown)", "释放(KeyUp)" });
      comboBoxAction.Location = new Point(12, 85);
      comboBoxAction.Name = "comboBoxAction";
      comboBoxAction.Size = new Size(360, 25);
      comboBoxAction.TabIndex = 2;
      // 
      // labelKey
      // 
      labelKey.AutoSize = true;
      labelKey.Location = new Point(15, 25);
      labelKey.Name = "labelKey";
      labelKey.Size = new Size(32, 17);
      labelKey.TabIndex = 0;
      labelKey.Text = "按键";
      // 
      // comboBoxKey
      // 
      comboBoxKey.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxKey.FormattingEnabled = true;
      comboBoxKey.Location = new Point(15, 45);
      comboBoxKey.Name = "comboBoxKey";
      comboBoxKey.Size = new Size(323, 25);
      comboBoxKey.TabIndex = 3;
      comboBoxKey.SelectedIndexChanged += ComboBoxKey_SelectedIndexChanged;
      // 
      // groupBoxKeyInfo
      // 
      groupBoxKeyInfo.Controls.Add(textBoxKeyCode);
      groupBoxKeyInfo.Controls.Add(labelKeyCode);
      groupBoxKeyInfo.Controls.Add(labelKey);
      groupBoxKeyInfo.Controls.Add(comboBoxKey);
      groupBoxKeyInfo.Location = new Point(12, 125);
      groupBoxKeyInfo.Name = "groupBoxKeyInfo";
      groupBoxKeyInfo.Size = new Size(360, 125);
      groupBoxKeyInfo.TabIndex = 5;
      groupBoxKeyInfo.TabStop = false;
      groupBoxKeyInfo.Text = "按键信息";
      // 
      // textBoxKeyCode
      // 
      textBoxKeyCode.Location = new Point(15, 90);
      textBoxKeyCode.Name = "textBoxKeyCode";
      textBoxKeyCode.ReadOnly = true;
      textBoxKeyCode.Size = new Size(323, 23);
      textBoxKeyCode.TabIndex = 4;
      textBoxKeyCode.TabStop = false;
      // 
      // labelKeyCode
      // 
      labelKeyCode.AutoSize = true;
      labelKeyCode.Location = new Point(15, 70);
      labelKeyCode.Name = "labelKeyCode";
      labelKeyCode.Size = new Size(56, 17);
      labelKeyCode.TabIndex = 0;
      labelKeyCode.Text = "键值代码";
      // 
      // groupBoxDelay
      // 
      groupBoxDelay.Controls.Add(comboBoxDelayUnit);
      groupBoxDelay.Controls.Add(numericUpDownDelay);
      groupBoxDelay.Controls.Add(labelDelayUnit);
      groupBoxDelay.Controls.Add(labelDelay);
      groupBoxDelay.Location = new Point(12, 265);
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
      btnOk.Location = new Point(208, 380);
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
      btnCancel.Location = new Point(297, 380);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new Size(75, 31);
      btnCancel.TabIndex = 10;
      btnCancel.Text = "取消(&C)";
      btnCancel.UseVisualStyleBackColor = true;
      btnCancel.Click += BtnCancel_Click;
      // 
      // EditKeyboardEventForm
      // 
      AcceptButton = btnOk;
      AutoScaleDimensions = new SizeF(7F, 17F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = btnCancel;
      ClientSize = new Size(384, 421);
      Controls.Add(btnCancel);
      Controls.Add(btnOk);
      Controls.Add(groupBoxDelay);
      Controls.Add(groupBoxKeyInfo);
      Controls.Add(comboBoxAction);
      Controls.Add(labelAction);
      Controls.Add(textBoxEventName);
      Controls.Add(labelEventName);
      FormBorderStyle = FormBorderStyle.FixedToolWindow;
      Name = "EditKeyboardEventForm";
      StartPosition = FormStartPosition.CenterParent;
      Text = "编辑键盘事件";
      Load += EditKeyboardEventForm_Load;
      groupBoxKeyInfo.ResumeLayout(false);
      groupBoxKeyInfo.PerformLayout();
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
    private Label labelKey;
    private ComboBox comboBoxKey;
    private GroupBox groupBoxKeyInfo;
    private TextBox textBoxKeyCode;
    private Label labelKeyCode;
    private GroupBox groupBoxDelay;
    private ComboBox comboBoxDelayUnit;
    private NumericUpDown numericUpDownDelay;
    private Label labelDelayUnit;
    private Label labelDelay;
    private Button btnOk;
    private Button btnCancel;
  }
}
