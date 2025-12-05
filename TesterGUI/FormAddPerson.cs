using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aus2_Heap.Data;

namespace TesterGUI
{
    public partial class FormAddPerson : Form
    {
        // Resulting patient created/edited by dialog
        public Patient Result { get; private set; } = new Patient();

        public FormAddPerson()
        {
            InitializeComponent();
            buttonAdd.Click += ButtonAdd_Click;
        }

        // constructor for edit mode
        public FormAddPerson(Patient existing) : this()
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            // populate fields
            textBox1.Text = existing.Name;
            textBox2.Text = existing.Surname;
            dateTimePicker1.Value = existing.BirthDate == default ? DateTime.Now : existing.BirthDate;
            Result = new Patient()
            {
                Name = existing.Name,
                Surname = existing.Surname,
                BirthDate = existing.BirthDate,
                UniqueID = existing.UniqueID,
                PCRTests = existing.PCRTests != null ? (int[])existing.PCRTests.Clone() : new int[6]
            };
        }

        private void ButtonAdd_Click(object? sender, EventArgs e)
        {
            var name = textBox1.Text?.Trim() ?? string.Empty;
            var surname = textBox2.Text?.Trim() ?? string.Empty;
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

            // If Result already had UniqueID (edit mode) preserve it
            string uid = Result?.UniqueID ?? string.Empty;

            Result = new Patient(name, surname, birth, uid);
            if (!string.IsNullOrEmpty(uid))
            {
                Result.SetUID(uid);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
