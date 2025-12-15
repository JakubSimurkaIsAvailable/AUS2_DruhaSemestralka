using Aus2_Heap.Data;
using Aus2_Heap.PacientModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Aus2_Heap
{
    public class Generator
    {
        WHO model;
        Random rand = new Random();
        public Generator(WHO model)
        {
            this.model = model;
        }

        public void Generate(int patientCount, int testPerPersonCount)
        {
            for (int i = 0; i < patientCount; i++)
            {
                var mena = new[] { "Jan", "Peter", "Marek", "Tomas", "Lukas", "Martin", "Filip", "Michal", "Andrej", "Roman", "Maria", "Anna", "Eva", "Petra", "Lucia" };
                var priezviska = new[] { "Novak", "Kovac", "Horvath", "Varga", "Toth", "Balaz", "Kucera", "Molnar", "Bartos", "Svec", "Svoboda", "Micka", "Urban" };
                string name = mena[rand.Next(mena.Length)];
                string surname = priezviska[rand.Next(priezviska.Length)];
                DateTime birth = DateTime.Now.AddYears(-rand.Next(1, 90)).AddDays(-rand.Next(0, 365));
                Patient patient = new Patient(name, surname, birth);
                model.VlozOsobu(patient);
                string[] poznamkyDobre = { "V poriadku", "Zdravy", "Nic", "Nezavazne", "Neskodlive", "Negativny", ""};
                string[] poznamkyZle = { "Pozitivny", "Infikovany", "Chory", "Nebezpecny", "Nakazlivy", ""};
                for (int j = 0; j < testPerPersonCount; j++)
                {
                    var test = new PCR()
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
