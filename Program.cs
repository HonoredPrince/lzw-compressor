using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using SourceCode;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace SourceCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Console.Write("Filepath: ");
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                LZW compressor = new LZW();

                compressor.FilePath = Console.ReadLine();
                string filename = compressor.FilePath.Split('.')[0];
                string format = compressor.FilePath.Split('.')[1];

                string outputCompressedFilePath = "./CompressionOutput/OutputCompressed";
                string outputDecompressedFilePath = "./DecompressionOutput/OutputDecompressed" + $".{format}";

                stopwatch.Start();
                compressor.Compress(compressor.FilePath, outputCompressedFilePath);
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
