using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public class Heap<T> where T : IRecord
    {
        private const int BlokHeaderSize = 4;
        private int blockSize;
        private readonly string filePath;
        private readonly string metaPath;
        private readonly int recordSize = Activator.CreateInstance<T>().GetSize();
        private readonly int recordsPerBlock;
        private FileStream fileStream;
        private FileStream metaStream;

        private int blockCount = 0;
        private List<int> freeBlocks = new List<int>();
        private List<int> notFullBlocks = new List<int>();

        public Heap(int blokSize, string path, string metaPath)
        {
            blockSize = blokSize;
            filePath = path;
            this.metaPath = metaPath;
            recordsPerBlock = (blockSize - BlokHeaderSize) / recordSize;

            LoadFromFile();
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
                if (fileStream.Length % blockSize != 0)
                {
                    //throw new InvalidDataException("Heap file length is not a multiple of block size.");
                }
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
                using (var reader = new BinaryReader(metaStream, Encoding.UTF8, leaveOpen: true))
                {
                    try
                    {
                        int freeCount = reader.ReadInt32();
                        for (int i = 0; i < freeCount; i++)
                        {
                            int blokIndex = reader.ReadInt32();
                            if (blokIndex >= 0 && blokIndex < blockCount)
                                freeBlocks.Add(blokIndex);
                        }
                        int notFullCount = reader.ReadInt32();
                        for (int i = 0; i < notFullCount; i++)
                        {
                            int blokIndex = reader.ReadInt32();
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
        private Block<T> ReadBlock(int index)
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

        private void WriteBlock(Block<T> block)
        {
            byte[] data = block.GetBytes();
            if (data.Length != blockSize)
                throw new InvalidOperationException("Block.GetBytes produced unexpected size.");

            fileStream.Seek((long)block.GetIndex() * blockSize, SeekOrigin.Begin);
            fileStream.Write(data, 0, data.Length);
            fileStream.Flush();
        }

        public int Insert(T record)
        {
            Block<T> block;
            int index;

            if (notFullBlocks.Count > 0)
            {
                index = notFullBlocks.First();
                // read-modify-write
                block = ReadBlock(index);
                block.AddRecord(record);
                
                if (block.GetValidCount() == recordsPerBlock)
                {
                    // now full: remove from notFull list
                    notFullBlocks.RemoveAt(0);
                }
                WriteBlock(block);
            }
            else if (freeBlocks.Count > 0)
            {
                index = freeBlocks.First();
                block = ReadBlock(index);
                block.AddRecord(record);

                // remove from free, add to notFull
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
                // create new block at end
                index = blockCount;
                block = new Block<T>(blockSize, recordSize, index);
                block.AddRecord(record);

                // append by writing at the new position (will extend file)
                WriteBlock(block);
                blockCount++;
                if (block.GetValidCount() < recordsPerBlock)
                {
                    notFullBlocks.Add(index);
                    notFullBlocks.Sort();
                }
            }
            //freeBlocks.Sort();
            //notFullBlocks.Sort();
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

        public void Delete(int id, T record)
        {
            if (id < 0 || id >= blockCount) throw new ArgumentOutOfRangeException(nameof(id));
            var block = ReadBlock(id);
            var blockList = block.GetRecords();
            int foundIndex = blockList.FindIndex(r => r.Equals(record));
            if (foundIndex < 0) throw new ArgumentOutOfRangeException($"Record was not found in block {id}");

            // decrement valid count and swap removed with last valid element
            block.DecrementValidCount();
            int newValidCount = block.GetValidCount();

            if (newValidCount == 0)
            {
                // become free
                if (!freeBlocks.Contains(id))
                {
                    freeBlocks.Add(id);
                    freeBlocks.Sort();
                }
                // ensure it's not in notFull
                notFullBlocks.Remove(id);

                // truncate trailing free blocks (remove from file end)
                while (blockCount > 0 && freeBlocks.Count > 0 && freeBlocks.Last() == blockCount - 1)
                {
                    int tailIndex = blockCount - 1;
                    fileStream.SetLength((long)tailIndex * blockSize);
                    blockCount--;
                    freeBlocks.RemoveAt(freeBlocks.Count - 1);
                    notFullBlocks.Remove(tailIndex);
                }
            }
            else
            {
                // make sure index is in notFullBlocks
                if (!notFullBlocks.Contains(id))
                {
                    notFullBlocks.Add(id);
                    notFullBlocks.Sort();
                }

                // swap the removed item with the element at position newValidCount
                // (which previously held the "last valid" element)
                if (foundIndex != newValidCount)
                {
                    var temp = blockList[newValidCount];
                    blockList[newValidCount] = blockList[foundIndex];
                    blockList[foundIndex] = temp;
                }
            }

            // write modified block back (unless its been truncated away)
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

        public void Close()
        {
            if (fileStream == null)
            {
                throw new InvalidOperationException("Heap is not open.");
            }

            // ensure file length corresponds to current blockCount
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
            using (var writer = new BinaryWriter(metaStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(freeBlocks.Count);
                foreach (var blockIndex in freeBlocks)
                {
                    writer.Write(blockIndex);
                }
                writer.Write(notFullBlocks.Count);
                foreach (var blockIndex in notFullBlocks)
                {
                    writer.Write(blockIndex);
                }
            }

            metaStream.Close();
            metaStream.Dispose();
            metaStream = null;

            fileStream.Dispose();
            fileStream = null;
        }
    }
}

