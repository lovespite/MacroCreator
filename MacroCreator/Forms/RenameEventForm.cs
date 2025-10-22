using MacroCreator.Models.Events;
using MacroCreator.Utils;

namespace MacroCreator.Forms
{
    public partial class RenameEventForm : Form
    {
        private readonly MacroEvent? _editing = null;
        public string? EventName => string.IsNullOrWhiteSpace(textBox1.Text) ? null : textBox1.Text.Trim();

        public event ContainsEventNameDelegate? ContainsEventName;

        private bool HasEventName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return ContainsEventName?.Invoke(name, _editing) ?? true;
        }

        public RenameEventForm(MacroEvent @event)
        {
            InitializeComponent();
            _editing = @event;
            textBox1.Text = _editing.EventName ?? $"{_editing.TypeName}_{Rnd.GetString(5)}";
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
}
