using MacroCreator.Controller;
using MacroCreator.Models;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace MacroCreator.Forms;

public partial class MainForm : Form
{
    private readonly MacroController _controller;
    private EditFlowControlEventForm? _fcEventEditForm;
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
            Controller_StatusChanged("ERR! 播放过程中发生错误：" + ex.Message);
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
        if (_fcEventEditForm is null) return;

        if (ActiveEvent is null || !ActiveEvent.HasName) return;
        _fcEventEditForm.SetSelectedTarget(ActiveEvent);
    }

    private void LvEvents_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
        ActiveEvent = e.IsSelected ? e.Item?.Tag as RecordedEvent : null;
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
            Controller_StatusChanged("ERR! 加载文件时发生错误：" + ex.Message);
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
            Controller_StatusChanged("ERR! 保存文件时发生错误：" + ex.Message);
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
            Controller_StatusChanged("ERR! 保存文件时发生错误：" + ex.Message);
            MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InsertFcEventToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // 如果窗体已经存在且未关闭，则激活它
        if (_fcEventEditForm != null && !_fcEventEditForm.IsDisposed)
        {
            _fcEventEditForm.Activate();
            return;
        }

        _fcEventEditForm = new EditFlowControlEventForm($"FlowControl_{lvEvents.Items.Count}");
        _fcEventEditForm.ContainsEventName += ContainsEventWithName;

        // 订阅事件
        _fcEventEditForm.JumpEventCreated += (jumpEvent) =>
        {
            if (ActiveEvent is null)
                _controller.AddEvent(jumpEvent);
            else
                _controller.InsertEventBefore(ActiveEvent, jumpEvent);
        };

        _fcEventEditForm.FormClosed += (s, args) =>
        {
            _fcEventEditForm = null;
        };

        // 设置位置在主窗体右侧
        _fcEventEditForm.StartPosition = FormStartPosition.Manual;
        _fcEventEditForm.Location = new Point(
            this.Location.X + this.Width + 10,
            this.Location.Y
        );

        _fcEventEditForm.Show(this);
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
        using var inputDialog = new EditDelayEventForm(nameof(DelayEvent) + "_" + lvEvents.Items.Count);

        inputDialog.ContainsEventNameCallback += ContainsEventWithName;

        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;

        var ev = new DelayEvent()
        {
            DelayMilliseconds = (int)inputDialog.DelayMilliseconds,
            EventName = inputDialog.EventName,
        };

        if (ActiveEvent is null)
            _controller.AddEvent(ev);
        else
            _controller.InsertEventBefore(ActiveEvent, ev);
    }

    private void RenameEventToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (ActiveEvent is not RecordedEvent ev) return;

        var originalName = ev.EventName is null ? "匿名事件" : $"'{ev.EventName}'";

        // 创建输入对话框
        using var inputDialog = new RenameEventForm(ev.EventName ?? $"{ev.GetType().Name}_{_controller.IndexOfEvent(ev)}");
        inputDialog.ContainsEventNameCallback += ContainsEventWithName;

        if (inputDialog.ShowDialog() != DialogResult.OK) return;

        // 更新事件名称
        ev.EventName = inputDialog.EventName;

        RefreshEventList();
        Controller_StatusChanged(originalName + (string.IsNullOrWhiteSpace(ev.EventName) ? " 已设为匿名" : $" 已重命名为 '{ev.EventName}'"));
    }

    private void DeleteToolStripMenuItem5_Click(object sender, EventArgs e)
    {
        if (lvEvents.SelectedItems.Count == 0) return;
        var indices = new List<int>();

        foreach (ListViewItem item in lvEvents.SelectedItems)
            indices.Add(item.Index);

        _controller.DeleteEventsAt(indices);
    }

    private void ContextMenuStripEvents_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (lvEvents.SelectedItems.Count == 0)
        {
            e.Cancel = true;
            return;
        }
        if (lvEvents.SelectedItems.Count > 1)
        {
            renameEventToolStripMenuItem.Enabled = false;
            editEventToolStripMenuItem.Enabled = false;
        }
        else
        {
            renameEventToolStripMenuItem.Enabled = true;
            editEventToolStripMenuItem.Enabled = true;
        }
    }

    private void EditEventToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (lvEvents.SelectedItems.Count != 1) return;
        if (_fcEventEditForm is not null)
        {
            Controller_StatusChanged("请先关闭当前事件编辑窗口");
            MessageBox.Show(this, "请先关闭当前事件编辑窗口", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (ActiveEvent is not RecordedEvent ev) return;

        if (ev is FlowControlEvent fcEvent)
        {
            EditFlowControlEvent(fcEvent);
        }
        if (ev is DelayEvent delayEvent)
        {
            EditDelayEvent(delayEvent);
        }
        else
        {
            Controller_StatusChanged($"无法编辑 {ev.TypeName} 事件");
        }
    }

    #endregion

    #region Private Methods    

    private void EditDelayEvent(DelayEvent ev)
    {
        using var inputDialog = new EditDelayEventForm(ev);
        inputDialog.ContainsEventNameCallback += ContainsEventWithName;
        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;

        ev.DelayMilliseconds = (int)inputDialog.DelayMilliseconds;
        ev.EventName = inputDialog.EventName;

        RefreshEventList();
        Controller_StatusChanged($"'{ev.DisplayName}' 事件已更新");
    }

    private void EditFlowControlEvent(FlowControlEvent ev)
    {
        // 打开编辑窗体
        _fcEventEditForm = new EditFlowControlEventForm(ev);
        _fcEventEditForm.ContainsEventName += ContainsEventWithName;

        // 订阅事件更新
        _fcEventEditForm.JumpEventCreated += (updatedEvent) =>
        {
            // 替换原有事件 
            var replaced = _controller.ReplaceEvent(_controller.IndexOfEvent(ev), updatedEvent);
            // 保持原有的时间戳
            updatedEvent.TimeSinceLastEvent = ev.TimeSinceLastEvent;
            RefreshEventList();
            Controller_StatusChanged($"'{replaced.DisplayName}' 事件已更新");
        };

        _fcEventEditForm.FormClosed += (s, args) =>
        {
            _fcEventEditForm = null;
        };

        // 设置位置在主窗体右侧
        _fcEventEditForm.StartPosition = FormStartPosition.Manual;
        _fcEventEditForm.Location = new Point(
            this.Location.X + this.Width + 10,
            this.Location.Y
        );

        _fcEventEditForm.Show(this);
    }

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
        var selectedIndex = lvEvents.SelectedIndices.Count > 0 ? lvEvents.SelectedIndices[0] : -1;
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
                ev.TypeName,
                ev.GetDescription(),
                $"{ev.TimeSinceLastEvent:0.00}"
            ])
            {
                Tag = ev,
            };

            lvEvents.Items.Add(item);
        }
        lvEvents.EndUpdate();

        if (selectedIndex >= lvEvents.Items.Count)
            selectedIndex = lvEvents.Items.Count - 1;

        if (selectedIndex >= 0)
        {
            lvEvents.Items[selectedIndex].Selected = true;
            lvEvents.Items[selectedIndex].EnsureVisible();
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
        Text = $"MacroCreator - {fileName}";
    }

    #endregion

    public bool ContainsEventWithName(string eventName)
    {
        return _controller.EventSequence.Any(ev => string.Equals(ev.EventName, eventName, StringComparison.OrdinalIgnoreCase));
    }
}
