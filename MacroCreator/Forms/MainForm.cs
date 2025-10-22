using MacroCreator.Controller;
using MacroCreator.Models;

namespace MacroCreator.Forms;

public partial class MainForm : Form
{
    private readonly MacroController _controller;
    private EditFlowControlEventForm? _fcEventEditForm;
    private MacroEvent? _activeEvent;

    public MacroEvent? ActiveEvent
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
        _controller.EventSequenceChanged += OnEventSequenceChanged;
        _controller.StatusMessageChanged += Controller_StatusChanged;

        UpdateTitle();
        OnAppStateChanged(AppState.Idle); // 设置初始UI状态
    }

    public bool ContainsEventWithName(string eventName, MacroEvent? except = null)
    {
        foreach (var e in _controller.EventSequence)
        {
            if (except is not null && ReferenceEquals(e, except))
                continue;

            if (string.Equals(e.EventName, eventName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #region Private Methods    

    private void EditDelayEvent(DelayEvent ev)
    {
        using var inputDialog = new EditDelayEventForm(ev);
        inputDialog.ContainsEventName += ContainsEventWithName;
        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;

        ev.DelayMilliseconds = (int)inputDialog.DelayMilliseconds;
        ev.EventName = inputDialog.EventName;

        // 直接更新显示，不需要完全刷新
        var index = _controller.IndexOfEvent(ev);
        if (index >= 0 && index < lvEvents.Items.Count)
        {
            UpdateListViewItem(lvEvents.Items[index], ev);
        }
        Controller_StatusChanged($"'{ev.DisplayName}' 事件已更新");
    }

    private void EditMouseEvent(MouseEvent ev)
    {
        using var inputDialog = new EditMouseEventForm(ev);
        inputDialog.ContainsEventName += ContainsEventWithName;
        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;

        ev.X = inputDialog.MouseX;
        ev.Y = inputDialog.MouseY;
        ev.Action = inputDialog.MouseAction;
        ev.WheelDelta = inputDialog.WheelDelta;
        ev.EventName = inputDialog.EventName;
        ev.TimeSinceLastEvent = inputDialog.DelayMilliseconds;

        // 直接更新显示，不需要完全刷新
        var index = _controller.IndexOfEvent(ev);
        if (index >= 0 && index < lvEvents.Items.Count)
        {
            UpdateListViewItem(lvEvents.Items[index], ev);
        }
        Controller_StatusChanged($"'{ev.DisplayName}' 事件已更新");
    }

    private void InsertMouseEvent()
    {
        using var inputDialog = new EditMouseEventForm($"MouseEvent_{lvEvents.Items.Count}");
        inputDialog.ContainsEventName += ContainsEventWithName;
        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;
        var ev = new MouseEvent()
        {
            X = inputDialog.MouseX,
            Y = inputDialog.MouseY,
            Action = inputDialog.MouseAction,
            WheelDelta = inputDialog.WheelDelta,
            EventName = inputDialog.EventName,
        };
        if (ActiveEvent is null)
            _controller.AddEvent(ev);
        else
            _controller.InsertEventBefore(ActiveEvent, ev);
        Controller_StatusChanged($"'{ev.DisplayName}' 事件已创建");
    }

    private void EditKeyboardEvent(KeyboardEvent ev)
    {
        using var inputDialog = new EditKeyboardEventForm(ev);
        inputDialog.ContainsEventName += ContainsEventWithName;
        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;
        ev.Key = inputDialog.Key;
        ev.Action = inputDialog.KeyboardAction;
        ev.EventName = inputDialog.EventName;
        ev.TimeSinceLastEvent = inputDialog.DelayMilliseconds;
        // 直接更新显示，不需要完全刷新
        var index = _controller.IndexOfEvent(ev);
        if (index >= 0 && index < lvEvents.Items.Count)
        {
            UpdateListViewItem(lvEvents.Items[index], ev);
        }
        Controller_StatusChanged($"'{ev.DisplayName}' 事件已更新");
    }

    private void InsertKeyboardEvent()
    {
        using var inputDialog = new EditKeyboardEventForm($"KeyboardEvent_{lvEvents.Items.Count}");
        inputDialog.ContainsEventName += ContainsEventWithName;
        var ret = inputDialog.ShowDialog(this);
        if (ret != DialogResult.OK) return;
        var ev = new KeyboardEvent()
        {
            Key = inputDialog.Key,
            Action = inputDialog.KeyboardAction,
            EventName = inputDialog.EventName,
        };
        if (ActiveEvent is null)
            _controller.AddEvent(ev);
        else
            _controller.InsertEventBefore(ActiveEvent, ev);
        Controller_StatusChanged($"'{ev.DisplayName}' 事件已创建");
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
            // ReplaceEvent 会自动触发 EventSequenceChanged 事件，不需要手动刷新
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

    /// <summary>
    /// 响应事件序列变更，执行增量更新
    /// </summary>
    private void OnEventSequenceChanged(EventSequenceChangeArgs args)
    {
        lvEvents.BeginUpdate();
        try
        {
            switch (args.ChangeType)
            {
                case EventSequenceChangeType.Add:
                    // 添加到末尾
                    if (args.Event != null && args.Index.HasValue)
                    {
                        var newItem = CreateListViewItem(args.Event);
                        lvEvents.Items.Add(newItem);
                    }
                    break;

                case EventSequenceChangeType.Insert:
                    // 在指定位置插入
                    if (args.Event != null && args.Index.HasValue)
                    {
                        var insertItem = CreateListViewItem(args.Event);
                        lvEvents.Items.Insert(args.Index.Value, insertItem);
                    }
                    break;

                case EventSequenceChangeType.Delete:
                    // 删除事件（批量）
                    if (args.Indices != null)
                    {
                        // 从后往前删除，避免索引变化
                        foreach (var idx in args.Indices.OrderByDescending(i => i))
                        {
                            if (idx >= 0 && idx < lvEvents.Items.Count)
                            {
                                lvEvents.Items.RemoveAt(idx);
                            }
                        }
                    }
                    break;

                case EventSequenceChangeType.Replace:
                    // 替换指定位置的事件
                    if (args.Event != null && args.Index.HasValue && args.Index.Value < lvEvents.Items.Count)
                    {
                        var replaceItem = CreateListViewItem(args.Event);
                        var oldSelected = lvEvents.Items[args.Index.Value].Selected;
                        lvEvents.Items[args.Index.Value] = replaceItem;
                        replaceItem.Selected = oldSelected;
                    }
                    break;

                case EventSequenceChangeType.Update:
                    // 更新指定位置的事件显示
                    if (args.Event != null && args.Index.HasValue && args.Index.Value < lvEvents.Items.Count)
                    {
                        UpdateListViewItem(lvEvents.Items[args.Index.Value], args.Event);
                    }
                    break;

                case EventSequenceChangeType.Clear:
                    // 清空列表
                    lvEvents.Items.Clear();
                    break;

                case EventSequenceChangeType.FullRefresh:
                    // 完全刷新（用于加载文件等场景）
                    RefreshEventListFull();
                    break;
            }
        }
        finally
        {
            lvEvents.EndUpdate();
        }
    }

    /// <summary>
    /// 完全刷新事件列表（用于加载文件等场景）
    /// </summary>
    private void RefreshEventListFull()
    {
        var selectedIndex = lvEvents.SelectedIndices.Count > 0 ? lvEvents.SelectedIndices[0] : -1;
        lvEvents.Items.Clear();
        var eventSequence = _controller.EventSequence;

        foreach (var ev in eventSequence)
        {
            var item = CreateListViewItem(ev);
            lvEvents.Items.Add(item);
        }

        if (selectedIndex >= lvEvents.Items.Count)
            selectedIndex = lvEvents.Items.Count - 1;

        if (selectedIndex >= 0)
        {
            lvEvents.Items[selectedIndex].Selected = true;
            lvEvents.Items[selectedIndex].EnsureVisible();
        }
    }

    /// <summary>
    /// 为完全刷新保留的旧方法（现在调用FullRefresh）
    /// </summary>
    private void RefreshEventList()
    {
        lvEvents.BeginUpdate();
        RefreshEventListFull();
        lvEvents.EndUpdate();
    }

    /// <summary>
    /// 创建ListViewItem
    /// </summary>
    private ListViewItem CreateListViewItem(MacroEvent ev)
    {
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

        return item;
    }

    /// <summary>
    /// 更新现有ListViewItem的显示内容
    /// </summary>
    private void UpdateListViewItem(ListViewItem item, MacroEvent ev)
    {
        string indexDisplay = string.IsNullOrWhiteSpace(ev.EventName)
            ? "(匿名)"
            : $"{ev.EventName}";

        item.SubItems[0].Text = indexDisplay;
        item.SubItems[1].Text = ev.TypeName;
        item.SubItems[2].Text = ev.GetDescription();
        item.SubItems[3].Text = $"{ev.TimeSinceLastEvent:0.00}";
        item.Tag = ev;
    }

    private void OnAppStateChanged(AppState state)
    {
        bool isIdle = state == AppState.Idle;

        lvEvents.Visible = state != AppState.Recording;
        lvEvents.Enabled = state != AppState.Playing;

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
        catch (EventFlowControlException ex)
        {
            Controller_StatusChanged($"ERR! 播放在 {ex.EventIndex} ({ex.Event.DisplayName}) 中断：{ex.Message}");
        }
        catch (EventPlayerException ex)
        {
            Controller_StatusChanged($"ERR! 播放在 {ex.EventIndex} ({ex.Event.DisplayName}) 中断：{ex.Message}");
        }
        catch (Exception ex)
        {
            Controller_StatusChanged($"ERR! 未知错误：{ex.Message}");
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
        ActiveEvent = e.IsSelected ? e.Item?.Tag as MacroEvent : null;
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

    private void ClearStripMenuItem_Click(object sender, EventArgs e)
    {
        _controller.ClearSequence();
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

            Controller_StatusChanged($"'{jumpEvent.DisplayName}' 事件已创建");
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
    private void InsertDelayToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        using var inputDialog = new EditDelayEventForm(nameof(DelayEvent) + "_" + lvEvents.Items.Count);

        inputDialog.ContainsEventName += ContainsEventWithName;

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

        Controller_StatusChanged($"'{ev.DisplayName}' 事件已创建");
    }

    private void InsertMouseEventToolStripMenuItem_Click(object sender, EventArgs e)
    {
        InsertMouseEvent();
    }

    private void InsertKbdEventToolStripMenuItem5_Click(object sender, EventArgs e)
    {
        InsertKeyboardEvent();
    }

    private void RenameEventToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (ActiveEvent is not MacroEvent ev) return;

        var originalName = ev.EventName is null ? "匿名事件" : $"'{ev.EventName}'";

        // 创建输入对话框
        using var inputDialog = new RenameEventForm(ev);
        inputDialog.ContainsEventName += ContainsEventWithName;

        if (inputDialog.ShowDialog() != DialogResult.OK) return;

        // 更新事件名称
        ev.EventName = inputDialog.EventName;

        // 直接更新显示，不需要完全刷新
        var index = _controller.IndexOfEvent(ev);
        if (index >= 0 && index < lvEvents.Items.Count)
        {
            UpdateListViewItem(lvEvents.Items[index], ev);
        }
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
        if (ActiveEvent is not MacroEvent ev) return;

        if (ev is FlowControlEvent fcEvent)
        {
            EditFlowControlEvent(fcEvent);
        }
        else if (ev is DelayEvent delayEvent)
        {
            EditDelayEvent(delayEvent);
        }
        else if (ev is MouseEvent mouseEvent)
        {
            EditMouseEvent(mouseEvent);
        }
        else if (ev is KeyboardEvent keyboardEvent)
        {
            EditKeyboardEvent(keyboardEvent);
        }
        else
        {
            Controller_StatusChanged($"无法编辑 {ev.TypeName} 事件");
        }
    }

    #endregion

}
