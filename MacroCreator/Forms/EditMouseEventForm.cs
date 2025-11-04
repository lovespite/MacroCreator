using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Utils;

namespace MacroCreator.Forms;

public partial class EditMouseEventForm : Form
{
    public int MouseX => Action.IsMoveAction() ? (int)numericUpDownX.Value : 0;
    public int MouseY => Action.IsMoveAction() ? (int)numericUpDownY.Value : 0;
    public int WheelDelta => (int)numericUpDownWheelDelta.Value;
    public MouseAction Action => (MouseAction)comboBoxAction.SelectedIndex;
    public string? EventName => string.IsNullOrWhiteSpace(textBoxEventName.Text) ? null : textBoxEventName.Text.Trim();

    public bool CreatePairedEvent => _editing is null && Action.IsDownAction() && chkPairedEvents.Checked;

    private readonly MouseEvent? _editing = null;

    public double DelayMilliseconds
    {
        get
        {
            double multiplier = comboBoxDelayUnit.SelectedIndex switch
            {
                0 => 1.0,          // 毫秒
                1 => 1000.0,       // 秒
                2 => 60000.0,      // 分钟
                _ => 1.0,
            };
            return (double)numericUpDownDelay.Value * multiplier;
        }
    }

    public event ContainsEventNameDelegate? ContainsEventName;

    public EditMouseEventForm(string? defaultEvName = null)
    {
        InitializeComponent();
        textBoxEventName.Text = defaultEvName ?? string.Empty;
    }

    public EditMouseEventForm(MouseEvent mouseEvent) : this(mouseEvent.EventName)
    {
        ArgumentNullException.ThrowIfNull(mouseEvent);
        _editing = mouseEvent;

        textBoxEventName.Text = mouseEvent.EventName ?? $"{mouseEvent.TypeName}_{Rnd.GetString(5)}";

        numericUpDownX.Value = mouseEvent.X;
        numericUpDownY.Value = mouseEvent.Y;
        numericUpDownWheelDelta.Value = mouseEvent.WheelDelta;
        comboBoxAction.SelectedIndex = (int)mouseEvent.Action;

        // 设置延迟时间
        double delayMs = mouseEvent.TimeSinceLastEvent;
        if (delayMs >= 60000.0 && delayMs % 60000.0 == 0)
        {
            comboBoxDelayUnit.SelectedIndex = 2; // 分钟
            numericUpDownDelay.Value = (decimal)(delayMs / 60000.0);
        }
        else if (delayMs >= 1000.0 && delayMs % 1000.0 == 0)
        {
            comboBoxDelayUnit.SelectedIndex = 1; // 秒
            numericUpDownDelay.Value = (decimal)(delayMs / 1000.0);
        }
        else
        {
            comboBoxDelayUnit.SelectedIndex = 0; // 毫秒
            numericUpDownDelay.Value = (decimal)delayMs;
        }

        UpdateWheelDeltaVisibility();
    }

    private bool HasEventName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return ContainsEventName?.Invoke(name, _editing) ?? true;
    }

    private void EditMouseEventForm_Load(object sender, EventArgs e)
    {
        if (comboBoxAction.SelectedIndex == -1)
            comboBoxAction.SelectedIndex = 0;

        if (comboBoxDelayUnit.SelectedIndex == -1)
            comboBoxDelayUnit.SelectedIndex = 0;

        UpdateWheelDeltaVisibility();
    }

    private void ComboBoxAction_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateWheelDeltaVisibility();
    }

    private void UpdateWheelDeltaVisibility()
    {
        bool isWheelScroll = comboBoxAction.SelectedIndex == (int)MouseAction.Wheel;
        labelWheelDelta.Visible = isWheelScroll;
        numericUpDownWheelDelta.Visible = isWheelScroll;
        chkPairedEvents.Visible = _editing is null && Action.IsDownAction();
        numericUpDownX.Enabled = Action.IsMoveAction();
        numericUpDownY.Enabled = Action.IsMoveAction();
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
        var eName = EventName;
        if (HasEventName(eName))
        {
            MessageBox.Show(this, $"事件名称 '{eName}' 已存在，请使用其他名称。", "名称冲突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
