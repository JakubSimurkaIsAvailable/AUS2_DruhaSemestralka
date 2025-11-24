using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Aus2_Heap.Data;

namespace TesterGUI
{
    public partial class Form2 : Form
    {
        // Resulting patient created by the dialog
        public Patient Result { get; private set; } = new Patient();

        // existing UIDs passed in so the dialog can create a unique UID
        private readonly HashSet<string> existingUids;

        public Form2(IEnumerable<string> existingUids)
        {
            InitializeComponent();
            this.existingUids = new HashSet<string>(existingUids ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
        }

        private void ButtonInsert_Click(object? sender, EventArgs e)
        {
            var name = textBoxName.Text?.Trim() ?? string.Empty;
            var surname = textBoxSurname.Text?.Trim() ?? string.Empty;
            var birth = dateTimePicker1.Value.Date;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(this, "Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(surname))
            {
                MessageBox.Show(this, "Surname is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // generate a unique UID (max length 10 as Patient enforces)
            string uid = GenerateUniqueUid(name, surname, birth);

            Result = new Patient(name, surname, birth, uid);
            DialogResult = DialogResult.OK;
            Close();
        }

        // Static helper to show the dialog and get a Patient back.
        public static bool ShowDialog(Form owner, IEnumerable<string> existingUids, out Patient patient)
        {
            using var dlg = new Form2(existingUids);
            var res = dlg.ShowDialog(owner);
            if (res == DialogResult.OK)
            {
                patient = dlg.Result;
                return true;
            }
            patient = null!;
            return false;
        }

        private string GenerateUniqueUid(string name, string surname, DateTime birthDate)
        {
            // Base candidate using first letters + birth date yyMMdd
            string n = string.IsNullOrEmpty(name) ? "X" : name.ToUpperInvariant().Trim()[0].ToString();
            string s = string.IsNullOrEmpty(surname) ? "X" : surname.ToUpperInvariant().Trim()[0].ToString();
            string datePart = birthDate.ToString("yyMMdd"); // 6 chars
            string basePart = $"{n}{s}{datePart}"; // typically 8 chars

            // Try to make UID up to 10 chars by appending numeric suffix if needed.
            // If basePart is already too long, truncate.
            if (basePart.Length > 10) basePart = basePart.Substring(0, 10);

            string candidate = basePart;
            int counter = 0;
            while (existingUids.Contains(candidate) && counter < 10000)
            {
                counter++;
                // append counter and truncate to 10 chars from left
                string suffix = counter.ToString();
                string combined = (basePart + suffix);
                if (combined.Length > 10)
                    candidate = combined.Substring(0, 10);
                else
                    candidate = combined;
            }

            // fallback: GUID-based short UID if collision persists
            if (existingUids.Contains(candidate))
            {
                candidate = Guid.NewGuid().ToString("N").ToUpperInvariant().Substring(0, 10);
                while (existingUids.Contains(candidate))
                {
                    candidate = Guid.NewGuid().ToString("N").ToUpperInvariant().Substring(0, 10);
                }
            }

            return candidate;
        }

        private void labelRecord_Click(object sender, EventArgs e)
        {
            // no-op (designer hook)
        }
    }
}
