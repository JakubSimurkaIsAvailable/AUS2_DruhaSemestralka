using Aus2_Heap;
using Aus2_Heap.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TesterGUI
{
    public partial class Form1 : Form
    {
        private Heap<Patient>? heap;
        private List<List<Patient>> listMatrix = new();
        private Random rand = new();
        private ManualResetEventSlim waitHandle = new(false);
        private volatile bool stopRequested = false;
        private volatile bool spacePressed = false;
        private volatile bool randomRunning = false;
        private string heapDataPath = string.Empty;
        private string heapMetaPath = string.Empty;
        private int recordSize = 150;
        private int operationsCount = 0;
        

        public Form1()
        {
            InitializeComponent();

            // capture keys at form level (space / esc)
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // wire up UI buttons
            buttonLoad.Click += ButtonLoad_Click;
            buttonAddRecord.Click += ButtonAddRecord_Click;
            buttonRemoveRecord.Click += ButtonRemoveRecord_Click;
            buttonGetRecord.Click += ButtonGetRecord_Click;
            buttonRandomOperations.Click += ButtonRandomOperations_Click;

            labelInfo.Text = "Load a heap file to begin.";

            // initialize labels for block meta
            UpdateBlockMetaLabels();
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!randomRunning) return;

            if (e.KeyCode == Keys.Space)
            {
                spacePressed = true;
                waitHandle.Set();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                stopRequested = true;
                waitHandle.Set();
            }
        }

        private void ButtonLoad_Click(object? sender, EventArgs e)
        {
            try
            {
                using var ofdData = new OpenFileDialog();
                ofdData.Title = "Select Heap data file";
                ofdData.CheckFileExists = false;
                if (ofdData.ShowDialog() != DialogResult.OK) return;

                using var ofdMeta = new OpenFileDialog();
                ofdMeta.Title = "Select Heap metadata file";
                ofdMeta.CheckFileExists = false;
                if (ofdMeta.ShowDialog() != DialogResult.OK) return;

                heap?.Close();
                heapDataPath = ofdData.FileName;
                heapMetaPath = ofdMeta.FileName;

                heap = new Heap<Patient>(recordSize, heapDataPath, heapMetaPath);

                listMatrix.Clear();
                int blockCount = heap.GetBlockCount();
                for (int i = 0; i < blockCount; i++)
                {
                    var records = heap.GetAllRecordsInBlock(i);
                    listMatrix.Add(new List<Patient>(records));
                }

                RenderHeapPanel();

                UpdateBlockMetaLabels();
                labelInfo.Text = $"Loaded heap with {blockCount} blocks.";
            }
            catch (Exception ex)
            {
                labelInfo.Text = $"Load failed: {ex.Message}";
                UpdateBlockMetaLabels();
            }
        }

        private void ButtonAddRecord_Click(object? sender, EventArgs e)
        {
            if (heap == null)
            {
                labelInfo.Text = "Heap not loaded.";
                return;
            }

            // collect existing UIDs so Form2 can generate a unique one
            var existingUids = GetAllUids();

            // open Form2 to get a Patient (no block id)
            if (!Form2.ShowDialog(this, existingUids, out Patient patient))
                return;

            var prevText = buttonAddRecord.Text;
            try
            {
                buttonAddRecord.Text = "Inserting...";
                buttonAddRecord.Refresh();

                // Insert returns the block index where the record was placed.
                int addrseek = heap.Seek();
                int validCountBeforeInsert;
                try
                {
                    validCountBeforeInsert = heap.GetValidCountInBlock(addrseek);
                }
                catch (Exception)
                {
                    validCountBeforeInsert = 0;
                }
                int addr = heap.Insert(patient);

                // ensure listMatrix has entry for addr
                while (listMatrix.Count <= addr)
                    listMatrix.Add(new List<Patient>());

                listMatrix[addr].Add(patient);
                operationsCount++;
                labelInfo.Text = $"Inserted {patient.Name} {patient.Surname} ({patient.UniqueID}) at block {addr}.";

                // perform checks like in Tester.cs
                var valueHeap = heap.Get(addr, patient);
                var valueList = listMatrix[addr].Last();

                if (!valueList.Equals(valueHeap) || !patient.Equals(valueHeap))
                {
                    labelInfo.Text = "ERROR: Inserted value does not match retrieved value!";
                    RenderHeapPanel();

                    UpdateBlockMetaLabels();
                    return;
                }

                if (listMatrix.Count != heap.GetBlockCount())
                {
                    labelInfo.Text = "ERROR: Block count mismatch after insertion!";
                    RenderHeapPanel();

                    UpdateBlockMetaLabels();
                    return;
                }
                if (validCountBeforeInsert + 1 != heap.GetValidCountInBlock(addr))
                {
                    labelInfo.Text = "ERROR: Valid count in block mismatch after insertion!";
                    RenderHeapPanel();

                    UpdateBlockMetaLabels();
                    return;
                }


                RenderHeapPanel();

                UpdateBlockMetaLabels();
            }
            catch (Exception ex)
            {
                labelInfo.Text = $"Insert failed: {ex.Message}";
                UpdateBlockMetaLabels();
            }
            finally
            {
                buttonAddRecord.Text = prevText;
            }
        }

        private void ButtonRemoveRecord_Click(object? sender, EventArgs e)
        {
            if (heap == null)
            {
                labelInfo.Text = "Heap not loaded.";
                return;
            }

            // show Form3 with delete mode (Get button hidden)
            if (!Form3.ShowDialog(this, defaultDelete: true, out int blockIndex, out string uniqueId, out bool isDelete))
                return;

            var prevText = buttonRemoveRecord.Text;
            try
            {
                buttonRemoveRecord.Text = "Deleting...";
                buttonRemoveRecord.Refresh();

                // Validate existence in local listMatrix before attempting delete
                if (blockIndex < 0 || blockIndex >= listMatrix.Count || !listMatrix[blockIndex].Any(x => x.UniqueID == uniqueId))
                {
                    labelInfo.Text = $"No matching record {uniqueId} found at block {blockIndex}.";
                    return;
                }

                var dataToDelete = listMatrix[blockIndex].First(x => x.UniqueID == uniqueId);

                int validCountBeforeDelete = heap.GetValidCountInBlock(blockIndex);
                heap.Delete(blockIndex, dataToDelete);
                listMatrix[blockIndex].Remove(dataToDelete);
                operationsCount++;
                // try to get deleted record from heap - should throw
                bool retrievalSucceeded = true;
                try
                {
                    var shouldNotExist = heap.Get(blockIndex, dataToDelete);
                }
                catch
                {
                    retrievalSucceeded = false; // expected
                }

                if (retrievalSucceeded)
                {
                    labelInfo.Text = "ERROR: Deleted value still retrievable!";
                    RenderHeapPanel();

                    UpdateBlockMetaLabels();
                    return;
                }

                // remove trailing empty blocks to match heap truncation
                while (listMatrix.Count > 0 && listMatrix.Last().Count == 0)
                    listMatrix.RemoveAt(listMatrix.Count - 1);

                if (listMatrix.Count != heap.GetBlockCount())
                {
                    labelInfo.Text = "ERROR: Block count mismatch after deletion!";
                    RenderHeapPanel();

                    UpdateBlockMetaLabels();
                    return;
                }
                int validCountAfterDelete;
                try
                {
                    validCountAfterDelete = heap.GetValidCountInBlock(blockIndex);
                }
                catch
                {
                    validCountAfterDelete = 0;
                }
                if (validCountBeforeDelete - 1 != validCountAfterDelete)
                {
                    labelInfo.Text = "ERROR: Valid count in block mismatch after deletion!";
                    RenderHeapPanel();

                    UpdateBlockMetaLabels();
                    return;
                }
                RenderHeapPanel();

                UpdateBlockMetaLabels();
                labelInfo.Text = $"Deleted {dataToDelete.Name} {dataToDelete.Surname} ({dataToDelete.UniqueID}) from block {blockIndex}.";
            }
            catch (Exception ex)
            {
                labelInfo.Text = $"Delete failed: {ex.Message}";
                UpdateBlockMetaLabels();
            }
            finally
            {
                buttonRemoveRecord.Text = prevText;
            }
        }

        private void ButtonGetRecord_Click(object? sender, EventArgs e)
        {
            if (heap == null)
            {
                labelInfo.Text = "Heap not loaded.";
                return;
            }

            // show Form3 with get mode (Delete button hidden)
            if (!Form3.ShowDialog(this, defaultDelete: false, out int blockIndex, out string uniqueId, out bool isDelete))
                return;

            var prevText = buttonGetRecord.Text;
            try
            {
                buttonGetRecord.Text = "Getting...";
                buttonGetRecord.Refresh();

                // Validate existence in local listMatrix before attempting get
                if (blockIndex < 0 || blockIndex >= listMatrix.Count || !listMatrix[blockIndex].Any(x => x.UniqueID == uniqueId))
                {
                    labelInfo.Text = $"No matching record {uniqueId} found at block {blockIndex}.";
                    return;
                }

                var dataToGet = listMatrix[blockIndex].First(x => x.UniqueID == uniqueId);

                var valueHeap = heap.Get(blockIndex, dataToGet);
                var valueList = listMatrix[blockIndex].First(x => x.UniqueID == uniqueId);
                operationsCount++;
                if (!valueList.Equals(valueHeap))
                {
                    labelInfo.Text = "ERROR: Retrieved value does not match expected value!";
                    return;
                }

                labelInfo.Text = $"Retrieved {valueHeap.Name} {valueHeap.Surname} ({valueHeap.UniqueID}) from block {blockIndex}.";
            }
            catch (Exception ex)
            {
                labelInfo.Text = $"Get failed: {ex.Message}";
            }
            finally
            {
                buttonGetRecord.Text = prevText;
            }
        }

        private void ButtonRandomOperations_Click(object? sender, EventArgs e)
        {
            if (heap == null)
            {
                labelInfo.Text = "Heap not loaded.";
                return;
            }

            if (randomRunning)
            {
                // already running
                return;
            }

            randomRunning = true;
            stopRequested = false;
            spacePressed = false;
            waitHandle.Reset();
            Task.Run(() => RandomOperationsLoop());

            // keep original simple message (no auto/step explanatory text)
            labelInfo.Text = "Random operations started. Press Space to perform next operation, Esc to stop.";
        }

        private int GetTargetOperations()
        {
            int target = 0;
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(() =>
                    {
                        int.TryParse(textBox1.Text, out target);
                    });
                }
                else
                {
                    int.TryParse(textBox1.Text, out target);
                }
            }
            catch
            {
                int.TryParse(textBox1.Text, out target);
            }
            return Math.Max(0, target);
        }

        private void RandomOperationsLoop()
        {
            try
            {
                while (!stopRequested)
                {
                    int target = GetTargetOperations();

                    // If target is zero -> step mode: wait for Space (or Esc)
                    if (target == 0)
                    {
                        spacePressed = false;
                        waitHandle.Reset();
                        waitHandle.Wait();
                        if (stopRequested)
                            break;
                    }
                    else
                    {
                        // auto mode: stop when operationsCount reaches target
                        if (operationsCount >= target)
                            break;
                    }

                    // perform a single random operation
                    operationsCount++;
                    int chance = rand.Next(0, 100);
                    if (chance < 60)
                    {
                        // Insert
                        var patient = GenerateRandomPatient();
                        int valCountBeforeInsert;
                        try
                        {
                            valCountBeforeInsert = heap!.GetValidCountInBlock(heap.Seek());
                        }
                        catch (Exception)
                        {
                            valCountBeforeInsert = 0;
                        }
                        int addr = heap!.Insert(patient);

                        // update UI state and perform checks on UI thread
                        this.Invoke(() =>
                        {
                            while (listMatrix.Count <= addr)
                                listMatrix.Add(new List<Patient>());

                            listMatrix[addr].Add(patient);
                            RenderHeapPanel();


                            try
                            {
                                var valueHeap = heap.Get(addr, patient);
                                var valueList = listMatrix[addr].Last();

                                if (!valueList.Equals(valueHeap) || !patient.Equals(valueHeap))
                                {
                                    labelInfo.Text = "ERROR: Inserted value does not match retrieved value!";
                                    return;
                                }

                                if (listMatrix.Count != heap.GetBlockCount())
                                {
                                    labelInfo.Text = "ERROR: Block count mismatch after insertion!";
                                    return;
                                }
                                if (valCountBeforeInsert + 1 != heap.GetValidCountInBlock(addr))
                                {
                                    labelInfo.Text = "ERROR: Valid count in block mismatch after insertion!";
                                    return;
                                }

                                labelInfo.Text = $"Random Insert: {patient.Name} {patient.Surname} ({patient.UniqueID}) at block {addr}.";
                            }
                            catch (Exception ex)
                            {
                                labelInfo.Text = $"Random Insert check failed: {ex.Message}";
                            }
                            UpdateBlockMetaLabels();
                        });
                    }
                    else if (chance < 95)
                    {
                        // Delete
                        this.Invoke(() =>
                        {
                            var nonEmptyBlocks = listMatrix
                                .Select((lst, idx) => (lst, idx))
                                .Where(x => x.lst.Count > 0)
                                .ToList();

                            if (nonEmptyBlocks.Count == 0)
                            {
                                labelInfo.Text = "Random Delete: no data to delete.";
                                return;
                            }

                            var chosen = nonEmptyBlocks[rand.Next(nonEmptyBlocks.Count)];
                            int addr = chosen.idx;
                            int dataIndex = rand.Next(chosen.lst.Count);
                            var dataToDelete = chosen.lst[dataIndex];

                            try
                            {
                                int validCountBeforeDelete = heap!.GetValidCountInBlock(addr);
                                heap!.Delete(addr, dataToDelete);
                                listMatrix[addr].RemoveAt(dataIndex);

                                // verify deleted cannot be retrieved
                                bool retrievalSucceeded = true;
                                try
                                {
                                    var shouldNotExist = heap.Get(addr, dataToDelete);
                                }
                                catch
                                {
                                    retrievalSucceeded = false;
                                }

                                if (retrievalSucceeded)
                                {
                                    labelInfo.Text = "ERROR: Deleted value still retrievable!";
                                    return;
                                }

                                while (listMatrix.Count > 0 && listMatrix.Last().Count == 0)
                                    listMatrix.RemoveAt(listMatrix.Count - 1);

                                if (listMatrix.Count != heap.GetBlockCount())
                                {
                                    labelInfo.Text = "ERROR: Block count mismatch after deletion!";
                                    return;
                                }
                                int validCountAfterDelete;
                                try
                                {
                                    validCountAfterDelete = heap.GetValidCountInBlock(addr);
                                }
                                catch
                                {
                                    validCountAfterDelete = 0;
                                }
                                if (validCountBeforeDelete - 1 != validCountAfterDelete)
                                {
                                    labelInfo.Text = "ERROR: Valid count in block mismatch after deletion!";
                                    return;
                                }

                                RenderHeapPanel();

                                UpdateBlockMetaLabels();
                                labelInfo.Text = $"Random Delete: {dataToDelete.Name} {dataToDelete.Surname} ({dataToDelete.UniqueID}) from block {addr}.";
                            }
                            catch (Exception ex)
                            {
                                labelInfo.Text = $"Random Delete failed: {ex.Message}";
                            }
                        });
                    }
                    else
                    {
                        // Get
                        this.Invoke(() =>
                        {
                            var nonEmptyBlocks = listMatrix
                                .Select((lst, idx) => (lst, idx))
                                .Where(x => x.lst.Count > 0)
                                .ToList();

                            if (nonEmptyBlocks.Count == 0)
                            {
                                labelInfo.Text = "Random Get: no data to get.";
                                return;
                            }

                            var chosen = nonEmptyBlocks[rand.Next(nonEmptyBlocks.Count)];
                            int addr = chosen.idx;
                            int dataIndex = rand.Next(chosen.lst.Count);
                            var dataToGet = chosen.lst[dataIndex];

                            try
                            {
                                var valueHeap = heap!.Get(addr, dataToGet);
                                var valueList = listMatrix[addr][dataIndex];

                                if (!valueList.Equals(valueHeap))
                                {
                                    labelInfo.Text = "ERROR: Retrieved value does not match expected value!";
                                    return;
                                }

                                labelInfo.Text = $"Random Get: Retrieved {valueHeap.Name} {valueHeap.Surname} ({valueHeap.UniqueID}) from block {addr}.";
                            }
                            catch (Exception ex)
                            {
                                labelInfo.Text = $"Random Get failed: {ex.Message}";
                            }
                            UpdateBlockMetaLabels();
                        });
                    }

                    // in step mode, reset and wait next loop iteration will wait again
                    if (GetTargetOperations() == 0)
                    {
                        spacePressed = false;
                        waitHandle.Reset();
                    }
                }
            }
            finally
            {
                randomRunning = false;
                stopRequested = false;
                waitHandle.Set();
                this.Invoke(() => labelInfo.Text = "Random operations stopped.");
            }
        }

        private Patient GenerateRandomPatient()
        {
            var mena = new[] { "Jan", "Peter", "Marek", "Tomas", "Lukas", "Martin", "Filip", "Michal", "Andrej", "Roman", "Maria", "Anna", "Eva", "Petra", "Lucia" };
            var priezviska = new[] { "Novak", "Kovac", "Horvath", "Varga", "Toth", "Balaz", "Kucera", "Molnar", "Bartos", "Svec", "Svoboda", "Micka", "Urban" };
            string name = mena[rand.Next(mena.Length)];
            string surname = priezviska[rand.Next(priezviska.Length)];
            DateTime birth = DateTime.Now.AddYears(-rand.Next(1, 90)).AddDays(-rand.Next(0, 365));
            var existing = new HashSet<string>(GetAllUids(), StringComparer.Ordinal);
            string uid;
            do
            {
                uid = Guid.NewGuid().ToString("N").ToUpperInvariant().Substring(0, 10);
            } while (existing.Contains(uid));
            return new Patient(name, surname, birth, uid);
        }

        private IEnumerable<string> GetAllUids()
        {
            return listMatrix.SelectMany(x => x).Select(p => p.UniqueID).Where(u => !string.IsNullOrEmpty(u));
        }

        private void RenderHeapPanel()
        {
            panelHeap.SuspendLayout();
            panelHeap.Controls.Clear();

            int y = 5;
            int blockSpacing = 6;
            int blockHeightBase = 24;

            if (heap == null)
            {
                var lbl = new Label { Text = "No heap loaded.", Location = new Point(5, y), AutoSize = true };
                panelHeap.Controls.Add(lbl);
                panelHeap.ResumeLayout();
                return;
            }

            int blockCount = heap.GetBlockCount();
            for (int i = 0; i < blockCount; i++)
            {
                var records = listMatrix.ElementAtOrDefault(i) ?? new List<Patient>();
                int blockHeight = blockHeightBase + Math.Max(0, records.Count) * 22;

                var gb = new GroupBox
                {
                    Text = $"Block {i}",
                    Location = new Point(5, y),
                    Size = new Size(panelHeap.ClientSize.Width - 12, Math.Min(blockHeight, 200))
                };

                for (int r = 0; r < records.Count; r++)
                {
                    var p = records[r];
                    var lbl = new Label
                    {
                        Text = $"{p.Name} {p.Surname} ({p.UniqueID}) - {p.BirthDate:yyyy-MM-dd}",
                        Location = new Point(8, 18 + r * 22),
                        AutoSize = true
                    };
                    gb.Controls.Add(lbl);
                }

                panelHeap.Controls.Add(gb);
                y += gb.Height + blockSpacing;
            }

            panelHeap.ResumeLayout();
        }



        private void UpdateBlockMetaLabels()
        {
            // label3 -> Free Blocks, label4 -> Not Full Blocks
            if (label3 == null || label4 == null)
                return;

            if (heap == null)
            {
                label3.Text = "Free Blocks: N/A";
                label4.Text = "Not Full Blocks: N/A";
                label2.Text = "Block Count: N/A";
                label5.Text = "Operations Count: N/A";
                return;
            }

            try
            {
                var free = heap.GetFreeBlocks();
                var notFull = heap.GetNotFullBlocks();

                string freeText = "None";
                if (free != null && free.Count > 0)
                    freeText = string.Join(", ", free.OrderBy(i => i));

                string notFullText = "None";
                if (notFull != null && notFull.Count > 0)
                    notFullText = string.Join(", ", notFull.OrderBy(i => i));

                label3.Text = $"Free Blocks: {freeText}";
                label4.Text = $"Not Full Blocks: {notFullText}";
                label2.Text = $"Block Count: {heap.GetBlockCount()}";
                label5.Text = $"Operations Count: {operationsCount}";
            }
            catch
            {
                label3.Text = "Free Blocks: N/A";
                label4.Text = "Not Full Blocks: N/A";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                stopRequested = true;
                waitHandle.Set();

                if (heap != null)
                {
                    heap.Close();
                    heap = null;
                }
                UpdateBlockMetaLabels();
            }
            catch
            {
                // ignore
            }
            heap = new Heap<Patient>(recordSize, heapDataPath, heapMetaPath);
            for (int i = 0; i < listMatrix.Count; i++)
            {
                var records = heap.GetAllRecordsInBlock(i);
                if(records.Count() != listMatrix[i].Count)
                {
                    throw new Exception("Data mismatch on closing. Counts not equal");
                    
                }
                for (int j = 0; j < records.Count(); j++)
                {
                    if(!records[j].Equals(listMatrix[i][j]))
                    {
                        throw new Exception("Data mismatch on closing. Records not equal");
                    }
                }
            }
            heap.Close();
            base.OnFormClosing(e);
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Select folder to create heap.bin and header.bin"
            };

            if (fbd.ShowDialog() != DialogResult.OK)
            {
                labelInfo.Text = "Create cancelled.";
                return;
            }

            string selected = fbd.SelectedPath;
            string dataPath = Path.Combine(selected, "heap.bin");
            string metaPath = Path.Combine(selected, "header.bin");

            // if either exists, ask for overwrite confirmation
            if (File.Exists(dataPath) || File.Exists(metaPath))
            {
                var result = MessageBox.Show(this,
                    "One or both files already exist. Overwrite?",
                    "Confirm overwrite",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    labelInfo.Text = "Create cancelled.";
                    return;
                }
            }

            try
            {
                // create empty files (truncate if exist)
                using (var fs = File.Create(dataPath)) { }
                using (var fs = File.Create(metaPath)) { }

                labelInfo.Text = $"Created: {Path.GetFileName(dataPath)}, {Path.GetFileName(metaPath)} in {selected}";
                UpdateBlockMetaLabels();
            }
            catch (Exception ex)
            {
                labelInfo.Text = $"Create failed: {ex.Message}";
                UpdateBlockMetaLabels();
            }
        }
    }
}
