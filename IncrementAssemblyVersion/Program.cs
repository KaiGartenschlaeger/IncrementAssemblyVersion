using System;
using System.IO;
using System.Text.RegularExpressions;

namespace IncrementAssemblyVersion
{
    class Program
    {
        private const int MaxBuildVersion = 999;
        private static string AssemblyVersionPattern =
            "\\[(?:\\s+)?assembly(?:\\s+)?:(?:\\s+)?AssemblyVersion(?:\\s+)?\\(\"(?:\\s +)?(\\d+).(\\d+).(\\d+).(\\d+)\"(?:\\s+)?\\)(?:\\s+)?\\]";

        static int Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Syntax: IncrementAssemblyVersion.exe [root path of you solution]");
                return 1;
            }

            string applicationRootPath = args[0]; // @"D:\Desktop\IncrementAssemblyVersion\IncrementAssemblyVersion\";
            string pathAssemblyInfo = Path.Combine(applicationRootPath, @"Properties\", "AssemblyInfo.cs");

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("------------- IncrementAssemblyVersion -------------");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Path to solution: '" + applicationRootPath + "'");
            Console.WriteLine("File path: '" + pathAssemblyInfo + "'");

            FileInfo info = new FileInfo(pathAssemblyInfo);
            if (info.Exists)
            {
                bool removedReadOnlyFlag = false;
                if (info.IsReadOnly)
                {
                    Console.WriteLine("Remove ReadOnly Flag");

                    removedReadOnlyFlag = true;
                    File.SetAttributes(info.FullName, File.GetAttributes(info.FullName) & ~FileAttributes.ReadOnly);
                }

                string[] lines = File.ReadAllLines(info.FullName);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim().StartsWith("//"))
                    {
                        continue;
                    }

                    var assemblyVersionMatch = Regex.Match(lines[i], AssemblyVersionPattern);
                    if (assemblyVersionMatch.Success && assemblyVersionMatch.Groups.Count == 5)
                    {
                        Console.WriteLine($"Match for AssemblyVersion found on line {i + 1}");

                        int major = Convert.ToInt32(assemblyVersionMatch.Groups[1].Value);
                        int minor = Convert.ToInt32(assemblyVersionMatch.Groups[2].Value);
                        int build = Convert.ToInt32(assemblyVersionMatch.Groups[3].Value);
                        int revision = Convert.ToInt32(assemblyVersionMatch.Groups[4].Value);

                        Console.Write($"Change Version from {major}.{minor}.{build}.{revision}");

                        build++;
                        if (build > MaxBuildVersion)
                        {
                            minor++;
                            build = 0;
                        }

                        Console.WriteLine($" to {major}.{minor}.{build}.{revision}");

                        string newLineContent = $"[assembly: AssemblyVersion(\"{major}.{minor}.{build}.{revision}\")]";
                        lines[i] = lines[i].Replace(assemblyVersionMatch.Value, newLineContent);
                    }
                }

                Console.WriteLine("Save file");
                File.WriteAllLines(pathAssemblyInfo, lines);

                if (removedReadOnlyFlag)
                {
                    Console.WriteLine("Set ReadOnly flag");
                    File.SetAttributes(pathAssemblyInfo, File.GetAttributes(pathAssemblyInfo) | FileAttributes.ReadOnly);
                }

                return 0;
            }
            else
            {
                Console.WriteLine($"Cannot find the assembly info file '{pathAssemblyInfo}'");
                return 1;
            }
        }
    }
}