using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AUS2_DruhaSemestralka
{
    public class Blok : IBytes
    {
        private int validCount;
        private List<IRecord> records;
        private int recordSize;
        private int blockSize;
        private int capacity;
        public Blok(int blockSize, int recordSize)
        {
            validCount = 0;
            
            this.recordSize = recordSize;
            this.blockSize = blockSize;
            capacity = blockSize / recordSize;
            records = new List<IRecord>(capacity);
        }
        public int GetValidCount()
        {
            return validCount;
        }
        public List<IRecord> GetRecords()
        {
            return records;
        }
        public void FromBytes(byte[] bytes)
        {
            for(int i = 0; i < capacity; i++)
            {
                byte[] recordBytes = new byte[recordSize];
                Array.Copy(bytes, i * recordSize, recordBytes, 0, recordSize);
                IRecord record = Activator.CreateInstance<IRecord>();
                record.FromBytes(recordBytes);
                records.Add(record);
                validCount++;
            }
        }

        public byte[] GetBytes()
        {
            var validRecords = records.Take(validCount).ToList();
            byte[] result = new byte[blockSize];
            int offset = 0;
            foreach(var record in validRecords)
            {
                byte[] recordBytes = record.GetBytes();
                Array.Copy(recordBytes, 0, result, offset, recordSize);
                offset += recordSize;
            }
            return result;

        }

        public int GetSize()
        {
            return blockSize;
        }
        public int InsertRecord(IRecord record)
        {
            if(record.GetSize() != recordSize)
            {
                throw new ArgumentException("Record size does not match block size.");
            }
            if(validCount + 1> capacity)
            {
                return -1; // Not enough space
            }
            records[validCount] = record;
            validCount++;
            return validCount - 1; // Return index of inserted record
        }

        public IRecord GetRecord(int index)
        {
            if(index < 0 || index >= validCount)
            {
                throw new ArgumentOutOfRangeException("Index out of range.");
            }
            return records[index];
        }

        public bool DeleteRecord(int index)
        {
            if (index < 0 || index >= validCount || validCount <= 0)
            {
                return false;
            }
            var temp = records[validCount];
            records[validCount] = records[index];
            records[index] = temp;
            validCount--;
            return true;
        }
    }
}
