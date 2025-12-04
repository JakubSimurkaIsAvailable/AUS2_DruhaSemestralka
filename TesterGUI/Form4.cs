using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Aus2_Heap;
using Aus2_Heap.Data;

namespace TesterGUI
{
    public partial class Form4 : Form
    {
        private LinearHash<Patient>? linearHash;
        private string? pathHash;
        private string? pathOverflow;
        private string? metaPathHash;
        private string? metaPathOverflow;
        private readonly Random rand = new Random();
        private readonly List<Patient> liveRecords = new List<Patient>();
        private readonly HashSet<string> uids = new HashSet<string>(StringComparer.Ordinal);
        private int uidCounter = 0;
        private TextBox? txtPanelView;

        public Form4()
        {
            InitializeComponent();

            // wire up button events
            buttonCreate.Click += ButtonCreate_Click;
            buttonLoad.Click += ButtonLoad_Click;
            buttonRandomOperations.Click += ButtonRandomOperations_Click;

            // create a multiline readonly textbox inside the panel to show ToString()
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
        }

        private void ButtonCreate_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            dlg.Description = "Select folder to create linear hash files";
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var folder = dlg.SelectedPath;

            pathHash = Path.Combine(folder, "hashFile.bin");
            pathOverflow = Path.Combine(folder, "overflowFile.bin");
            metaPathHash = Path.Combine(folder, "metaHashFile.bin");
            metaPathOverflow = Path.Combine(folder, "metaOverflowFile.bin");

            // close previous if any
            TryCloseLinearHash();

            try
            {
                // use same default sizes as TestLinearHash
                int blockSizeHash = 200;
                int blockSizeOverflow = 100;

                linearHash = new LinearHash<Patient>(
                    blokSizeHash: blockSizeHash,
                    blockSizeOveflow: blockSizeOverflow,
                    pathHash: pathHash,
                    metaPathHash: metaPathHash,
                    pathOverflow: pathOverflow,
                    metaOverflow: metaPathOverflow
                );

                // reset bookkeeping
                liveRecords.Clear();
                uids.Clear();
                uidCounter = 0;

                UpdatePanelView();
                label1.Text = $"Created linear hash files in: {folder}";
            }
            catch (Exception ex)
            {
                label1.Text = $"Create failed: {ex.Message}";
            }
        }

        private void ButtonLoad_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            dlg.Description = "Select folder that contains the linear hash files";
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var folder = dlg.SelectedPath;

            pathHash = Path.Combine(folder, "hashFile.bin");
            pathOverflow = Path.Combine(folder, "overflowFile.bin");
            metaPathHash = Path.Combine(folder, "metaHashFile.bin");
            metaPathOverflow = Path.Combine(folder, "metaOverflowFile.bin");

            // close previous if any
            TryCloseLinearHash();

            try
            {
                int blockSizeHash = 150;
                int blockSizeOverflow = 80;

                linearHash = new LinearHash<Patient>(
                    blokSizeHash: blockSizeHash,
                    blockSizeOveflow: blockSizeOverflow,
                    pathHash: pathHash,
                    metaPathHash: metaPathHash,
                    pathOverflow: pathOverflow,
                    metaOverflow: metaPathOverflow
                );

                // populate liveRecords / uids from file contents (like TestLinearHash)
                liveRecords.Clear();
                uids.Clear();
                uidCounter = 0;
                try
                {
                    int blockCount = linearHash.GetBlockCount();
                    for (int i = 0; i < blockCount; i++)
                    {
                        var records = linearHash.GetAllRecordsInBlock(i);
                        foreach (var r in records)
                        {
                            if (!string.IsNullOrEmpty(r.UniqueID) && !uids.Contains(r.UniqueID))
                            {
                                liveRecords.Add(r);
                                uids.Add(r.UniqueID);
                            }
                        }
                    }
                    uidCounter = liveRecords.Count;
                }
                catch { /* ignore read problems */ }

                UpdatePanelView();
                label1.Text = $"Loaded linear hash from: {folder}";
            }
            catch (Exception ex)
            {
                label1.Text = $"Load failed: {ex.Message}";
            }
        }

        private void ButtonRandomOperations_Click(object? sender, EventArgs e)
        {
            if (linearHash == null)
            {
                label1.Text = "LinearHash not created/loaded.";
                return;
            }

            int targetOps = 1;
            if (!string.IsNullOrWhiteSpace(textBoxNumbers.Text))
            {
                if (!int.TryParse(textBoxNumbers.Text, out targetOps) || targetOps <= 0)
                {
                    label1.Text = "Invalid number in textbox.";
                    return;
                }
            }

            try
            {
                for (int op = 0; op < targetOps; op++)
                {
                    int chance = rand.Next(0, 100);
                    if (chance < 60)
                    {
                        // Insert
                        var patient = GenerateRandomPatient(rand, uids);
                        int blockId = linearHash.Insert(patient); // returns address index
                        // verify retrievable
                        var got = linearHash.Get(patient);
                        if (!patient.Equals(got))
                            throw new Exception("Inserted value does not match retrieved value!");
                        if (linearHash.CalculateMaxBlockIndex() != linearHash.GetBlockCount())
                            throw new Exception("Block count is less than max block index after insert!");
                        liveRecords.Add(patient);
                        uids.Add(patient.UniqueID);

                        label1.Text = $"Inserted to block {blockId}: {patient}";
                    }
                    else
                    {
                        // Get
                        if (liveRecords.Count == 0)
                        {
                            label1.Text = $"Get skipped - no records (op {op + 1}/{targetOps})";
                        }
                        else
                        {
                            var chosen = liveRecords[rand.Next(liveRecords.Count)];
                            var got = linearHash.Get(chosen);
                            if (!chosen.Equals(got))
                                throw new Exception("Retrieved value does not match expected value!");
                            label1.Text = $"Got: {got}";
                        }
                    }
                }

                UpdatePanelView();
            }
            catch (Exception ex)
            {
                label1.Text = $"Operation failed: {ex.Message}";
            }
        }

        private Patient GenerateRandomPatient(Random rand, HashSet<string> existingUids)
        {
            var mena = new[] { "Jan", "Peter", "Marek", "Tomas", "Lukas", "Martin", "Filip", "Michal", "Andrej", "Roman", "Maria", "Anna", "Eva", "Petra", "Lucia" };
            var priezviska = new[] { "Novak", "Kovac", "Horvath", "Varga", "Toth", "Balaz", "Kucera", "Molnar", "Bartos", "Svec", "Svoboda", "Micka", "Urban" };
            string name = mena[rand.Next(mena.Length)];
            string surname = priezviska[rand.Next(priezviska.Length)];
            DateTime birth = DateTime.Now.AddYears(-rand.Next(1, 90)).AddDays(-rand.Next(0, 365));

            string uid = uidCounter.ToString();
            uidCounter++;

            return new Patient(name, surname, birth, uid);
        }

        private void UpdatePanelView()
        {
            if (linearHash == null)
            {
                txtPanelView!.Text = "<no linear hash loaded>";
                return;
            }

            try
            {
                txtPanelView!.Text = linearHash.ToString();
            }
            catch (Exception ex)
            {
                txtPanelView!.Text = $"Error reading LinearHash.ToString(): {ex.Message}";
            }
        }

        private void TryCloseLinearHash()
        {
            try
            {
                linearHash?.Close();
            }
            catch { }
            linearHash = null;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            TryCloseLinearHash();
            base.OnFormClosing(e);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
