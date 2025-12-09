using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap.Data
{
    public class PCR : IRecord, IHash
    {
        private const int unikatneCisloPacientaSize = 10;
        private const int poznamkaSize = 11;

        public DateTime DatumACasTestu { get; set; }
        public string UnikatneCisloPacienta { get; set; }
        public string Poznamka { get; set; }
        public int UnikatnyKodTestu { get; set; }
        public bool VysledokTestu { get; set; }
        public double HodnotaTestu { get; set; }

        public PCR(DateTime datumACasTestu, string unikatneCisloPacienta, string poznamka, int unikatnyKodTestu, bool vysledokTestu, double hodnotaTestu)
        {
            DatumACasTestu = datumACasTestu;
            UnikatneCisloPacienta = unikatneCisloPacienta;
            Poznamka = poznamka;
            UnikatnyKodTestu = unikatnyKodTestu;
            VysledokTestu = vysledokTestu;
            HodnotaTestu = hodnotaTestu;
        }
        public PCR() { }

        public PCR(DateTime datumACasTestu, string unikatneCisloPacienta, string poznamka, bool vysledokTestu, double hodnotaTestu)
        {
            DatumACasTestu = datumACasTestu;
            UnikatneCisloPacienta = unikatneCisloPacienta;
            Poznamka = poznamka;
            VysledokTestu = vysledokTestu;
            HodnotaTestu = hodnotaTestu;
        }

        public void SetKod(int kod)
        {
            UnikatnyKodTestu = kod;
        }

        public bool Equals(IRecord other)
        {
            return UnikatnyKodTestu == ((PCR)other).UnikatnyKodTestu;
        }

        public int GetSize()
        {
            return sizeof(long) + unikatneCisloPacientaSize + poznamkaSize + sizeof(int) + sizeof(bool) + sizeof(double);
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[GetSize()];
            int offset = 0;
            Array.Copy(BitConverter.GetBytes(DatumACasTestu.ToBinary()), 0, bytes, offset, sizeof(long));
            offset += sizeof(long);
            string paddedUnikatneCisloPacienta = (UnikatneCisloPacienta ?? string.Empty).PadRight(unikatneCisloPacientaSize).Substring(0, unikatneCisloPacientaSize);
            Array.Copy(Encoding.ASCII.GetBytes(paddedUnikatneCisloPacienta), 0, bytes, offset, unikatneCisloPacientaSize);
            offset += unikatneCisloPacientaSize;
            string paddedPoznamka = (Poznamka ?? string.Empty).PadRight(poznamkaSize).Substring(0, poznamkaSize);
            Array.Copy(Encoding.ASCII.GetBytes(paddedPoznamka), 0, bytes, offset, poznamkaSize);
            offset += poznamkaSize;
            Array.Copy(BitConverter.GetBytes(UnikatnyKodTestu), 0, bytes, offset, sizeof(int));
            offset += sizeof(int);
            Array.Copy(BitConverter.GetBytes(VysledokTestu), 0, bytes, offset, sizeof(bool));
            offset += sizeof(bool);
            Array.Copy(BitConverter.GetBytes(HodnotaTestu), 0, bytes, offset, sizeof(double));
            offset += sizeof(double);
            return bytes;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;
            long datumBinary = BitConverter.ToInt64(bytes, offset);
            DatumACasTestu = DateTime.FromBinary(datumBinary);
            offset += sizeof(long);
            UnikatneCisloPacienta = Encoding.ASCII.GetString(bytes, offset, unikatneCisloPacientaSize).Trim();
            offset += unikatneCisloPacientaSize;
            Poznamka = Encoding.ASCII.GetString(bytes, offset, poznamkaSize).Trim();
            offset += poznamkaSize;
            UnikatnyKodTestu = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            VysledokTestu = BitConverter.ToBoolean(bytes, offset);
            offset += sizeof(bool);
            HodnotaTestu = BitConverter.ToDouble(bytes, offset);
            offset += sizeof(double);
        }
        public int GetHashCode()
        {
            return UnikatnyKodTestu.GetHashCode();
        }
    }

}
