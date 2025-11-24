using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aus2_Heap
{
    public interface IRecord : IBytes
    {
        bool Equals(IRecord other);
    }
}
