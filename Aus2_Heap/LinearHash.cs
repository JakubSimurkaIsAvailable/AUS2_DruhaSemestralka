using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public class LinearHash<T> : Heap<T> where T : IRecord, IHash
    {
        private Heap<T> overflowFile;
        private int splitPointer;
        private int groupCount;
        private int level = 0;
        private int M = 4;
        private int maxValueCount;
        private int currentValueCount;
        private const double dmax = 0.8;
        private const double dmin = 0.64;
        public LinearHash(int blokSizeHash, int blockSizeOveflow, string pathHash, string metaPathHash,
            string pathOverflow, string metaOverflow, bool manageFreeBlocks = false) 
            : base(blokSizeHash, pathHash, metaPathHash, manageFreeBlocks)
        {
            overflowFile = new Heap<T>(blockSizeOveflow, pathOverflow, metaOverflow);
            maxValueCount = M * recordsPerBlock;
        }
        private int PowerOfTwo(int exp)
        {
            int result = 1;
            for (int i = 0; i < exp; i++)
            {
                result *= 2;
            }
            return result;
        }
        public int Insert(T record)
        {
            List<T> recordsToAdd = new List<T>();
            recordsToAdd.Add(record);
            int Hu = M * PowerOfTwo(level);
            int hashValue = record.GetHashCode() % Hu;
            if(hashValue < splitPointer)
            {
                Hu = M * PowerOfTwo(level + 1);
                hashValue = record.GetHashCode() % Hu;
            }
            int address = base.Insert(record, hashValue);
            if (address == -1)
            {
                bool inserted = false;
                bool prevHash = true;
                int prevBlockIndex = hashValue;
                int overflowAddress = base.GetNextBlockIndex(hashValue);
                while (overflowAddress != -1)
                {
                    if (overflowFile.Insert(record, overflowAddress) != -1) { inserted = true; break; };
                    prevBlockIndex = overflowAddress;
                    prevHash = false;
                    overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);
                }
                if (!inserted)
                {
                    if (prevHash)
                    {
                        int newOverflowAddress = overflowFile.Insert(record);
                        base.SetNextBlockIndex(prevBlockIndex, newOverflowAddress);
                        address = newOverflowAddress;
                    }
                    else
                    {
                        int newOverflowAddress = overflowFile.Insert(record);
                        overflowFile.SetNextBlockIndex(prevBlockIndex, newOverflowAddress);
                        address = newOverflowAddress;
                        
                    }
                    maxValueCount += overflowFile.recordsPerBlock;
                }
            }
            currentValueCount++;
            double d = (double)currentValueCount / maxValueCount;
            if (d > dmax)
            {
            }
            return address;
        }

        public void AfterInsert()
        {
            int address = base.CreateBlock();
            List<T> recordsToRehash = base.GetAllRecordsInBlock(splitPointer);
            List<int> overflowPath = new List<int>();
            int overflowAddress = base.GetNextBlockIndex(splitPointer);
            while (overflowAddress != -1)
            {
                overflowPath.Add(overflowAddress);
                var validRecords = overflowFile.GetAllRecordsInBlock(overflowAddress);
                recordsToRehash.AddRange(validRecords);
                int overflowAddressTemp = overflowFile.GetNextBlockIndex(overflowAddress);
                for (int i = 0; i < validRecords.Count; i++)
                {
                    overflowFile.Delete(overflowAddress, validRecords[i]);
                }
                overflowAddress = overflowAddressTemp;
            }
            foreach(var record in recordsToRehash)
            {

            }
            
        }

        public void Delete(T record, int address)
        {

        }

        public T Get(int id, T record)
        {
            return base.Get(id, record);
        }
    }
}
