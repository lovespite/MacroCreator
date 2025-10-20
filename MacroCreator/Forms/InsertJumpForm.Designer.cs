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
            btnSelectTarget = new Button();
            lblTargetLabel = new Label();
            txtTargetLabel = new TextBox();
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
            pnlJumpTargets = new Panel();
            panel2 = new Panel();
            rdFalseFilePath = new RadioButton();
            rdFalseEventName = new RadioButton();
            txtFalseTargetEventName = new TextBox();
            btnBrowseFalseFile = new Button();
            txtFalseFilePath = new TextBox();
            btnSelectFalseTarget = new Button();
            panel1 = new Panel();
            rdTrueFilePath = new RadioButton();
            rdTrueEventName = new RadioButton();
            txtTrueTargetEventName = new TextBox();
            btnBrowseTrueFile = new Button();
            btnSelectTrueTarget = new Button();
            txtTrueFilePath = new TextBox();
            lblTrueTarget = new Label();
            chkFalseTargetEnabled = new CheckBox();
            lblFalseHint = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            colorPickerTimer = new System.Windows.Forms.Timer(components);
            label1 = new Label();
            textBoxEventName = new TextBox();
            pnlJumpType.SuspendLayout();
            pnlUnconditional.SuspendLayout();
            pnlConditional.SuspendLayout();
            pnlPixelCondition.SuspendLayout();
            pnlCustomCondition.SuspendLayout();
            pnlJumpTargets.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlJumpType
            // 
            pnlJumpType.Controls.Add(rbUnconditionalJump);
            pnlJumpType.Controls.Add(rbConditionalJump);
            pnlJumpType.Controls.Add(rbBreak);
            pnlJumpType.Location = new Point(20, 58);
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
            pnlUnconditional.Controls.Add(btnSelectTarget);
            pnlUnconditional.Controls.Add(lblTargetLabel);
            pnlUnconditional.Controls.Add(txtTargetLabel);
            pnlUnconditional.Location = new Point(20, 126);
            pnlUnconditional.Name = "pnlUnconditional";
            pnlUnconditional.Size = new Size(550, 52);
            pnlUnconditional.TabIndex = 1;
            // 
            // btnSelectTarget
            // 
            btnSelectTarget.Location = new Point(341, 11);
            btnSelectTarget.Name = "btnSelectTarget";
            btnSelectTarget.Size = new Size(78, 25);
            btnSelectTarget.TabIndex = 4;
            btnSelectTarget.Text = "选择...";
            btnSelectTarget.UseVisualStyleBackColor = true;
            btnSelectTarget.Click += BtnSelectTarget_Click;
            // 
            // lblTargetLabel
            // 
            lblTargetLabel.AutoSize = true;
            lblTargetLabel.Location = new Point(8, 16);
            lblTargetLabel.Name = "lblTargetLabel";
            lblTargetLabel.Size = new Size(107, 17);
            lblTargetLabel.TabIndex = 2;
            lblTargetLabel.Text = "跳转目标事件名称:";
            // 
            // txtTargetLabel
            // 
            txtTargetLabel.Location = new Point(129, 13);
            txtTargetLabel.Name = "txtTargetLabel";
            txtTargetLabel.PlaceholderText = "留空表示匿名事件（使用索引）";
            txtTargetLabel.Size = new Size(200, 23);
            txtTargetLabel.TabIndex = 3;
            // 
            // pnlConditional
            // 
            pnlConditional.BorderStyle = BorderStyle.FixedSingle;
            pnlConditional.Controls.Add(lblConditionType);
            pnlConditional.Controls.Add(cmbConditionType);
            pnlConditional.Controls.Add(pnlPixelCondition);
            pnlConditional.Controls.Add(pnlCustomCondition);
            pnlConditional.Controls.Add(pnlJumpTargets);
            pnlConditional.Location = new Point(20, 126);
            pnlConditional.Name = "pnlConditional";
            pnlConditional.Size = new Size(550, 378);
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
            lblCoords.Location = new Point(9, 19);
            lblCoords.Name = "lblCoords";
            lblCoords.Size = new Size(93, 17);
            lblCoords.TabIndex = 0;
            lblCoords.Text = "像素坐标 (X, Y):";
            // 
            // txtX
            // 
            txtX.Location = new Point(119, 17);
            txtX.Name = "txtX";
            txtX.Size = new Size(100, 23);
            txtX.TabIndex = 1;
            // 
            // txtY
            // 
            txtY.Location = new Point(225, 17);
            txtY.Name = "txtY";
            txtY.Size = new Size(94, 23);
            txtY.TabIndex = 2;
            // 
            // lblColor
            // 
            lblColor.AutoSize = true;
            lblColor.Location = new Point(10, 54);
            lblColor.Name = "lblColor";
            lblColor.Size = new Size(59, 17);
            lblColor.TabIndex = 3;
            lblColor.Text = "目标颜色:";
            // 
            // colorPanel
            // 
            colorPanel.BackColor = Color.Red;
            colorPanel.Location = new Point(121, 51);
            colorPanel.Name = "colorPanel";
            colorPanel.Size = new Size(98, 22);
            colorPanel.TabIndex = 4;
            // 
            // lblColorHex
            // 
            lblColorHex.AutoSize = true;
            lblColorHex.Location = new Point(225, 54);
            lblColorHex.Name = "lblColorHex";
            lblColorHex.Size = new Size(56, 17);
            lblColorHex.TabIndex = 5;
            lblColorHex.Text = "#FF0000";
            // 
            // btnPickColor
            // 
            btnPickColor.Location = new Point(417, 54);
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
            pnlCustomCondition.Location = new Point(10, 57);
            pnlCustomCondition.Name = "pnlCustomCondition";
            pnlCustomCondition.Size = new Size(530, 91);
            pnlCustomCondition.TabIndex = 3;
            pnlCustomCondition.Visible = false;
            // 
            // lblCustomCondition
            // 
            lblCustomCondition.AutoSize = true;
            lblCustomCondition.Location = new Point(12, 11);
            lblCustomCondition.Name = "lblCustomCondition";
            lblCustomCondition.Size = new Size(71, 17);
            lblCustomCondition.TabIndex = 0;
            lblCustomCondition.Text = "条件表达式:";
            // 
            // txtCustomCondition
            // 
            txtCustomCondition.Location = new Point(89, 11);
            txtCustomCondition.Multiline = true;
            txtCustomCondition.Name = "txtCustomCondition";
            txtCustomCondition.Size = new Size(428, 69);
            txtCustomCondition.TabIndex = 1;
            txtCustomCondition.Text = "true";
            // 
            // pnlJumpTargets
            // 
            pnlJumpTargets.Controls.Add(panel2);
            pnlJumpTargets.Controls.Add(panel1);
            pnlJumpTargets.Controls.Add(lblTrueTarget);
            pnlJumpTargets.Controls.Add(chkFalseTargetEnabled);
            pnlJumpTargets.Controls.Add(lblFalseHint);
            pnlJumpTargets.Location = new Point(10, 159);
            pnlJumpTargets.Name = "pnlJumpTargets";
            pnlJumpTargets.Size = new Size(530, 204);
            pnlJumpTargets.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.Controls.Add(rdFalseFilePath);
            panel2.Controls.Add(rdFalseEventName);
            panel2.Controls.Add(txtFalseTargetEventName);
            panel2.Controls.Add(btnBrowseFalseFile);
            panel2.Controls.Add(txtFalseFilePath);
            panel2.Controls.Add(btnSelectFalseTarget);
            panel2.Location = new Point(126, 101);
            panel2.Name = "panel2";
            panel2.Size = new Size(391, 69);
            panel2.TabIndex = 14;
            // 
            // rdFalseFilePath
            // 
            rdFalseFilePath.AutoSize = true;
            rdFalseFilePath.Enabled = false;
            rdFalseFilePath.Location = new Point(12, 43);
            rdFalseFilePath.Name = "rdFalseFilePath";
            rdFalseFilePath.Size = new Size(14, 13);
            rdFalseFilePath.TabIndex = 13;
            rdFalseFilePath.UseVisualStyleBackColor = true;
            rdFalseFilePath.CheckedChanged += rdFalseFilePath_CheckedChanged;
            // 
            // rdFalseEventName
            // 
            rdFalseEventName.AutoSize = true;
            rdFalseEventName.Checked = true;
            rdFalseEventName.Enabled = false;
            rdFalseEventName.Location = new Point(12, 13);
            rdFalseEventName.Name = "rdFalseEventName";
            rdFalseEventName.Size = new Size(14, 13);
            rdFalseEventName.TabIndex = 14;
            rdFalseEventName.TabStop = true;
            rdFalseEventName.UseVisualStyleBackColor = true;
            rdFalseEventName.CheckedChanged += rdFalseEventName_CheckedChanged;
            // 
            // txtFalseTargetEventName
            // 
            txtFalseTargetEventName.Enabled = false;
            txtFalseTargetEventName.Location = new Point(32, 8);
            txtFalseTargetEventName.Name = "txtFalseTargetEventName";
            txtFalseTargetEventName.PlaceholderText = "事件名称";
            txtFalseTargetEventName.Size = new Size(265, 23);
            txtFalseTargetEventName.TabIndex = 7;
            // 
            // btnBrowseFalseFile
            // 
            btnBrowseFalseFile.Enabled = false;
            btnBrowseFalseFile.Location = new Point(303, 36);
            btnBrowseFalseFile.Name = "btnBrowseFalseFile";
            btnBrowseFalseFile.Size = new Size(75, 24);
            btnBrowseFalseFile.TabIndex = 9;
            btnBrowseFalseFile.Text = "浏览...";
            btnBrowseFalseFile.UseVisualStyleBackColor = true;
            btnBrowseFalseFile.Click += BtnBrowseFalseFile_Click;
            // 
            // txtFalseFilePath
            // 
            txtFalseFilePath.Enabled = false;
            txtFalseFilePath.Location = new Point(32, 37);
            txtFalseFilePath.Name = "txtFalseFilePath";
            txtFalseFilePath.PlaceholderText = "或执行外部文件 (可选)";
            txtFalseFilePath.Size = new Size(265, 23);
            txtFalseFilePath.TabIndex = 8;
            // 
            // btnSelectFalseTarget
            // 
            btnSelectFalseTarget.Enabled = false;
            btnSelectFalseTarget.Location = new Point(303, 7);
            btnSelectFalseTarget.Name = "btnSelectFalseTarget";
            btnSelectFalseTarget.Size = new Size(75, 24);
            btnSelectFalseTarget.TabIndex = 12;
            btnSelectFalseTarget.Text = "选择...";
            btnSelectFalseTarget.UseVisualStyleBackColor = true;
            btnSelectFalseTarget.Click += BtnSelectFalseTarget_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(rdTrueFilePath);
            panel1.Controls.Add(rdTrueEventName);
            panel1.Controls.Add(txtTrueTargetEventName);
            panel1.Controls.Add(btnBrowseTrueFile);
            panel1.Controls.Add(btnSelectTrueTarget);
            panel1.Controls.Add(txtTrueFilePath);
            panel1.Location = new Point(126, 17);
            panel1.Name = "panel1";
            panel1.Size = new Size(391, 69);
            panel1.TabIndex = 13;
            // 
            // rdTrueFilePath
            // 
            rdTrueFilePath.AutoSize = true;
            rdTrueFilePath.Location = new Point(12, 44);
            rdTrueFilePath.Name = "rdTrueFilePath";
            rdTrueFilePath.Size = new Size(14, 13);
            rdTrueFilePath.TabIndex = 12;
            rdTrueFilePath.UseVisualStyleBackColor = true;
            rdTrueFilePath.CheckedChanged += rdTrueFilePath_CheckedChanged;
            // 
            // rdTrueEventName
            // 
            rdTrueEventName.AutoSize = true;
            rdTrueEventName.Checked = true;
            rdTrueEventName.Location = new Point(12, 15);
            rdTrueEventName.Name = "rdTrueEventName";
            rdTrueEventName.Size = new Size(14, 13);
            rdTrueEventName.TabIndex = 12;
            rdTrueEventName.TabStop = true;
            rdTrueEventName.UseVisualStyleBackColor = true;
            rdTrueEventName.CheckedChanged += rdTrueEventName_CheckedChanged;
            // 
            // txtTrueTargetEventName
            // 
            txtTrueTargetEventName.Location = new Point(32, 9);
            txtTrueTargetEventName.Name = "txtTrueTargetEventName";
            txtTrueTargetEventName.PlaceholderText = "事件名称";
            txtTrueTargetEventName.Size = new Size(265, 23);
            txtTrueTargetEventName.TabIndex = 2;
            // 
            // btnBrowseTrueFile
            // 
            btnBrowseTrueFile.Enabled = false;
            btnBrowseTrueFile.Location = new Point(303, 37);
            btnBrowseTrueFile.Name = "btnBrowseTrueFile";
            btnBrowseTrueFile.Size = new Size(75, 24);
            btnBrowseTrueFile.TabIndex = 4;
            btnBrowseTrueFile.Text = "浏览...";
            btnBrowseTrueFile.UseVisualStyleBackColor = true;
            btnBrowseTrueFile.Click += BtnBrowseTrueFile_Click;
            // 
            // btnSelectTrueTarget
            // 
            btnSelectTrueTarget.Location = new Point(303, 7);
            btnSelectTrueTarget.Name = "btnSelectTrueTarget";
            btnSelectTrueTarget.Size = new Size(75, 24);
            btnSelectTrueTarget.TabIndex = 11;
            btnSelectTrueTarget.Text = "选择...";
            btnSelectTrueTarget.UseVisualStyleBackColor = true;
            btnSelectTrueTarget.Click += BtnSelectTrueTarget_Click;
            // 
            // txtTrueFilePath
            // 
            txtTrueFilePath.Enabled = false;
            txtTrueFilePath.Location = new Point(32, 38);
            txtTrueFilePath.Name = "txtTrueFilePath";
            txtTrueFilePath.PlaceholderText = "或执行外部文件 (可选)";
            txtTrueFilePath.Size = new Size(265, 23);
            txtTrueFilePath.TabIndex = 3;
            // 
            // lblTrueTarget
            // 
            lblTrueTarget.AutoSize = true;
            lblTrueTarget.Location = new Point(10, 28);
            lblTrueTarget.Name = "lblTrueTarget";
            lblTrueTarget.Size = new Size(83, 17);
            lblTrueTarget.TabIndex = 0;
            lblTrueTarget.Text = "条件真时跳转:";
            // 
            // chkFalseTargetEnabled
            // 
            chkFalseTargetEnabled.AutoSize = true;
            chkFalseTargetEnabled.Location = new Point(10, 111);
            chkFalseTargetEnabled.Name = "chkFalseTargetEnabled";
            chkFalseTargetEnabled.Size = new Size(102, 21);
            chkFalseTargetEnabled.TabIndex = 5;
            chkFalseTargetEnabled.Text = "条件假时跳转:";
            chkFalseTargetEnabled.UseVisualStyleBackColor = true;
            chkFalseTargetEnabled.CheckedChanged += ChkFalseTargetEnabled_CheckedChanged;
            // 
            // lblFalseHint
            // 
            lblFalseHint.AutoSize = true;
            lblFalseHint.ForeColor = Color.Gray;
            lblFalseHint.Location = new Point(126, 173);
            lblFalseHint.Name = "lblFalseHint";
            lblFalseHint.Size = new Size(236, 17);
            lblFalseHint.TabIndex = 10;
            lblFalseHint.Text = "不选中表示条件为假时继续执行下一个事件";
            // 
            // btnOK
            // 
            btnOK.Location = new Point(362, 528);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(101, 36);
            btnOK.TabIndex = 3;
            btnOK.Text = "确定(&O)";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(469, 528);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(101, 36);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消(&C)";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // colorPickerTimer
            // 
            colorPickerTimer.Tick += ColorPickerTimer_Tick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 23);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 5;
            label1.Text = "事件名称";
            // 
            // textBoxEventName
            // 
            textBoxEventName.Location = new Point(82, 20);
            textBoxEventName.Name = "textBoxEventName";
            textBoxEventName.PlaceholderText = "(可选)";
            textBoxEventName.Size = new Size(488, 23);
            textBoxEventName.TabIndex = 6;
            // 
            // InsertJumpForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(589, 578);
            Controls.Add(textBoxEventName);
            Controls.Add(label1);
            Controls.Add(pnlJumpType);
            Controls.Add(pnlUnconditional);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(pnlConditional);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            KeyPreview = true;
            MaximizeBox = false;
            Name = "InsertJumpForm";
            StartPosition = FormStartPosition.Manual;
            Text = "插入跳转事件";
            Load += InsertJumpForm_Load;
            KeyDown += InsertJumpForm_KeyDown;
            pnlJumpType.ResumeLayout(false);
            pnlJumpType.PerformLayout();
            pnlUnconditional.ResumeLayout(false);
            pnlUnconditional.PerformLayout();
            pnlConditional.ResumeLayout(false);
            pnlConditional.PerformLayout();
            pnlPixelCondition.ResumeLayout(false);
            pnlPixelCondition.PerformLayout();
            pnlCustomCondition.ResumeLayout(false);
            pnlCustomCondition.PerformLayout();
            pnlJumpTargets.ResumeLayout(false);
            pnlJumpTargets.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlJumpType;
        private RadioButton rbUnconditionalJump;
        private RadioButton rbConditionalJump;
        private RadioButton rbBreak;
        private Panel pnlUnconditional;
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
        private Panel pnlJumpTargets;
        private Label lblTrueTarget;
        private TextBox txtTrueTargetEventName;
        private TextBox txtTrueFilePath;
        private Button btnBrowseTrueFile;
        private CheckBox chkFalseTargetEnabled;
        private TextBox txtFalseTargetEventName;
        private TextBox txtFalseFilePath;
        private Button btnBrowseFalseFile;
        private Label lblFalseHint;
        private Button btnOK;
        private Button btnCancel;
        private System.Windows.Forms.Timer colorPickerTimer;
        private Button btnSelectTarget;
        private Button btnSelectTrueTarget;
        private Button btnSelectFalseTarget;
        private Label label1;
        private TextBox textBoxEventName;
        private Panel panel1;
        private RadioButton rdTrueFilePath;
        private RadioButton rdTrueEventName;
        private Panel panel2;
        private RadioButton rdFalseFilePath;
        private RadioButton rdFalseEventName;
    }
}