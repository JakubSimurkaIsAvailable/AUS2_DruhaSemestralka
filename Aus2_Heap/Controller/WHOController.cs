using Aus2_Heap.PacientModel;
using Aus2_Heap.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aus2_Heap.Controller
{
    public sealed class WHOController
    {
        readonly WHO model;
        public Generator generator;

        public WHOController()
        {
            model = new WHO();
            generator = new Generator(model);
        }

        // Insert from GUI -> model (clone input to avoid shared references)
        // Return created patient (with UID assigned by model)
        public Patient InsertPatient(Patient patientFromGui)
        {
            if (patientFromGui == null) throw new ArgumentNullException(nameof(patientFromGui));
            var clone = ClonePatient(patientFromGui);
            model.VlozOsobu(clone);
            return ClonePatient(clone);
        }

        // Insert PCR test and return the created test (with assigned kod)
        public PCR InsertPCRTest(PCR pcrFromGui)
        {
            if (pcrFromGui == null) throw new ArgumentNullException(nameof(pcrFromGui));
            var clone = ClonePCR(pcrFromGui);
            model.VlozPCRTest(clone);
            return ClonePCR(clone);
        }

        // Insert generator data
        public void InsertGeneratorData(int numberOfPatients, int maxTestsPerPatient)
        {
            generator.Generate(numberOfPatients, maxTestsPerPatient);
        }

        // Find and return copies for GUI (deep copies)
        public (Patient patient, List<PCR> tests) FindPatient(string uniqueID)
        {
            var (p, tests) = model.VyhladajOsobu(uniqueID);
            return (ClonePatient(p), tests?.Select(ClonePCR).ToList());
        }

        public (PCR test, Patient patient) FindPCRTest(int testCode)
        {
            var (test, patient) = model.VyhladajPCRTest(testCode);
            return (ClonePCR(test), ClonePatient(patient));
        }

        // Get single records for edit (GUI gets a copy)
        public Patient GetPatientForEdit(string uniqueID)
        {
            var p = model.DajPacienta(uniqueID);
            return ClonePatient(p);
        }

        public PCR GetPCRForEdit(int testCode)
        {
            var t = model.DajPCRTest(testCode);
            return ClonePCR(t);
        }

        // Apply edits coming from GUI (clone before passing to model)
        public void UpdatePatient(Patient editedFromGui)
        {
            if (editedFromGui == null) throw new ArgumentNullException(nameof(editedFromGui));
            model.EditujOsobu(ClonePatient(editedFromGui));
        }

        public void UpdatePCRTest(PCR editedFromGui)
        {
            if (editedFromGui == null) throw new ArgumentNullException(nameof(editedFromGui));
            model.EditujPCRTest(ClonePCR(editedFromGui));
        }

        // File / lifecycle operations forwarded to model
        public void LoadFiles() => model.NacitajZoSuborov();
        public void CreateFiles() => model.VytvorSubory();
        public void CloseSystem() => model.ZatvorSystem();

        // allow GUI to set block sizes before creating files
        public void SetPatientBlockSizes(int main, int overflow)
        {
            model.PatientHashBlockSize = main;
            model.PatientHashOverflowBlockSize = overflow;
        }

        public void SetPCRBlockSizes(int main, int overflow)
        {
            model.PCRHashBlockSize = main;
            model.PCROverflowBlockSize = overflow;
        }

        // --- Private cloning helpers to ensure GUI never holds references to model instances ---
        private static Patient ClonePatient(Patient src)
        {
            if (src == null) return null;
            var clone = new Patient()
            {
                Name = src.Name,
                Surname = src.Surname,
                BirthDate = src.BirthDate,
                UniqueID = src.UniqueID,
                PCRTests = src.PCRTests != null ? (int[])src.PCRTests.Clone() : new int[6]
            };
            return clone;
        }

        private static PCR ClonePCR(PCR src)
        {
            if (src == null) return null;
            return new PCR()
            {
                DatumACasTestu = src.DatumACasTestu,
                UnikatneCisloPacienta = src.UnikatneCisloPacienta,
                Poznamka = src.Poznamka,
                UnikatnyKodTestu = src.UnikatnyKodTestu,
                VysledokTestu = src.VysledokTestu,
                HodnotaTestu = src.HodnotaTestu
            };
        }

        public string ReturnInfoAboutPatient(Patient patient, List<PCR> tests)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INFORMACIE O PACIENTOVI:");
            sb.AppendLine("======================");
            sb.AppendLine($"Meno: {patient.Name}");
            sb.AppendLine($"Priezvisko: {patient.Surname}");
            sb.AppendLine($"Datum narodenia: {patient.BirthDate.ToShortDateString()}");
            sb.AppendLine($"Unikatne cislo pacienta: {patient.UniqueID}");
            sb.AppendLine("======================");
            sb.AppendLine("PCR testy:");
            sb.AppendLine("----------------------");
            foreach (var test in tests)
            {
                sb.AppendLine($"Kod testu: {test.UnikatnyKodTestu}");
                sb.AppendLine($"Datum a cas testu: {test.DatumACasTestu}");
                sb.AppendLine($"Poznamka: {test.Poznamka}");
                sb.AppendLine($"Vysledok testu: {(test.VysledokTestu ? "T" : "F")}");
                sb.AppendLine($"Hodnota testu: {test.HodnotaTestu}");
                sb.AppendLine("----------------------");
            }
            return sb.ToString();
        }
        public string ReturnInfoAboutPCRTest(PCR test, Patient patient)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INFORMACIE O PCR TESTE:");
            sb.AppendLine("======================");
            sb.AppendLine($"Kod testu: {test.UnikatnyKodTestu}");
            sb.AppendLine($"Datum a cas testu: {test.DatumACasTestu}");
            sb.AppendLine($"Poznamka: {test.Poznamka}");
            sb.AppendLine($"Vysledok testu: {(test.VysledokTestu ? "T" : "F")}");
            sb.AppendLine($"Hodnota testu: {test.HodnotaTestu}");
            sb.AppendLine("======================");
            sb.AppendLine("INFORMACIE O PACIENTOVI:");
            sb.AppendLine("----------------------");
            sb.AppendLine($"Meno: {patient.Name}");
            sb.AppendLine($"Priezvisko: {patient.Surname}");
            sb.AppendLine($"Datum narodenia: {patient.BirthDate.ToShortDateString()}");
            sb.AppendLine($"Unikatne cislo pacienta: {patient.UniqueID}");
            sb.AppendLine("----------------------");
            return sb.ToString();
        }
    }
}
