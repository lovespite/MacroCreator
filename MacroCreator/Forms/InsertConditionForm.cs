// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Native;

namespace MacroCreator.Forms;
#region UI - 窗体和控件 (View)

/// <summary>
/// 用于插入条件判断事件的对话框 (保持不变)
/// </summary>
public class InsertConditionForm : Form
{
    public PixelConditionEvent ConditionEvent { get; private set; }
    private Label lblCoords, lblColor, lblIfMatch, lblIfNotMatch;
    private TextBox txtX, txtY;
    private Panel colorPanel;
    private Button btnPickColor, btnBrowseMatch, btnBrowseNotMatch, btnOK, btnCancel;
    private TextBox txtIfMatch, txtIfNotMatch;
    private Label lblColorHex;
    private System.Windows.Forms.Timer timer;

    public InsertConditionForm()
    {
        InitializeComponent();
        Text = "插入条件判断";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(500, 300);
        ConditionEvent = new PixelConditionEvent();
    }

    private void InitializeComponent()
    {
        lblCoords = new Label { Text = "像素点坐标 (X, Y):", Location = new Point(20, 20), AutoSize = true };
        txtX = new TextBox { Location = new Point(150, 20), Width = 50 };
        txtY = new TextBox { Location = new Point(210, 20), Width = 50 };
        lblColor = new Label { Text = "目标颜色:", Location = new Point(20, 60), AutoSize = true };
        colorPanel = new Panel { Location = new Point(150, 60), Width = 50, Height = 20, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Red };
        lblColorHex = new Label { Text = ColorTranslator.ToHtml(Color.Red), Location = new Point(210, 60), AutoSize = true };
        btnPickColor = new Button { Text = "拾取颜色(F8)", Location = new Point(300, 58), Width = 100 };

        lblIfMatch = new Label { Text = "如果颜色匹配，执行:", Location = new Point(20, 100), AutoSize = true };
        txtIfMatch = new TextBox { Location = new Point(150, 100), Width = 220 };
        btnBrowseMatch = new Button { Text = "浏览...", Location = new Point(380, 98) };

        lblIfNotMatch = new Label { Text = "如果不匹配，执行:", Location = new Point(20, 140), AutoSize = true };
        txtIfNotMatch = new TextBox { Location = new Point(150, 140), Width = 220 };
        btnBrowseNotMatch = new Button { Text = "浏览...", Location = new Point(380, 138) };

        btnOK = new Button { Text = "确定", Location = new Point(280, 220) };
        btnCancel = new Button { Text = "取消", Location = new Point(370, 220) };

        Controls.AddRange(new Control[] { lblCoords, txtX, txtY, lblColor, colorPanel, lblColorHex, btnPickColor, lblIfMatch, txtIfMatch, btnBrowseMatch, lblIfNotMatch, txtIfNotMatch, btnBrowseNotMatch, btnOK, btnCancel });

        btnPickColor.Click += BtnPickColor_Click;
        btnBrowseMatch.Click += (s, e) => BrowseForFile(txtIfMatch);
        btnBrowseNotMatch.Click += (s, e) => BrowseForFile(txtIfNotMatch);

        btnOK.Click += BtnOK_Click;
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        AcceptButton = btnOK;
        CancelButton = btnCancel;

        timer = new System.Windows.Forms.Timer() { Interval = 100 };
        timer.Tick += Timer_Tick;
        KeyPreview = true;
        KeyDown += Picker_KeyDown;
    }

    private void Picker_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F8)
        {
            BtnPickColor_Click(sender, e);
        }
    }
    private void BtnPickColor_Click(object sender, EventArgs e)
    {
        Text = "移动鼠标到目标位置, 按下 Ctrl 键拾取颜色和坐标...";
        Cursor = Cursors.Cross;
        timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        Point pos = Cursor.Position;
        txtX.Text = pos.X.ToString();
        txtY.Text = pos.Y.ToString();

        nint hdc = NativeMethods.GetDC(nint.Zero);
        uint pixel = NativeMethods.GetPixel(hdc, pos.X, pos.Y);
        NativeMethods.ReleaseDC(nint.Zero, hdc);
        Color color = Color.FromArgb((int)(pixel & 0x000000FF), (int)(pixel & 0x0000FF00) >> 8, (int)(pixel & 0x00FF0000) >> 16);

        colorPanel.BackColor = color;
        lblColorHex.Text = ColorTranslator.ToHtml(color);

        // 检测 Ctrl 键是否按下
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            timer.Stop();
            Cursor = Cursors.Default;
            Text = "插入条件判断";
        }
    }
    private void BrowseForFile(TextBox target)
    {
        using (var ofd = new OpenFileDialog { Filter = "XML 文件 (*.xml)|*.xml" })
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                target.Text = ofd.FileName;
            }
        }
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(txtX.Text, out int x) || !int.TryParse(txtY.Text, out int y))
        {
            MessageBox.Show("请输入有效的 X 和 Y 坐标。");
            return;
        }

        ConditionEvent.X = x;
        ConditionEvent.Y = y;
        ConditionEvent.ExpectedColorHex = lblColorHex.Text;
        ConditionEvent.FilePathIfMatch = txtIfMatch.Text;
        ConditionEvent.FilePathIfNotMatch = txtIfNotMatch.Text;
        DialogResult = DialogResult.OK;
    }
}

#endregion

