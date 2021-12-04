using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace S7DataBlockParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage: {System.AppDomain.CurrentDomain.FriendlyName} data_block.db");
                return;
            }

            var parser = new S7Parser();
            var filename = args[0];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"The file {filename} doesn't exist");
                return;
            }

            // TODO: avoid printing inside ParseFile method
            var dbs = parser.ParseFile(filename);
            if (dbs is null)
            {
                return;
            }

            foreach (var db in dbs)
            {
                Console.WriteLine($"DB NAME: {db.Name}");
                S7Parser.PrintOffsets(db.Name, 0, db.Fields);
            }
        }
    }
}