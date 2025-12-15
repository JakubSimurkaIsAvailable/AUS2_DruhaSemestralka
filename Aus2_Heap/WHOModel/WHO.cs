using Aus2_Heap.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap.PacientModel
{
    public class WHO
    {
        public string PatientHashFilePath = "patient_hash.bin";
        public string PatientOverflowFilePath = "patient_overflow.bin";
        public string PatientHashMetadataFilePath = "patient_hash_metadata.bin";
        public string PatientOverflowMetadataFilePath = "patient_overflow_metadata.bin";
        public int PatientHashBlockSize = 512;
        public int PatientHashOverflowBlockSize = 512;
        LinearHash<Patient> pacienti;
        public string PCRHashFilePath = "pcr_hash.bin";
        public string PCROverflowFilePath = "pcr_overflow.bin";
        public string PCRHashMetadataFilePath = "pcr_hash_metadata.bin";
        public string PCROverflowMetadataFilePath = "pcr_overflow_metadata.bin";
        public int PCRHashBlockSize = 512;
        public int PCROverflowBlockSize = 512;
        LinearHash<PCR> pcrTesty;
        private string modelMetadataFile = "model_metadata.bin";

        public int pcrTestCounter = 1;
        public int pacientCounter = 1;

        public WHO()
        {

        }

        //1. Vlozenie vysledku PCR testu do systemu (system zabezpeci unikatnost kodu PCR testu ale neoveruje ju)
        public void VlozPCRTest(PCR test)
        {
            test.SetKod(pcrTestCounter);
            pcrTestCounter++;
            Patient pacient;
            try
            {
                pacient = pacienti.Get(new Patient(test.UnikatneCisloPacienta));
            }
            catch (KeyNotFoundException ex)
            {
                throw new ArgumentException("Pacient s danym unikatnym cislom neexistuje.", ex);
            }
            pacient.AddPCRTest(test.UnikatnyKodTestu);
            pacienti.Update(pacient);
            pcrTesty.Insert(test);
        }

        //2. Vyhladanie osoby (definovana unikatnym cislom pacienta) pricom sa zobrazia vsetky udaje o osobe vratane vypisu vsetkych uskutocnenych PCR testov.
        public (Patient, List<PCR>) VyhladajOsobu(string unikatneCisloPacienta)
        {
            var pacient = pacienti.Get(new Patient(unikatneCisloPacienta));
            List<PCR> testy = new List<PCR>();
            foreach (var kod in pacient.PCRTests)
            {
                if (kod != 0)
                {
                    var test = pcrTesty.Get(new PCR() { UnikatnyKodTestu = kod });
                    testy.Add(test);
                }
            }
            return (pacient, testy);
        }

        //3. Vyhladanie PCR testu podla jeho kodu, pricom sa zobrazia aj udaje o pacientovi
        public (PCR, Patient) VyhladajPCRTest(int unikatnyKodTestu)
        {
            var test = pcrTesty.Get(new PCR() { UnikatnyKodTestu = unikatnyKodTestu });
            var pacient = pacienti.Get(new Patient(test.UnikatneCisloPacienta));
            return (test, pacient);
        }

        //4. Vlozenie osoby do systemu (system zabezpeci unikatnost cislo pacienta ale neoveruje ju)
        public void VlozOsobu(Patient pacient)
        {
            string uid = pacientCounter.ToString();
            pacient.SetUID(uid);
            pacientCounter++;
            pacienti.Insert(pacient);
        }

        //7. Vyhladanie osoby (definovana unikatnym cislom pacienta), pricom bude mozne editovat jej udaje okrem testov
        public Patient DajPacienta(string unikatneCisloPacienta)
        {
            var pacient = pacienti.Get(new Patient(unikatneCisloPacienta));
            return pacient;
        }
        public void EditujOsobu(Patient noveUdaje)
        {
            pacienti.Update(noveUdaje);
        }

        //8. Vyhladanie PCR testu podla jeho kodu, pricom bude umoznene editovat jeho udaje
        public PCR DajPCRTest(int unikatnyKodTestu)
        {
            var test = pcrTesty.Get(new PCR() { UnikatnyKodTestu = unikatnyKodTestu });
            return test;
        }
        public void EditujPCRTest(PCR noveUdaje)
        {
            pcrTesty.Update(noveUdaje);
        }

        public void NacitajZoSuborov()
        {
            using var dlg = new FolderBrowserDialog()
            {
                Description = "Select folder to load data files from",
                UseDescriptionForTitle = true
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string folder = dlg.SelectedPath;

            PatientHashFilePath = Path.Combine(folder, Path.GetFileName(PatientHashFilePath));
            PatientOverflowFilePath = Path.Combine(folder, Path.GetFileName(PatientOverflowFilePath));
            PatientHashMetadataFilePath = Path.Combine(folder, Path.GetFileName(PatientHashMetadataFilePath));
            PatientOverflowMetadataFilePath = Path.Combine(folder, Path.GetFileName(PatientOverflowMetadataFilePath));
            PCRHashFilePath = Path.Combine(folder, Path.GetFileName(PCRHashFilePath));
            PCROverflowFilePath = Path.Combine(folder, Path.GetFileName(PCROverflowFilePath));
            PCRHashMetadataFilePath = Path.Combine(folder, Path.GetFileName(PCRHashMetadataFilePath));
            PCROverflowMetadataFilePath = Path.Combine(folder, Path.GetFileName(PCROverflowMetadataFilePath));

            modelMetadataFile = Path.Combine(folder, Path.GetFileName(modelMetadataFile));

            var required = new[]
            {
                PatientHashFilePath, PatientOverflowFilePath, PatientHashMetadataFilePath, PatientOverflowMetadataFilePath,
                PCRHashFilePath, PCROverflowFilePath, PCRHashMetadataFilePath, PCROverflowMetadataFilePath, modelMetadataFile
            };

            var missing = required.Where(p => !File.Exists(p)).ToList();
            if (missing.Count > 0)
            {
                MessageBox.Show("One or more required data files are missing in the selected folder. Use 'VytvorSubory' to create them first.", "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try { pacienti?.Close(); } catch { }
            try { pcrTesty?.Close(); } catch { }

            
            FileStream fs = new FileStream(modelMetadataFile, FileMode.Open, FileAccess.Read);
            int pacientiCounterLocal = 1;
            int pcrTestyCounterLocal = 1;
            int pcrTestyBlockSizeLocal = 512;
            int pcrOverflowBlockSizeLocal = 512;
            int pacientiBlockSizeLocal = 512;
            int pacientiOverflowBlockSizeLocal = 512;
            using (BinaryReader br = new BinaryReader(fs))
            {
                try
                {
                    pacientiCounterLocal = br.ReadInt32();
                    pcrTestyCounterLocal = br.ReadInt32();
                    pacientCounter = pacientiCounterLocal;
                    pcrTestCounter = pcrTestyCounterLocal;
                    pcrTestyBlockSizeLocal = br.ReadInt32();
                    pcrOverflowBlockSizeLocal = br.ReadInt32();
                    pacientiBlockSizeLocal = br.ReadInt32();
                    pacientiOverflowBlockSizeLocal = br.ReadInt32();
                    PCRHashBlockSize = pcrTestyBlockSizeLocal;
                    PCROverflowBlockSize = pcrOverflowBlockSizeLocal;
                    PatientHashBlockSize = pacientiBlockSizeLocal;
                    PatientHashOverflowBlockSize = pacientiOverflowBlockSizeLocal;
                }
                catch (EndOfStreamException)
                {

                }
            }
            try
            {
                pacienti = new LinearHash<Patient>(PatientHashBlockSize, PatientHashOverflowBlockSize, PatientHashFilePath, PatientHashMetadataFilePath, PatientOverflowFilePath, PatientOverflowMetadataFilePath, true);
                pcrTesty = new LinearHash<PCR>(PCRHashBlockSize, PCROverflowBlockSize, PCRHashFilePath, PCRHashMetadataFilePath, PCROverflowFilePath, PCROverflowMetadataFilePath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data files: {ex.Message}", "Load error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void VytvorSubory()
        {
            using var dlg = new FolderBrowserDialog()
            {
                Description = "Select folder to create data files in",
                UseDescriptionForTitle = true
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string folder = dlg.SelectedPath;

            PatientHashFilePath = Path.Combine(folder, Path.GetFileName(PatientHashFilePath));
            PatientOverflowFilePath = Path.Combine(folder, Path.GetFileName(PatientOverflowFilePath));
            PatientHashMetadataFilePath = Path.Combine(folder, Path.GetFileName(PatientHashMetadataFilePath));
            PatientOverflowMetadataFilePath = Path.Combine(folder, Path.GetFileName(PatientOverflowMetadataFilePath));
            PCRHashFilePath = Path.Combine(folder, Path.GetFileName(PCRHashFilePath));
            PCROverflowFilePath = Path.Combine(folder, Path.GetFileName(PCROverflowFilePath));
            PCRHashMetadataFilePath = Path.Combine(folder, Path.GetFileName(PCRHashMetadataFilePath));
            PCROverflowMetadataFilePath = Path.Combine(folder, Path.GetFileName(PCROverflowMetadataFilePath));

            modelMetadataFile = Path.Combine(folder, Path.GetFileName(modelMetadataFile));

            FileStream fs = new FileStream(modelMetadataFile, FileMode.Create, FileAccess.Write);
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(pacientCounter);
                bw.Write(pcrTestCounter);
                bw.Write(PCRHashBlockSize);
                bw.Write(PCROverflowBlockSize);
                bw.Write(PatientHashBlockSize);
                bw.Write(PatientHashOverflowBlockSize);
            }
            fs.Close();

            var toCreate = new[]
            {
                PatientHashFilePath, PatientOverflowFilePath, PatientHashMetadataFilePath, PatientOverflowMetadataFilePath,
                PCRHashFilePath, PCROverflowFilePath, PCRHashMetadataFilePath, PCROverflowMetadataFilePath, modelMetadataFile
            };

            foreach (var path in toCreate)
            {
                if (!File.Exists(path))
                {
                    using (File.Create(path)) { }
                }
            }

        }
        public void ZatvorSystem()
        {
            pacienti.Close();
            pcrTesty.Close();
            FileStream fs = new FileStream(modelMetadataFile, FileMode.Create, FileAccess.Write);
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(pacientCounter);
                bw.Write(pcrTestCounter);
                bw.Write(PCRHashBlockSize);
                bw.Write(PCROverflowBlockSize);
                bw.Write(PatientHashBlockSize);
                bw.Write(PatientHashOverflowBlockSize);
            }
            fs.Close();
        }

        public string ReturnPatientMainFileInfo()
        {
            return pacienti.BaseToString();
        }

        public string ReturnPatientOverflowFileInfo()
        {
            return pacienti.OverflowToString();
        }
        public string ReturnPCRMainFileInfo()
        {
            return pcrTesty.BaseToString();
        }
        public string ReturnPCROverflowFileInfo()
        {
            return pcrTesty.OverflowToString();
        }

        public string ReturnPatientToString()
        {
            return pacienti.ToString();
        }
        public string ReturnPCRToString()
        {
            return pcrTesty.ToString();
        }
    }
    }
