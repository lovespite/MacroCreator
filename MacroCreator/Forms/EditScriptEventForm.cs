using MacroCreator.Models.Events;
using MacroCreator.Native;
using MacroCreator.Services;
using MacroCreator.Utils;

namespace MacroCreator.Forms
{
    public partial class EditScriptEventForm : Form
    {
        private readonly ScriptEvent? _editing;
        private readonly DynamicExpresso.Interpreter _interpreter;

        public string? EventName => string.IsNullOrWhiteSpace(textBoxEventName.Text) ? null : textBoxEventName.Text.Trim();

        public string[] ScriptLines => [.. textBoxScriptCode.Lines];

        public event ContainsEventNameDelegate? ContainsEventName;

        public bool HasEventNameConflict()
        {
            var name = EventName;
            if (name is null || ContainsEventName is null) return false;

            return ContainsEventName(name, _editing);
        }

        private bool ValidateScriptLines()
        {

            int errorCount = 0;
            int lnIndex;
            lvErrors.Items.Clear();
            lvErrors.BeginUpdate();

            for (lnIndex = 0; lnIndex < ScriptLines.Length; lnIndex++)
            {
                var line = ScriptLines[lnIndex].Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.StartsWith("//"))
                    continue;

                try
                {
                    _ = _interpreter.Parse(line);
                }
                catch (Exception ex)
                {
                    ++errorCount;
                    var item = lvErrors.Items.Add(new ListViewItem([(lnIndex + 1).ToString(), ex.Message]));
                    item.ImageIndex = 0;
                }
            }

            lvErrors.EndUpdate();
            return errorCount == 0;
        }

        private void UpdateTextBoxLineCursor()
        {
            var line = textBoxScriptCode.GetLineFromCharIndex(textBoxScriptCode.SelectionStart);
            var col = textBoxScriptCode.SelectionStart - textBoxScriptCode.GetFirstCharIndexFromLine(line);

            lbCursor.Text = $"行: {line + 1}, 列: {col + 1}";
        }

        public EditScriptEventForm(string? defaultName = null)
        {
            InitializeComponent();
            _interpreter = new PlaybackContext().CreateInterpreter()
                .SetVariable("clipboard", new Win32.Win32Clipboard())
                .SetVariable("hid", new NopSimulator());

            textBoxEventName.Text = defaultName ?? ("Script_" + Rnd.GetString(5));
        }

        public EditScriptEventForm(ScriptEvent editing) : this(editing.EventName)
        {
            _editing = editing;
            textBoxScriptCode.Lines = editing.ScriptLines;
        }

        private void EditScriptEventForm_Load(object sender, EventArgs e)
        {

        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (HasEventNameConflict())
            {
                MessageBox.Show(this, $"事件名称 '{EventName}' 已存在，请使用其他名称。", "名称冲突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateScriptLines()) return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void TextBoxScriptCode_Click(object sender, EventArgs e)
        {
            UpdateTextBoxLineCursor();
        }

        private void TextBoxScriptCode_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateTextBoxLineCursor();
        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            ValidateScriptLines();
        }

        private void LvErrors_Click(object sender, EventArgs e)
        {
            if (lvErrors.SelectedItems.Count == 0) return;

            var item = lvErrors.SelectedItems[0];

            var lineNumStr = item.SubItems[0].Text;
            if (!int.TryParse(lineNumStr, out int lineNum)) return;

            if (lineNum - 1 < 0 || lineNum - 1 >= textBoxScriptCode.Lines.Length) return;

            int charIndex = textBoxScriptCode.GetFirstCharIndexFromLine(lineNum - 1);
            textBoxScriptCode.Focus();
            textBoxScriptCode.SelectionStart = charIndex;
            textBoxScriptCode.ScrollToCaret();
            textBoxScriptCode.SelectionLength = textBoxScriptCode.Lines[lineNum - 1].Length;

            UpdateTextBoxLineCursor();

        }
    }
}
