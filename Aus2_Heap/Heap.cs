using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public class Heap<T> where T : IRecord
    {
        private const int BlokHeaderSize = 2*sizeof(int);
        private int blockSize;
        private readonly string filePath;
        private readonly string metaPath;
        private readonly int recordSize = Activator.CreateInstance<T>().GetSize();
        public readonly int recordsPerBlock;
        private FileStream fileStream;
        private FileStream metaStream;
        private bool manageFreeBlocks;
        private int blockCount = 0;
        private List<int> freeBlocks = new List<int>();
        private List<int> notFullBlocks = new List<int>();
        private bool enableNotFullBlocksPrefference = true;

        public Heap(int blokSize, string path, string metaPath, bool manageFreeBlocks = true)
        {
            this.manageFreeBlocks = manageFreeBlocks;
            blockSize = blokSize;
            filePath = path;
            this.metaPath = metaPath;
            recordsPerBlock = (blockSize - BlokHeaderSize) / recordSize;

            LoadFromFile();
        }
        public void DisableNotFullBlocksPrefference()
        {
            enableNotFullBlocksPrefference = false;
        }
        public int GetRecordsPerBlock()
        {
            return recordsPerBlock;
        }
        public int GetBlockCount()
        {
            return blockCount;
        }
        public List<int> GetFreeBlocks()
        {
            return freeBlocks;
        }
        public List<int> GetNotFullBlocks()
        {
            return notFullBlocks;
        }
        private void LoadFromFile()
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
            }

            if (!string.IsNullOrEmpty(metaPath))
            {
                var metaDir = Path.GetDirectoryName(metaPath);
                if (!string.IsNullOrEmpty(metaDir))
                    Directory.CreateDirectory(metaDir);
            }
            fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            if (fileStream.Length > 0)
            {
                blockCount = (int)(fileStream.Length / blockSize);
            }
            else
            {
                blockCount = 0;
            }

            metaStream = new FileStream(metaPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            freeBlocks.Clear();
            notFullBlocks.Clear();
            if (metaStream.Length > 0)
            {
                metaStream.Seek(0, SeekOrigin.Begin);
                byte[] buffer = new byte[metaStream.Length];
                int read = 0;
                while (read < buffer.Length)
                {
                    int r = metaStream.Read(buffer, read, buffer.Length - read);
                    if (r == 0) break;
                    read += r;
                }

                int offset = 0;
                try
                {
                  
                    int freeCount = BitConverter.ToInt32(buffer, offset);
                    offset += 4;

                    for (int i = 0; i < freeCount; i++)
                    {
                        
                        int blokIndex = BitConverter.ToInt32(buffer, offset);
                        offset += 4;
                        if (blokIndex >= 0 && blokIndex < blockCount)
                            freeBlocks.Add(blokIndex);
                    }
                    int notFullCount = BitConverter.ToInt32(buffer, offset);
                    offset += 4;

                    for (int i = 0; i < notFullCount; i++)
                    {
                        
                        int blokIndex = BitConverter.ToInt32(buffer, offset);
                        offset += 4;
                        if (blokIndex >= 0 && blokIndex < blockCount)
                            notFullBlocks.Add(blokIndex);
                    }
                }
                catch (EndOfStreamException)
                {
                    freeBlocks.Clear();
                    notFullBlocks.Clear();
                }
            }
            freeBlocks.Sort();
            notFullBlocks.Sort();
        }
        public int Seek()
        {
            if (notFullBlocks.Count > 0)
            {
                return notFullBlocks.First();
            }
            else if (freeBlocks.Count > 0)
            {
                return freeBlocks.First();
            }
            return blockCount;
        }
        protected Block<T> ReadBlock(int index)
        {
            if (index < 0 || index >= blockCount) throw new ArgumentOutOfRangeException(nameof(index));
            byte[] buffer = new byte[blockSize];
            fileStream.Seek((long)index * blockSize, SeekOrigin.Begin);
            int read = 0;
            while (read < blockSize)
            {
                int r = fileStream.Read(buffer, read, blockSize - read);
                if (r == 0) break;
                read += r;
            }
            var block = new Block<T>(blockSize, recordSize, index);
            block.FromBytes(buffer);
            return block;
        }

        protected void WriteBlock(Block<T> block)
        {
            byte[] data = block.GetBytes();
            if (data.Length != blockSize)
                throw new InvalidOperationException("Block.GetBytes produced unexpected size.");

            fileStream.Seek((long)block.GetIndex() * blockSize, SeekOrigin.Begin);
            fileStream.Write(data, 0, data.Length);
            fileStream.Flush();
        }

        public int Insert(T record, int? insertIndex = null)
        {
           
            Block<T> block;
            int index;
            if(insertIndex != null)
            {
                index = insertIndex.Value;
                if (index < 0 || index >= blockCount) throw new ArgumentOutOfRangeException(nameof(index));
                block = ReadBlock(index);
                try
                {
                    
                    block.AddRecord(record);
                } catch (InvalidOperationException e)
                {
                    return -1;
                }
                
                if (block.GetValidCount() == recordsPerBlock)
                {
                    notFullBlocks.Remove(index);
                }
                else
                {
                    if (!notFullBlocks.Contains(index))
                    {
                        notFullBlocks.Add(index);
                        notFullBlocks.Sort();
                    }
                }
                WriteBlock(block);
                return index;
            }
            if (notFullBlocks.Count > 0 && enableNotFullBlocksPrefference)
            {
                index = notFullBlocks.First();
                block = ReadBlock(index);
                block.AddRecord(record);
                
                if (block.GetValidCount() == recordsPerBlock)
                {
                    notFullBlocks.RemoveAt(0);
                }
                WriteBlock(block);
            }
            else if (freeBlocks.Count > 0)
            {
                index = freeBlocks.First();
                block = ReadBlock(index);
                block.AddRecord(record);

                freeBlocks.RemoveAt(0);
                if (block.GetValidCount() < recordsPerBlock)
                {
                    notFullBlocks.Add(index);
                    notFullBlocks.Sort();
                }
                WriteBlock(block);
            }
            else
            {
                index = blockCount;
                block = new Block<T>(blockSize, recordSize, index);
                block.AddRecord(record);

                WriteBlock(block);
                blockCount++;
                if (block.GetValidCount() < recordsPerBlock)
                {
                    notFullBlocks.Add(index);
                    notFullBlocks.Sort();
                }
            }
            return index;
            
        }

        public T Get(int id, T record)
        {
            var block = ReadBlock(id);
            var value = block.GetRecords().Take(block.GetValidCount()).FirstOrDefault(r => r.Equals(record));
            if (value is null)
            {
                throw new ArgumentOutOfRangeException($"Record was not found in block {id}");
            }
            return value;
        }
        public int GetValidCountInBlock(int id)
        {
            if (id < 0 || id >= blockCount) throw new ArgumentOutOfRangeException(nameof(id));
            var block = ReadBlock(id);
            return block.GetValidCount();
        }
        public int GetDeletedCountInBlock(int id)
        {
            if (id < 0 || id >= blockCount) throw new ArgumentOutOfRangeException(nameof(id));
            var block = ReadBlock(id);
            return block.GetDeletedCount();
        }
        public void IncrementDeletedCountInBlock(int id)
        {
            if (id < 0 || id >= blockCount) throw new ArgumentOutOfRangeException(nameof(id));
            var block = ReadBlock(id);
            block.IncrementDeletedCount();
            WriteBlock(block);
        }

        public void FloorDeletedCountInBlock(int id)
        {
            if (id < 0 || id >= blockCount) throw new ArgumentOutOfRangeException(nameof(id));
            var block = ReadBlock(id);
            block.FloorDeletedCount();
            WriteBlock(block);
        }

        public void Delete(int id, T record)
        {
            if (id < 0 || id >= blockCount) throw new ArgumentOutOfRangeException(nameof(id));
            var block = ReadBlock(id);
            var blockList = block.GetRecords();
            int foundIndex = blockList.FindIndex(r => r.Equals(record));
            if (foundIndex < 0) throw new KeyNotFoundException($"Record was not found in block {id}");

            block.DecrementValidCount();
            int newValidCount = block.GetValidCount();

            if (newValidCount == 0)
            {
                if (!freeBlocks.Contains(id))
                {
                    freeBlocks.Add(id);
                    freeBlocks.Sort();
                }
                notFullBlocks.Remove(id);
                int tailIndex = blockCount; 
                while (blockCount > 0 && freeBlocks.Count > 0 && freeBlocks.Last() == blockCount - 1 && manageFreeBlocks)
                {
                    tailIndex = blockCount - 1;
                    blockCount--;
                    freeBlocks.RemoveAt(freeBlocks.Count - 1);
                    //notFullBlocks.Remove(tailIndex);
                }
                fileStream.SetLength((long)tailIndex * blockSize);
            }
            else
            {
                if (!notFullBlocks.Contains(id))
                {
                    notFullBlocks.Add(id);
                    notFullBlocks.Sort();
                }

                if (foundIndex != newValidCount)
                {
                    var temp = blockList[newValidCount];
                    blockList[newValidCount] = blockList[foundIndex];
                    blockList[foundIndex] = temp;
                }
            }

            if (id < blockCount)
            {
                WriteBlock(block);
            }
        }
        public List<T> GetAllRecordsInBlock(int id)
        {
            if (id < 0 || id >= blockCount) throw new ArgumentOutOfRangeException(nameof(id));
            var block = ReadBlock(id);
            return block.GetRecords().Take(block.GetValidCount()).ToList();
        }
        public int GetNextBlockIndex(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex >= blockCount) throw new ArgumentOutOfRangeException(nameof(currentIndex));
            var block = ReadBlock(currentIndex);
            return block.GetNextBlockIndex();
        }
        public void SetNextBlockIndex(int currentIndex, int nextIndex)
        {
            if (currentIndex < 0 || currentIndex >= blockCount) throw new ArgumentOutOfRangeException(nameof(currentIndex));
            var block = ReadBlock(currentIndex);
            block.SetNextBlockIndex(nextIndex);
            WriteBlock(block);
        }
        protected int CreateBlock()
        {
            var block = new Block<T>(blockSize, recordSize, blockCount);
            WriteBlock(block);
            blockCount++;
            freeBlocks.Add(block.GetIndex());
            freeBlocks.Sort();
            return block.GetIndex();
        }
        protected void DeleteBlock(int index)
        {
            if (index < 0 || index >= blockCount) throw new ArgumentOutOfRangeException(nameof(index));
            int tailIndex = blockCount; 
            int validCount = ReadBlock(index).GetValidCount();
            if (validCount > 0)
            {
                throw new InvalidOperationException($"There are still records inside block {index}");
            }
            if (blockCount > 0)
            {
                tailIndex = blockCount - 1;
                blockCount--;
                freeBlocks.RemoveAt(freeBlocks.Count - 1);
            }
            fileStream.SetLength((long)tailIndex * blockSize);
        }

        public void Close()
        {
            if (fileStream == null)
            {
                throw new InvalidOperationException("Heap is not open.");
            }
            fileStream.SetLength((long)blockCount * blockSize);
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Flush();
            fileStream.Close();

            if (metaStream == null)
            {
                throw new InvalidOperationException("Meta stream is not open.");
            }

            metaStream.SetLength(0);
            metaStream.Seek(0, SeekOrigin.Begin);

            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(freeBlocks.Count));
            foreach (var blockIndex in freeBlocks)
            {
                bytes.AddRange(BitConverter.GetBytes(blockIndex));
            }
            bytes.AddRange(BitConverter.GetBytes(notFullBlocks.Count));
            foreach (var blockIndex in notFullBlocks)
            {
                bytes.AddRange(BitConverter.GetBytes(blockIndex));
            }
            var outArray = bytes.ToArray();
            metaStream.Write(outArray, 0, outArray.Length);
            metaStream.Flush();

            metaStream.Close();
            metaStream.Dispose();
            metaStream = null;

            fileStream.Dispose();
            fileStream = null;
        }
    }
}

