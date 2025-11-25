using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public class Block<T> : IBytes where T : IRecord
    {
        
        private int validCount;
        private List<T> records;
        private int recordSize;
        private int blockSize;
        private int capacity;
        private int index;
        public Block(int blockSize, int recordSize, int index)
        {
            validCount = 0;
            this.recordSize = recordSize;
            this.blockSize = blockSize;
            capacity = (blockSize - sizeof(int)) / recordSize;
            records = new List<T>();
            this.index = index;
        }
        public int GetValidCount()
        {
            return validCount;
        }
        public void AddRecord(T record)
        {
            IncrementValidCount();
            if (validCount -1 == records.Count)
            {
                records.Add(record);
            } else
            {
                records[validCount-1] = record;
            }

        }
        public void IncrementValidCount()
        {
            if (validCount < capacity)
                validCount++;
            else 
                throw new InvalidOperationException("Block is full");
        }
        public void DecrementValidCount()
        {
            if (validCount > 0)
                validCount--;
            else
                throw new InvalidOperationException("Block is empty");
        }
        public List<T> GetRecords()
        {
            return records;
        }
        public int GetIndex()
        {
            return index;
        }
        public void FromBytes(byte[] bytes)
        {
            if(bytes is null || bytes.Length != blockSize)
                throw new ArgumentException("Invalid byte array size");
            records.Clear();
            validCount = BitConverter.ToInt32(bytes, 0);
            int offset = sizeof(int);
            for (int i = 0; i < capacity; i++)
            {
                byte[] recordBytes = new byte[recordSize];
                Array.Copy(bytes, offset, recordBytes, 0, recordSize);
                T record = Activator.CreateInstance<T>();
                record.FromBytes(recordBytes);
                records.Add(record);
                offset += recordSize;
            }
        }

        public byte[] GetBytes()
        {
            byte[] result = new byte[blockSize];
            Array.Copy(BitConverter.GetBytes(validCount), 0, result, 0, sizeof(int));
            int offset = sizeof(int);
            foreach (var record in records)
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
    }
}
