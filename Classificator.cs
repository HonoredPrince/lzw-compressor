using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SourceCode;

namespace Implementation
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

                string[] categoryFoldersPaths = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

                Dictionary<string, (int[], int[], int[])> categories = new Dictionary<string, (int[], int[], int[])>();
                List<string> testFilesPaths = new List<string>();

                foreach (var item in categoryFoldersPaths)
                {
                    var sets = GenerateSets(item);

                    categories.Add(item, GenerateCategoryModelDictionary(sets.trainerSet));
                    testFilesPaths.Add(sets.testSet);
                }

                // foreach (var item in categories)
                // {
                //     System.Console.WriteLine($"{item.Key} ------- {item.Value.Length}");
                // }

                // foreach (var item in testFilesPaths)
                // {
                //     System.Console.WriteLine($"{item}");
                // }

                string temporaryFileHolderPath = "./ClassificationCompressionsOutput/TestFileCompressed";

                foreach (var testFilePath in testFilesPaths)
                {
                    for (int i = 0; i < categories.Count(); i++)
                    {
                        LZWClassificator compressor = new LZWClassificator(categories.ElementAt(i).Value);
                        //LZW compressor = new LZW();

                        compressor.Compress(testFilePath, temporaryFileHolderPath);

                        long length = new System.IO.FileInfo(temporaryFileHolderPath).Length;
                        System.Console.WriteLine($"Size for {testFilePath} compressed file size: {length}, category filepath: {categories.ElementAt(i).Key}");
                    }
                }

                //Considerar o stopwatch apenas sem ser em modo de debug e na parte que chama o compressor e descompressor
                //stopwatch.Start();
                //stopwatch.Stop();

                // TimeSpan ts = stopwatch.Elapsed;

                // string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //     ts.Hours, ts.Minutes, ts.Seconds,
                //     ts.Milliseconds / 10);
                // Console.WriteLine("Runtime:  " + elapsedTime);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Error on running the program: {e.Message}, please try again");
                return;
            }
        }

        public static (int[], int[], int[]) GetCompressionDictonary(string filePath)
        {
            try
            {
                LZW compressor = new LZW();

                //Seguir o padrão de estutura com os formato dos arquivos 
                Regex rgx = new Regex("^.*\\.(PGM|pgm|PNG|png|jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|mp4|txt)$");

                if (!rgx.IsMatch(filePath))
                {
                    throw new ArgumentException();
                }

                string outputCompressedFilePath = "./CompressionOutput/OutputCompressed";

                compressor.Compress(filePath, outputCompressedFilePath);

                return (compressor.GetCodeTable(), compressor.GetPrefixTable(), compressor.GetCharTable());
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Error on running the program: {e.Message}, please try again");
                return (null, null, null);
            }
        }

        public static (string[] trainerSet, string testSet) GenerateSets(string folderPath)
        {
            List<string> trainingSet = new List<string>();
            string testingSet = null;

            string[] filesPaths = Directory.GetFiles(folderPath, "*.pgm", SearchOption.TopDirectoryOnly);

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

        public static (int[], int[], int[]) GenerateCategoryModelDictionary(string[] trainingSet)
        {
            (int[], int[], int[]) dictionaryTuple;

            string mergedTrainingSetFilesPath = "./ClassificationCompressionsInput/MergedTrainingSet.pgm";


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

            dictionaryTuple = GetCompressionDictonary(mergedTrainingSetFilesPath);

            //System.Console.WriteLine(aggregatedCodeTable.Length);
            return dictionaryTuple;
        }
    }
}
