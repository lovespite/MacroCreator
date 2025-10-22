using MacroCreator.Models;
using MacroCreator.Utils;

namespace MacroCreator.Forms;

public partial class EditKeyboardEventForm : Form
{
  public Keys Key => (Keys)comboBoxKey.SelectedValue!;
  public KeyboardAction KeyboardAction => (KeyboardAction)comboBoxAction.SelectedIndex;
  public string? EventName => string.IsNullOrWhiteSpace(textBoxEventName.Text) ? null : textBoxEventName.Text.Trim();

  private readonly KeyboardEvent? _editing = null;

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

  public EditKeyboardEventForm(string? defaultEvName = null)
  {
    InitializeComponent();
    textBoxEventName.Text = defaultEvName ?? string.Empty;
    InitializeKeyComboBox();
  }

  public EditKeyboardEventForm(KeyboardEvent keyboardEvent) : this(keyboardEvent.EventName)
  {
    ArgumentNullException.ThrowIfNull(keyboardEvent);
    _editing = keyboardEvent;

    textBoxEventName.Text = keyboardEvent.EventName ?? $"{keyboardEvent.Action}_{Rnd.GetString(4)}";

    comboBoxAction.SelectedIndex = (int)keyboardEvent.Action;

    // 设置按键选择
    comboBoxKey.SelectedValue = keyboardEvent.Key;

    // 设置延迟时间
    double delayMs = keyboardEvent.TimeSinceLastEvent;
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
  }

