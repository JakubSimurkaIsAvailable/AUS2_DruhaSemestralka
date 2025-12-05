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
    public partial class FormAddTest : Form
    {
        // Resulting PCR created/edited by dialog
        public PCR Result { get; private set; } = new PCR();

        private readonly bool isEditMode;

        public FormAddTest()
        {
            InitializeComponent();
            buttonAdd.Click += ButtonAdd_Click;
            isEditMode = false;
            buttonAdd.Text = "Pridaj";
        }

        // overload for editing existing PCR
        public FormAddTest(PCR existing) : this()
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            isEditMode = true;
            buttonAdd.Text = "Uloz";
            LoadFromPCR(existing);
        }

        private void LoadFromPCR(PCR p)
        {
            textBoxPatientUID.Text = p.UnikatneCisloPacienta ?? string.Empty;
            textBoxTestResult.Text = p.VysledokTestu ? "1" : "0";
            textBox3.Text = p.HodnotaTestu.ToString(System.Globalization.CultureInfo.InvariantCulture);
            textBoxNote.Text = p.Poznamka ?? string.Empty;
            dateTimePicker1.Value = p.DatumACasTestu == default ? DateTime.Now : p.DatumACasTestu;
            Result = new PCR()
            {
                UnikatneCisloPacienta = p.UnikatneCisloPacienta,
                UnikatnyKodTestu = p.UnikatnyKodTestu,
                DatumACasTestu = p.DatumACasTestu,
                HodnotaTestu = p.HodnotaTestu,
                Poznamka = p.Poznamka,
                VysledokTestu = p.VysledokTestu
            };
        }

        private void ButtonAdd_Click(object? sender, EventArgs e)
        {
            var pid = textBoxPatientUID.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(pid))
            {
                MessageBox.Show(this, "Patient unique id is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (pid.Length > 10)
            {
                MessageBox.Show(this, "Patient unique id cannot exceed 10 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var resText = textBoxTestResult.Text?.Trim() ?? string.Empty;
            if (resText != "0" && resText != "1")
            {
                MessageBox.Show(this, "Test result must be 1 (positive) or 0 (negative).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool resultBool = resText == "1";

            if (!double.TryParse(textBox3.Text?.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                MessageBox.Show(this, "Test value must be a number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var note = textBoxNote.Text?.Trim() ?? string.Empty;
            var date = dateTimePicker1.Value;

            // preserve code when editing
            int code = Result?.UnikatnyKodTestu ?? 0;

            Result = new PCR()
            {
                UnikatneCisloPacienta = pid,
                VysledokTestu = resultBool,
                HodnotaTestu = value,
                Poznamka = note,
                DatumACasTestu = date,
                UnikatnyKodTestu = code
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
