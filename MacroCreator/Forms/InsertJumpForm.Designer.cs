using MacroCreator.Models;

namespace MacroCreator.Forms
{
    partial class InsertJumpForm
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
            if (disposing)
            {
                colorPickerTimer?.Dispose();
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
            components = new System.ComponentModel.Container();
            pnlJumpType = new Panel();
            rbUnconditionalJump = new RadioButton();
            rbConditionalJump = new RadioButton();
            rbBreak = new RadioButton();
            pnlUnconditional = new Panel();
            lblTargetIndex = new Label();
            nudTargetIndex = new NumericUpDown();
            lblTargetLabel = new Label();
            txtTargetLabel = new TextBox();
            btnSelectTarget = new Button();
            pnlConditional = new Panel();
            lblConditionType = new Label();
            cmbConditionType = new ComboBox();
            pnlPixelCondition = new Panel();
            lblCoords = new Label();
            txtX = new TextBox();
            txtY = new TextBox();
            lblColor = new Label();
            colorPanel = new Panel();
            lblColorHex = new Label();
            btnPickColor = new Button();
            pnlCustomCondition = new Panel();
            lblCustomCondition = new Label();
            txtCustomCondition = new TextBox();
            lblHint = new Label();
            pnlJumpTargets = new Panel();
            lblTrueTarget = new Label();
            nudTrueTarget = new NumericUpDown();
            txtTrueLabel = new TextBox();
            txtTrueFilePath = new TextBox();
            btnBrowseTrueFile = new Button();
            btnSelectTrueTarget = new Button();
            chkFalseTargetEnabled = new CheckBox();
            nudFalseTarget = new NumericUpDown();
            txtFalseLabel = new TextBox();
            txtFalseFilePath = new TextBox();
            btnBrowseFalseFile = new Button();
            btnSelectFalseTarget = new Button();
            lblFalseHint = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            colorPickerTimer = new System.Windows.Forms.Timer(components);
            pnlJumpType.SuspendLayout();
            pnlUnconditional.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudTargetIndex).BeginInit();
            pnlConditional.SuspendLayout();
            pnlPixelCondition.SuspendLayout();
            pnlCustomCondition.SuspendLayout();
            pnlJumpTargets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudTrueTarget).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFalseTarget).BeginInit();
            SuspendLayout();
            // 
            // pnlJumpType
            // 
            pnlJumpType.Controls.Add(rbUnconditionalJump);
            pnlJumpType.Controls.Add(rbConditionalJump);
            pnlJumpType.Controls.Add(rbBreak);
            pnlJumpType.Location = new Point(20, 23);
            pnlJumpType.Name = "pnlJumpType";
            pnlJumpType.Size = new Size(550, 56);
            pnlJumpType.TabIndex = 0;
            // 
            // rbUnconditionalJump
            // 
            rbUnconditionalJump.AutoSize = true;
            rbUnconditionalJump.Checked = true;
            rbUnconditionalJump.Location = new Point(10, 17);
            rbUnconditionalJump.Name = "rbUnconditionalJump";
            rbUnconditionalJump.Size = new Size(86, 21);
            rbUnconditionalJump.TabIndex = 0;
            rbUnconditionalJump.TabStop = true;
            rbUnconditionalJump.Text = "无条件跳转";
            rbUnconditionalJump.UseVisualStyleBackColor = true;
            rbUnconditionalJump.CheckedChanged += RbJumpType_CheckedChanged;
            // 
            // rbConditionalJump
            // 
            rbConditionalJump.AutoSize = true;
            rbConditionalJump.Location = new Point(156, 17);
            rbConditionalJump.Name = "rbConditionalJump";
            rbConditionalJump.Size = new Size(74, 21);
            rbConditionalJump.TabIndex = 1;
            rbConditionalJump.Text = "条件跳转";
            rbConditionalJump.UseVisualStyleBackColor = true;
            rbConditionalJump.CheckedChanged += RbJumpType_CheckedChanged;
            // 
            // rbBreak
            // 
            rbBreak.AutoSize = true;
            rbBreak.Location = new Point(280, 17);
            rbBreak.Name = "rbBreak";
            rbBreak.Size = new Size(74, 21);
            rbBreak.TabIndex = 2;
            rbBreak.Text = "中断执行";
            rbBreak.UseVisualStyleBackColor = true;
            rbBreak.CheckedChanged += RbJumpType_CheckedChanged;
            // 
            // pnlUnconditional
            // 
            pnlUnconditional.BorderStyle = BorderStyle.FixedSingle;
            pnlUnconditional.Controls.Add(lblTargetIndex);
            pnlUnconditional.Controls.Add(nudTargetIndex);
            pnlUnconditional.Controls.Add(btnSelectTarget);
            pnlUnconditional.Controls.Add(lblTargetLabel);
            pnlUnconditional.Controls.Add(txtTargetLabel);
            pnlUnconditional.Location = new Point(20, 91);
            pnlUnconditional.Name = "pnlUnconditional";
            pnlUnconditional.Size = new Size(550, 113);
            pnlUnconditional.TabIndex = 1;
            // 
            // lblTargetIndex
            // 
            lblTargetIndex.AutoSize = true;
            lblTargetIndex.Location = new Point(10, 23);
            lblTargetIndex.Name = "lblTargetIndex";
            lblTargetIndex.Size = new Size(95, 17);
            lblTargetIndex.TabIndex = 0;
            lblTargetIndex.Text = "跳转到事件索引:";
            // 
            // nudTargetIndex
            // 
            nudTargetIndex.Location = new Point(120, 20);
            nudTargetIndex.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudTargetIndex.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudTargetIndex.Name = "nudTargetIndex";
            nudTargetIndex.Size = new Size(80, 23);
            nudTargetIndex.TabIndex = 1;
            nudTargetIndex.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblTargetLabel
            // 
            lblTargetLabel.AutoSize = true;
            lblTargetLabel.Location = new Point(10, 57);
            lblTargetLabel.Name = "lblTargetLabel";
            lblTargetLabel.Size = new Size(71, 17);
            lblTargetLabel.TabIndex = 2;
            lblTargetLabel.Text = "标签 (可选):";
            // 
            // txtTargetLabel
            // 
            txtTargetLabel.Location = new Point(120, 54);
            txtTargetLabel.Name = "txtTargetLabel";
            txtTargetLabel.Size = new Size(200, 23);
            txtTargetLabel.TabIndex = 3;
            // 
            // btnSelectTarget
            // 
            btnSelectTarget.Location = new Point(210, 19);
            btnSelectTarget.Name = "btnSelectTarget";
            btnSelectTarget.Size = new Size(110, 25);
            btnSelectTarget.TabIndex = 4;
            btnSelectTarget.Text = "从列表选择...";
            btnSelectTarget.UseVisualStyleBackColor = true;
            btnSelectTarget.Click += BtnSelectTarget_Click;
            // 
            // pnlConditional
            // 
            pnlConditional.BorderStyle = BorderStyle.FixedSingle;
            pnlConditional.Controls.Add(lblConditionType);
            pnlConditional.Controls.Add(cmbConditionType);
            pnlConditional.Controls.Add(pnlPixelCondition);
            pnlConditional.Controls.Add(pnlCustomCondition);
            pnlConditional.Controls.Add(pnlJumpTargets);
            pnlConditional.Location = new Point(20, 91);
            pnlConditional.Name = "pnlConditional";
            pnlConditional.Size = new Size(550, 340);
            pnlConditional.TabIndex = 2;
            pnlConditional.Visible = false;
            // 
            // lblConditionType
            // 
            lblConditionType.AutoSize = true;
            lblConditionType.Location = new Point(10, 23);
            lblConditionType.Name = "lblConditionType";
            lblConditionType.Size = new Size(59, 17);
            lblConditionType.TabIndex = 0;
            lblConditionType.Text = "条件类型:";
            // 
            // cmbConditionType
            // 
            cmbConditionType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbConditionType.FormattingEnabled = true;
            cmbConditionType.Items.AddRange(new object[] { "像素颜色检查", "自定义条件" });
            cmbConditionType.Location = new Point(80, 20);
            cmbConditionType.Name = "cmbConditionType";
            cmbConditionType.Size = new Size(150, 25);
            cmbConditionType.TabIndex = 1;
            cmbConditionType.SelectedIndexChanged += CmbConditionType_SelectedIndexChanged;
            // 
            // pnlPixelCondition
            // 
            pnlPixelCondition.Controls.Add(lblCoords);
            pnlPixelCondition.Controls.Add(txtX);
            pnlPixelCondition.Controls.Add(txtY);
            pnlPixelCondition.Controls.Add(lblColor);
            pnlPixelCondition.Controls.Add(colorPanel);
            pnlPixelCondition.Controls.Add(lblColorHex);
            pnlPixelCondition.Controls.Add(btnPickColor);
            pnlPixelCondition.Location = new Point(10, 57);
            pnlPixelCondition.Name = "pnlPixelCondition";
            pnlPixelCondition.Size = new Size(530, 91);
            pnlPixelCondition.TabIndex = 2;
            // 
            // lblCoords
            // 
            lblCoords.AutoSize = true;
            lblCoords.Location = new Point(10, 14);
            lblCoords.Name = "lblCoords";
            lblCoords.Size = new Size(93, 17);
            lblCoords.TabIndex = 0;
            lblCoords.Text = "像素坐标 (X, Y):";
            // 
            // txtX
            // 
            txtX.Location = new Point(120, 12);
            txtX.Name = "txtX";
            txtX.Size = new Size(50, 23);
            txtX.TabIndex = 1;
            // 
            // txtY
            // 
            txtY.Location = new Point(180, 12);
            txtY.Name = "txtY";
            txtY.Size = new Size(50, 23);
            txtY.TabIndex = 2;
            // 
            // lblColor
            // 
            lblColor.AutoSize = true;
            lblColor.Location = new Point(10, 48);
            lblColor.Name = "lblColor";
            lblColor.Size = new Size(59, 17);
            lblColor.TabIndex = 3;
            lblColor.Text = "目标颜色:";
            // 
            // colorPanel
            // 
            colorPanel.BackColor = Color.Red;
            colorPanel.BorderStyle = BorderStyle.FixedSingle;
            colorPanel.Location = new Point(120, 48);
            colorPanel.Name = "colorPanel";
            colorPanel.Size = new Size(50, 22);
            colorPanel.TabIndex = 4;
            colorPanel.Paint += colorPanel_Paint;
            // 
            // lblColorHex
            // 
            lblColorHex.AutoSize = true;
            lblColorHex.Location = new Point(180, 51);
            lblColorHex.Name = "lblColorHex";
            lblColorHex.Size = new Size(56, 17);
            lblColorHex.TabIndex = 5;
            lblColorHex.Text = "#FF0000";
            // 
            // btnPickColor
            // 
            btnPickColor.Location = new Point(260, 46);
            btnPickColor.Name = "btnPickColor";
            btnPickColor.Size = new Size(100, 26);
            btnPickColor.TabIndex = 6;
            btnPickColor.Text = "拾取颜色(F8)";
            btnPickColor.UseVisualStyleBackColor = true;
            btnPickColor.Click += BtnPickColor_Click;
            // 
            // pnlCustomCondition
            // 
            pnlCustomCondition.Controls.Add(lblCustomCondition);
            pnlCustomCondition.Controls.Add(txtCustomCondition);
            pnlCustomCondition.Controls.Add(lblHint);
            pnlCustomCondition.Location = new Point(10, 57);
            pnlCustomCondition.Name = "pnlCustomCondition";
            pnlCustomCondition.Size = new Size(530, 91);
            pnlCustomCondition.TabIndex = 3;
            pnlCustomCondition.Visible = false;
            // 
            // lblCustomCondition
            // 
            lblCustomCondition.AutoSize = true;
            lblCustomCondition.Location = new Point(0, 11);
            lblCustomCondition.Name = "lblCustomCondition";
            lblCustomCondition.Size = new Size(71, 17);
            lblCustomCondition.TabIndex = 0;
            lblCustomCondition.Text = "条件表达式:";
            // 
            // txtCustomCondition
            // 
            txtCustomCondition.Location = new Point(0, 34);
            txtCustomCondition.Name = "txtCustomCondition";
            txtCustomCondition.Size = new Size(400, 23);
            txtCustomCondition.TabIndex = 1;
            txtCustomCondition.Text = "true";
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.ForeColor = Color.Gray;
            lblHint.Location = new Point(0, 62);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(199, 17);
            lblHint.TabIndex = 2;
            lblHint.Text = "提示: 可使用 hour, random 等变量";
            // 
            // pnlJumpTargets
            // 
            pnlJumpTargets.Controls.Add(lblTrueTarget);
            pnlJumpTargets.Controls.Add(nudTrueTarget);
            pnlJumpTargets.Controls.Add(btnSelectTrueTarget);
            pnlJumpTargets.Controls.Add(txtTrueLabel);
            pnlJumpTargets.Controls.Add(txtTrueFilePath);
            pnlJumpTargets.Controls.Add(btnBrowseTrueFile);
            pnlJumpTargets.Controls.Add(chkFalseTargetEnabled);
            pnlJumpTargets.Controls.Add(nudFalseTarget);
            pnlJumpTargets.Controls.Add(btnSelectFalseTarget);
            pnlJumpTargets.Controls.Add(txtFalseLabel);
            pnlJumpTargets.Controls.Add(txtFalseFilePath);
            pnlJumpTargets.Controls.Add(btnBrowseFalseFile);
            pnlJumpTargets.Controls.Add(lblFalseHint);
            pnlJumpTargets.Location = new Point(10, 159);
            pnlJumpTargets.Name = "pnlJumpTargets";
            pnlJumpTargets.Size = new Size(530, 170);
            pnlJumpTargets.TabIndex = 4;
            // 
            // lblTrueTarget
            // 
            lblTrueTarget.AutoSize = true;
            lblTrueTarget.Location = new Point(10, 13);
            lblTrueTarget.Name = "lblTrueTarget";
            lblTrueTarget.Size = new Size(83, 17);
            lblTrueTarget.TabIndex = 0;
            lblTrueTarget.Text = "条件真时跳转:";
            // 
            // nudTrueTarget
            // 
            nudTrueTarget.Location = new Point(154, 10);
            nudTrueTarget.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudTrueTarget.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudTrueTarget.Name = "nudTrueTarget";
            nudTrueTarget.Size = new Size(80, 23);
            nudTrueTarget.TabIndex = 1;
            nudTrueTarget.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // txtTrueLabel
            // 
            txtTrueLabel.Location = new Point(244, 10);
            txtTrueLabel.Name = "txtTrueLabel";
            txtTrueLabel.PlaceholderText = "标签 (可选)";
            txtTrueLabel.Size = new Size(150, 23);
            txtTrueLabel.TabIndex = 2;
            // 
            // btnSelectTrueTarget
            // 
            btnSelectTrueTarget.Location = new Point(400, 9);
            btnSelectTrueTarget.Name = "btnSelectTrueTarget";
            btnSelectTrueTarget.Size = new Size(75, 24);
            btnSelectTrueTarget.TabIndex = 11;
            btnSelectTrueTarget.Text = "选择...";
            btnSelectTrueTarget.UseVisualStyleBackColor = true;
            btnSelectTrueTarget.Click += BtnSelectTrueTarget_Click;
            // 
            // txtTrueFilePath
            // 
            txtTrueFilePath.Location = new Point(154, 39);
            txtTrueFilePath.Name = "txtTrueFilePath";
            txtTrueFilePath.PlaceholderText = "或执行外部文件 (可选)";
            txtTrueFilePath.Size = new Size(240, 23);
            txtTrueFilePath.TabIndex = 3;
            // 
            // btnBrowseTrueFile
            // 
            btnBrowseTrueFile.Location = new Point(400, 38);
            btnBrowseTrueFile.Name = "btnBrowseTrueFile";
            btnBrowseTrueFile.Size = new Size(75, 24);
            btnBrowseTrueFile.TabIndex = 4;
            btnBrowseTrueFile.Text = "浏览...";
            btnBrowseTrueFile.UseVisualStyleBackColor = true;
            btnBrowseTrueFile.Click += BtnBrowseTrueFile_Click;
            // 
            // chkFalseTargetEnabled
            // 
            chkFalseTargetEnabled.AutoSize = true;
            chkFalseTargetEnabled.Location = new Point(34, 75);
            chkFalseTargetEnabled.Name = "chkFalseTargetEnabled";
            chkFalseTargetEnabled.Size = new Size(102, 21);
            chkFalseTargetEnabled.TabIndex = 5;
            chkFalseTargetEnabled.Text = "条件假时跳转:";
            chkFalseTargetEnabled.UseVisualStyleBackColor = true;
            chkFalseTargetEnabled.CheckedChanged += ChkFalseTargetEnabled_CheckedChanged;
            // 
            // nudFalseTarget
            // 
            nudFalseTarget.Enabled = false;
            nudFalseTarget.Location = new Point(154, 73);
            nudFalseTarget.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudFalseTarget.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudFalseTarget.Name = "nudFalseTarget";
            nudFalseTarget.Size = new Size(80, 23);
            nudFalseTarget.TabIndex = 6;
            nudFalseTarget.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // txtFalseLabel
            // 
            txtFalseLabel.Enabled = false;
            txtFalseLabel.Location = new Point(244, 73);
            txtFalseLabel.Name = "txtFalseLabel";
            txtFalseLabel.PlaceholderText = "标签 (可选)";
            txtFalseLabel.Size = new Size(150, 23);
            txtFalseLabel.TabIndex = 7;
            // 
            // btnSelectFalseTarget
            // 
            btnSelectFalseTarget.Enabled = false;
            btnSelectFalseTarget.Location = new Point(400, 72);
            btnSelectFalseTarget.Name = "btnSelectFalseTarget";
            btnSelectFalseTarget.Size = new Size(75, 24);
            btnSelectFalseTarget.TabIndex = 12;
            btnSelectFalseTarget.Text = "选择...";
            btnSelectFalseTarget.UseVisualStyleBackColor = true;
            btnSelectFalseTarget.Click += BtnSelectFalseTarget_Click;
            // 
            // txtFalseFilePath
            // 
            txtFalseFilePath.Enabled = false;
            txtFalseFilePath.Location = new Point(154, 102);
            txtFalseFilePath.Name = "txtFalseFilePath";
            txtFalseFilePath.PlaceholderText = "或执行外部文件 (可选)";
            txtFalseFilePath.Size = new Size(240, 23);
            txtFalseFilePath.TabIndex = 8;
            // 
            // btnBrowseFalseFile
            // 
            btnBrowseFalseFile.Enabled = false;
            btnBrowseFalseFile.Location = new Point(400, 101);
            btnBrowseFalseFile.Name = "btnBrowseFalseFile";
            btnBrowseFalseFile.Size = new Size(75, 24);
            btnBrowseFalseFile.TabIndex = 9;
            btnBrowseFalseFile.Text = "浏览...";
            btnBrowseFalseFile.UseVisualStyleBackColor = true;
            btnBrowseFalseFile.Click += BtnBrowseFalseFile_Click;
            // 
            // lblFalseHint
            // 
            lblFalseHint.AutoSize = true;
            lblFalseHint.ForeColor = Color.Gray;
            lblFalseHint.Location = new Point(34, 137);
            lblFalseHint.Name = "lblFalseHint";
            lblFalseHint.Size = new Size(236, 17);
            lblFalseHint.TabIndex = 10;
            lblFalseHint.Text = "不选中表示条件为假时继续执行下一个事件";
            // 
            // btnOK
            // 
            btnOK.Location = new Point(362, 450);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(101, 36);
            btnOK.TabIndex = 3;
            btnOK.Text = "确定";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(469, 450);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(101, 36);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // colorPickerTimer
            // 
            colorPickerTimer.Tick += ColorPickerTimer_Tick;
            // 
            // InsertJumpForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(589, 510);
            Controls.Add(pnlJumpType);
            Controls.Add(pnlUnconditional);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(pnlConditional);
            FormBorderStyle = FormBorderStyle.Sizable;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = true;
            Name = "InsertJumpForm";
            StartPosition = FormStartPosition.Manual;
            Text = "插入跳转事件";
            KeyDown += InsertJumpForm_KeyDown;
            pnlJumpType.ResumeLayout(false);
            pnlJumpType.PerformLayout();
            pnlUnconditional.ResumeLayout(false);
            pnlUnconditional.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudTargetIndex).EndInit();
            pnlConditional.ResumeLayout(false);
            pnlConditional.PerformLayout();
            pnlPixelCondition.ResumeLayout(false);
            pnlPixelCondition.PerformLayout();
            pnlCustomCondition.ResumeLayout(false);
            pnlCustomCondition.PerformLayout();
            pnlJumpTargets.ResumeLayout(false);
            pnlJumpTargets.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudTrueTarget).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFalseTarget).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlJumpType;
        private RadioButton rbUnconditionalJump;
        private RadioButton rbConditionalJump;
        private RadioButton rbBreak;
        private Panel pnlUnconditional;
        private Label lblTargetIndex;
        private NumericUpDown nudTargetIndex;
        private Label lblTargetLabel;
        private TextBox txtTargetLabel;
        private Panel pnlConditional;
        private Label lblConditionType;
        private ComboBox cmbConditionType;
        private Panel pnlPixelCondition;
        private Label lblCoords;
        private TextBox txtX;
        private TextBox txtY;
        private Label lblColor;
        private Panel colorPanel;
        private Label lblColorHex;
        private Button btnPickColor;
        private Panel pnlCustomCondition;
        private Label lblCustomCondition;
        private TextBox txtCustomCondition;
        private Label lblHint;
        private Panel pnlJumpTargets;
        private Label lblTrueTarget;
        private NumericUpDown nudTrueTarget;
        private TextBox txtTrueLabel;
        private TextBox txtTrueFilePath;
        private Button btnBrowseTrueFile;
        private CheckBox chkFalseTargetEnabled;
        private NumericUpDown nudFalseTarget;
        private TextBox txtFalseLabel;
        private TextBox txtFalseFilePath;
        private Button btnBrowseFalseFile;
        private Label lblFalseHint;
        private Button btnOK;
        private Button btnCancel;
        private System.Windows.Forms.Timer colorPickerTimer;
        private Button btnSelectTarget;
        private Button btnSelectTrueTarget;
        private Button btnSelectFalseTarget;
    }
}