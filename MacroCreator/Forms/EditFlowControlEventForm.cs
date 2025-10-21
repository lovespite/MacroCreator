// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Utils;

namespace MacroCreator.Forms;

/// <summary>
/// 用于插入跳转事件的窗体
/// </summary>
public partial class EditFlowControlEventForm : Form
{
    public MacroEvent? JumpEvent { get; private set; }

    private bool isSelectingTarget = false;
    private Action<MacroEvent>? onTargetSelected;
    private bool isEditMode = false;
    private string? originalEventName = null;

    public event ContainsEventNameDelegate? ContainsEventName;
    public event Action<MacroEvent>? JumpEventCreated;

    public string? EventName => string.IsNullOrWhiteSpace(textBoxEventName.Text) ? null : textBoxEventName.Text.Trim();

    /// <summary>
    /// 创建新跳转事件的构造函数
    /// </summary>
    public EditFlowControlEventForm(string? defaultEventName = null)
    {
        InitializeComponent();

        // 设置初始值
        textBoxEventName.Text = defaultEventName ?? string.Empty;
        cmbConditionType.SelectedIndex = 0;
        lblColorHex.Text = Color.Red.ExpressAsRgbColor();
        isEditMode = false;
    }

    /// <summary>
    /// 编辑现有跳转事件的构造函数
    /// </summary>
    public EditFlowControlEventForm(MacroEvent existingEvent) : this()
    {
        if (existingEvent is null)
            throw new ArgumentNullException(nameof(existingEvent));

        isEditMode = true;
        originalEventName = existingEvent.EventName;
        LoadEventData(existingEvent);
        Text = "编辑跳转事件";
    }

    private bool HasEventName(string name)
    {
        return ContainsEventName?.Invoke(name) ?? true;
    }

