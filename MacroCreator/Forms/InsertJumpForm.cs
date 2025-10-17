// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Forms;

/// <summary>
/// 用于插入跳转事件的窗体
/// </summary>
public partial class InsertJumpForm : Form
{
    public RecordedEvent? JumpEvent { get; private set; }

    private readonly int totalEventCount;
    private bool isSelectingTarget = false;
    private Action<RecordedEvent>? onTargetSelected;

    public event Action<RecordedEvent>? JumpEventCreated;

    public InsertJumpForm(int eventCount)
    {
        totalEventCount = eventCount;
        InitializeComponent();

        // 设置初始值
        cmbConditionType.SelectedIndex = 0;
        lblColorHex.Text = ColorTranslator.ToHtml(Color.Red);

        // 设置数值控件的最大值
        var maxValue = Math.Max(1, totalEventCount);
        nudTargetIndex.Maximum = maxValue;
    }

    private void RbJumpType_CheckedChanged(object sender, EventArgs e)
    {
        pnlUnconditional.Visible = rbUnconditionalJump.Checked;
        pnlConditional.Visible = rbConditionalJump.Checked;
    }

    private void CmbConditionType_SelectedIndexChanged(object sender, EventArgs e)
    {
        pnlPixelCondition.Visible = cmbConditionType.SelectedIndex == 0;
        pnlCustomCondition.Visible = cmbConditionType.SelectedIndex == 1;
    }

    private void ChkFalseTargetEnabled_CheckedChanged(object sender, EventArgs e)
    {
        txtFalseLabel.Enabled = chkFalseTargetEnabled.Checked;
        txtFalseFilePath.Enabled = chkFalseTargetEnabled.Checked;
        btnBrowseFalseFile.Enabled = chkFalseTargetEnabled.Checked;
        btnSelectFalseTarget.Enabled = chkFalseTargetEnabled.Checked;
    }

