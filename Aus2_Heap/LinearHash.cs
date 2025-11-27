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
        private int M = 2;
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
            var toProcess = new List<(T rec, bool increment)>();
            toProcess.Add((record, true));

            int lastAddress = -1;

            for (int idx = 0; idx < toProcess.Count; idx++)
            {
                var (rec, incrementCount) = toProcess[idx];

                int rawHash = rec.GetHashCode();

                int Hu = M * PowerOfTwo(level);
                int hashValue = rawHash % Hu;
                if (hashValue < splitPointer)
                {
                    Hu = M * PowerOfTwo(level + 1);
                    hashValue = rawHash % Hu;
                }

                int address = base.Insert(rec, hashValue);
                if (address == -1)
                {
                    bool inserted = false;
                    bool prevHash = true;
                    int prevBlockIndex = hashValue;
                    int overflowAddress = base.GetNextBlockIndex(hashValue);

                    while (overflowAddress != -1)
                    {
                        if (overflowFile.Insert(rec, overflowAddress) != -1)
                        {
                            inserted = true;
                            lastAddress = overflowAddress;
                            break;
                        }
                        prevBlockIndex = overflowAddress;
                        prevHash = false;
                        overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);
                    }

                    if (!inserted)
                    {
                        int newOverflowAddress = overflowFile.Insert(rec);
                        if (prevHash)
                        {
                            base.SetNextBlockIndex(prevBlockIndex, newOverflowAddress);
                        }
                        else
                        {
                            overflowFile.SetNextBlockIndex(prevBlockIndex, newOverflowAddress);
                        }
                        lastAddress = newOverflowAddress;
                        maxValueCount += overflowFile.GetRecordsPerBlock();
                    }
                }
                else
                {
                    lastAddress = address;
                }

                if (incrementCount)
                {
                    currentValueCount++;
                }

                double d = (double)currentValueCount / maxValueCount;
                if (d > dmax)
                {
                    // perform one split (create new main block)
                    base.CreateBlock();
                    maxValueCount += base.GetRecordsPerBlock();

                    // collect records from the bucket at splitPointer (main block)
                    var recordsToRehash = new List<T>();
                    recordsToRehash.AddRange(base.GetAllRecordsInBlock(splitPointer));

                    // collect from overflow chain attached to this bucket
                    int ovAddr = base.GetNextBlockIndex(splitPointer);
                    while (ovAddr != -1)
                    {
                        var validRecords = overflowFile.GetAllRecordsInBlock(ovAddr);
                        recordsToRehash.AddRange(validRecords);
                        int nextOv = overflowFile.GetNextBlockIndex(ovAddr);

                        // delete moved records from that overflow block
                        for (int r = 0; r < validRecords.Count; r++)
                        {
                            overflowFile.Delete(ovAddr, validRecords[r]);
                        }

                        ovAddr = nextOv;
                    }

                    // delete records from the main block (they will be re-inserted)
                    for (int r = 0; r < recordsToRehash.Count; r++)
                    {
                        base.Delete(splitPointer, recordsToRehash[r]);
                    }

                    for (int r = 0; r < recordsToRehash.Count; r++)
                    {
                        toProcess.Add((recordsToRehash[r], false));
                    }

                    // advance split pointer and possibly level
                    splitPointer++;
                    int splitThreshold = M * PowerOfTwo(level);
                    if (splitPointer >= splitThreshold)
                    {
                        splitPointer = 0;
                        level++;
                    }

                    // continue the for-loop: newly appended records will be processed
                }
            }

            return lastAddress;
        }

        public void Delete(T record)
        {

        }

        public T Get(T record)
        {
            int Hu = M * PowerOfTwo(level);
            int id = record.GetHashCode() % Hu;
            if (id < splitPointer)
            {
                Hu = M * PowerOfTwo(level + 1);
                id = record.GetHashCode() % Hu;
            }
            T value = default(T);
            try
            {
                value = base.Get(id, record);
            }
            catch (KeyNotFoundException)
            {
                int overflowAddress = base.GetNextBlockIndex(id);
                while (overflowAddress != -1)
                {
                    try
                    {
                        value = overflowFile.Get(overflowAddress, record);
                        return value;
                    }
                    catch (KeyNotFoundException)
                    {
                        overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);
                    }
                }
            }
            throw new KeyNotFoundException("Record not found in Linear Hash.");
        }
    }
}
