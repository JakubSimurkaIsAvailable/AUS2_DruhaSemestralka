using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap.Data
{
    public class NumberData : IRecord
    {
        public int Number { get; set; }
        public NumberData(int number)
        {
            Number = number;
        }
        public NumberData()
        {
        }

        public bool Equals(IRecord other)
        {
            return other is NumberData data && data.Number == Number;
        }

        public void FromBytes(byte[] bytes)
        {
            Number = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(Number);
        }

        public int GetSize()
        {
            return sizeof(int);
        }
    }
}
