using Alphaleonis.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
   class Program
   {
      static void Main(string[] args)
      {
         string path = @"D:\Coding\Projects\GitHub\Alphaleonis.Reflection.Metadata\ClassLibrary1\bin\Debug\ClassLibrary1.dll";

         PrintInfo(path);
      }

      private static void PrintInfo(string path)
      {
         var asmInfo = BasicAssemblyInfo.GetBasicAssemblyInfo(path);
         Console.WriteLine(path);
         Console.WriteLine($"AssemblyName: {asmInfo.AssemblyName.FullName}");
         Console.WriteLine($"TargetFramework: {asmInfo.TargetFramework}");
         Console.WriteLine("References:");
         foreach (var reference in asmInfo.AssemblyReferences)
         {
            Console.WriteLine($"- {reference.FullName}");
         }
         Console.WriteLine();
      }
   }
}
