namespace MacroCreator.Forms
{
    partial class MainForm
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
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            insertConditionToolStripMenuItem = new ToolStripMenuItem();
            insertJumpToolStripMenuItem = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolStripContainer = new ToolStripContainer();
            lvEvents = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            buttonPanel = new FlowLayoutPanel();
            btnRecord = new Button();
            btnPlay = new Button();
            btnStop = new Button();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            toolStripContainer.BottomToolStripPanel.SuspendLayout();
            toolStripContainer.ContentPanel.SuspendLayout();
            toolStripContainer.TopToolStripPanel.SuspendLayout();
            toolStripContainer.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Dock = DockStyle.None;
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(761, 25);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(58, 21);
            fileToolStripMenuItem.Text = "文件(&F)";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newToolStripMenuItem.Size = new Size(215, 22);
            newToolStripMenuItem.Text = "新建(&N)";
            newToolStripMenuItem.Click += NewToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new Size(215, 22);
            openToolStripMenuItem.Text = "打开(&O)...";
            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new Size(215, 22);
            saveToolStripMenuItem.Text = "保存(&S)";
            saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsToolStripMenuItem.Size = new Size(215, 22);
            saveAsToolStripMenuItem.Text = "另存为(&A)...";
            saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(212, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitToolStripMenuItem.Size = new Size(215, 22);
            exitToolStripMenuItem.Text = "退出(&X)";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { insertConditionToolStripMenuItem, insertJumpToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(59, 21);
            editToolStripMenuItem.Text = "编辑(&E)";
            // 
            // insertConditionToolStripMenuItem
            // 
            insertConditionToolStripMenuItem.Name = "insertConditionToolStripMenuItem";
            insertConditionToolStripMenuItem.Size = new Size(170, 22);
            insertConditionToolStripMenuItem.Text = "插入条件判断(&I)...";
            insertConditionToolStripMenuItem.Click += InsertConditionToolStripMenuItem_Click;
            // 
            // insertJumpToolStripMenuItem
            // 
            insertJumpToolStripMenuItem.Name = "insertJumpToolStripMenuItem";
            insertJumpToolStripMenuItem.Size = new Size(170, 22);
            insertJumpToolStripMenuItem.Text = "插入跳转事件(&J)...";
            insertJumpToolStripMenuItem.Click += InsertJumpToolStripMenuItem_Click;
            // 
            // statusStrip
            // 
            statusStrip.Dock = DockStyle.None;
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 0);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(761, 22);
            statusStrip.TabIndex = 0;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(56, 17);
            statusLabel.Text = "准备就绪";
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.BottomToolStripPanel
            // 
            toolStripContainer.BottomToolStripPanel.Controls.Add(statusStrip);
            // 
            // toolStripContainer.ContentPanel
            // 
            toolStripContainer.ContentPanel.Controls.Add(lvEvents);
            toolStripContainer.ContentPanel.Controls.Add(buttonPanel);
            toolStripContainer.ContentPanel.Margin = new Padding(4, 3, 4, 3);
            toolStripContainer.ContentPanel.Size = new Size(761, 482);
            toolStripContainer.Dock = DockStyle.Fill;
            toolStripContainer.Location = new Point(4, 0);
            toolStripContainer.Margin = new Padding(4, 3, 4, 3);
            toolStripContainer.Name = "toolStripContainer";
            toolStripContainer.Size = new Size(761, 529);
            toolStripContainer.TabIndex = 0;
            toolStripContainer.Text = "toolStripContainer";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            toolStripContainer.TopToolStripPanel.Controls.Add(menuStrip);
            // 
            // lvEvents
            // 
            lvEvents.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            lvEvents.Dock = DockStyle.Fill;
            lvEvents.FullRowSelect = true;
            lvEvents.Location = new Point(0, 54);
            lvEvents.Margin = new Padding(4, 3, 4, 3);
            lvEvents.Name = "lvEvents";
            lvEvents.Size = new Size(761, 428);
            lvEvents.TabIndex = 1;
            lvEvents.UseCompatibleStateImageBehavior = false;
            lvEvents.View = View.Details;
            lvEvents.KeyDown += EventListView_KeyDown;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "ID";
            columnHeader1.Width = 50;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "事件类型";
            columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "描述";
            columnHeader3.Width = 450;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "延迟(ms)";
            columnHeader4.Width = 100;
            // 
            // buttonPanel
            // 
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(btnRecord);
            buttonPanel.Controls.Add(btnPlay);
            buttonPanel.Controls.Add(btnStop);
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Location = new Point(0, 0);
            buttonPanel.Margin = new Padding(4, 3, 4, 3);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(5);
            buttonPanel.Size = new Size(761, 54);
            buttonPanel.TabIndex = 0;
            // 
            // btnRecord
            // 
            btnRecord.Location = new Point(10, 10);
            btnRecord.Margin = new Padding(5);
            btnRecord.Name = "btnRecord";
            btnRecord.Size = new Size(105, 34);
            btnRecord.TabIndex = 0;
            btnRecord.Text = "录制 (F9)";
            btnRecord.UseVisualStyleBackColor = true;
            btnRecord.Click += BtnRecord_Click;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(125, 10);
            btnPlay.Margin = new Padding(5);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(105, 34);
            btnPlay.TabIndex = 1;
            btnPlay.Text = "播放 (F10)";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += BtnPlay_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(240, 10);
            btnStop.Margin = new Padding(5);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(105, 34);
            btnStop.TabIndex = 2;
            btnStop.Text = "停止 (F11)";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += BtnStop_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(769, 533);
            Controls.Add(toolStripContainer);
            KeyPreview = true;
            MainMenuStrip = menuStrip;
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            Padding = new Padding(4, 0, 4, 4);
            Text = "自动化宏工具";
            KeyDown += MainForm_KeyDown;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            toolStripContainer.BottomToolStripPanel.PerformLayout();
            toolStripContainer.ContentPanel.ResumeLayout(false);
            toolStripContainer.ContentPanel.PerformLayout();
            toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer.TopToolStripPanel.PerformLayout();
            toolStripContainer.ResumeLayout(false);
            toolStripContainer.PerformLayout();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem insertConditionToolStripMenuItem;
        private ToolStripMenuItem insertJumpToolStripMenuItem;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripContainer toolStripContainer;
        private ListView lvEvents;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private FlowLayoutPanel buttonPanel;
        private Button btnRecord;
        private Button btnPlay;
        private Button btnStop;
    }
}