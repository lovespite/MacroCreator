namespace MacroCreator.Forms
{
    partial class EditScriptEventForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditScriptEventForm));
            textBoxScriptCode = new TextBox();
            textBoxEventName = new TextBox();
            btnCancel = new Button();
            btnOk = new Button();
            label1 = new Label();
            splitContainer1 = new SplitContainer();
            lvErrors = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            imageList1 = new ImageList(components);
            lbCursor = new Label();
            btnCheck = new Button();
            columnHeader3 = new ColumnHeader();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // textBoxScriptCode
            // 
            textBoxScriptCode.Dock = DockStyle.Fill;
            textBoxScriptCode.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxScriptCode.Location = new Point(4, 4);
            textBoxScriptCode.Multiline = true;
            textBoxScriptCode.Name = "textBoxScriptCode";
            textBoxScriptCode.Size = new Size(552, 224);
            textBoxScriptCode.TabIndex = 0;
            textBoxScriptCode.Click += TextBoxScriptCode_Click;
            textBoxScriptCode.KeyUp += TextBoxScriptCode_KeyUp;
            // 
            // textBoxEventName
            // 
            textBoxEventName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxEventName.Location = new Point(74, 12);
            textBoxEventName.Name = "textBoxEventName";
            textBoxEventName.Size = new Size(494, 23);
            textBoxEventName.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(463, 400);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(105, 33);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "取消(&C)";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Location = new Point(352, 400);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(105, 33);
            btnOk.TabIndex = 2;
            btnOk.Text = "确定(&O)";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 3;
            label1.Text = "事件名称";
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(12, 41);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(textBoxScriptCode);
            splitContainer1.Panel1.Padding = new Padding(4);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(lvErrors);
            splitContainer1.Panel2.Padding = new Padding(4);
            splitContainer1.Size = new Size(560, 353);
            splitContainer1.SplitterDistance = 232;
            splitContainer1.TabIndex = 5;
            // 
            // lvErrors
            // 
            lvErrors.Columns.AddRange(new ColumnHeader[] { columnHeader3, columnHeader1, columnHeader2 });
            lvErrors.Cursor = Cursors.Hand;
            lvErrors.Dock = DockStyle.Fill;
            lvErrors.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Underline, GraphicsUnit.Point, 134);
            lvErrors.ForeColor = SystemColors.MenuHighlight;
            lvErrors.FullRowSelect = true;
            lvErrors.Location = new Point(4, 4);
            lvErrors.Name = "lvErrors";
            lvErrors.Size = new Size(552, 109);
            lvErrors.SmallImageList = imageList1;
            lvErrors.TabIndex = 0;
            lvErrors.UseCompatibleStateImageBehavior = false;
            lvErrors.View = View.Details;
            lvErrors.Click += LvErrors_Click;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "位置";
            columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "描述";
            columnHeader2.Width = 320;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "error.png");
            imageList1.Images.SetKeyName(1, "warning.png");
            // 
            // lbCursor
            // 
            lbCursor.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lbCursor.AutoSize = true;
            lbCursor.Location = new Point(16, 408);
            lbCursor.Name = "lbCursor";
            lbCursor.Size = new Size(66, 17);
            lbCursor.TabIndex = 6;
            lbCursor.Text = "行 0，列 0";
            // 
            // btnCheck
            // 
            btnCheck.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCheck.Location = new Point(241, 400);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(105, 33);
            btnCheck.TabIndex = 2;
            btnCheck.Text = "检查(&I)";
            btnCheck.UseVisualStyleBackColor = true;
            btnCheck.Click += BtnCheck_Click;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "类型";
            columnHeader3.Width = 80;
            // 
            // EditScriptEventForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 441);
            Controls.Add(lbCursor);
            Controls.Add(splitContainer1);
            Controls.Add(label1);
            Controls.Add(btnCheck);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            Controls.Add(textBoxEventName);
            MinimumSize = new Size(600, 480);
            Name = "EditScriptEventForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "编辑脚本";
            Load += EditScriptEventForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxScriptCode;
        private TextBox textBoxEventName;
        private Button btnCancel;
        private Button btnOk;
        private Label label1;
        private SplitContainer splitContainer1;
        private ListView lvErrors;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private Label lbCursor;
        private Button btnCheck;
        private ImageList imageList1;
        private ColumnHeader columnHeader3;
    }
}