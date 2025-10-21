using MacroCreator.Models;

namespace MacroCreator.Forms;

public partial class EditDelayEventForm : Form
{
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

    public event ContainsEventNameDelegate? ContainsEventNameCallback;

    public EditDelayEventForm(string? defaultEvName = null)
    {
        InitializeComponent();
        textBox1.Text = defaultEvName ?? string.Empty;
    }

    public EditDelayEventForm(DelayEvent delayEvent)
        : this(delayEvent.EventName)
    {
        double delayMs = delayEvent.DelayMilliseconds;
        if (delayMs >= 60000.0 && delayMs % 60000.0 == 0)
        {
            comboBox1.SelectedIndex = 2; // 分钟
            numericUpDown1.Value = (decimal)(delayMs / 60000.0);
        }
        else if (delayMs >= 1000.0 && delayMs % 1000.0 == 0)
        {
            comboBox1.SelectedIndex = 1; // 秒
            numericUpDown1.Value = (decimal)(delayMs / 1000.0);
        }
        else
        {
            comboBox1.SelectedIndex = 0; // 毫秒
            numericUpDown1.Value = (decimal)delayMs;
        }
    }

    private void InsertDelayForm_Load(object sender, EventArgs e)
    {
        comboBox1.SelectedIndex = 0;
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
        if (ContainsEventNameCallback is not null
            && !string.IsNullOrWhiteSpace(EventName)
            && ContainsEventNameCallback(EventName))
        {
            MessageBox.Show(this, "事件名称已存在，请使用其他名称。", "名称冲突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
