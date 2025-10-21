using MacroCreator.Controller;
using MacroCreator.Models;

namespace MacroCreator.Forms;

public partial class MainForm : Form
{
    private readonly MacroController _controller;
    private InsertJumpForm? _insertJumpForm;
    private RecordedEvent? _activeEvent;

    public RecordedEvent? ActiveEvent
    {
        get => _activeEvent;
        private set
        {
            _activeEvent = value;
            playFromCursorToolStripMenuItem.Enabled = _activeEvent is not null;
        }
    }

    public MainForm()
    {
        _controller = new MacroController();
        InitializeComponent();

        ActiveEvent = null;

        // 订阅 Controller 事件
        _controller.StateChanged += OnAppStateChanged;
        _controller.EventSequenceChanged += RefreshEventList;
        _controller.StatusMessageChanged += Controller_StatusChanged;

        UpdateTitle();
        OnAppStateChanged(AppState.Idle); // 设置初始UI状态
    }

    #region Event Handlers 

    private void Controller_StatusChanged(string message)
    {
        statusLabel.Text = message;
        textBoxLogger.Text += message + Environment.NewLine;
        textBoxLogger.SelectionStart = textBoxLogger.Text.Length;
        textBoxLogger.ScrollToCaret();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        InstallGlobalHotkeys();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        UninstallGlobalHotkeys();
        base.OnFormClosed(e);
    }

    protected override void WndProc(ref Message m)
    {
        HandleGlobalHotkey(m);
        base.WndProc(ref m);
    }

