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
        private int nextBlockIndex;
        private int deletedCount;
        private int chainLength;
        private bool writeNextBlockIndex;
        private bool writeChainLength;
        public Block(int blockSize, int recordSize, int index, int nextBlockIndex = -1, int chainLength = 0, bool writeNextBlockIndex = false, bool writeChainLength = false)
        {
            validCount = 0;
            this.recordSize = recordSize;
            this.blockSize = blockSize;
            
            
            records = new List<T>();
            this.index = index;
            this.nextBlockIndex = nextBlockIndex;
            this.chainLength = chainLength;
            this.writeNextBlockIndex = writeNextBlockIndex;
            this.writeChainLength = writeChainLength;
            int howManyInts = 1;
            if (writeNextBlockIndex) howManyInts++;
            if (writeChainLength) howManyInts++;
            capacity = (blockSize - howManyInts * sizeof(int)) / recordSize;
        }
        public void SetRecords(List<T> newRecords, int validCount)
        {
            if (newRecords.Count > capacity)
                throw new ArgumentException("Too many records for this block");
            this.records = newRecords;
            this.validCount = validCount;
        }
        public void OverwriteBlock(List<T> records, int validCount)
        {
            if (records.Count > capacity)
            {
                throw new ArgumentException("Too many records for this block");
            }
            var currentRecords = GetRecords();
            if (records.Count < validCount)
            {
                for (int i = records.Count; i < validCount; i++)
                {
                    currentRecords[i] = records[i];
                }
            }
            else
            {
                currentRecords = records;
            }
            SetRecords(currentRecords, validCount);
        }
        public void EnableWriteNextBlockIndex()
        {
            this.writeNextBlockIndex = true;
        }
        public void EnableWriteChainLength()
        {
            this.writeChainLength = true;
        }
        public void IncrementDeletedCount()
        {
            deletedCount++;
        }
        public void FloorDeletedCount()
        {
            deletedCount = 0;
        }
        public void SetNextBlockIndex(int nextIndex)
        {
            nextBlockIndex = nextIndex;
        }
        public void IncreaseChainLength()
        {
            chainLength++;
        }
        public int GetChainLength()
        {
            return chainLength;
        }
        public void DecreaseChainLength()
        {
            if (chainLength > 0)
                chainLength--;
        }
        public int GetDeletedCount()
        {
            return deletedCount;
        }
        public void SetChainLength(int length)
        {
            chainLength = length;
        }
        public int GetValidCount()
        {
            return validCount;
        }
        public int GetNextBlockIndex()
        {
            return nextBlockIndex;
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
            if(writeNextBlockIndex)
            {
                nextBlockIndex = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);
                
            }
            if(writeChainLength)
            {
                chainLength = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);
            }
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
            if (writeNextBlockIndex)
            {
                Array.Copy(BitConverter.GetBytes(nextBlockIndex), 0, result, offset, sizeof(int));
                offset += sizeof(int);
            }
            if (writeChainLength)
            {
                Array.Copy(BitConverter.GetBytes(chainLength), 0, result, offset, sizeof(int));
                offset += sizeof(int);
            }
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