    /// <summary>
    /// 加载现有事件数据到表单
    /// </summary>
    private void LoadEventData(MacroEvent @event)
    {
        textBoxEventName.Text = @event.EventName ?? string.Empty;

        if (@event is BreakEvent)
        {
            rbBreak.Checked = true;
        }
        else if (@event is JumpEvent jumpEvent)
        {
            rbUnconditionalJump.Checked = true;
            txtTargetLabel.Text = jumpEvent.TargetEventName ?? string.Empty;
        }
        else if (@event is ConditionalJumpEvent conditionalJump)
        {
            rbConditionalJump.Checked = true;

            // 设置条件类型
            if (conditionalJump.ConditionType == ConditionType.PixelColor)
            {
                cmbConditionType.SelectedIndex = 0;
                txtX.Text = conditionalJump.X.ToString();
                txtY.Text = conditionalJump.Y.ToString();
                lblColorHex.Text = ColorHelper.ExpressAsRgbColor(Color.FromArgb(conditionalJump.ExpectedColor));
                nudColorTolerance.Value = conditionalJump.PixelTolerance;
            }
            else if (conditionalJump.ConditionType == ConditionType.CustomExpression)
            {
                cmbConditionType.SelectedIndex = 1;
                txtCustomCondition.Text = conditionalJump.CustomCondition ?? string.Empty;
            }

            // 设置真分支
            if (!string.IsNullOrEmpty(conditionalJump.TrueTargetFilePath))
            {
                rdTrueFilePath.Checked = true;
                txtTrueFilePath.Text = conditionalJump.TrueTargetFilePath;
            }
            else
            {
                rdTrueEventName.Checked = true;
                txtTrueTargetEventName.Text = conditionalJump.TrueTargetEventName ?? string.Empty;
            }

            // 设置假分支
            if (!string.IsNullOrEmpty(conditionalJump.FalseTargetEventName) || !string.IsNullOrEmpty(conditionalJump.FalseTargetFilePath))
            {
                chkFalseTargetEnabled.Checked = true;

                if (!string.IsNullOrEmpty(conditionalJump.FalseTargetFilePath))
                {
                    rdFalseFilePath.Checked = true;
                    txtFalseFilePath.Text = conditionalJump.FalseTargetFilePath;
                }
                else
                {
                    rdFalseEventName.Checked = true;
                    txtFalseTargetEventName.Text = conditionalJump.FalseTargetEventName ?? string.Empty;
                }
            }
        }
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
        var chk = chkFalseTargetEnabled.Checked;

        rdFalseEventName.Enabled = chk;
        rdFalseFilePath.Enabled = chk;

        txtFalseTargetEventName.Enabled = chk && rdFalseEventName.Checked;
        btnSelectFalseTarget.Enabled = chk && rdFalseEventName.Checked;
        txtFalseFilePath.Enabled = chk && rdFalseFilePath.Checked;
        btnBrowseFalseFile.Enabled = chk && rdFalseFilePath.Checked;

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
            lblColorHex.Text = color.ExpressAsRgbColor();
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
        // 验证事件名称：如果是编辑模式且名称未改变，或者名称为空，则跳过验证
        if (EventName is not null)
        {
            // 如果是编辑模式且名称未改变，允许保留原名称
            if (!(isEditMode && string.Equals(EventName, originalEventName, StringComparison.OrdinalIgnoreCase)))
            {
                if (HasEventName(EventName))
                {
                    MessageBox.Show(this, $"事件名称 '{EventName}' 已被使用，请选择一个唯一的名称。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        // 中断
        if (rbBreak.Checked)
        {
            JumpEvent = new BreakEvent();
        }

        // 无条件跳转
        else if (rbUnconditionalJump.Checked)
        {
            // 验证目标事件名称（如果提供）
            string targetName = txtTargetLabel.Text.Trim();
            if (!MacroEvent.IsValidEventName(targetName))
            {
                MessageBox.Show(this, "目标事件不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            JumpEvent = new JumpEvent
            {
                TargetEventName = targetName
            };
        }

        // 条件跳转
        else if (rbConditionalJump.Checked)
        {
            // 验证目标事件名称（如果提供）
            string tTargetEventName = txtTrueTargetEventName.Text.Trim();
            string? tTargetFilePath = rdTrueFilePath.Checked ? txtTrueFilePath.Text.Trim(): null;

            if (rdTrueEventName.Checked && !MacroEvent.IsValidEventName(tTargetEventName))
            {
                txtTrueTargetEventName.ForeColor = Color.Red;
                MessageBox.Show(this, "目标事件名称不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (rdTrueFilePath.Checked && !PathHelper.IsValidFilePath(tTargetFilePath))
            {
                txtTrueFilePath.ForeColor = Color.Red;
                MessageBox.Show(this, "请提供有效的文件路径", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? fTargetEventName =
                chkFalseTargetEnabled.Checked && rdFalseEventName.Checked && !string.IsNullOrWhiteSpace(txtFalseTargetEventName.Text)
                ? txtFalseTargetEventName.Text.Trim()
                : null;
            string? fTargetFilePath =
                chkFalseTargetEnabled.Checked && rdFalseFilePath.Checked && !string.IsNullOrWhiteSpace(txtFalseFilePath.Text)
                ? txtFalseFilePath.Text.Trim()
                : null;

            if (chkFalseTargetEnabled.Checked && rdFalseEventName.Checked && !MacroEvent.IsValidEventName(fTargetEventName))
            {
                txtFalseTargetEventName.ForeColor = Color.Red;
                MessageBox.Show(this, "目标事件不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (chkFalseTargetEnabled.Checked && rdTrueFilePath.Checked && !PathHelper.IsValidFilePath(fTargetFilePath))
            {
                txtFalseFilePath.ForeColor = Color.Red;
                MessageBox.Show(this, "请提供有效的文件路径", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conditionalJump = new ConditionalJumpEvent
            {
                TrueTargetEventName = tTargetEventName,
                TrueTargetFilePath = tTargetFilePath,
                FalseTargetEventName = fTargetEventName,
                FalseTargetFilePath = fTargetFilePath
            };

            if (cmbConditionType.SelectedIndex == 0) // 像素颜色检查
            {
                if (!int.TryParse(txtX.Text, out int x) || !int.TryParse(txtY.Text, out int y))
                {
                    MessageBox.Show(this, "请输入有效的 X 和 Y 坐标。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ColorHelper.TryParseExpressionColor(lblColorHex.Text, out Color c))
                {
                    MessageBox.Show(this, "请输入有效的颜色值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                conditionalJump.ConditionType = ConditionType.PixelColor;
                conditionalJump.X = x;
                conditionalJump.Y = y;
                conditionalJump.ExpectedColor = c.ToArgb();
                conditionalJump.PixelTolerance = (byte)nudColorTolerance.Value;
            }
            else // 自定义条件
            {
                if (string.IsNullOrWhiteSpace(txtCustomCondition.Text))
                {
                    MessageBox.Show(this, "请输入自定义条件表达式。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                conditionalJump.ConditionType = ConditionType.CustomExpression;
                conditionalJump.CustomCondition = txtCustomCondition.Text.Trim();
            }

            JumpEvent = conditionalJump;
        }

        if (JumpEvent is not null)
        {
            JumpEvent.EventName = EventName;
            JumpEventCreated?.Invoke(JumpEvent);
        }
        Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 开始从主窗口选择目标索引
    /// </summary>
    public void StartSelectingTarget(Action<MacroEvent> callback)
    {
        isSelectingTarget = true;
        onTargetSelected = callback;
        UpdateSelectionModeUI(true);
    }

    /// <summary>
    /// 从主窗口接收选中的目标索引
    /// </summary>
    public void SetSelectedTarget(MacroEvent @event)
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
            txtTrueTargetEventName.Text = ev.EventName;
        });
    }

    private void BtnSelectFalseTarget_Click(object sender, EventArgs e)
    {
        StartSelectingTarget((ev) =>
        {
            txtFalseTargetEventName.Text = ev.EventName;
        });
    }

    private void InsertJumpForm_Load(object sender, EventArgs e)
    {

    }

    private void RdTrueEventName_CheckedChanged(object sender, EventArgs e)
    {
        txtTrueTargetEventName.Enabled = rdTrueEventName.Checked;
        btnSelectTrueTarget.Enabled = rdTrueEventName.Checked;
    }

    private void RdTrueFilePath_CheckedChanged(object sender, EventArgs e)
    {
        txtTrueFilePath.Enabled = rdTrueFilePath.Checked;
        btnBrowseTrueFile.Enabled = rdTrueFilePath.Checked;
    }

    private void RdFalseEventName_CheckedChanged(object sender, EventArgs e)
    {
        txtFalseTargetEventName.Enabled = rdFalseEventName.Checked && chkFalseTargetEnabled.Checked;
        btnSelectFalseTarget.Enabled = rdFalseEventName.Checked && chkFalseTargetEnabled.Checked;
    }

    private void RdFalseFilePath_CheckedChanged(object sender, EventArgs e)
    {
        txtFalseFilePath.Enabled = rdFalseFilePath.Checked;
        btnBrowseFalseFile.Enabled = rdFalseFilePath.Checked;
    }

    private void LblColorHex_TextChanged(object sender, EventArgs e)
    {
        if (ColorHelper.TryParseExpressionColor(lblColorHex.Text, out Color color))
            colorPanel.BackColor = color;
        else
            colorPanel.BackColor = Color.Black;
    }
}

public delegate void TargetSelectedHandler(MacroEvent recordEvent);