using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SourceCode;
using SourceCodeUtils;

namespace Services
{
    public class Classificator
    {

        public static void LZWCompressionClassification()
        {
            System.Console.Write("Folder Path: ");
            try
            {
                Stopwatch stopwatch = new Stopwatch();

                string folderPath = Console.ReadLine();

                //Seguir o padrão de estutura com o formato sendo uma pasta
                FileAttributes pathAttr = File.GetAttributes(folderPath);

                if (!((pathAttr & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    throw new ArgumentException("Filepath passed is not a directory");
                }

                stopwatch.Start();

                string[] categoryFoldersPaths = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

                Dictionary<string, (int[], int[], int[])> categories = new Dictionary<string, (int[], int[], int[])>();
                List<string> testFilesPaths = new List<string>();

                foreach (var item in categoryFoldersPaths)
                {
                    var sets = GenerateCategorySets(item);

                    var mergedFilePath = MergeTrainingSetToOneFile(sets.trainerSet, "Content/ClassificationCompressionsInput/MergedTrainingSet");

                    categories.Add(item, GetMergedTrainingSetDictionaries(mergedFilePath));
                    testFilesPaths.Add(sets.testSet);
                }

                string temporaryFileHolderPath = "Content/ClassificationCompressionsOutput/TestFileCompressed";

                foreach (var testFilePath in testFilesPaths)
                {
                    Dictionary<string, long> testSetClassificationResults = new Dictionary<string, long>();

                    for (int i = 0; i < categories.Count(); i++)
                    {
                        LZWCompressorClassificator compressor = new LZWCompressorClassificator(categories.ElementAt(i).Value);

                        compressor.Compress(testFilePath, temporaryFileHolderPath);

                        long currentLength = new System.IO.FileInfo(temporaryFileHolderPath).Length;

                        testSetClassificationResults.Add(categories.ElementAt(i).Key, currentLength);
                    }

                    var minFileSize = testSetClassificationResults.Min(x => x.Value);
                    var categoryForMinSize = testSetClassificationResults.FirstOrDefault(x => x.Value == minFileSize).Key;

                    System.Console.WriteLine($"\n");
                    System.Console.WriteLine($"TEST FOR ----> {testFilePath}");
                    System.Console.WriteLine($"Size for lowest compressed file size: {minFileSize}, category filepath for that result: {categoryForMinSize}");
                    System.Console.WriteLine($"\n");
                }

                stopwatch.Stop();

                TimeSpan ts = stopwatch.Elapsed;

                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("Runtime:  " + elapsedTime);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Error on running the program: {e.Message}, please try again");
                return;
            }
        }

        public static (int[], int[], int[]) GetCompressionDictionaries(string filePath)
        {
            try
            {
                LZWCompressor compressor = new LZWCompressor();

                //Seguir o padrão de estutura com os formato dos arquivos 
                // Regex rgx = new Regex("^.*\\.(PGM|pgm|PNG|png|jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|mp4|txt)$");

                // if (!rgx.IsMatch(filePath))
                // {
                //     throw new ArgumentException();
                // }

                string outputCompressedFilePath = "Content/CompressionOutput/OutputCompressed";

                compressor.Compress(filePath, outputCompressedFilePath);

                return (compressor.GetCodeTable(), compressor.GetPrefixTable(), compressor.GetCharTable());
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Error on running the program: {e.Message}, please try again");
                return (null, null, null);
            }
        }

        public static (string[] trainerSet, string testSet) GenerateCategorySets(string folderPath)
        {
            //Atentar ao formato do arquivo
            string[] filesPaths = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);

            List<string> trainingSet = new List<string>();
            string testingSet = null;

            int trainerSetSize = filesPaths.Length - 1;

            Utils.Shuffle<string>(filesPaths);

            for (int i = 0; i < filesPaths.Length; i++)
            {
                if (i == filesPaths.Length - 1)
                {
                    testingSet = filesPaths[i];
                    break;
                }

                trainingSet.Add(filesPaths[i]);
            }

            return (trainingSet.ToArray(), testingSet);
        }

        public static string MergeTrainingSetToOneFile(string[] trainingSet, string outputPath)
        {
            string mergedTrainingSetFilesPath = outputPath;

            using (Stream destStream = File.OpenWrite(mergedTrainingSetFilesPath))
            {
                for (int i = 0; i < trainingSet.Length; i++)
                {
                    using (Stream srcStream = File.OpenRead(trainingSet[i]))
                    {
                        srcStream.CopyTo(destStream);
                    }
                }
            }

            return mergedTrainingSetFilesPath;
        }

        public static (int[], int[], int[]) GetMergedTrainingSetDictionaries(string mergedTrainingSetFilePath)
        {
            (int[], int[], int[]) dictionaryTuple = GetCompressionDictionaries(mergedTrainingSetFilePath);

            return dictionaryTuple;
        }
    }
}