    private void NewToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _controller.NewSequence();
        UpdateTitle();
    }

    private void BtnRecord_Click(object sender, EventArgs e)
    {
        _controller.StartRecording();
    }

    private async void BtnPlay_Click(object sender, EventArgs e)
    {
        try
        {
            if (cbHideForm.Checked) Hide();
            await _controller.StartPlayback();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Show();
        }
    }

    private void PlayFromCursorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (ActiveEvent is null) return;
    }

    private void BtnStop_Click(object sender, EventArgs e)
    {
        _controller.Stop();
    }

    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void EventListView_Click(object sender, EventArgs e)
    {
        ActiveEvent = null;
        if (lvEvents.SelectedItems.Count == 0) return;
        if (lvEvents.SelectedItems[0].Tag is not RecordedEvent ev) return;

        ActiveEvent = ev;

        // 如果 InsertJumpForm 处于选择模式，处理选择
        if (_insertJumpForm is not null)
        {
            if (!ActiveEvent.HasName) return;
            _insertJumpForm.SetSelectedTarget(ActiveEvent);
            return;
        }
    }

    private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            _controller.LoadSequence(ofd.FileName);
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_controller.CurrentFilePath))
        {
            SaveAsToolStripMenuItem_Click(sender, e);
            return;
        }

        try
        {
            _controller.SaveSequence();
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog { Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*" };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            _controller.SaveSequence(sfd.FileName);
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InsertJumpToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // 如果窗体已经存在且未关闭，则激活它
        if (_insertJumpForm != null && !_insertJumpForm.IsDisposed)
        {
            _insertJumpForm.Activate();
            return;
        }

        _insertJumpForm = new InsertJumpForm($"FlowControl_{lvEvents.Items.Count}");
        _insertJumpForm.ContainsEventName += ContainsEventWithName;

        // 订阅事件
        _insertJumpForm.JumpEventCreated += (jumpEvent) =>
        {
            _controller.AddEvent(jumpEvent);
        };

        _insertJumpForm.FormClosed += (s, args) =>
        {
            _insertJumpForm = null;
        };

        // 设置位置在主窗体右侧
        _insertJumpForm.StartPosition = FormStartPosition.Manual;
        _insertJumpForm.Location = new Point(
            this.Location.X + this.Width + 10,
            this.Location.Y
        );

        _insertJumpForm.Show(this);
    }

    private void ClearStripMenuItem_Click(object sender, EventArgs e)
    {
        _controller.ClearSequence();
    }

    private void LvEvents_ItemActivate(object sender, EventArgs e)
    {
    }

    private void InsertDelayToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        using var inputDialog = new InsertDelayForm(nameof(DelayEvent) + "_" + lvEvents.Items.Count);

        inputDialog.ContainsEventNameCallback += ContainsEventWithName;

        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;

        var ev = new DelayEvent()
        {
            DelayMilliseconds = (int)inputDialog.DelayMilliseconds,
            EventName = inputDialog.EventName,
        };

        _controller.AddEvent(ev);
    }

    private void RenameEventToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (lvEvents.SelectedItems.Count != 1)
            return;

        int selectedIndex = lvEvents.SelectedItems[0].Index;
        var ev = _controller.EventSequence[selectedIndex];

        // 创建输入对话框
        using var inputDialog = new RenameEventForm(ev.EventName ?? $"{ev.GetType().Name}_{lvEvents.Items.Count}");
        inputDialog.ContainsEventNameCallback += ContainsEventWithName;

        if (inputDialog.ShowDialog() != DialogResult.OK) return;

        var newName = inputDialog.EventName;

        // 验证名称
        if (!string.IsNullOrWhiteSpace(newName))
        {
            if (!RecordedEvent.IsValidEventName(newName))
            {
                MessageBox.Show("事件名称不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查名称是否已存在
            for (int i = 0; i < _controller.EventSequence.Count; i++)
            {
                if (i != selectedIndex && _controller.EventSequence[i].EventName == newName)
                {
                    MessageBox.Show($"事件名称 '{newName}' 已被其他事件使用。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        // 更新事件名称
        ev.EventName = string.IsNullOrWhiteSpace(newName) ? null : newName;
        RefreshEventList();

        Controller_StatusChanged(string.IsNullOrWhiteSpace(newName)
            ? "事件已设为匿名。"
            : $"事件已重命名为 '{newName}'。");
    }

    private void DeleteToolStripMenuItem5_Click(object sender, EventArgs e)
    {
        if (lvEvents.SelectedItems.Count == 0) return;
        var indices = new List<int>();

        foreach (ListViewItem item in lvEvents.SelectedItems)
            indices.Add(item.Index);

        _controller.DeleteEventsAt(indices);
    }

    #endregion

    #region Private Methods

    private void InstallGlobalHotkeys()
    {
        var handle = this.Handle;
        Native.NativeMethods.InstallHotKey(handle, Native.NativeMethods.HOTKEY_ID_RECORD,
            Native.NativeMethods.ModifierKeys.Control | Native.NativeMethods.ModifierKeys.Shift,
            Keys.F9);
        Native.NativeMethods.InstallHotKey(handle, Native.NativeMethods.HOTKEY_ID_PLAYBACK,
            Native.NativeMethods.ModifierKeys.Control | Native.NativeMethods.ModifierKeys.Shift,
            Keys.F10);
        Native.NativeMethods.InstallHotKey(handle, Native.NativeMethods.HOTKEY_ID_STOP,
            Native.NativeMethods.ModifierKeys.Control | Native.NativeMethods.ModifierKeys.Shift,
            Keys.F11);
    }

    private void UninstallGlobalHotkeys()
    {
        var handle = this.Handle;
        Native.NativeMethods.UninstallHotKey(handle, Native.NativeMethods.HOTKEY_ID_RECORD);
        Native.NativeMethods.UninstallHotKey(handle, Native.NativeMethods.HOTKEY_ID_PLAYBACK);
        Native.NativeMethods.UninstallHotKey(handle, Native.NativeMethods.HOTKEY_ID_STOP);
    }

    private void HandleGlobalHotkey(Message m)
    {
        if (m.Msg != Native.NativeMethods.WM_HOTKEY)
            return;
        int id = m.WParam.ToInt32();
        switch (id)
        {
            case Native.NativeMethods.HOTKEY_ID_RECORD:
                if (btnRecord.Enabled)
                    BtnRecord_Click(this, EventArgs.Empty);
                break;
            case Native.NativeMethods.HOTKEY_ID_PLAYBACK:
                if (btnPlay.Enabled)
                    BtnPlay_Click(this, EventArgs.Empty);
                break;
            case Native.NativeMethods.HOTKEY_ID_STOP:
                if (btnStop.Enabled)
                    BtnStop_Click(this, EventArgs.Empty);
                break;
        }
    }

    private void RefreshEventList()
    {
        // BeginUpdate/EndUpdate 防止闪烁
        lvEvents.BeginUpdate();
        lvEvents.Items.Clear();
        var eventSequence = _controller.EventSequence;
        for (int i = 0; i < eventSequence.Count; i++)
        {
            var ev = eventSequence[i];

            // 如果事件有名称，在序号后显示名称
            string indexDisplay = string.IsNullOrWhiteSpace(ev.EventName)
                ? "(匿名)"
                : $"{ev.EventName}";

            var item = new ListViewItem([
                indexDisplay,
                ev.GetType().Name,
                ev.GetDescription(),
                $"{ev.TimeSinceLastEvent:0.00}"
            ])
            {
                Tag = ev,
            };

            lvEvents.Items.Add(item);
        }
        lvEvents.EndUpdate();
        if (lvEvents.Items.Count > 0)
        {
            lvEvents.EnsureVisible(lvEvents.Items.Count - 1);
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        bool isIdle = state == AppState.Idle;

        lvEvents.Visible = state != AppState.Recording;

        btnPlay.Enabled = isIdle;
        btnStop.Enabled = !isIdle;
        btnRecord.Enabled = isIdle;

        menuStrip.Enabled = isIdle;
    }

    private void UpdateTitle()
    {
        string fileName = string.IsNullOrEmpty(_controller.CurrentFilePath) ? "未命名" : Path.GetFileName(_controller.CurrentFilePath);
        Text = $"自动化宏工具 - {fileName}";
    }

    #endregion

    public bool ContainsEventWithName(string eventName)
    {
        return _controller.EventSequence.Any(ev => string.Equals(ev.EventName, eventName, StringComparison.OrdinalIgnoreCase));
    }
}
