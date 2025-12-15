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
            string pathOverflow, string metaOverflow, bool enableWriteNextBlockIndex = true, bool enableWriteChainLength = true, bool enableWriteDeleteDetails = true,
            bool manageFreeBlocks = false)
            : base(blokSizeHash, pathHash, metaPathHash, enableWriteNextBlockIndex, enableWriteChainLength, enableWriteDeleteDetails, manageFreeBlocks)
        {
            this.blockSizeHash = blokSizeHash;
            this.blockSizeOverflow = blockSizeOveflow;
            overflowFile = new Heap<T>(blockSizeOveflow, pathOverflow, metaOverflow, true, false, false);
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

            int address = -1;
            var primaryBlock = base.ReadBlock(hashValue);
            if (primaryBlock.GetValidCount() < base.GetRecordsPerBlock())
            {
                lastAddress = hashValue;
                base.Insert(record, hashValue);
                address = hashValue;
            }

            if (address == -1)
            {
                bool inserted = false;
                bool prevWasPrimary = true;
                int prevBlockIndex = hashValue;
                int overflowAddress;

                overflowAddress = primaryBlock.GetNextBlockIndex();
                Block<T>? overflowBlock = null;
                while (overflowAddress != -1)
                {
                    overflowBlock = overflowFile.ReadBlock(overflowAddress);
                    int res = -1;
                    if (overflowBlock.GetValidCount() < overflowFile.GetRecordsPerBlock())
                    {
                        res = overflowFile.Insert(record, overflowAddress);
                    }

                    if (res != -1)
                    {
                        inserted = true;
                        lastAddress = overflowAddress;
                        break;
                    }

                    prevBlockIndex = overflowAddress;
                    prevWasPrimary = false;
                    overflowAddress = overflowBlock.GetNextBlockIndex();

                }
                primaryBlock.SetLastIndexValidCount(primaryBlock.GetValidCount());
                if (!inserted)
                {
                    int newOverflowAddress = overflowFile.Insert(record);
                    lastAddress = newOverflowAddress;

                    if (prevWasPrimary)
                    {
                        primaryBlock.SetNextBlockIndex(newOverflowAddress);
                    }
                    else if (overflowBlock != null)
                    {
                        overflowBlock.SetNextBlockIndex(newOverflowAddress);
                        overflowFile.WriteBlock(overflowBlock);
                    }

                    maxValueCount += overflowFile.GetRecordsPerBlock();
                }
                base.WriteBlock(primaryBlock);
            }

            currentValueCount++;
            int primaryBlocks = base.GetBlockCount();
            int overflowBlocks = overflowFile.GetBlockCount() - overflowFile.GetFreeBlocks().Count();
            int result = (int)Math.Ceiling((primaryBlocks / 2 * Math.Log10(primaryBlocks))) + 1;
            while (overflowBlocks > result)
            {
                int value = Split(record);
                if (value > -1)
                {
                    lastAddress = value;
                }
                primaryBlocks = base.GetBlockCount();
                overflowBlocks = overflowFile.GetBlockCount() - overflowFile.GetFreeBlocks().Count();
                result = (int)Math.Ceiling((primaryBlocks / 2 * Math.Log10(primaryBlocks))) + 1;
            }
            /*
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
            */

            return lastAddress;
        }

        public int Split(T record)
        {
            int lastAddress = -1;

            int newBlockIndex = base.CreateBlock();
            maxValueCount += base.GetRecordsPerBlock();
            var spBlock = base.ReadBlock(splitPointer);
            var recordsToRehash = new List<T>();
            var addressesToRehash = new List<int>();
            var existingOvBlocks = new List<Block<T>>();

            recordsToRehash.AddRange(spBlock.GetRecords().Take(spBlock.GetValidCount()));
            int ovAddr = spBlock.GetNextBlockIndex();
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
            primaryBlockOld.EnableDeletedCount();
            primaryBlockOld.EnableLastIndexValidCount();
            primaryBlockOld.OverwriteBlock(primaryOldList, primaryOldList.Count);
            primaryBlockOld.SetChainLength(oldOverflowNeeded);

            var primaryBlockNew = new Block<T>(this.blockSizeHash, recordSize, newBlockIndex);
            primaryBlockNew.EnableWriteNextBlockIndex();
            primaryBlockNew.EnableWriteChainLength();
            primaryBlockNew.EnableDeletedCount();
            primaryBlockNew.EnableLastIndexValidCount();
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
                    int addr = overflowFile.InsertRecordsToBlock(slice);
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
            primaryBlockOld.SetLastIndexValidCount(usedOldAddresses.Count > 0 ? oldChunks[oldChunks.Count - 1].Count : 0);

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
                    int addr = overflowFile.InsertRecordsToBlock(slice);
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
                /*
                else
                {
                    overflowFile.SetNextBlockIndex(addr, nextAddr);
                }
                */
            }
            primaryBlockNew.SetLastIndexValidCount(usedNewAddresses.Count > 0 ? newChunks[newChunks.Count - 1].Count : 0);
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
            if (base.freeBlocks.Contains(newBlockIndex) && primaryBlockNew.GetValidCount() > 0)
            {
                base.freeBlocks.Remove(newBlockIndex);
                if (!base.notFullBlocks.Contains(newBlockIndex))
                {
                    base.notFullBlocks.Add(newBlockIndex);
                }
            }
            if(primaryBlockNew.GetValidCount() == recordsPerBlock && base.notFullBlocks.Contains(newBlockIndex))
            {
                base.notFullBlocks.Remove(newBlockIndex);
            }

            if (base.freeBlocks.Contains(splitPointer) && primaryBlockOld.GetValidCount() > 0)
            {
                base.freeBlocks.Remove(splitPointer);
                if (!base.notFullBlocks.Contains(splitPointer))
                {
                    base.notFullBlocks.Add(splitPointer);
                }
            }
            if (primaryBlockOld.GetValidCount() == recordsPerBlock && base.notFullBlocks.Contains(splitPointer))
            {
                base.notFullBlocks.Remove(splitPointer);
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
            var primaryBlock = base.ReadBlock(id);
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
                int overflowAddress = primaryBlock.GetNextBlockIndex();
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
                        }
                        catch (ArgumentOutOfRangeException)
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
            }

            currentValueCount--;
            if (primaryBlock.GetDeletedCount() >= primaryBlock.GetValidCount())
            {
                GetTogether(primaryBlock);
            }
            double d = (double)currentValueCount / maxValueCount;
            while (d < dmin && base.GetBlockCount() > M)
            {
                Merge();
                d = (double)currentValueCount / maxValueCount;
            }
        }

        private void GetTogether(Block<T> block)
        {
            // Collect primary records
            var combinedRecords = new List<T>();
            combinedRecords.AddRange(block.GetRecords().Take(block.GetValidCount()));

            // Gather overflow chain addresses and blocks
            var addressesToRehash = new List<int>();
            var existingOvBlocks = new List<Block<T>>();
            int ovAddr = block.GetNextBlockIndex();

            while (ovAddr != -1)
            {
                addressesToRehash.Add(ovAddr);
                var ovBlock = overflowFile.ReadBlock(ovAddr);
                existingOvBlocks.Add(ovBlock);
                combinedRecords.AddRange(ovBlock.GetRecords().Take(ovBlock.GetValidCount()));
                ovAddr = ovBlock.GetNextBlockIndex();
            }

            int primaryCapacity = GetRecordsPerBlock();
            int overflowCapacity = overflowFile.GetRecordsPerBlock();

            // Rebuild primary list and overflow chunks
            var primaryList = combinedRecords.Take(Math.Min(combinedRecords.Count, primaryCapacity)).ToList();
            var remainder = combinedRecords.Skip(primaryCapacity).ToList();

            var chunks = new List<List<T>>();
            for (int i = 0; i < remainder.Count; i += overflowCapacity)
                chunks.Add(remainder.Skip(i).Take(Math.Min(overflowCapacity, remainder.Count - i)).ToList());

            int neededOverflow = chunks.Count;
            int existingCount = addressesToRehash.Count;

            // Do not create new blocks: there must be at least as many existing overflow blocks as needed.
            if (neededOverflow > existingCount)
                throw new InvalidOperationException("Defragmentation would require creating overflow blocks; operation not allowed in this context.");

            int recordSize = Activator.CreateInstance<T>().GetSize();

            // Prepare and write primary block
            var primaryBlock = new Block<T>(this.blockSizeHash, recordSize, block.GetIndex());
            primaryBlock.EnableWriteNextBlockIndex();
            primaryBlock.EnableWriteChainLength();
            primaryBlock.EnableDeletedCount();
            primaryBlock.EnableLastIndexValidCount();
            primaryBlock.OverwriteBlock(primaryList, primaryList.Count);
            primaryBlock.SetChainLength(neededOverflow);
            if (neededOverflow == 0)
                primaryBlock.SetNextBlockIndex(-1);
            primaryBlock.FloorDeletedCount();
            // Reuse the first 'neededOverflow' existing overflow blocks and overwrite them with chunks.
            var usedAddresses = new List<int>();
            for (int i = 0; i < neededOverflow; i++)
            {
                int addr = addressesToRehash[i];
                var ovBlock = existingOvBlocks[i];
                var slice = chunks[i];
                ovBlock.OverwriteBlock(slice, slice.Count);
                usedAddresses.Add(addr);
            }
            primaryBlock.SetLastIndexValidCount(neededOverflow > 0 ? chunks[neededOverflow - 1].Count : 0);

            // Wire primary to first overflow (or -1)
            if (usedAddresses.Count > 0)
                primaryBlock.SetNextBlockIndex(usedAddresses[0]);

            base.WriteBlock(primaryBlock);

            // Update next pointers and persist overflow blocks used
            for (int k = 0; k < usedAddresses.Count; k++)
            {
                int addr = usedAddresses[k];
                int nextAddr = (k + 1 < usedAddresses.Count) ? usedAddresses[k + 1] : -1;
                var ovBlock = existingOvBlocks[k];
                ovBlock.SetNextBlockIndex(nextAddr);
                overflowFile.WriteBlock(ovBlock);
            }

            // Any original overflow blocks beyond those we reused become free -> delete them
            if (existingCount > neededOverflow)
            {
                for (int i = neededOverflow; i < existingCount; i++)
                {
                    int addrRem = addressesToRehash[i];
                    overflowFile.DeleteBlockWithId(addrRem);
                    maxValueCount -= overflowFile.GetRecordsPerBlock();
                }
            }
        }

        private void Merge()
        {
            int a;
            int b;
            if (splitPointer > 0)
            {
                b = splitPointer - 1;
                a = splitPointer + M * PowerOfTwo(level) - 1;

            }
            else
            {
                b = M * PowerOfTwo(level - 1) - 1;
                a = M * PowerOfTwo(level) - 1;
                level--;
            }

            // Merge block 'a' into block 'b' using an approach similar to Split():
            var combinedRecords = new List<T>();
            var addressesToRehash = new List<int>();
            var existingOvBlocks = new List<Block<T>>();

            // Read all records from block 'a' and its overflow chain
            combinedRecords.AddRange(base.GetAllRecordsInBlock(a));
            int ovAddr = base.GetNextBlockIndex(a);
            while (ovAddr != -1)
            {
                addressesToRehash.Add(ovAddr);
                var ovBlock = overflowFile.ReadBlock(ovAddr);
                existingOvBlocks.Add(ovBlock);
                combinedRecords.AddRange(ovBlock.GetRecords().Take(ovBlock.GetValidCount()));
                try { ovAddr = overflowFile.GetNextBlockIndex(ovAddr); }
                catch { ovAddr = -1; }
            }

            // Read all records from block 'b' and its overflow chain (we will rebuild b)
            var bPrimaryRecords = base.GetAllRecordsInBlock(b);
            combinedRecords.AddRange(bPrimaryRecords);
            int ovAddrB = base.GetNextBlockIndex(b);
            while (ovAddrB != -1)
            {
                addressesToRehash.Add(ovAddrB);
                var ovBlock = overflowFile.ReadBlock(ovAddrB);
                existingOvBlocks.Add(ovBlock);
                combinedRecords.AddRange(ovBlock.GetRecords().Take(ovBlock.GetValidCount()));
                try { ovAddrB = overflowFile.GetNextBlockIndex(ovAddrB); }
                catch { ovAddrB = -1; }
            }

            // Prepare primary block 'b' and overflow chunks
            int primaryCapacity = GetRecordsPerBlock();
            int overflowCapacity = overflowFile.GetRecordsPerBlock();

            var primaryBList = combinedRecords.Take(Math.Min(combinedRecords.Count, primaryCapacity)).ToList();
            var remainder = combinedRecords.Skip(primaryCapacity).ToList();

            int overflowNeeded = Math.Max(0, (remainder.Count - 1 + Math.Max(0, overflowCapacity)) / Math.Max(1, overflowCapacity));

            int recordSize = Activator.CreateInstance<T>().GetSize();

            var primaryBlockB = new Block<T>(this.blockSizeHash, recordSize, b);
            primaryBlockB.EnableWriteNextBlockIndex();
            primaryBlockB.EnableWriteChainLength();
            primaryBlockB.OverwriteBlock(primaryBList, primaryBList.Count);
            primaryBlockB.SetChainLength(overflowNeeded);

            var chunks = new List<List<T>>();
            for (int i = 0; i < remainder.Count; i += overflowCapacity)
                chunks.Add(remainder.Skip(i).Take(Math.Min(overflowCapacity, remainder.Count - i)).ToList());

            var usedAddresses = new List<int>();
            int existingCount = addressesToRehash.Count;

            for (int i = 0; i < chunks.Count; i++)
            {
                var slice = chunks[i];
                if (i < existingCount)
                {
                    int addr = addressesToRehash[i];
                    var ovBlock = existingOvBlocks[i];
                    ovBlock.OverwriteBlock(slice, slice.Count);
                    usedAddresses.Add(addr);
                }
                else
                {
                    int addr = overflowFile.InsertRecordsToBlock(slice);
                    usedAddresses.Add(addr);
                }
            }

            if (usedAddresses.Count == 0)
            {
                primaryBlockB.SetNextBlockIndex(-1);
            }
            else
            {
                primaryBlockB.SetNextBlockIndex(usedAddresses[0]);
            }

            base.WriteBlock(primaryBlockB);

            for (int k = 0; k < usedAddresses.Count; k++)
            {
                int addr = usedAddresses[k];
                int nextAddr = (k + 1 < usedAddresses.Count) ? usedAddresses[k + 1] : -1;
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

            int consumed = Math.Min(existingCount, chunks.Count);
            if (consumed < addressesToRehash.Count)
            {
                for (int i = consumed; i < addressesToRehash.Count; i++)
                {
                    int addrRem = addressesToRehash[i];
                    overflowFile.DeleteBlockWithId(addrRem);
                    maxValueCount -= overflowFile.GetRecordsPerBlock();
                }
            }

            base.DeleteBlockWithId(a);
            maxValueCount -= base.GetRecordsPerBlock();
            splitPointer = b;
        }


        public T OldGet(T record)
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
            var primaryBlock = base.ReadBlock(id);
            var records = primaryBlock.GetRecords().Take(primaryBlock.GetValidCount());
            value = records.FirstOrDefault(r => r.Equals(record));
            if (value != null)
            {
                return value;
            }
            int overflowAddress = primaryBlock.GetNextBlockIndex();
            while (overflowAddress != -1)
            {
                var overflowBlock = overflowFile.ReadBlock(overflowAddress);
                var ovRecords = overflowBlock.GetRecords().Take(overflowBlock.GetValidCount());
                value = ovRecords.FirstOrDefault(r => r.Equals(record));
                if (value != null)
                {
                    return value;
                }
                overflowAddress = overflowBlock.GetNextBlockIndex();
            }
            throw new KeyNotFoundException("Record not found in Linear Hash.");
        }

        public void OldUpdate(T record)
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
            var primaryBlock = base.ReadBlock(id);
            var records = primaryBlock.GetRecords().Take(primaryBlock.GetValidCount()).ToArray();
            for (int i = 0; i < primaryBlock.GetValidCount(); i++)
            {
                if (records.ElementAt(i).Equals(record))
                {
                    primaryBlock.GetRecords()[i] = record;
                    base.WriteBlock(primaryBlock);
                    updated = true;
                    break;
                }
            }
            int overflowAddress = primaryBlock.GetNextBlockIndex();
            while (overflowAddress != -1 && !updated)
            {
                var overflowBlock = overflowFile.ReadBlock(overflowAddress);
                var ovRecords = overflowBlock.GetRecords().Take(overflowBlock.GetValidCount()).ToArray();
                for (int i = 0; i < overflowBlock.GetValidCount(); i++)
                {
                    if (ovRecords.ElementAt(i).Equals(record))
                    {
                        overflowBlock.GetRecords()[i] = record;
                        overflowFile.WriteBlock(overflowBlock);
                        updated = true;
                        break;
                    }
                }
                overflowAddress = overflowBlock.GetNextBlockIndex();
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
            catch { }

            return sb.ToString();

        }
        public override void Close()
        {
            base.Close();
            overflowFile.Close();
        }

        public string BaseToString()
        {
            return base.ToString();
        }

        public string OverflowToString()
        {
            return overflowFile.ToString();
        }
    }

}
