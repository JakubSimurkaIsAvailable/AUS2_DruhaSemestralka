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
            overflowFile.DisableNotFullBlocksPrefference();
            maxValueCount = M * recordsPerBlock;
            currentValueCount = 0;
            splitPointer = 0;
            groupCount = 0;
            level = 0;
            for (int i = 0; i < M; i++)
            {
                base.CreateBlock();
            }
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
            int lastAddress = -1;
            int rawHash = record.GetHashCode();
            int Hu = M * PowerOfTwo(level);
            int hashValue = rawHash % Hu;
            if (hashValue < splitPointer)
            {
                Hu = M * PowerOfTwo(level + 1);
                hashValue = rawHash % Hu;
            }
            lastAddress = hashValue;
            int address = base.Insert(record, hashValue);
            if (address == -1)
            {
                bool inserted = false;
                bool prevHash = true;
                int prevBlockIndex = hashValue;
                int overflowAddress = base.GetNextBlockIndex(hashValue);
                while (overflowAddress != -1)
                {
                    if (overflowFile.Insert(record, overflowAddress) != -1)
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
                    int newOverflowAddress = overflowFile.Insert(record);
                    if (prevHash)
                    {
                        base.SetNextBlockIndex(prevBlockIndex, newOverflowAddress);
                    }
                    else
                    {
                        overflowFile.SetNextBlockIndex(prevBlockIndex, newOverflowAddress);
                    }
                    
                    maxValueCount += overflowFile.GetRecordsPerBlock();
                }
            }
            
            currentValueCount++;
            double d = (double)currentValueCount / maxValueCount;
            while(d > dmax)
            {
                int newBlockIndex = base.CreateBlock();
                maxValueCount += base.GetRecordsPerBlock();
                // collect records from the bucket at splitPointer (main block)
                var recordsToRehash = new List<T>();
                
                recordsToRehash.AddRange(base.GetAllRecordsInBlock(splitPointer));
                for (int r = 0; r < recordsToRehash.Count; r++)
                {
                    base.Delete(splitPointer, recordsToRehash[r]);
                }
                // collect from overflow chain attached to this bucket
                int ovAddr = base.GetNextBlockIndex(splitPointer);
                base.SetNextBlockIndex(splitPointer, -1);
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
                    try
                    {
                        //ak existuje nastav next na -1
                        overflowFile.SetNextBlockIndex(ovAddr, -1);
                    }
                    catch (Exception ex)
                    {

                    }
                    
                    ovAddr = nextOv;
                }
                //re-insert with Hu+1 mapping
                int HuNew = M * PowerOfTwo(level + 1);
                foreach(var rec in recordsToRehash)
                {
                    int raw = rec.GetHashCode();
                    int hashVal = raw % HuNew;
                    int addr = base.Insert(rec, hashVal);

                    if (addr == -1)
                    {
                        bool ins = false;
                        bool prevH = true;
                        int prevBlockIdx = hashVal;
                        int overflowAddr = base.GetNextBlockIndex(hashVal);
                        while (overflowAddr != -1)
                        {
                            if (overflowFile.Insert(rec, overflowAddr) != -1)
                            {
                                ins = true;
                                break;
                            }
                            prevBlockIdx = overflowAddr;
                            prevH = false;
                            overflowAddr = overflowFile.GetNextBlockIndex(overflowAddr);
                        }
                        if (!ins)
                        {
                            int newOverflowAddr = overflowFile.Insert(rec);
                            if (prevH)
                            {
                                base.SetNextBlockIndex(prevBlockIdx, newOverflowAddr);
                            }
                            else
                            {
                                overflowFile.SetNextBlockIndex(prevBlockIdx, newOverflowAddr);
                            }
                            maxValueCount += overflowFile.GetRecordsPerBlock();
                            if (rec.Equals(record))
                            {
                                lastAddress = hashVal;
                            }
                        }
                    }
                    else
                    {
                        if (rec.Equals(record))
                        {
                            lastAddress = hashVal;
                        }
                    }
                }
                splitPointer++;
                int splitThreshold = M * PowerOfTwo(level);
                if (splitPointer >= splitThreshold)
                {
                    splitPointer = 0;
                    level++;
                }
                d = (double)currentValueCount / maxValueCount;
            }
            return lastAddress;
        }

        public void Delete(T record)
        {
            int Hu = M * PowerOfTwo(level);
            int id = record.GetHashCode() % Hu;
            if (id < splitPointer)
            {
                Hu = M * PowerOfTwo(level + 1);
                id = record.GetHashCode() % Hu;
            }
            bool deleted = false;
            try
            {
                base.Delete(id, record);
                deleted = true;
            }
            catch (KeyNotFoundException)
            {
                int overflowAddress = base.GetNextBlockIndex(id);
                while (overflowAddress != -1)
                {
                    try
                    {
                        overflowFile.Delete(overflowAddress, record);
                        deleted = true;
                        break;
                    }
                    catch (KeyNotFoundException)
                    {
                        overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);

                    }
                }
            }
            if (!deleted)
            {
                throw new KeyNotFoundException("Record not found in Linear Hash.");
            } else
            {
                base.IncrementDeletedCountInBlock(id);
            }
            /*
            if(base.GetDeletedCountInBlock(id) >= overflowFile.GetRecordsPerBlock())
            {

            }
            */
            currentValueCount--;
            double d = (double)currentValueCount / maxValueCount;
            while (d < dmin && base.GetBlockCount() > M)
            {
                int a;
                int b;
                if(splitPointer > 0)
                {
                    b = splitPointer - 1;
                    a = splitPointer + M * PowerOfTwo(level) - 1;

                } else
                {
                    b = M * PowerOfTwo(level-1) - 1;
                    a = M * PowerOfTwo(level) - 1;
                    level--;
                }
                // collect records from block a (main block)
                var recordsToRehash = new List<T>();
                recordsToRehash.AddRange(base.GetAllRecordsInBlock(a));
                int ovAddr = base.GetNextBlockIndex(a);
                // delete records from the main block (they will be re-inserted)
                for (int r = 0; r < recordsToRehash.Count; r++)
                {
                    base.Delete(a, recordsToRehash[r]);
                }
                // collect from overflow chain attached to this bucket
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
                foreach(var rec in recordsToRehash)
                {
                    int address = base.Insert(rec, b);
                    if (address == -1)
                    {
                        bool inserted = false;
                        bool prevHash = true;
                        int prevBlockIndex = b;
                        int overflowAddress = base.GetNextBlockIndex(b);
                        while (overflowAddress != -1)
                        {
                            if (overflowFile.Insert(rec, overflowAddress) != -1)
                            {
                                inserted = true;
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
                            maxValueCount += overflowFile.GetRecordsPerBlock();
                        }
                    }
                }
                base.DeleteBlock(a);
                maxValueCount -= base.GetRecordsPerBlock();
                splitPointer = b;
                d = (double)currentValueCount / maxValueCount;
            }
            
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
                return value;
            }
            catch (ArgumentOutOfRangeException)
            {
                int overflowAddress = base.GetNextBlockIndex(id);
                while (overflowAddress != -1)
                {
                    try
                    {
                        value = overflowFile.Get(overflowAddress, record);
                        return value;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);
                    }
                }
            }
            throw new KeyNotFoundException("Record not found in Linear Hash.");
        }
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"LinearHash<{typeof(T).Name}>");
            sb.AppendLine($"  M = {M}, level = {level}, splitPointer = {splitPointer}");
            sb.AppendLine($"  recordsPerBlock = {recordsPerBlock}");
            sb.AppendLine($"  currentValueCount = {currentValueCount}, maxValueCount = {maxValueCount}");
            sb.AppendLine($"  load factor = {(maxValueCount == 0 ? 0.0 : (double)currentValueCount / maxValueCount):P2}");
            sb.AppendLine($"  primary blocks = {GetBlockCount()}, overflow blocks = {overflowFile.GetBlockCount()}");

            try
            {
                var freePrimary = GetFreeBlocks();
                var notFullPrimary = GetNotFullBlocks();
                sb.AppendLine($"  primary free blocks: {(freePrimary != null && freePrimary.Count > 0 ? string.Join(", ", freePrimary) : "None")}");
                sb.AppendLine($"  primary not-full blocks: {(notFullPrimary != null && notFullPrimary.Count > 0 ? string.Join(", ", notFullPrimary) : "None")}");
            }
            catch { /* ignore metadata errors */ }

            sb.AppendLine();
            sb.AppendLine("Primary buckets and overflow chains:");

            int primaryCount = 0;
            try { primaryCount = GetBlockCount(); } catch { primaryCount = 0; }

            for (int i = 0; i < primaryCount; i++)
            {
                sb.AppendLine($"-- Bucket {i} --");
                try
                {
                    var records = GetAllRecordsInBlock(i);
                    sb.AppendLine($"   Main[{i}] valid = {records.Count}");
                    for (int r = 0; r < records.Count; r++)
                    {
                        sb.AppendLine($"     [{r}] {records[r]?.ToString() ?? "<null>"}");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"   (could not read main block {i}: {ex.Message})");
                }

                // print overflow chain attached to this bucket (overflow indexes are stored in the main block's next index)
                try
                {
                    int ov = GetNextBlockIndex(i);
                    if (ov == -1)
                    {
                        sb.AppendLine("   Overflow: none");
                    }
                    else
                    {
                        int chainIdx = 0;
                        while (ov != -1)
                        {
                            sb.AppendLine($"   Overflow[{chainIdx}] (overflow block index {ov})");
                            try
                            {
                                var ovRecords = overflowFile.GetAllRecordsInBlock(ov);
                                sb.AppendLine($"     valid = {ovRecords.Count}");
                                for (int r = 0; r < ovRecords.Count; r++)
                                    sb.AppendLine($"       [{r}] {ovRecords[r]?.ToString() ?? "<null>"}");
                            }
                            catch (Exception ex)
                            {
                                sb.AppendLine($"     (could not read overflow block {ov}: {ex.Message})");
                            }
                            try
                            {
                                ov = overflowFile.GetNextBlockIndex(ov);
                            }
                            catch
                            {
                                // if overflowFile.GetNextBlockIndex fails, break to avoid infinite loop
                                break;
                            }
                            chainIdx++;
                        }
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    sb.AppendLine("   Overflow: invalid next index");
                }
                catch (Exception)
                {
                    sb.AppendLine("   Overflow: error reading chain");
                }
            }

            // show any overflow blocks that are not referenced from primary buckets (orphaned overflow blocks)
            try
            {
                var referenced = new HashSet<int>();
                for (int i = 0; i < primaryCount; i++)
                {
                    try
                    {
                        int ov = GetNextBlockIndex(i);
                        while (ov != -1)
                        {
                            referenced.Add(ov);
                            ov = overflowFile.GetNextBlockIndex(ov);
                        }
                    }
                    catch { }
                }

                var orphaned = Enumerable.Range(0, overflowFile.GetBlockCount()).Where(idx => !referenced.Contains(idx)).ToList();
                if (orphaned.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Orphaned overflow blocks:");
                    foreach (var idx in orphaned)
                    {
                        sb.AppendLine($"-- overflow {idx} --");
                        try
                        {
                            var ovRecords = overflowFile.GetAllRecordsInBlock(idx);
                            sb.AppendLine($"   valid = {ovRecords.Count}");
                            for (int r = 0; r < ovRecords.Count; r++)
                                sb.AppendLine($"     [{r}] {ovRecords[r]?.ToString() ?? "<null>"}");
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"   (could not read overflow block {idx}: {ex.Message})");
                        }
                    }
                }
            }
            catch { /* ignore orphan scan errors */ }

            return sb.ToString();

        }
    }
    
}
