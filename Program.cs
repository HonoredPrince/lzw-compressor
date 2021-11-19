using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Implementation;

namespace SourceCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CompressSingleFile();

            Classificator.LZWCompressionClassification();
        }

        public static void CompressSingleFile(string filePath = null)
        {
            if (filePath == null)
                System.Console.Write("Filepath: ");

            try
            {
                Stopwatch stopwatch = new Stopwatch();
                LZW compressor = new LZW();

                //Seguir o padrão de estutura com os formato dos arquivos 
                Regex rgx = new Regex("^.*\\.(PGM|pgm|PNG|png|jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|mp4|txt)$");

                if (filePath == null)
                    filePath = Console.ReadLine();

                if (!rgx.IsMatch(filePath))
                {
                    throw new ArgumentException();
                }

                string filename = filePath.Split('.')[0];
                string format = filePath.Split('.')[1];

                string outputCompressedFilePath = "./CompressionOutput/OutputCompressed";
                string outputDecompressedFilePath = "./DecompressionOutput/OutputDecompressed" + $".{format}";

                //Considerar o stopwatch apenas sem ser em modo de debug e na parte que chama o compressor e descompressor
                stopwatch.Start();
                compressor.Compress(filePath, outputCompressedFilePath);
                compressor.Decompress(outputCompressedFilePath, outputDecompressedFilePath);
                stopwatch.Stop();

                TimeSpan ts = stopwatch.Elapsed;

                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("Compression and Decompression Runtime:  " + elapsedTime);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Error on running the program: {e.Message}, please try again");
                return;
            }
        }
    }
}
