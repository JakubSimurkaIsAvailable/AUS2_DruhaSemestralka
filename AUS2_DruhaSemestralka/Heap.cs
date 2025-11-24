using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AUS2_DruhaSemestralka
{
    public class Heap<T> where T : IRecord
    {
        private const int BlokHeaderSize = 4;
        List<Blok> bloks = new List<Blok>();
        public List<Blok> freeBlocks = new List<Blok>();
        public List<Blok> notFullBlocks = new List<Blok>();
        private int blockSize;
        private readonly string filePath;
        private readonly string metaPath;
        private readonly int recordSize = Activator.CreateInstance<T>().GetSize();
        private readonly int recordsPerBlock;

        private FileStream mainStream;
        private FileStream metaStream;

        public Heap(int blokSize, string path)
        {
            blockSize = blokSize;
            filePath = path;
            if(blockSize <= BlokHeaderSize) 
                throw new ArgumentException("Block size must be greater than header size.");
            recordsPerBlock = (blockSize - BlokHeaderSize) / recordSize;

            LoadFromFile();
        }
        private void LoadFromFile()
        {
            mainStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            if(mainStream.Length > 0)
            {
                mainStream.Seek(0, SeekOrigin.Begin);
                long remaining = mainStream.Length;
                while (remaining >= 0)
                {
                    int toRead = (int)Math.Min(blockSize, remaining);
                    byte[] buffer = new byte[blockSize];
                    int read = 0;
                    while (read < toRead)
                    {
                        int r = mainStream.Read(buffer, read, toRead - read);
                        if (r == 0) break;
                        read += r;
                    }

                    var blok = new Blok(blockSize, recordSize);
                    blok.FromBytes(buffer);
                    bloks.Add(blok);

                    remaining = mainStream.Length - mainStream.Position;
                }
                
                metaStream = new FileStream(metaPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
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
                                if (blokIndex >= 0 && blokIndex < bloks.Count)
                                    freeBlocks.Add(bloks[blokIndex]);
                            }
                            int notFullCount = reader.ReadInt32();
                            for (int i = 0; i < notFullCount; i++)
                            {
                                int blokIndex = reader.ReadInt32();
                                if (blokIndex >= 0 && blokIndex < bloks.Count)
                                    notFullBlocks.Add(bloks[blokIndex]);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            freeBlocks.Clear();
                            notFullBlocks.Clear();
                        }
                    }
                }
            }
        }
        private Blok CreateNewBlock()
        {
            Blok newBlock = new Blok(blockSize, recordSize);
            bloks.Add(newBlock);
            freeBlocks.Add(newBlock);
            return newBlock;
        }

        public int Insert(T record)
        {
            Blok targetBlok;
            if(notFullBlocks.Count > 0)
            {
                targetBlok = notFullBlocks.First();
            } else if (freeBlocks.Count > 0)
            {
                targetBlok = freeBlocks.First();
                freeBlocks.RemoveAt(0);
                notFullBlocks.Add(targetBlok);
            } else
            {
                targetBlok = CreateNewBlock();
            }
            int recordId = targetBlok.InsertRecord(record);
            if(targetBlok.GetValidCount() == recordsPerBlock)
            {
                notFullBlocks.Remove(targetBlok);
            }
            int blockId = bloks.IndexOf(targetBlok);
            return blockId * recordsPerBlock + recordId;
        }

        public T Get(int id)
        {
            int blockId = id / recordsPerBlock;
            int recordId = id % recordsPerBlock;
            if(blockId < 0 || blockId >= bloks.Count)
            {
                throw new IndexOutOfRangeException("Invalid record ID");
            }
            IRecord record = bloks[blockId].GetRecord(recordId);

            return (T)record;
        }

        public void Delete(int id)
        {
            int blockId = id / recordsPerBlock;
            int recordId = id % recordsPerBlock;

            if (blockId < 0 || blockId >= bloks.Count)
            {
                throw new IndexOutOfRangeException("Block is out of range");
            }
            Blok blok = bloks[blockId];
            bool deleted = blok.DeleteRecord(recordId);
            if (!deleted)
            {
                throw new InvalidOperationException("Record does not exist.");
            }
            int validCount = blok.GetValidCount();
            if (validCount == 0)
            {
                if (!freeBlocks.Contains(blok))
                {
                    freeBlocks.Add(blok);
                }
                notFullBlocks.Remove(blok);
            }
            else if (validCount < recordsPerBlock)
            {
                if (!notFullBlocks.Contains(blok))
                {
                    notFullBlocks.Add(blok);
                }
            }
        }

        public void Close()
        {
            if(mainStream == null)
            {
                throw new InvalidOperationException("Heap is not open.");
            }
            mainStream.SetLength(0);
            mainStream.Seek(0, SeekOrigin.Begin);
            foreach(var blok in bloks)
            {
                byte[] data = blok.GetBytes();
                mainStream.Write(data, 0, data.Length);
            }
            mainStream.Close();
            if(metaStream == null)
            {
                throw new InvalidOperationException("Meta stream is not open.");
            }
            metaStream.SetLength(0);
            metaStream.Seek(0, SeekOrigin.Begin);
            using (var writer = new BinaryWriter(metaStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(freeBlocks.Count);
                foreach(var blok in freeBlocks)
                {
                    int index = bloks.IndexOf(blok);
                    writer.Write(index);
                }
                writer.Write(notFullBlocks.Count);
                foreach(var blok in notFullBlocks)
                {
                    int index = bloks.IndexOf(blok);
                    writer.Write(index);
                }
            }
            metaStream.Close();
            metaStream.Dispose();
            metaStream = null;

            mainStream.Dispose();
            mainStream = null;

        }
    }
}
