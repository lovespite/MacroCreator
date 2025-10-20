using MacroCreator.Models;

namespace MacroCreator.Forms;

public partial class InsertDelayForm : Form
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

    public InsertDelayForm(string? defaultEvName = null)
    {
        InitializeComponent();
        textBox1.Text = defaultEvName ?? string.Empty;
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
