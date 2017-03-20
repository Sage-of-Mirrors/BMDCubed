using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using System.IO;
using BMDCubed.src;
using GameFormatReader.Common;

namespace BMDCubed
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFileName = "";
            string outputFileName = "";

            #region Get input/output filenames

            // The user supplied at least an input file
            if (args.Length != 0)
            {
                // Just an input file
                if (args.Length == 1)
                {
                    inputFileName = args[0];
                }
                // Input file and a file name to output to
                else if (args.Length == 2)
                {
                    inputFileName = args[0];
                    outputFileName = args[1];
                }
            }
            // User didn't give any files to convert, display help instead
            else
            {
                DisplayHelpMessage();
                return;
            }

            // Output file name wasn't set. So we'll just make it the input file name and replace its extension with .bmd
            if (outputFileName == "")
            {
                outputFileName = string.Format("{0}\\{1}.bmd", Path.GetDirectoryName(inputFileName), 
                                                               Path.GetFileNameWithoutExtension(inputFileName));
            }

            #endregion

            Grendgine_Collada sourceModel = Grendgine_Collada.Grendgine_Load_File(inputFileName);
            BMDManager manager = new BMDManager(sourceModel);

            using (FileStream stream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                manager.WriteBMD(writer);
            }
        }

        static void DisplayHelpMessage()
        {
            Console.WriteLine("BMDCubed written by Sage_of_Mirrors (@SageOfMirrors) and Lord Ned (@LordNed).");
            Console.WriteLine("Special thanks to Shin/Kaio for making models to test with.");
            Console.WriteLine("Thanks to those who came before us.");
            Console.WriteLine("Usage: BMDCubed input_file [output_file]");
        }
    }
}
