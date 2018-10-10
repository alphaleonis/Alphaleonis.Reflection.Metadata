using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Alphaleonis.Reflection.Metadata.Tests
{
   public partial class TypeIdentifierTests
   {
      public class ExpectedData : IEquatable<ExpectedData>
      {
         public ExpectedData()
         {
         }

         public ExpectedData(TypeIdentifier identifier)
         {
            Name = identifier.Name;
            Namespace = identifier.Namespace;
            NamespaceTypeName = identifier.NamespaceTypeName;
            FullName = identifier.FullName;
            GenericArguments = identifier.GenericArguments.Select(arg => new ExpectedData(arg)).ToArray();
            AssemblyQualifiedName = identifier.AssemblyQualifiedName;
            AssemblyName = identifier.AssemblyName?.FullName;
            IsArray = identifier.IsArray;
            IsPointer = identifier.IsPointer;
            IsReference = identifier.IsReference;
            TypeSpecifiers = identifier.TypeSpecifiers.ToArray();
         }

         public ExpectedData(Type type)
         {
            Namespace = type.Namespace;
            FullName = type.FullName;
            GenericArguments = type.GetGenericArguments().Select(arg => new ExpectedData(arg)).ToArray();
            AssemblyQualifiedName = type.AssemblyQualifiedName;
            AssemblyName = type.Assembly.FullName;
            IsArray = type.IsArray;
            IsPointer = type.IsPointer;
            IsReference = type.IsByRef;

            List<TypeSpecifier> typeSpecifiers = new List<TypeSpecifier>();
            while (true)
            {
               if (type.IsArray)
               {
                  typeSpecifiers.Add(TypeSpecifier.Array(type.GetArrayRank()));
               }
               else if (type.IsPointer)
               {
                  typeSpecifiers.Add(TypeSpecifier.Pointer);
               }
               else if (type.IsByRef)
               {
                  typeSpecifiers.Add(TypeSpecifier.Reference);
               }
               else
               {
                  break;
               }

               type = type.GetElementType();
            }

            Name = type.Name;
            NamespaceTypeName = type.Namespace + "." + type.Name;
            TypeSpecifiers = typeSpecifiers.AsEnumerable().Reverse().ToArray();
         }

         

         public string Name { get; set; }
         public string Namespace { get; set; }
         public string NamespaceTypeName { get; set; }
         public string FullName { get; set; }
         public ExpectedData[] GenericArguments { get; set; }
         public string AssemblyQualifiedName { get; set; }
         public string AssemblyName { get; set; }
         public bool IsArray { get; set; }
         public bool IsPointer { get; set; }
         public bool IsReference { get; set; }
         public TypeSpecifier[] TypeSpecifiers { get; set; }

         public bool Equals(ExpectedData other)
         {
            Assert.Equal(Name, other.Name);
            Assert.Equal(Namespace, other.Namespace);
            Assert.Equal(NamespaceTypeName, other.NamespaceTypeName);
            Assert.Equal(FullName, other.FullName);
            Assert.Equal(GenericArguments.Length, other.GenericArguments.Length);
            for (int i = 0; i < GenericArguments.Length; i++)
            {
               Assert.Equal(GenericArguments[i], other.GenericArguments[i]);
            }
            Assert.Equal(AssemblyQualifiedName, other.AssemblyQualifiedName);
            Assert.Equal(AssemblyName, other.AssemblyName);
            Assert.Equal(IsArray, other.IsArray);
            Assert.Equal(IsPointer, other.IsPointer);
            Assert.Equal(IsReference, other.IsReference);
            Assert.Equal(TypeSpecifiers, other.TypeSpecifiers);
            return true;
         }

         public override bool Equals(object obj)
         {
            return obj is ExpectedData other && Equals(other);
         }

         public override int GetHashCode()
         {
            return 0;
         }
      }

      private static ExpectedData SystemString => new ExpectedData
      {
         Name = "String",
         Namespace = "System",
         NamespaceTypeName = "System.String",
         FullName = "System.String",
         GenericArguments = new ExpectedData[0],
         AssemblyQualifiedName = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         IsArray = false,
         IsPointer = false,
         IsReference = false,
         TypeSpecifiers = new TypeSpecifier[] { }
      };

      private static ExpectedData SystemInt => new ExpectedData
      {
         Name = "Int32",
         Namespace = "System",
         NamespaceTypeName = "System.Int32",
         FullName = "System.Int32",
         GenericArguments = new ExpectedData[0],
         AssemblyQualifiedName = "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         IsArray = false,
         IsPointer = false,
         IsReference = false,
         TypeSpecifiers = new TypeSpecifier[] { }
      };

      private static ExpectedData SystemIntArray => new ExpectedData
      {
         Name = "Int32",
         Namespace = "System",
         NamespaceTypeName = "System.Int32",
         FullName = "System.Int32[]",
         GenericArguments = new ExpectedData[0],
         AssemblyQualifiedName = "System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         IsArray = true,
         IsPointer = false,
         IsReference = false,
         TypeSpecifiers = new TypeSpecifier[] { TypeSpecifier.Array(1) }
      };

      private static ExpectedData SystemStackedArray => new ExpectedData
      {
         Name = "Int32",
         Namespace = "System",
         NamespaceTypeName = "System.Int32",
         FullName = "System.Int32[,,][]",
         GenericArguments = new ExpectedData[0],
         AssemblyQualifiedName = "System.Int32[,,][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         IsArray = true,
         IsPointer = false,
         IsReference = false,
         TypeSpecifiers = new TypeSpecifier[] { TypeSpecifier.Array(3), TypeSpecifier.Array(1) }
      };

      private static ExpectedData ListOfString => new ExpectedData
      {
         Name = "List`1",
         Namespace = "System.Collections.Generic",
         NamespaceTypeName = "System.Collections.Generic.List`1",
         FullName = "System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
         GenericArguments = new ExpectedData[] { SystemString },
         AssemblyQualifiedName = "System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
         IsArray = false,
         IsPointer = false,
         IsReference = false,
         TypeSpecifiers = new TypeSpecifier[] { }
      };

      public static IEnumerable<object[]> GetTestData()
      {
         

         yield return new object[] {
            "String",
            new ExpectedData
            {
               Name = "String",
               Namespace = null,
               NamespaceTypeName = "String",
               FullName = "String",
               GenericArguments = new ExpectedData[0],
               AssemblyQualifiedName = "String",
               AssemblyName = null,
               IsArray = false,
               IsPointer = false,
               IsReference = false,
               TypeSpecifiers = new TypeSpecifier[] { }
            }
         };

         yield return new object[] {
            "System.String",
            new ExpectedData
            {
               Name = "String",
               Namespace = "System",
               NamespaceTypeName = "System.String",
               FullName = "System.String",
               GenericArguments = new ExpectedData[0],
               AssemblyQualifiedName = "System.String",
               AssemblyName = null,
               IsArray = false,
               IsPointer = false,
               IsReference = false,
               TypeSpecifiers = new TypeSpecifier[] { }
            }
         };

         yield return new object[] {
            "First.Second.TypeName+Nested1+Nested2",
            new ExpectedData
            {
               Name = "Nested2",
               Namespace = "First.Second",
               NamespaceTypeName = "First.Second.TypeName+Nested1+Nested2",
               FullName = "First.Second.TypeName+Nested1+Nested2",
               GenericArguments = new ExpectedData[0],
               AssemblyQualifiedName = "First.Second.TypeName+Nested1+Nested2",
               AssemblyName = null,
               IsArray = false,
               IsPointer = false,
               IsReference = false,
               TypeSpecifiers = new TypeSpecifier[] { }
            }
         };

         // FullName of Dictionary<int, List<string>> 
         yield return new object[] {
            "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            new ExpectedData
            {
               Name = "Dictionary`2",
               Namespace = "System.Collections.Generic",
               NamespaceTypeName = "System.Collections.Generic.Dictionary`2",
               FullName = "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
               GenericArguments = new ExpectedData[] { SystemInt, ListOfString },
               AssemblyQualifiedName = "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
               AssemblyName = null,
               IsArray = false,
               IsPointer = false,
               IsReference = false,
               TypeSpecifiers = new TypeSpecifier[] { }
            }
         };

         // AssemblyQualifiedName of Dictionary<int, List<string>>
         yield return new object[] {
            "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            new ExpectedData
            {
               Name = "Dictionary`2",
               Namespace = "System.Collections.Generic",
               NamespaceTypeName = "System.Collections.Generic.Dictionary`2",
               FullName = "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
               GenericArguments = new ExpectedData[] { SystemInt, ListOfString },
               AssemblyQualifiedName = "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
               AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
               IsArray = false,
               IsPointer = false,
               IsReference = false,
               TypeSpecifiers = new TypeSpecifier[] { }
            }
         };

         yield return new object[] {
            "System.Int32[][,][,,]",
            new ExpectedData
            {
               Name = "Int32",
               Namespace = "System",
               NamespaceTypeName = "System.Int32",
               FullName = "System.Int32[][,][,,]",
               GenericArguments = new ExpectedData[0],
               AssemblyQualifiedName = "System.Int32[][,][,,]",
               AssemblyName = null,
               IsArray = true,
               IsPointer = false,
               IsReference = false,
               TypeSpecifiers = new TypeSpecifier[] { TypeSpecifier.Array(1), TypeSpecifier.Array(2), TypeSpecifier.Array(3) }
            }
         };


         // AssemblyQualifiedName of List<int[][,,]>[]         
         yield return new object[] {
            "System.Collections.Generic.List`1[[System.Int32[,,][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            new ExpectedData
            {
               Name = "List`1",
               Namespace = "System.Collections.Generic",
               NamespaceTypeName = "System.Collections.Generic.List`1",
               FullName = "System.Collections.Generic.List`1[[System.Int32[,,][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][]",
               GenericArguments = new ExpectedData[] { SystemStackedArray },
               AssemblyQualifiedName = "System.Collections.Generic.List`1[[System.Int32[,,][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
               AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
               IsArray = true,
               IsPointer = false,
               IsReference = false,
               TypeSpecifiers = new TypeSpecifier[] { TypeSpecifier.Array(1) }
            }
         };

         yield return new object[] {
            "System.Int32[,]*[*]*&, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            new ExpectedData
            {
               Name = "Int32",
               Namespace = "System",
               NamespaceTypeName = "System.Int32",
               FullName = "System.Int32[,]*[]*&",
               GenericArguments = new ExpectedData[0],
               AssemblyQualifiedName = "System.Int32[,]*[]*&, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
               AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
               IsArray = false,
               IsPointer = false,
               IsReference = true,
               TypeSpecifiers = new TypeSpecifier[] { TypeSpecifier.Array(2), TypeSpecifier.Pointer, TypeSpecifier.Array(1), TypeSpecifier.Pointer, TypeSpecifier.Reference }
            }
         };
      }
   }
}
