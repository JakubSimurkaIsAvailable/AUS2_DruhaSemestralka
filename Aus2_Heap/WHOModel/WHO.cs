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
            var pacient = pacienti.Get(new Patient(test.UnikatneCisloPacienta));
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
            string uid = pacientCounter.ToString().PadLeft(10, '0');
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

            var required = new[]
            {
                PatientHashFilePath, PatientOverflowFilePath, PatientHashMetadataFilePath, PatientOverflowMetadataFilePath,
                PCRHashFilePath, PCROverflowFilePath, PCRHashMetadataFilePath, PCROverflowMetadataFilePath
            };

            var missing = required.Where(p => !File.Exists(p)).ToList();
            if (missing.Count > 0)
            {
                MessageBox.Show("One or more required data files are missing in the selected folder. Use 'VytvorSubory' to create them first.", "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try { pacienti?.Close(); } catch { }
            try { pcrTesty?.Close(); } catch { }

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

            var toCreate = new[]
            {
                PatientHashFilePath, PatientOverflowFilePath, PatientHashMetadataFilePath, PatientOverflowMetadataFilePath,
                PCRHashFilePath, PCROverflowFilePath, PCRHashMetadataFilePath, PCROverflowMetadataFilePath
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
        }
    }
}
