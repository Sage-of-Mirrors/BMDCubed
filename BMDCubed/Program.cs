using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;
using System.IO;

namespace BMDCubed
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFileName = "";
            string outputFileName = "";

            #region Get input/output filenames

            if (args.Length != 0)
            {
                if (args.Length == 1)
                {
                    inputFileName = args[0];
                }
                else if (args.Length == 2)
                {
                    inputFileName = args[0];
                    outputFileName = args[1];
                }
            }
            else
            {
                DisplayHelpMessage();
                return;
            }

            if (outputFileName == "")
            {
                outputFileName = string.Format("{0}\\{1}.bmd", Path.GetDirectoryName(inputFileName), 
                                                               Path.GetFileNameWithoutExtension(inputFileName));
            }

            #endregion

            Grendgine_Collada sourceModel = Grendgine_Collada.Grendgine_Load_File(inputFileName);


        }

        static void DisplayHelpMessage()
        {
            Console.WriteLine("BMDCubed written by Sage_of_Mirrors (@SageOfMirrors).");
            Console.WriteLine("Thanks to LordNed and all who came before us.");
            Console.WriteLine("Usage: BMDCubed input_file [output_file]");
        }
    }
}
