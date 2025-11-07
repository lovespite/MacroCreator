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
        private readonly string[] ERROR_TYPES = ["错误", "警告"];

        public string? EventName => string.IsNullOrWhiteSpace(textBoxEventName.Text) ? null : textBoxEventName.Text.Trim();

        public string[] ScriptLines => [.. textBoxScriptCode.Lines];

        public event ContainsEventNameDelegate? ContainsEventName;

        public bool HasEventNameConflict()
        {
            var name = EventName;
            if (name is null || ContainsEventName is null) return false;

            return ContainsEventName(name, _editing);
        }

        private bool ValidateScriptLines(out bool hasWarning)
        {

            int errorCount = 0;
            int warnCount = 0;
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
                catch (DynamicExpresso.Exceptions.UnknownIdentifierException ex)
                {
                    ++warnCount;
                    AddErrorItem(1, (lnIndex, -1), $"未知标识符: {ex.Identifier}");
                }
                catch (Exception ex)
                {
                    ++errorCount;
                    AddErrorItem(1, (lnIndex, -1), ex.Message);
                }
            }

            lvErrors.EndUpdate();
            hasWarning = warnCount > 0;
            return errorCount == 0;
        }

        private ListViewItem AddErrorItem(int type, (int, int) position, string message)
        {
            (int line, _) = position;
            var item = new ListViewItem([ERROR_TYPES[type], $"行: {line + 1}", message]);
            lvErrors.Items.Add(item);
            item.ImageIndex = type;
            item.Tag = position;
            return item;
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

            if (!ValidateScriptLines(out var hasWarning)) return;
            if (hasWarning)
            {
                var ret = MessageBox.Show(this, "脚本中存在警告，是否仍然保存？", "存在警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ret != DialogResult.Yes) return;
            }

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
            ValidateScriptLines(out _);
        }

        private void LvErrors_Click(object sender, EventArgs e)
        {
            if (lvErrors.SelectedItems.Count == 0) return;

            var item = lvErrors.SelectedItems[0];
            if (item.Tag is not (int lineNum, int _)) return;

            if (lineNum < 0 || lineNum >= textBoxScriptCode.Lines.Length) return;

            int charIndex = textBoxScriptCode.GetFirstCharIndexFromLine(lineNum);

            textBoxScriptCode.Focus();
            textBoxScriptCode.SelectionStart = charIndex;
            textBoxScriptCode.ScrollToCaret();
            textBoxScriptCode.SelectionLength = textBoxScriptCode.Lines[lineNum].Length;

            UpdateTextBoxLineCursor();
        }
    }
}
