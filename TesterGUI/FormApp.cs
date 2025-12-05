using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aus2_Heap.Controller;
using Aus2_Heap.Data;

namespace TesterGUI
{
    public partial class FormApp : Form
    {
        private readonly WHOController controller;
        private TextBox txtPanelView;

        public FormApp()
        {
            InitializeComponent();

            controller = new WHOController();

            // Add a readonly multiline textbox into panel1 to display info
            txtPanelView = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 9F),
                WordWrap = false
            };
            panel1.Controls.Add(txtPanelView);

            // wire menu items and buttons
            createFilesToolStripMenuItem.Click += CreateFilesToolStripMenuItem_Click;
            loadFilesToolStripMenuItem.Click += LoadFilesToolStripMenuItem_Click;
            closeFilesToolStripMenuItem.Click += CloseFilesToolStripMenuItem_Click;

            buttonInsertPatient.Click += ButtonInsertPatient_Click;
            buttonInsertTest.Click += ButtonInsertTest_Click;
            buttonGetPatient.Click += ButtonGetPatient_Click;
            buttonGetTest.Click += ButtonGetTest_Click;
            buttonEditPatient.Click += ButtonEditPatient_Click;
            buttonEditTest.Click += ButtonEditTest_Click;
        }

        // CREATE FILES: validate block size textboxes contain positive integers, set sizes then open create dialog
        private void CreateFilesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!TryReadFourPositiveInts(textBoxPatientMain.Text, textBoxPatientOverflow.Text, textBoxPCRMain.Text, textBoxPCROverflow.Text,
                out int patientMain, out int patientOverflow, out int pcrMain, out int pcrOverflow))
            {
                MessageBox.Show(this, "Please enter positive integers for all block size fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // apply sizes to model via controller
            controller.SetPatientBlockSizes(patientMain, patientOverflow);
            controller.SetPCRBlockSizes(pcrMain, pcrOverflow);

            // call controller to create files (it will show folder browser dialog)
            try
            {
                controller.CreateFiles();
                txtPanelView.Text = $"Data files created (block sizes applied):\r\nPatient main={patientMain}, patient overflow={patientOverflow}\r\nPCR main={pcrMain}, PCR overflow={pcrOverflow}";
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Create files failed: {ex.Message}";
            }
        }

        private void LoadFilesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                controller.LoadFiles();
                txtPanelView.Text = "Files loaded successfully.";
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Load failed: {ex.Message}";
            }
        }

        private void CloseFilesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                controller.CloseSystem();
                txtPanelView.Text = "Data files closed.";
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Close failed: {ex.Message}";
            }
        }

        // Insert Patient: show FormAddPerson; controller.InsertPatient returns created Patient with assigned UID
        private void ButtonInsertPatient_Click(object? sender, EventArgs e)
        {
            using var dlg = new FormAddPerson();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // retrieve patient from dialog (FormAddPerson sets Result)
            var patient = (dlg as FormAddPerson)!.Result;
            try
            {
                var created = controller.InsertPatient(patient);
                // fetch tests (should be none) and display
                var (p, tests) = controller.FindPatient(created.UniqueID);
                txtPanelView.Text = controller.ReturnInfoAboutPatient(p, tests ?? new List<PCR>());
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Insert patient failed: {ex.Message}";
            }
        }

        // Edit Patient: ask for unique id, load into dialog, allow editing, then update via controller
        private void ButtonEditPatient_Click(object? sender, EventArgs e)
        {
            var uid = PromptString(this, "Edit person", "Enter patient's unique id:");
            if (string.IsNullOrWhiteSpace(uid)) return;

            try
            {
                var patientForEdit = controller.GetPatientForEdit(uid.Trim());
                if (patientForEdit == null)
                {
                    MessageBox.Show(this, "Patient not found.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var dlg = new FormAddPerson(patientForEdit);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var edited = (dlg as FormAddPerson)!.Result;
                controller.UpdatePatient(edited);

                var (p, tests) = controller.FindPatient(edited.UniqueID);
                txtPanelView.Text = controller.ReturnInfoAboutPatient(p, tests ?? new List<PCR>());
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Edit patient failed: {ex.Message}";
            }
        }

        // Insert PCR test
        private void ButtonInsertTest_Click(object? sender, EventArgs e)
        {
            using var dlg = new FormAddTest();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var pcr = dlg.Result;
            try
            {
                var created = controller.InsertPCRTest(pcr);
                // show info about test + patient
                var (test, patient) = controller.FindPCRTest(created.UnikatnyKodTestu);
                txtPanelView.Text = controller.ReturnInfoAboutPCRTest(test, patient);
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Insert PCR failed: {ex.Message}";
            }
        }

        // Edit PCR test: prompt for test code, fetch, prefill dialog, update
        private void ButtonEditTest_Click(object? sender, EventArgs e)
        {
            var s = PromptString(this, "Edit PCR test", "Enter PCR test code (integer):");
            if (string.IsNullOrWhiteSpace(s)) return;
            if (!int.TryParse(s.Trim(), out int code))
            {
                MessageBox.Show(this, "Invalid integer code.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var t = controller.GetPCRForEdit(code);
                if (t == null)
                {
                    MessageBox.Show(this, "PCR test not found.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var dlg = new FormAddTest(t);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var edited = dlg.Result;
                controller.UpdatePCRTest(edited);

                var (test, patient) = controller.FindPCRTest(edited.UnikatnyKodTestu);
                txtPanelView.Text = controller.ReturnInfoAboutPCRTest(test, patient);
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Edit PCR test failed: {ex.Message}";
            }
        }

        // Find patient and display
        private void ButtonGetPatient_Click(object? sender, EventArgs e)
        {
            var uid = PromptString(this, "Find person", "Enter patient's unique id:");
            if (string.IsNullOrWhiteSpace(uid)) return;

            try
            {
                var (p, tests) = controller.FindPatient(uid.Trim());
                txtPanelView.Text = controller.ReturnInfoAboutPatient(p, tests ?? new List<PCR>());
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Find patient failed: {ex.Message}";
            }
        }

        // Find PCR and display
        private void ButtonGetTest_Click(object? sender, EventArgs e)
        {
            var s = PromptString(this, "Find PCR test", "Enter PCR test code (integer):");
            if (string.IsNullOrWhiteSpace(s)) return;
            if (!int.TryParse(s.Trim(), out int code))
            {
                MessageBox.Show(this, "Invalid integer code.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var (test, patient) = controller.FindPCRTest(code);
                txtPanelView.Text = controller.ReturnInfoAboutPCRTest(test, patient);
            }
            catch (Exception ex)
            {
                txtPanelView.Text = $"Find PCR test failed: {ex.Message}";
            }
        }

        // helper: try parse four positive ints
        private bool TryReadFourPositiveInts(string a, string b, string c, string d, out int a1, out int b1, out int c1, out int d1)
        {
            a1 = b1 = c1 = d1 = 0;
            if (!int.TryParse(a, out a1) || a1 <= 0) return false;
            if (!int.TryParse(b, out b1) || b1 <= 0) return false;
            if (!int.TryParse(c, out c1) || c1 <= 0) return false;
            if (!int.TryParse(d, out d1) || d1 <= 0) return false;
            return true;
        }

        // simple prompt dialog (string)
        private static string? PromptString(IWin32Window owner, string title, string prompt)
        {
            using var f = new Form()
            {
                Width = 400,
                Height = 140,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                Text = title,
                MinimizeBox = false,
                MaximizeBox = false
            };
            var lbl = new Label() { Left = 10, Top = 10, Text = prompt, AutoSize = true };
            var txt = new TextBox() { Left = 10, Top = 35, Width = 360 };
            var ok = new Button() { Text = "OK", Left = 210, Width = 75, Top = 65, DialogResult = DialogResult.OK };
            var cancel = new Button() { Text = "Cancel", Left = 295, Width = 75, Top = 65, DialogResult = DialogResult.Cancel };
            f.Controls.Add(lbl);
            f.Controls.Add(txt);
            f.Controls.Add(ok);
            f.Controls.Add(cancel);
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            var res = f.ShowDialog(owner);
            if (res == DialogResult.OK)
                return txt.Text;
            return null;
        }
    }
}
