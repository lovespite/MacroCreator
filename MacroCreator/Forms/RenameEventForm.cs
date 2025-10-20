using MacroCreator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacroCreator.Forms
{
    public partial class RenameEventForm : Form
    {
        public string? EventName => string.IsNullOrWhiteSpace(textBox1.Text) ? null : textBox1.Text.Trim();

        public event ContainsEventNameDelegate? ContainsEventNameCallback;

        public RenameEventForm(string name)
        {
            InitializeComponent();
            textBox1.Text = name;
        }

        private void RenameEventForm_Load(object sender, EventArgs e)
        {

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
}
