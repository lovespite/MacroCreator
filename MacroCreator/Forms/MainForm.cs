// 命名空间定义了应用程序的入口点
using MacroCreator.Controller;
using MacroCreator;
using MacroCreator.Models;

namespace MacroCreator.Forms;

public partial class MainForm : Form
{
    private readonly MacroController _controller;

    // UI 控件
    private MenuStrip menuStrip;
    private ToolStripMenuItem fileToolStripMenuItem, newToolStripMenuItem, openToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, exitToolStripMenuItem;
    private ToolStripMenuItem editToolStripMenuItem, insertConditionToolStripMenuItem;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripContainer toolStripContainer;
    private FlowLayoutPanel buttonPanel;
    private Button recordButton, playButton, stopButton;
    private ListView eventListView;

    public MainForm()
    {
        _controller = new MacroController();
        InitializeComponent();
        Text = "自动化宏工具";
        Size = new Size(800, 600);

        // 订阅 Controller 事件
        _controller.StateChanged += OnAppStateChanged;
        _controller.EventSequenceChanged += RefreshEventList;
        _controller.StatusMessageChanged += (msg) => statusLabel.Text = msg;

        UpdateTitle();
        OnAppStateChanged(AppState.Idle); // 设置初始UI状态
    }

    private void InitializeComponent()
    {
        // 初始化控件
        menuStrip = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem("文件(&F)");
        newToolStripMenuItem = new ToolStripMenuItem("新建(&N)");
        openToolStripMenuItem = new ToolStripMenuItem("打开(&O)...");
        saveToolStripMenuItem = new ToolStripMenuItem("保存(&S)");
        saveAsToolStripMenuItem = new ToolStripMenuItem("另存为(&A)...");
        exitToolStripMenuItem = new ToolStripMenuItem("退出(&X)");

        editToolStripMenuItem = new ToolStripMenuItem("编辑(&E)");
        insertConditionToolStripMenuItem = new ToolStripMenuItem("插入条件判断(&I)...");

        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("准备就绪");
        toolStripContainer = new ToolStripContainer();
        buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(5) };
        recordButton = new Button { Text = "录制 (F9)", Width = 120, Height = 40, Margin = new Padding(5) };
        playButton = new Button { Text = "播放 (F10)", Width = 120, Height = 40, Margin = new Padding(5) };
        stopButton = new Button { Text = "停止 (F11)", Width = 120, Height = 40, Margin = new Padding(5), Enabled = false };
        eventListView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };

        // 配置 ListView
        eventListView.Columns.Add("ID", 50, HorizontalAlignment.Left);
        eventListView.Columns.Add("事件类型", 150, HorizontalAlignment.Left);
        eventListView.Columns.Add("描述", 450, HorizontalAlignment.Left);
        eventListView.Columns.Add("延迟(ms)", 100, HorizontalAlignment.Left);

        // 组装菜单
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, new ToolStripSeparator(), exitToolStripMenuItem });
        editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { insertConditionToolStripMenuItem });
        menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem });

        // 组装状态栏
        statusStrip.Items.Add(statusLabel);

        // 组装按钮
        buttonPanel.Controls.AddRange(new Control[] { recordButton, playButton, stopButton });

        // 组装主容器
        toolStripContainer.ContentPanel.Controls.Add(eventListView);
        toolStripContainer.ContentPanel.Controls.Add(buttonPanel);
        toolStripContainer.TopToolStripPanel.Controls.Add(menuStrip);
        toolStripContainer.BottomToolStripPanel.Controls.Add(statusStrip);
        toolStripContainer.Dock = DockStyle.Fill;
        Controls.Add(toolStripContainer);

        // 注册事件
        newToolStripMenuItem.Click += (s, e) => _controller.NewSequence();
        openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
        saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
        saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
        exitToolStripMenuItem.Click += (s, e) => Close();
        insertConditionToolStripMenuItem.Click += InsertConditionToolStripMenuItem_Click;

        recordButton.Click += (s, e) => _controller.StartRecording();
        playButton.Click += async (s, e) => await _controller.StartPlayback();
        stopButton.Click += (s, e) => _controller.Stop();
        eventListView.KeyDown += EventListView_KeyDown;

        // 快捷键
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
    }

    private async void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F9:
                if (recordButton.Enabled) _controller.StartRecording();
                break;
            case Keys.F10:
                if (playButton.Enabled) await _controller.StartPlayback();
                break;
            case Keys.F11:
                if (stopButton.Enabled) _controller.Stop();
                break;
        }
    }

    private void EventListView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && eventListView.SelectedItems.Count > 0)
        {
            var indices = new List<int>();
            foreach (ListViewItem item in eventListView.SelectedItems)
            {
                indices.Add(item.Index);
            }
            _controller.DeleteEventsAt(indices);
        }
    }

    private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using (var ofd = new OpenFileDialog { Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*" })
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try { _controller.LoadSequence(ofd.FileName); UpdateTitle(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }

    private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_controller.CurrentFilePath))
        {
            SaveAsToolStripMenuItem_Click(sender, e);
        }
        else
        {
            try { _controller.SaveSequence(); UpdateTitle(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }

    private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using (var sfd = new SaveFileDialog { Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*" })
        {
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try { _controller.SaveSequence(sfd.FileName); UpdateTitle(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }

    private void InsertConditionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using (var dialog = new InsertConditionForm())
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _controller.AddEvent(dialog.ConditionEvent);
            }
        }
    }

    private void RefreshEventList()
    {
        // BeginUpdate/EndUpdate 防止闪烁
        eventListView.BeginUpdate();
        eventListView.Items.Clear();
        var eventSequence = _controller.EventSequence;
        for (int i = 0; i < eventSequence.Count; i++)
        {
            var ev = eventSequence[i];
            var item = new ListViewItem(new[] {
                (i + 1).ToString(),
                ev.GetType().Name,
                ev.GetDescription(),
                ev.TimeSinceLastEvent.ToString()
            });
            eventListView.Items.Add(item);
        }
        eventListView.EndUpdate();
        if (eventListView.Items.Count > 0)
        {
            eventListView.EnsureVisible(eventListView.Items.Count - 1);
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        bool isIdle = state == AppState.Idle;
        recordButton.Enabled = isIdle;
        playButton.Enabled = isIdle;
        stopButton.Enabled = !isIdle;
        menuStrip.Enabled = isIdle;
    }

    private void UpdateTitle()
    {
        string fileName = string.IsNullOrEmpty(_controller.CurrentFilePath) ? "未命名" : Path.GetFileName(_controller.CurrentFilePath);
        Text = $"自动化宏工具 - {fileName}";
    }
}