  private void InitializeKeyComboBox()
  {
    // 获取所有常用的 Keys 枚举值
    var keysList = new List<KeyValuePair<string, Keys>>();

    // 添加常用按键
    // 字母键
    for (char c = 'A'; c <= 'Z'; c++)
    {
      Keys key = (Keys)Enum.Parse(typeof(Keys), c.ToString());
      keysList.Add(new KeyValuePair<string, Keys>($"{c}", key));
    }

    // 数字键
    for (int i = 0; i <= 9; i++)
    {
      Keys key = (Keys)Enum.Parse(typeof(Keys), $"D{i}");
      keysList.Add(new KeyValuePair<string, Keys>($"{i}", key));
    }

    // 功能键
    for (int i = 1; i <= 24; i++)
    {
      Keys key = (Keys)Enum.Parse(typeof(Keys), $"F{i}");
      keysList.Add(new KeyValuePair<string, Keys>($"F{i}", key));
    }

    // 控制键
    keysList.Add(new KeyValuePair<string, Keys>("Enter", Keys.Enter));
    keysList.Add(new KeyValuePair<string, Keys>("Space", Keys.Space));
    keysList.Add(new KeyValuePair<string, Keys>("Tab", Keys.Tab));
    keysList.Add(new KeyValuePair<string, Keys>("Backspace", Keys.Back));
    keysList.Add(new KeyValuePair<string, Keys>("Delete", Keys.Delete));
    keysList.Add(new KeyValuePair<string, Keys>("Escape", Keys.Escape));
    keysList.Add(new KeyValuePair<string, Keys>("Insert", Keys.Insert));
    keysList.Add(new KeyValuePair<string, Keys>("Home", Keys.Home));
    keysList.Add(new KeyValuePair<string, Keys>("End", Keys.End));
    keysList.Add(new KeyValuePair<string, Keys>("PageUp", Keys.PageUp));
    keysList.Add(new KeyValuePair<string, Keys>("PageDown", Keys.PageDown));

    // 方向键
    keysList.Add(new KeyValuePair<string, Keys>("Up", Keys.Up));
    keysList.Add(new KeyValuePair<string, Keys>("Down", Keys.Down));
    keysList.Add(new KeyValuePair<string, Keys>("Left", Keys.Left));
    keysList.Add(new KeyValuePair<string, Keys>("Right", Keys.Right));

    // 修饰键
    keysList.Add(new KeyValuePair<string, Keys>("Shift (Left)", Keys.LShiftKey));
    keysList.Add(new KeyValuePair<string, Keys>("Shift (Right)", Keys.RShiftKey));
    keysList.Add(new KeyValuePair<string, Keys>("Control (Left)", Keys.LControlKey));
    keysList.Add(new KeyValuePair<string, Keys>("Control (Right)", Keys.RControlKey));
    keysList.Add(new KeyValuePair<string, Keys>("Alt (Left)", Keys.LMenu));
    keysList.Add(new KeyValuePair<string, Keys>("Alt (Right)", Keys.RMenu));
    keysList.Add(new KeyValuePair<string, Keys>("Win (Left)", Keys.LWin));
    keysList.Add(new KeyValuePair<string, Keys>("Win (Right)", Keys.RWin));

    // 符号键
    keysList.Add(new KeyValuePair<string, Keys>("- (Minus)", Keys.OemMinus));
    keysList.Add(new KeyValuePair<string, Keys>("= (Plus)", Keys.Oemplus));
    keysList.Add(new KeyValuePair<string, Keys>("[ (OpenBrackets)", Keys.OemOpenBrackets));
    keysList.Add(new KeyValuePair<string, Keys>("] (CloseBrackets)", Keys.OemCloseBrackets));
    keysList.Add(new KeyValuePair<string, Keys>("\\ (Backslash)", Keys.OemBackslash));
    keysList.Add(new KeyValuePair<string, Keys>("; (Semicolon)", Keys.OemSemicolon));
    keysList.Add(new KeyValuePair<string, Keys>("' (Quote)", Keys.OemQuotes));
    keysList.Add(new KeyValuePair<string, Keys>(", (Comma)", Keys.Oemcomma));
    keysList.Add(new KeyValuePair<string, Keys>(". (Period)", Keys.OemPeriod));
    keysList.Add(new KeyValuePair<string, Keys>("/ (Question)", Keys.OemQuestion));
    keysList.Add(new KeyValuePair<string, Keys>("` (Tilde)", Keys.Oemtilde));

    // 小键盘
    for (int i = 0; i <= 9; i++)
    {
      Keys key = (Keys)Enum.Parse(typeof(Keys), $"NumPad{i}");
      keysList.Add(new KeyValuePair<string, Keys>($"NumPad {i}", key));
    }
    keysList.Add(new KeyValuePair<string, Keys>("NumPad +", Keys.Add));
    keysList.Add(new KeyValuePair<string, Keys>("NumPad -", Keys.Subtract));
    keysList.Add(new KeyValuePair<string, Keys>("NumPad *", Keys.Multiply));
    keysList.Add(new KeyValuePair<string, Keys>("NumPad /", Keys.Divide));
    keysList.Add(new KeyValuePair<string, Keys>("NumPad .", Keys.Decimal));
    keysList.Add(new KeyValuePair<string, Keys>("NumLock", Keys.NumLock));

    // 其他常用键
    keysList.Add(new KeyValuePair<string, Keys>("CapsLock", Keys.CapsLock));
    keysList.Add(new KeyValuePair<string, Keys>("ScrollLock", Keys.Scroll));
    keysList.Add(new KeyValuePair<string, Keys>("Pause", Keys.Pause));
    keysList.Add(new KeyValuePair<string, Keys>("PrintScreen", Keys.PrintScreen));

    comboBoxKey.DisplayMember = "Key";
    comboBoxKey.ValueMember = "Value";
    comboBoxKey.DataSource = keysList;
  }

  private bool HasEventName(string? name)
  {
    if (string.IsNullOrWhiteSpace(name)) return false;
    return ContainsEventName?.Invoke(name, _editing) ?? true;
  }

  private void EditKeyboardEventForm_Load(object sender, EventArgs e)
  {
    if (comboBoxAction.SelectedIndex == -1)
      comboBoxAction.SelectedIndex = 0;

    if (comboBoxDelayUnit.SelectedIndex == -1)
      comboBoxDelayUnit.SelectedIndex = 0;

    if (comboBoxKey.SelectedIndex == -1)
      comboBoxKey.SelectedIndex = 0;

    UpdateKeyCodeDisplay();
  }

  private void ComboBoxKey_SelectedIndexChanged(object sender, EventArgs e)
  {
    UpdateKeyCodeDisplay();
  }

  private void UpdateKeyCodeDisplay()
  {
    if (comboBoxKey.SelectedValue != null)
    {
      Keys key = (Keys)comboBoxKey.SelectedValue;
      textBoxKeyCode.Text = $"{key} (0x{(int)key:X})";
    }
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
