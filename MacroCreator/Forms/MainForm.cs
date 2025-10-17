using MacroCreator.Controller;
using MacroCreator.Models;

namespace MacroCreator.Forms;

public partial class MainForm : Form
{
    private readonly MacroController _controller;

    public MainForm()
    {
        _controller = new MacroController();
        InitializeComponent();

        // 订阅 Controller 事件
        _controller.StateChanged += OnAppStateChanged;
        _controller.EventSequenceChanged += RefreshEventList;
        _controller.StatusMessageChanged += (msg) => statusLabel!.Text = msg;

        UpdateTitle();
        OnAppStateChanged(AppState.Idle); // 设置初始UI状态
    }

    #region Event Handlers

    private void NewToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _controller.NewSequence();
    }

    private void BtnRecord_Click(object sender, EventArgs e)
    {
        _controller.StartRecording();
    }

    private async void BtnPlay_Click(object sender, EventArgs e)
    {
        await _controller.StartPlayback();
    }

    private void BtnStop_Click(object sender, EventArgs e)
    {
        _controller.Stop();
    }

    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Close();
    }

    private async void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F9:
                if (btnRecord.Enabled) _controller.StartRecording();
                break;
            case Keys.F10:
                if (btnPlay.Enabled) await _controller.StartPlayback();
                break;
            case Keys.F11:
                if (btnStop.Enabled) _controller.Stop();
                break;
        }
    }

    private void EventListView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && lvEvents.SelectedItems.Count > 0)
        {
            var indices = new List<int>();
            foreach (ListViewItem item in lvEvents.SelectedItems)
            {
                indices.Add(item.Index);
            }
            _controller.DeleteEventsAt(indices);
        }
    }

    private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*" };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try { _controller.LoadSequence(ofd.FileName); UpdateTitle(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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

    private void InsertJumpToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using (var dialog = new InsertJumpForm(_controller.EventSequence.Count))
        {
            if (dialog.ShowDialog() == DialogResult.OK && dialog.JumpEvent != null)
            {
                _controller.AddEvent(dialog.JumpEvent);
            }
        }
    }

    #endregion

    #region Private Methods

    private void RefreshEventList()
    {
        // BeginUpdate/EndUpdate 防止闪烁
        lvEvents.BeginUpdate();
        lvEvents.Items.Clear();
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
}
