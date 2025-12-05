using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap.Data
{
    public class Patient : IRecord, IHash
    {
        private const int NameSize = 15;
        private const int SurnameSize = 14;
        private const int UniqueIDSize = 10;
        private static readonly Encoding FixedEncoding = Encoding.ASCII;

        public Patient(string name, string surname, DateTime birthDate, string uniqueID)
        {
            Name = name;
            Surname = surname;
            BirthDate = birthDate;
            UniqueID = uniqueID;
            PCRTests = new int[6];
        }
        public Patient(string name, string surname, DateTime birthDate)
        {
            Name = name;
            Surname = surname;
            BirthDate = birthDate;
            PCRTests = new int[6];
        }
        public void SetUID(string uniqueID)
        {
            UniqueID = uniqueID;
        }
        public Patient() 
        { 
            PCRTests = new int[6];
        }
        public Patient(string uniqueID)
        {
            UniqueID = uniqueID;
            PCRTests = new int[6];
        }
        // Max legth for name is 15 characters
        public string Name { get; set; }
        //Max length for surname is 14 characters
        public string Surname { get; set; }
        public DateTime BirthDate { get; set; }
        //Max length for uniqueID is 10 characters
        public string UniqueID { get; set; }
        public int[] PCRTests { get; set; }

        public bool Equals(IRecord other)
        {
            return other is Patient patient && string.Equals(UniqueID, patient.UniqueID, StringComparison.Ordinal);
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;
            int nameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            Name = FixedEncoding.GetString(bytes, offset, NameSize).Substring(0, nameLength);
            offset += NameSize;
            int surnameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            Surname = FixedEncoding.GetString(bytes, offset, SurnameSize).Substring(0, surnameLength);
            offset += SurnameSize;
            long birthDateBinary = BitConverter.ToInt64(bytes, offset);
            BirthDate = DateTime.FromBinary(birthDateBinary);
            offset += sizeof(long);
            int uniqueIDLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            UniqueID = FixedEncoding.GetString(bytes, offset, UniqueIDSize).Substring(0, uniqueIDLength);
            offset += UniqueIDSize;
            PCRTests = new int[6];
            for (int i = 0; i < PCRTests.Length; i++)
            {
                PCRTests[i] = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);
            }

        }

        public byte[] GetBytes()
        {
            int nameLength = Name?.Length ?? 0;
            int surnameLength = Surname?.Length ?? 0;
            int uniqueIDLength = UniqueID?.Length ?? 0;
            if(nameLength > NameSize)
            {
                Name = Name.Substring(0, NameSize);
            }
            if(surnameLength > SurnameSize)
            {
                Surname = Surname.Substring(0, SurnameSize);
            }
            if(uniqueIDLength > UniqueIDSize)
            {
                UniqueID = UniqueID.Substring(0, UniqueIDSize);
            }
            string paddedName = (Name ?? string.Empty).PadRight(NameSize).Substring(0, NameSize);
            string paddedSurname = (Surname ?? string.Empty).PadRight(SurnameSize).Substring(0, SurnameSize);
            string paddedUniqueID = (UniqueID ?? string.Empty).PadRight(UniqueIDSize).Substring(0, UniqueIDSize);

            List<byte> byteList = new List<byte>();
            byteList.AddRange(BitConverter.GetBytes(nameLength));
            byteList.AddRange(FixedEncoding.GetBytes(paddedName));
            byteList.AddRange(BitConverter.GetBytes(surnameLength));
            byteList.AddRange(FixedEncoding.GetBytes(paddedSurname));
            byteList.AddRange(BitConverter.GetBytes(BirthDate.ToBinary()));
            byteList.AddRange(BitConverter.GetBytes(uniqueIDLength));
            byteList.AddRange(FixedEncoding.GetBytes(paddedUniqueID));
            for (int i = 0; i < PCRTests.Length; i++)
            {
                byteList.AddRange(BitConverter.GetBytes(PCRTests[i]));
            }
            return byteList.ToArray();
        }

        public int GetSize()
        {
            return sizeof(int) + NameSize + sizeof(int) + SurnameSize + sizeof(long) + sizeof(int) + UniqueIDSize + (6 * sizeof(int));
        }

        public override int GetHashCode()
        {
            return Math.Abs(UniqueID.GetHashCode());
        }
        public void AddPCRTest(int testCode)
        {
            for (int i = 0; i < PCRTests.Length; i++)
            {
                if (PCRTests[i] == 0)
                {
                    PCRTests[i] = testCode;
                    return;
                }
            }
            throw new InvalidOperationException("No available slot for new PCR test.");
        }
        public override string ToString()
        {
            return $"Patient(Name: {Name}, Surname: {Surname}, BirthDate: {BirthDate.ToShortDateString()}, UniqueID: {UniqueID})";
        }
    }
}
