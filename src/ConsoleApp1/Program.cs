using Alphaleonis.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
   class RootClass
   {
      public class NestedClass
      {
         public class SubNestedClass
         {

         }
      }
   }

   class Program
   {
      static void Main(string[] args)
      {
      
         string value = "P.Peter+Pelle`2[System.String,System.Int32]*[,,,,]&*[]";
         value = typeof(IDictionary<string, int>).AssemblyQualifiedName;

         Console.WriteLine($"Input = {value}");
         TypeIdentifier.TypeNameParser parser = new TypeIdentifier.TypeNameParser();
         var typeName = TypeIdentifier.TypeNameParser.Parse(value);
         Console.WriteLine();
         Console.WriteLine($"NamespaceName     = \"{typeName.NamespaceName}\"");
         Console.WriteLine($"NamespaceTypeName = \"{typeName.NamespaceTypeName}\"");

         Console.WriteLine("Generic arguments:");
         foreach (var genArg in typeName.GenericArguments)
         {
            Console.WriteLine(genArg.NamespaceTypeName);
         }
         Console.WriteLine();
         Console.WriteLine($"TypeFullName: {typeName.TypeFullName}");
         Console.WriteLine();
         Console.WriteLine($"AQN: {typeName.AssemblyQualifiedName}");
         Console.WriteLine();

         return;
         Type type = typeof(RootClass.NestedClass.SubNestedClass);

         type = type.MakePointerType();
         type = type.MakeArrayType(2);


         //Console.WriteLine(type.AssemblyQualifiedName);
         PrintType(type);
         Console.WriteLine();
         PrintType(type.GetElementType());
         Console.WriteLine();
         PrintType(type.GetElementType().GetElementType());

         //Console.WriteLine("Hello World!");
         //TypeIdentifier ti = new TypeIdentifier("alpha\\,leonis, ClassLibrary1");
         //Console.WriteLine(ti.TypeFullName);
         //ti.TypeFullName = "alpha,deonis, ClassLibrary1";
         //Console.WriteLine(ti.TypeFullName);

      }

      static void PrintType(Type type)
      {
         Console.WriteLine($"FullName:  {type.FullName}");
         Console.WriteLine($"Name:      {type.Name}");
         Console.WriteLine($"Namespace: {type.Namespace}");
         Console.WriteLine($"IsPointer: {type.IsPointer}");
         Console.WriteLine($"IsArray:   {type.IsArray}");
         Console.WriteLine($"IsByRef:   {type.IsByRef}");
         Console.WriteLine($"Declaring: {type.DeclaringType?.FullName}");
      }
   }




}