    private void BtnBrowseTrueFile_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "XML 文件 (*.xml)|*.xml" };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            txtTrueFilePath.Text = ofd.FileName;
        }
    }

    private void BtnBrowseFalseFile_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "XML 文件 (*.xml)|*.xml" };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            txtFalseFilePath.Text = ofd.FileName;
        }
    }

    private void InsertJumpForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F8 && rbConditionalJump.Checked && cmbConditionType.SelectedIndex == 0)
        {
            BtnPickColor_Click(sender, e);
        }
    }

    private void BtnPickColor_Click(object sender, EventArgs e)
    {
        Text = "移动鼠标到目标位置, 按下 Ctrl 键拾取颜色和坐标...";
        Cursor = Cursors.Cross;
        colorPickerTimer.Start();
    }

    private void ColorPickerTimer_Tick(object sender, EventArgs e)
    {
        Point pos = Cursor.Position;
        txtX.Text = pos.X.ToString();
        txtY.Text = pos.Y.ToString();

        try
        {
            Color color = Native.NativeMethods.GetPixelColor(pos);

            colorPanel.BackColor = color;
            lblColorHex.Text = ColorTranslator.ToHtml(color);
        }
        catch { }

        // 检测 Ctrl 键是否按下
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            colorPickerTimer.Stop();
            Cursor = Cursors.Default;
            Text = "插入跳转事件";
        }
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        if (rbBreak.Checked)
        {
            JumpEvent = new BreakEvent();
        }
        else if (rbUnconditionalJump.Checked)
        {
            // 验证目标事件名称（如果提供）
            string targetName = txtTargetLabel.Text.Trim();
            if (!RecordedEvent.IsValidEventName(targetName))
            {
                MessageBox.Show("事件名称只能包含英文字母和数字。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            JumpEvent = new JumpEvent
            {
                TargetEventName = targetName
            };
        }
        else if (rbConditionalJump.Checked)
        {
            // 验证目标事件名称（如果提供）
            string trueTargetName = txtTrueLabel.Text.Trim();

            if (string.IsNullOrWhiteSpace(trueTargetName) || !RecordedEvent.IsValidEventName(trueTargetName))
            {
                MessageBox.Show("分支事件名称只能包含英文字母和数字。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? falseTargetName = chkFalseTargetEnabled.Checked && !string.IsNullOrWhiteSpace(txtFalseLabel.Text)
                ? txtFalseLabel.Text.Trim()
                : null;

            if (falseTargetName != null && !RecordedEvent.IsValidEventName(falseTargetName))
            {
                MessageBox.Show("分支事件名称只能包含英文字母和数字。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conditionalJump = new ConditionalJumpEvent
            {
                TrueTargetEventName = trueTargetName,
                FalseTargetEventName = falseTargetName,
                FilePathIfMatch = string.IsNullOrWhiteSpace(txtTrueFilePath.Text) ? null : txtTrueFilePath.Text.Trim(),
                FilePathIfNotMatch = chkFalseTargetEnabled.Checked && !string.IsNullOrWhiteSpace(txtFalseFilePath.Text) ? txtFalseFilePath.Text.Trim() : null
            };

            if (cmbConditionType.SelectedIndex == 0) // 像素颜色检查
            {
                if (!int.TryParse(txtX.Text, out int x) || !int.TryParse(txtY.Text, out int y))
                {
                    MessageBox.Show("请输入有效的 X 和 Y 坐标。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                conditionalJump.ConditionType = ConditionType.PixelColor;
                conditionalJump.X = x;
                conditionalJump.Y = y;
                conditionalJump.ExpectedColorHex = lblColorHex.Text;
            }
            else // 自定义条件
            {
                if (string.IsNullOrWhiteSpace(txtCustomCondition.Text))
                {
                    MessageBox.Show("请输入自定义条件表达式。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                conditionalJump.ConditionType = ConditionType.Custom;
                conditionalJump.CustomCondition = txtCustomCondition.Text.Trim();
            }

            JumpEvent = conditionalJump;
        }

        if (JumpEvent is not null) JumpEventCreated?.Invoke(JumpEvent);
        Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 开始从主窗口选择目标索引
    /// </summary>
    public void StartSelectingTarget(Action<RecordedEvent> callback)
    {
        isSelectingTarget = true;
        onTargetSelected = callback;
        UpdateSelectionModeUI(true);
    }

    /// <summary>
    /// 从主窗口接收选中的目标索引
    /// </summary>
    public void SetSelectedTarget(RecordedEvent @event)
    {
        if (isSelectingTarget && onTargetSelected != null)
        {
            onTargetSelected(@event);
            isSelectingTarget = false;
            onTargetSelected = null;
            UpdateSelectionModeUI(false);
        }
    }

    /// <summary>
    /// 取消选择模式
    /// </summary>
    public void CancelSelectingTarget()
    {
        isSelectingTarget = false;
        onTargetSelected = null;
        UpdateSelectionModeUI(false);
    }

    private void UpdateSelectionModeUI(bool selecting)
    {
        if (selecting)
        {
            Text = "插入跳转事件 - 请在主窗口选择目标事件...";
            Opacity = 0.8;
        }
        else
        {
            Text = "插入跳转事件";
            Opacity = 1.0;
        }
    }

    private void BtnSelectTarget_Click(object sender, EventArgs e)
    {
        StartSelectingTarget((ev) =>
        {
            txtTargetLabel.Text = ev.EventName;
        });
    }

    private void BtnSelectTrueTarget_Click(object sender, EventArgs e)
    {
        StartSelectingTarget((ev) =>
        {
            txtTrueLabel.Text = ev.EventName;
        });
    }

    private void BtnSelectFalseTarget_Click(object sender, EventArgs e)
    {
        StartSelectingTarget((ev) =>
        {
            txtFalseLabel.Text = ev.EventName;
        });
    }

    private void InsertJumpForm_Load(object sender, EventArgs e)
    {

    }
}

public delegate void TargetSelectedHandler(RecordedEvent recordEvent);