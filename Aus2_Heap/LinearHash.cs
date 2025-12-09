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
        private int level = 0;
        private int M = 2;
        private int maxValueCount;
        private int currentValueCount;
        private const double dmax = 0.8;
        private const double dmin = 0.64;
        private int blockSizeHash;
        private int blockSizeOverflow;


        public LinearHash(int blokSizeHash, int blockSizeOveflow, string pathHash, string metaPathHash,
            string pathOverflow, string metaOverflow, bool enableWriteNextBlockIndex = true, bool enableWriteChainLength = true,
            bool manageFreeBlocks = false)
            : base(blokSizeHash, pathHash, metaPathHash, enableWriteNextBlockIndex, enableWriteChainLength, manageFreeBlocks)
        {
            this.blockSizeHash = blokSizeHash;
            this.blockSizeOverflow = blockSizeOveflow;
            overflowFile = new Heap<T>(blockSizeOveflow, pathOverflow, metaOverflow, true, false);
            overflowFile.DisableNotFullBlocksPrefference();
            maxValueCount = M * recordsPerBlock;
            currentValueCount = 0;
            splitPointer = 0;
            level = 0;
            if (GetBlockCount() == 0)
            {
                for (int i = 0; i < M; i++)
                {
                    base.CreateBlock();
                }
            }
            else
            {
                try
                {
                    if (MetaBuffer != null && MetaExtraOffset < MetaBuffer.Length)
                    {
                        int offset = MetaExtraOffset;
                        if (offset + 4 <= MetaBuffer.Length)
                        {
                            splitPointer = BitConverter.ToInt32(MetaBuffer, offset);
                            offset += 4;
                        }
                        if (offset + 4 <= MetaBuffer.Length)
                        {
                            level = BitConverter.ToInt32(MetaBuffer, offset);
                            offset += 4;
                        }
                        if (offset + 4 <= MetaBuffer.Length)
                        {
                            M = BitConverter.ToInt32(MetaBuffer, offset);
                            offset += 4;
                        }
                    }
                }
                catch
                {
                    
                }
                maxValueCount = GetBlockCount() * recordsPerBlock + overflowFile.GetBlockCount() * overflowFile.GetRecordsPerBlock();

                int cv = 0;
                try
                {
                    for (int i = 0; i < GetBlockCount(); i++)
                    {
                        try { cv += GetValidCountInBlock(i); } catch { }
                    }
                    for (int i = 0; i < overflowFile.GetBlockCount(); i++)
                    {
                        try { cv += overflowFile.GetValidCountInBlock(i); } catch { }
                    }
                }
                catch { }

                currentValueCount = cv;
            }
        }
        protected override byte[] GetExtraMetaBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(splitPointer));
            bytes.AddRange(BitConverter.GetBytes(level));
            bytes.AddRange(BitConverter.GetBytes(M));
            return bytes.ToArray();
        }
        public int CalculateMaxBlockIndex()
        {
            return splitPointer + M * PowerOfTwo(level);
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

/*
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
            while (d > dmax)
            {
                int value = Split(record);
                if (value > -1)
                {
                    lastAddress = value;
                }
                d = (double)currentValueCount / maxValueCount;
            }
            return lastAddress;
            /*
            while(d > dmax)
            {
                int newBlockIndex = base.CreateBlock();
                maxValueCount += base.GetRecordsPerBlock();
                var recordsToRehash = new List<T>();
                
                recordsToRehash.AddRange(base.GetAllRecordsInBlock(splitPointer));
                for (int r = 0; r < recordsToRehash.Count; r++)
                {
                    base.Delete(splitPointer, recordsToRehash[r]);
                }
                int ovAddr = base.GetNextBlockIndex(splitPointer);
                base.SetNextBlockIndex(splitPointer, -1);
                while (ovAddr != -1)
                {
                    var validRecords = overflowFile.GetAllRecordsInBlock(ovAddr);
                    recordsToRehash.AddRange(validRecords);
                    int nextOv = overflowFile.GetNextBlockIndex(ovAddr);
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
        */
        public int Insert(T record)
        {
            int lastAddress = -1;

            int rawHash = record.GetHashCode();
            if (rawHash == int.MinValue) rawHash = 0;
            rawHash = Math.Abs(rawHash);

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
                bool prevWasPrimary = true;
                int prevBlockIndex = hashValue;
                int overflowAddress;
                try
                {
                    overflowAddress = base.GetNextBlockIndex(hashValue);
                }
                catch
                {
                    overflowAddress = -1;
                }

                while (overflowAddress != -1)
                {
                    int res = overflowFile.Insert(record, overflowAddress);
                    if (res != -1)
                    {
                        inserted = true;
                        lastAddress = overflowAddress;
                        break;
                    }

                    prevBlockIndex = overflowAddress;
                    prevWasPrimary = false;
                    try
                    {
                        overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);
                    }
                    catch
                    {
                        overflowAddress = -1;
                    }
                }

                if (!inserted)
                {
                    int newOverflowAddress = overflowFile.Insert(record);
                    lastAddress = newOverflowAddress;

                    if (prevWasPrimary)
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
            while (d > dmax)
            {
                int value = Split(record);
                if (value > -1)
                {
                    lastAddress = value;
                }
                d = (double)currentValueCount / maxValueCount;
            }

            return lastAddress;
        }

        public int Split(T record)
        {
            int lastAddress = -1;

            int newBlockIndex = base.CreateBlock();
            maxValueCount += base.GetRecordsPerBlock();

            var recordsToRehash = new List<T>();
            var addressesToRehash = new List<int>();
            var existingOvBlocks = new List<Block<T>>();

            recordsToRehash.AddRange(base.GetAllRecordsInBlock(splitPointer));
            int ovAddr = base.GetNextBlockIndex(splitPointer);
            while (ovAddr != -1)
            {
                addressesToRehash.Add(ovAddr);
                var ovBlock = overflowFile.ReadBlock(ovAddr);
                existingOvBlocks.Add(ovBlock);
                recordsToRehash.AddRange(ovBlock.GetRecords().Take(ovBlock.GetValidCount()));
                try { ovAddr = overflowFile.GetNextBlockIndex(ovAddr); }
                catch { ovAddr = -1; }
            }

            int HuNew = M * PowerOfTwo(level + 1);
            var oldValues = new List<T>();
            var newValues = new List<T>();
            foreach (var rec in recordsToRehash)
            {
                int raw = rec.GetHashCode();
                if (raw == int.MinValue) raw = 0;
                raw = Math.Abs(raw);
                int hashVal = raw % HuNew;
                if (hashVal == splitPointer) oldValues.Add(rec);
                else newValues.Add(rec);
            }

            if (oldValues.Contains(record)) lastAddress = splitPointer;
            else if (newValues.Contains(record)) lastAddress = newBlockIndex;

            int primaryCapacity = GetRecordsPerBlock();
            int overflowCapacity = overflowFile.GetRecordsPerBlock();
            int oldOverflowNeeded = Math.Max(0, (oldValues.Count - primaryCapacity + Math.Max(0, overflowCapacity) - 1) / Math.Max(1, overflowCapacity));
            int newOverflowNeeded = Math.Max(0, (newValues.Count - primaryCapacity + Math.Max(0, overflowCapacity) - 1) / Math.Max(1, overflowCapacity));

            var primaryOldList = oldValues.Take(Math.Min(oldValues.Count, primaryCapacity)).ToList();
            var remainderOld = oldValues.Skip(primaryCapacity).ToList();
            var primaryNewList = newValues.Take(Math.Min(newValues.Count, primaryCapacity)).ToList();
            var remainderNew = newValues.Skip(primaryCapacity).ToList();

            int recordSize = Activator.CreateInstance<T>().GetSize();

            var primaryBlockOld = new Block<T>(this.blockSizeHash, recordSize, splitPointer);
            primaryBlockOld.EnableWriteNextBlockIndex();
            primaryBlockOld.EnableWriteChainLength();
            primaryBlockOld.OverwriteBlock(primaryOldList, primaryOldList.Count);
            primaryBlockOld.SetChainLength(oldOverflowNeeded);

            var primaryBlockNew = new Block<T>(this.blockSizeHash, recordSize, newBlockIndex);
            primaryBlockNew.EnableWriteNextBlockIndex();
            primaryBlockNew.EnableWriteChainLength();
            primaryBlockNew.OverwriteBlock(primaryNewList, primaryNewList.Count);
            primaryBlockNew.SetChainLength(newOverflowNeeded);

            var usedOldAddresses = new List<int>();
            int existingCount = addressesToRehash.Count;
            var oldChunks = new List<List<T>>();
            for (int i = 0; i < remainderOld.Count; i += overflowCapacity)
                oldChunks.Add(remainderOld.Skip(i).Take(Math.Min(overflowCapacity, remainderOld.Count - i)).ToList());

            for (int i = 0; i < oldChunks.Count; i++)
            {
                var slice = oldChunks[i];
                if (i < existingCount)
                {
                    int addr = addressesToRehash[i];
                    var ovBlock = existingOvBlocks[i]; 
                    ovBlock.OverwriteBlock(slice, slice.Count);
                    usedOldAddresses.Add(addr);
                }
                else
                {
                    int addr = overflowFile.Insert(slice[0]);
                    for (int j = 1; j < slice.Count; j++)
                        overflowFile.Insert(slice[j], addr);
                    usedOldAddresses.Add(addr);
                }
            }

            if (usedOldAddresses.Count == 0)
            {
                primaryBlockOld.SetNextBlockIndex(-1);
            }
            else
            {
                primaryBlockOld.SetNextBlockIndex(usedOldAddresses[0]);
            }

            for (int k = 0; k < usedOldAddresses.Count; k++)
            {
                int addr = usedOldAddresses[k];
                int nextAddr = (k + 1 < usedOldAddresses.Count) ? usedOldAddresses[k + 1] : -1;
                if (k < existingCount)
                {
                    var ovBlock = existingOvBlocks[k];
                    ovBlock.SetNextBlockIndex(nextAddr);
                    overflowFile.WriteBlock(ovBlock); 
                }
                else
                {
                    overflowFile.SetNextBlockIndex(addr, nextAddr);
                }
            }

            base.WriteBlock(primaryBlockOld);

            var usedNewAddresses = new List<int>();
            int startIndexForNew = Math.Min(existingCount, oldChunks.Count);
            var newChunks = new List<List<T>>();
            for (int i = 0; i < remainderNew.Count; i += overflowCapacity)
                newChunks.Add(remainderNew.Skip(i).Take(Math.Min(overflowCapacity, remainderNew.Count - i)).ToList());

            for (int i = 0; i < newChunks.Count; i++)
            {
                var slice = newChunks[i];
                int idxInOriginal = startIndexForNew + i;
                if (idxInOriginal < existingCount)
                {
                    int addr = addressesToRehash[idxInOriginal];
                    var ovBlock = existingOvBlocks[idxInOriginal];
                    ovBlock.OverwriteBlock(slice, slice.Count);
                    usedNewAddresses.Add(addr);
                }
                else
                {
                    int addr = overflowFile.Insert(slice[0]);
                    for (int j = 1; j < slice.Count; j++)
                        overflowFile.Insert(slice[j], addr);
                    usedNewAddresses.Add(addr);
                }
            }

            if (usedNewAddresses.Count == 0)
            {
                primaryBlockNew.SetNextBlockIndex(-1);
            }
            else
            {
                primaryBlockNew.SetNextBlockIndex(usedNewAddresses[0]);
            }

            for (int k = 0; k < usedNewAddresses.Count; k++)
            {
                int addr = usedNewAddresses[k];
                int nextAddr = (k + 1 < usedNewAddresses.Count) ? usedNewAddresses[k + 1] : -1;
                int idxInOriginal = startIndexForNew + k;
                if (idxInOriginal < existingCount)
                {
                    var ovBlock = existingOvBlocks[idxInOriginal];
                    ovBlock.SetNextBlockIndex(nextAddr);
                    overflowFile.WriteBlock(ovBlock);
                }
                else
                {
                    overflowFile.SetNextBlockIndex(addr, nextAddr);
                }
            }

            base.WriteBlock(primaryBlockNew);

            int consumed = Math.Min(existingCount, oldChunks.Count + newChunks.Count);
            if (consumed < addressesToRehash.Count)
            {
                for (int i = consumed; i < addressesToRehash.Count; i++)
                {
                    int addr = addressesToRehash[i];
                    overflowFile.DeleteBlockWithId(addr);
                    maxValueCount -= overflowFile.GetRecordsPerBlock();
                }
            }

            splitPointer++;
            int splitThreshold = M * PowerOfTwo(level);
            if (splitPointer >= splitThreshold)
            {
                splitPointer = 0;
                level++;
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
            int addressPrevious = -1;
            int addressCurrent = -1;
            int addressNext = -1;
            bool prevHashFirst = true;
            try
            {
                base.Delete(id, record);
                deleted = true;
            }
            catch (KeyNotFoundException)
            {
                int overflowAddress = base.GetNextBlockIndex(id);
                addressPrevious = id;
                while (overflowAddress != -1)
                {
                    try
                    {
                        overflowFile.Delete(overflowAddress, record);
                        deleted = true;
                        addressCurrent = overflowAddress;
                        try
                        {
                            addressNext = overflowFile.GetNextBlockIndex(overflowAddress);
                        } catch(ArgumentOutOfRangeException)
                        {
                            addressNext = -1;
                        }
                        
                        break;
                    }
                    catch (KeyNotFoundException)
                    {
                        addressPrevious = overflowAddress;
                        overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);
                        prevHashFirst = false;
                    }
                }
            }
            if (!deleted)
            {
                throw new KeyNotFoundException("Record not found in Linear Hash.");
            } else
            {
                base.IncrementDeletedCountInBlock(id);
                if(addressCurrent > -1)
                {
                    int validCount = -1;
                    try
                    {
                        validCount = overflowFile.GetValidCountInBlock(addressCurrent);
                    } catch
                    {
                        validCount = -1;
                    }
                    if (prevHashFirst && validCount <= 0)
                    {
                        base.SetNextBlockIndex(addressPrevious, addressNext);
                        
                    } else if(!prevHashFirst && validCount <= 0)
                    {
                        overflowFile.SetNextBlockIndex(addressPrevious, addressNext);
                        
                    }
                    try
                    {
                        overflowFile.SetNextBlockIndex(addressCurrent, -1);
                    } catch
                    {

                    }
                }
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
                var recordsToRehash = new List<T>();
                recordsToRehash.AddRange(base.GetAllRecordsInBlock(a));
                int ovAddr = base.GetNextBlockIndex(a);
                for (int r = 0; r < recordsToRehash.Count; r++)
                {
                    base.Delete(a, recordsToRehash[r]);
                }
                while (ovAddr != -1)
                {
                    var validRecords = overflowFile.GetAllRecordsInBlock(ovAddr);
                    recordsToRehash.AddRange(validRecords);
                    int nextOv = overflowFile.GetNextBlockIndex(ovAddr);
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
        public void Update(T record)
        {
            int Hu = M * PowerOfTwo(level);
            int id = record.GetHashCode() % Hu;
            if (id < splitPointer)
            {
                Hu = M * PowerOfTwo(level + 1);
                id = record.GetHashCode() % Hu;
            }
            bool updated = false;
            try
            {
                base.Update(id, record);
                updated = true;
            }
            catch (ArgumentOutOfRangeException)
            {
                int overflowAddress = base.GetNextBlockIndex(id);
                while (overflowAddress != -1)
                {
                    try
                    {
                        overflowFile.Update(overflowAddress, record);
                        updated = true;
                        break;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        overflowAddress = overflowFile.GetNextBlockIndex(overflowAddress);
                    }
                }
            }
            if (!updated)
            {
                throw new KeyNotFoundException("Record not found in Linear Hash.");
            }
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
            catch { }

            sb.AppendLine();
            sb.AppendLine("Primary blocks and overflow chains:");

            int primaryCount = 0;
            try { primaryCount = GetBlockCount(); } catch { primaryCount = 0; }

            for (int i = 0; i < primaryCount; i++)
            {
                sb.AppendLine($"-- Block {i} --");
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
            catch {  }

            return sb.ToString();

        }
        public override void Close()
        {
            base.Close();
            overflowFile.Close();
        }
    }
    
}
