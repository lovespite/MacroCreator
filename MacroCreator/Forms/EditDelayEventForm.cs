using MacroCreator.Models;
using MacroCreator.Models.Events;

namespace MacroCreator.Forms;

public partial class EditDelayEventForm : Form
{
    private readonly DelayEvent? _editing = null;

    public double DelayMilliseconds
    {
        get
        {
            double multiplier = comboBox1.SelectedIndex switch
            {
                0 => 1.0,          // 毫秒
                1 => 1000.0,       // 秒
                2 => 60000.0,      // 分钟
                _ => 1.0,
            };
            return (double)numericUpDown1.Value * multiplier;
        }
    }

    public string? EventName => string.IsNullOrWhiteSpace(textBox1.Text) ? null : textBox1.Text.Trim();

    public event ContainsEventNameDelegate? ContainsEventName;

    private bool HasEventName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return ContainsEventName?.Invoke(name, _editing) ?? true;
    }

    public EditDelayEventForm(string? defaultEvName = null)
    {
        InitializeComponent();
        textBox1.Text = defaultEvName ?? string.Empty;
    }

    public EditDelayEventForm(DelayEvent delayEvent) : this(delayEvent.EventName)
    {
        ArgumentNullException.ThrowIfNull(delayEvent);

        _editing = delayEvent;
        double delayMs = delayEvent.DelayMilliseconds;

        comboBox1.SelectedIndex = 0; // 毫秒
        numericUpDown1.Value = (decimal)delayMs;
    }

    private void InsertDelayForm_Load(object sender, EventArgs e)
    {
        comboBox1.SelectedIndex = 0;
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
        Close();
    }
}
