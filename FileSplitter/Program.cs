using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace FileSplitter
{
    class Program
    { 
        private static int numberOfFilesProcessed = 0;

        static void Main(string[] args)
        {
            numberOfFilesProcessed = 0;

            var startTime = DateTime.Now;

            var sourceFilePath = args[0];

            if (File.Exists(sourceFilePath))
            {
                var numberOfLinesPerFile = 1;

                int.TryParse(args[1], out numberOfLinesPerFile);

                var directory = Path.GetDirectoryName(sourceFilePath);

                if (String.IsNullOrEmpty(directory))
                    directory = Directory.GetCurrentDirectory();

                var fileType = Path.GetExtension(sourceFilePath);

                var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

                var filePattern = String.Format("{0}\\{1}-[BATCH]{2}", directory, fileName, fileType);

                var lines = File.ReadLines(sourceFilePath);

                var numberOfBatches = (double)lines.Count() / numberOfLinesPerFile;

                var tasks = new List<Task>();

               
                for (int batchNumber = 0; batchNumber < numberOfBatches; batchNumber++)
                    tasks.Add(WriteLines(numberOfLinesPerFile, lines, filePattern, batchNumber));


                Task.WaitAll(tasks.ToArray());

                var timeTaken = DateTime.Now - startTime;

                Console.WriteLine(String.Format("{0} total files written in {1} seconds", numberOfFilesProcessed, timeTaken.TotalSeconds));


            }
            else
            {
                Console.WriteLine("File Not Found: " + sourceFilePath);
            }


#if DEBUG
            Console.ReadKey();
#endif


        }

        private static Task WriteLines(int numberOfLinesPerFile, IEnumerable<string> lines, string filePattern, int batchNumber)
        {
            return Task.Run(() =>
             { 
                 var outputFileName = filePattern.Replace("[BATCH]", (batchNumber + 1).ToString());

                 try
                 {
                     var startingLine = batchNumber * numberOfLinesPerFile;
                    
                     var lineBatch = lines.Skip(startingLine).Take(numberOfLinesPerFile);

                     File.WriteAllLines(outputFileName, lineBatch);

                     Console.WriteLine(String.Format("{0} lines written to {1}", lineBatch.Count(), outputFileName));
                     
                     numberOfFilesProcessed++;

                 }
                 catch (Exception ex)
                 {

                     Console.WriteLine(String.Format("Failed to write to file {0} with error: {1}", outputFileName, ex.Message));
                 }

             });
        }
    }
}
