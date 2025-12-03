using Aus2_Heap.PacientModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public class Generator
    {
        WHO model;
        public Generator(WHO model)
        {
            this.model = model;
        }

        public void Generate(int patientCount, int testPerPersonCount)
        {
            for (int i = 0; i < patientCount; i++)
            {
                var patient = new Data.Patient($"Name{i}", $"Surname{i}", DateTime.Now.AddYears(-20).AddDays(i));
                model.VlozOsobu(patient);
                string[] poznamkyDobre = { "V poriadku", "Zdravy", "Nic", "Nezavazne", "Neskodlive", "Negativny", ""};
                string[] poznamkyZle = { "Pozitivny", "Infikovany", "Chory", "Nebezpecny", "Nakazlivy", ""};
                for (int j = 0; j < testPerPersonCount; j++)
                {
                    var test = new Data.PCR()
                    {
                        DatumACasTestu = DateTime.Now.AddDays(-j),
                        UnikatneCisloPacienta = patient.UniqueID,
                        VysledokTestu = (j % 3 == 0) ? true : false,
                        Poznamka = (j % 3 == 0) ? poznamkyZle[j % poznamkyZle.Length] : poznamkyDobre[j % poznamkyDobre.Length],
                        HodnotaTestu = (j % 3 == 0) ? 35.0 + j : 20.0 + j
                    };
                    model.VlozPCRTest(test);
                }
            }
        }
    }
}
