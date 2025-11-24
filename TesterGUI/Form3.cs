using System;
using System.Windows.Forms;

namespace TesterGUI
{
    public partial class Form3 : Form
    {
        public int BlockIndex { get; private set; }
        // changed from int Value to string UniqueId
        public string UniqueId { get; private set; } = string.Empty;
        public bool IsDelete { get; private set; }

        public Form3(bool defaultDelete = false)
        {
            InitializeComponent();
            IsDelete = defaultDelete;

            // Configure visible buttons based on requested mode.
            if (defaultDelete)
            {
                // delete-only mode
                buttonGet.Visible = false;
                AcceptButton = buttonDelete;
            }
            else
            {
                // get-only mode
                buttonDelete.Visible = false;
                AcceptButton = buttonGet;
            }
        }

        private bool ValidateInputs()
        {
            if (!int.TryParse(textBoxBlock.Text.Trim(), out int b) || b < 0)
            {
                MessageBox.Show(this, "Block index must be a non-negative integer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            var v = textBoxValue.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(v))
            {
                MessageBox.Show(this, "Unique ID must not be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (v.Length > 10)
            {
                MessageBox.Show(this, "Unique ID cannot be longer than 10 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            BlockIndex = b;
            UniqueId = v;
            return true;
        }

        private void ButtonGet_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;
            IsDelete = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonDelete_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;
            IsDelete = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        // Static helper to show form and get result. defaultDelete controls which action is default and which button is visible.
        public static bool ShowDialog(Form owner, bool defaultDelete, out int blockIndex, out string uniqueId, out bool isDelete)
        {
            using var dlg = new Form3(defaultDelete);
            var res = dlg.ShowDialog(owner);
            if (res == DialogResult.OK)
            {
                blockIndex = dlg.BlockIndex;
                uniqueId = dlg.UniqueId;
                isDelete = dlg.IsDelete;
                return true;
            }
            blockIndex = -1;
            uniqueId = string.Empty;
            isDelete = false;
            return false;
        }
    }
}
