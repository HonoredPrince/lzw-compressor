using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceCode
{
    public interface ICompressorAlgorithm
    {
        bool Compress(string pIntputFileName, string pOutputFileName);
        bool Decompress(string pIntputFileName, string pOutputFileName);
    }
}
