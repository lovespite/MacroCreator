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
            components = new System.ComponentModel.Container();
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            ctrlToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            playToolStripMenuItem = new ToolStripMenuItem();
            stopToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            playFromCursorToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            RenameEvent2ToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            DeleteToolStripMenuItem5 = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            clearStripMenuItem = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolStripContainer = new ToolStripContainer();
            splitContainer1 = new SplitContainer();
            lvEvents = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            lvEventsContextMenu = new ContextMenuStrip(components);
            editEventToolStripMenuItem = new ToolStripMenuItem();
            renameEventToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            删除DToolStripMenuItem = new ToolStripMenuItem();
            textBoxLogger = new TextBox();
            buttonPanel = new FlowLayoutPanel();
            btnRecord = new Button();
            btnPlay = new Button();
            btnStop = new Button();
            cbHideForm = new CheckBox();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            toolStripContainer.BottomToolStripPanel.SuspendLayout();
            toolStripContainer.ContentPanel.SuspendLayout();
            toolStripContainer.TopToolStripPanel.SuspendLayout();
            toolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            lvEventsContextMenu.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Dock = DockStyle.None;
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, ctrlToolStripMenuItem1, editToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.RenderMode = ToolStripRenderMode.System;
            menuStrip.Size = new Size(945, 25);
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
            newToolStripMenuItem.Size = new Size(206, 22);
            newToolStripMenuItem.Text = "新建(&N)";
            newToolStripMenuItem.Click += NewToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new Size(206, 22);
            openToolStripMenuItem.Text = "打开(&O)";
            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new Size(206, 22);
            saveToolStripMenuItem.Text = "保存(&S)";
            saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsToolStripMenuItem.Size = new Size(206, 22);
            saveAsToolStripMenuItem.Text = "另存为(&A)";
            saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(203, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitToolStripMenuItem.Size = new Size(206, 22);
            exitToolStripMenuItem.Text = "退出(&X)";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // ctrlToolStripMenuItem1
            // 
            ctrlToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem4, toolStripSeparator5, playToolStripMenuItem, stopToolStripMenuItem, toolStripSeparator2, playFromCursorToolStripMenuItem });
            ctrlToolStripMenuItem1.Name = "ctrlToolStripMenuItem1";
            ctrlToolStripMenuItem1.Size = new Size(60, 21);
            ctrlToolStripMenuItem1.Text = "控制(&C)";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.ShortcutKeys = Keys.F9;
            toolStripMenuItem4.Size = new Size(226, 22);
            toolStripMenuItem4.Text = "录制(&R)";
            toolStripMenuItem4.Click += BtnRecord_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(223, 6);
            // 
            // playToolStripMenuItem
            // 
            playToolStripMenuItem.Name = "playToolStripMenuItem";
            playToolStripMenuItem.ShortcutKeys = Keys.F10;
            playToolStripMenuItem.Size = new Size(226, 22);
            playToolStripMenuItem.Text = "播放(&P)";
            playToolStripMenuItem.Click += BtnPlay_Click;
            // 
            // stopToolStripMenuItem
            // 
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.ShortcutKeys = Keys.F11;
            stopToolStripMenuItem.Size = new Size(226, 22);
            stopToolStripMenuItem.Text = "停止(&S)";
            stopToolStripMenuItem.Click += BtnStop_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(223, 6);
            // 
            // playFromCursorToolStripMenuItem
            // 
            playFromCursorToolStripMenuItem.Name = "playFromCursorToolStripMenuItem";
            playFromCursorToolStripMenuItem.ShortcutKeys = Keys.F12;
            playFromCursorToolStripMenuItem.Size = new Size(226, 22);
            playFromCursorToolStripMenuItem.Text = "从活动事件开始播放(&L)";
            playFromCursorToolStripMenuItem.Click += PlayFromCursorToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { RenameEvent2ToolStripMenuItem, toolStripSeparator3, toolStripMenuItem1, DeleteToolStripMenuItem5, toolStripSeparator4, clearStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(59, 21);
            editToolStripMenuItem.Text = "编辑(&E)";
            // 
            // RenameEvent2ToolStripMenuItem
            // 
            RenameEvent2ToolStripMenuItem.Name = "RenameEvent2ToolStripMenuItem";
            RenameEvent2ToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            RenameEvent2ToolStripMenuItem.Size = new Size(197, 22);
            RenameEvent2ToolStripMenuItem.Text = "重命名事件(&R)";
            RenameEvent2ToolStripMenuItem.Click += RenameEventToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(194, 6);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem2, toolStripMenuItem3 });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(197, 22);
            toolStripMenuItem1.Text = "插入(&I)";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(117, 22);
            toolStripMenuItem2.Text = "延迟(&D)";
            toolStripMenuItem2.Click += InsertDelayToolStripMenuItem1_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(117, 22);
            toolStripMenuItem3.Text = "控制(&C)";
            toolStripMenuItem3.Click += InsertFcEventToolStripMenuItem_Click;
            // 
            // DeleteToolStripMenuItem5
            // 
            DeleteToolStripMenuItem5.Name = "DeleteToolStripMenuItem5";
            DeleteToolStripMenuItem5.ShortcutKeys = Keys.Delete;
            DeleteToolStripMenuItem5.Size = new Size(197, 22);
            DeleteToolStripMenuItem5.Text = "删除(&D)";
            DeleteToolStripMenuItem5.Click += DeleteToolStripMenuItem5_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(194, 6);
            // 
            // clearStripMenuItem
            // 
            clearStripMenuItem.Name = "clearStripMenuItem";
            clearStripMenuItem.Size = new Size(197, 22);
            clearStripMenuItem.Text = "清理(&C)";
            clearStripMenuItem.Click += ClearStripMenuItem_Click;
            // 
            // statusStrip
            // 
            statusStrip.Dock = DockStyle.None;
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 0);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(945, 22);
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
            toolStripContainer.ContentPanel.Controls.Add(splitContainer1);
            toolStripContainer.ContentPanel.Controls.Add(buttonPanel);
            toolStripContainer.ContentPanel.Margin = new Padding(4, 3, 4, 3);
            toolStripContainer.ContentPanel.Size = new Size(945, 550);
            toolStripContainer.Dock = DockStyle.Fill;
            toolStripContainer.Location = new Point(4, 0);
            toolStripContainer.Margin = new Padding(4, 3, 4, 3);
            toolStripContainer.Name = "toolStripContainer";
            toolStripContainer.Size = new Size(945, 597);
            toolStripContainer.TabIndex = 0;
            toolStripContainer.Text = "toolStripContainer";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            toolStripContainer.TopToolStripPanel.Controls.Add(menuStrip);
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 54);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lvEvents);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(textBoxLogger);
            splitContainer1.Size = new Size(945, 496);
            splitContainer1.SplitterDistance = 639;
            splitContainer1.TabIndex = 2;
            // 
            // lvEvents
            // 
            lvEvents.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            lvEvents.ContextMenuStrip = lvEventsContextMenu;
            lvEvents.Dock = DockStyle.Fill;
            lvEvents.FullRowSelect = true;
            lvEvents.Location = new Point(0, 0);
            lvEvents.Margin = new Padding(4, 3, 4, 3);
            lvEvents.Name = "lvEvents";
            lvEvents.Size = new Size(639, 496);
            lvEvents.TabIndex = 1;
            lvEvents.UseCompatibleStateImageBehavior = false;
            lvEvents.View = View.Details;
            lvEvents.ItemSelectionChanged += LvEvents_ItemSelectionChanged;
            lvEvents.Click += EventListView_Click;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "名称";
            columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "事件类型";
            columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "描述";
            columnHeader3.Width = 250;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "延迟(ms)";
            columnHeader4.Width = 100;
            // 
            // lvEventsContextMenu
            // 
            lvEventsContextMenu.Items.AddRange(new ToolStripItem[] { editEventToolStripMenuItem, renameEventToolStripMenuItem, toolStripSeparator6, 删除DToolStripMenuItem });
            lvEventsContextMenu.Name = "contextMenuStripEvents";
            lvEventsContextMenu.RenderMode = ToolStripRenderMode.System;
            lvEventsContextMenu.Size = new Size(181, 98);
            lvEventsContextMenu.Opening += ContextMenuStripEvents_Opening;
            // 
            // editEventToolStripMenuItem
            // 
            editEventToolStripMenuItem.Name = "editEventToolStripMenuItem";
            editEventToolStripMenuItem.Size = new Size(180, 22);
            editEventToolStripMenuItem.Text = "编辑(&E)";
            editEventToolStripMenuItem.Click += EditEventToolStripMenuItem_Click;
            // 
            // renameEventToolStripMenuItem
            // 
            renameEventToolStripMenuItem.Name = "renameEventToolStripMenuItem";
            renameEventToolStripMenuItem.Size = new Size(180, 22);
            renameEventToolStripMenuItem.Text = "重命名事件(&R)";
            renameEventToolStripMenuItem.Click += RenameEventToolStripMenuItem_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(177, 6);
            // 
            // 删除DToolStripMenuItem
            // 
            删除DToolStripMenuItem.Name = "删除DToolStripMenuItem";
            删除DToolStripMenuItem.Size = new Size(180, 22);
            删除DToolStripMenuItem.Text = "删除(&D)";
            删除DToolStripMenuItem.Click += DeleteToolStripMenuItem5_Click;
            // 
            // textBoxLogger
            // 
            textBoxLogger.BackColor = Color.Gainsboro;
            textBoxLogger.BorderStyle = BorderStyle.None;
            textBoxLogger.Dock = DockStyle.Fill;
            textBoxLogger.Location = new Point(0, 0);
            textBoxLogger.Multiline = true;
            textBoxLogger.Name = "textBoxLogger";
            textBoxLogger.ReadOnly = true;
            textBoxLogger.Size = new Size(302, 496);
            textBoxLogger.TabIndex = 0;
            // 
            // buttonPanel
            // 
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(btnRecord);
            buttonPanel.Controls.Add(btnPlay);
            buttonPanel.Controls.Add(btnStop);
            buttonPanel.Controls.Add(cbHideForm);
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Location = new Point(0, 0);
            buttonPanel.Margin = new Padding(4, 3, 4, 3);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(5);
            buttonPanel.Size = new Size(945, 54);
            buttonPanel.TabIndex = 0;
            // 
            // btnRecord
            // 
            btnRecord.Location = new Point(10, 10);
            btnRecord.Margin = new Padding(5);
            btnRecord.Name = "btnRecord";
            btnRecord.Size = new Size(91, 34);
            btnRecord.TabIndex = 0;
            btnRecord.Text = "录制 (F9)";
            btnRecord.UseVisualStyleBackColor = true;
            btnRecord.Click += BtnRecord_Click;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(111, 10);
            btnPlay.Margin = new Padding(5);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(91, 34);
            btnPlay.TabIndex = 1;
            btnPlay.Text = "播放 (F10)";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += BtnPlay_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(212, 10);
            btnStop.Margin = new Padding(5);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(91, 34);
            btnStop.TabIndex = 2;
            btnStop.Text = "停止 (F11)";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += BtnStop_Click;
            // 
            // cbHideForm
            // 
            cbHideForm.AutoSize = true;
            cbHideForm.Checked = true;
            cbHideForm.CheckState = CheckState.Checked;
            cbHideForm.Location = new Point(311, 8);
            cbHideForm.Name = "cbHideForm";
            cbHideForm.Padding = new Padding(0, 16, 0, 0);
            cbHideForm.Size = new Size(128, 37);
            cbHideForm.TabIndex = 3;
            cbHideForm.Text = "播放时隐藏窗体(&H)";
            cbHideForm.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(953, 601);
            Controls.Add(toolStripContainer);
            KeyPreview = true;
            MainMenuStrip = menuStrip;
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            Padding = new Padding(4, 0, 4, 4);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "自动化宏工具";
            Load += MainForm_Load;
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
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            lvEventsContextMenu.ResumeLayout(false);
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
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
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripContainer toolStripContainer;
        private ListView lvEvents;
        private ContextMenuStrip lvEventsContextMenu;
        private ToolStripMenuItem renameEventToolStripMenuItem;
        private ToolStripMenuItem editEventToolStripMenuItem;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private FlowLayoutPanel buttonPanel;
        private Button btnRecord;
        private Button btnPlay;
        private Button btnStop;
        private ToolStripMenuItem ctrlToolStripMenuItem1;
        private ToolStripMenuItem playToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem playFromCursorToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem clearStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem RenameEvent2ToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripSeparator toolStripSeparator5;
        private CheckBox cbHideForm;
        private ToolStripMenuItem DeleteToolStripMenuItem5;
        private SplitContainer splitContainer1;
        private TextBox textBoxLogger;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem 删除DToolStripMenuItem;
    }
}