using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AUS2_DruhaSemestralka
{
    public interface IBytes
    {
        int GetSize();
        byte[] GetBytes();
        void FromBytes(byte[] bytes);
    }
}
