using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/*Classe interface para o compressor, 
possuí os métodos de compressão e descompressão recebendo os caminhos dos arquivos*/
namespace SourceCode
{
    public interface ICompressorAlgorithm
    {
        bool Compress(string pIntputFileName, string pOutputFileName);
        bool Decompress(string pIntputFileName, string pOutputFileName);
    }
}
