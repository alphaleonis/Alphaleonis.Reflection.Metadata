﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alphaleonis.Reflection.Metadata
{
   public enum TypeSpecifierKind
   {
      Pointer,
      Reference,
      Array
   }

   public struct TypeSpecifier
   {
      public static readonly TypeSpecifier Reference = new TypeSpecifier(TypeSpecifierKind.Reference, 0);
      public static readonly TypeSpecifier Pointer = new TypeSpecifier(TypeSpecifierKind.Pointer, 0);
      public static TypeSpecifier Array(int rank) => new TypeSpecifier(TypeSpecifierKind.Array, rank);

      private TypeSpecifier(TypeSpecifierKind kind, int arrayRank)
      {
         Kind = kind;
         ArrayRank = arrayRank;
      }

      public TypeSpecifierKind Kind { get; }
      public int ArrayRank { get; }

      public override string ToString()
      {
         switch (Kind)
         {
            case TypeSpecifierKind.Pointer:
               return "*";               
            case TypeSpecifierKind.Reference:
               return "&";               
            case TypeSpecifierKind.Array:
               if (ArrayRank == 1)
               {
                  return "[]";
               }
               else
               {
                  StringBuilder sb = new StringBuilder();
                  sb.Append('[');
                  for (int i = 1; i < ArrayRank; i++)
                  {
                     sb.Append(',');
                  }
                  sb.Append(']');
                  return sb.ToString();
               }

            default:
               return "<unknown specifier>";
         }
      }
   }

   public interface ITypeName
   {
      AssemblyName AssemblyName { get; set; }

      string AssemblyQualifiedName { get; }

      // My.Namespace.Specifier
      string NamespaceName { get; set; }

      // My.Namespace.Specifier.Root+Nested+Sub
      string NamespaceTypeName { get; set; }

      // My.Namespace.Specifier.Root+Nested+Sub[,]*
      string TypeFullName { get; set; }
      
      // My.Namespace.Specifier.Root+Nested ????
      ITypeName GetDeclaringType();

      IList<TypeSpecifier> TypeSpecifiers { get; }

      bool IsPointer { get; }

      bool IsReference { get; }

      bool IsArray { get; }

      int ArrayRank { get; }

      ITypeName GetElementType();
   }
   public class TypeName
   {
      private struct NamespaceTypeNameEntry
      {
         public NamespaceTypeNameEntry(string namespaceSpec, IReadOnlyList<string> nestedTypeName)
         {
            NamespaceSpec = namespaceSpec;
            NestedTypeName = nestedTypeName;
         }

         public string NamespaceSpec;
         public IReadOnlyList<string> NestedTypeName { get; }

      }

      private readonly NamespaceTypeNameEntry m_namespaceTypeName;
      private readonly IReadOnlyList<TypeSpecifier> m_typeSpecifiers;
      private readonly IList<TypeName> m_genericArguments;

      private TypeName(NamespaceTypeNameEntry namespaceTypeName, IReadOnlyList<TypeSpecifier> typeSpecifiers, IList<TypeName> genericArguments, AssemblyName assemblyName)
      {
         m_typeSpecifiers = typeSpecifiers;
         m_namespaceTypeName = namespaceTypeName;
         m_genericArguments = genericArguments;
         AssemblyName = assemblyName;
      }

      public AssemblyName AssemblyName { get; set; }

      public string NamespaceName
      {
         get => m_namespaceTypeName.NamespaceSpec;

         set
         {
            throw new NotImplementedException();
         }
      }
      

      // My.Namespace.Specifier.Root+Nested+Sub
      public string NamespaceTypeName
      {
         get
         {
            if (String.IsNullOrEmpty(m_namespaceTypeName.NamespaceSpec))
               return m_namespaceTypeName.NestedTypeName.FirstOrDefault();

            return $"{m_namespaceTypeName.NamespaceSpec}.{String.Join("+", m_namespaceTypeName.NestedTypeName)}";
         }

         set
         {
            throw new NotImplementedException();
         }
      }

      public string AssemblyQualifiedName
      {
         get
         {
            StringBuilder sb = new StringBuilder();
            BuildTypeFullName(sb);
            if (AssemblyName != null)
            {
               sb.Append(", ");
               sb.Append(AssemblyName.FullName);
            }

            return sb.ToString();
         }

         set
         {
            throw new NotImplementedException();
         }
      }
      public IList<TypeName> GenericArguments => m_genericArguments;

      // My.Namespace.Specifier.Root+Nested+Sub[,]*
      public string TypeFullName
      {
         get
         {
            StringBuilder result = new StringBuilder();
            BuildTypeFullName(result);
            return result.ToString();
         }

         set
         {
            throw new NotImplementedException();
         }
      }

      private void BuildTypeFullName(StringBuilder result)
      {
         result.Append(NamespaceTypeName);

         if (GenericArguments.Count > 0)
         {
            result.Append('[');

            for (int i = 0; i < GenericArguments.Count; i++)
            {
               if (i > 0)
                  result.Append(',');

               result.Append('[');
               result.Append(GenericArguments[i].AssemblyQualifiedName);
               result.Append(']');
            }


            result.Append(']');
         }

         if (m_typeSpecifiers != null)
         {
            foreach (var specifier in m_typeSpecifiers)
            {
               result.Append(specifier.ToString());
            }
         }
      }

      public class TypeNameParser
      {


         private static TypeName Parse(CharReader reader, bool fullyQualified)
         {
            var result = ParseNamespaceTypeName(reader, true);

            Console.WriteLine($"Namespace: \"{result.NamespaceSpec}\"");
            for (int i = 0; i < result.NestedTypeName.Count; i++)
            {
               Console.WriteLine($"Nested: {result.NestedTypeName[i]}");
            }

            IList<TypeName> genericArguments;
            int la1 = reader.Peek(1);
            if (reader.Peek() == '[' && la1 != ',' && la1 != '*' && la1 != ']')
               genericArguments = ParseGenericArguments(reader);
            else
               genericArguments = new List<TypeName>();

            IReadOnlyList<TypeSpecifier> spec = ParseRefPtrArrSpec(reader);
            if (spec == null)
            {
               Console.WriteLine("No Type Specifiers");
            }
            else
            {
               Console.WriteLine("Specifiers:");
               foreach (var ss in spec)
               {
                  Console.WriteLine(ss);
               }
            }

            AssemblyName assemblyName = null;
            if (fullyQualified && reader.Peek() == ',')
            {
               reader.Read();
               SkipWhitespace(reader);
               assemblyName = ParseAssemblyName(reader);
            }
            // TODO PP (2018-10-03): Allow trailing?
            
            return new TypeName(result, spec, genericArguments, assemblyName);
         }

         private static AssemblyName ParseAssemblyName(CharReader reader)
         {
            StringBuilder assemblyName = new StringBuilder();
            if (ConsumeAssemblyName(reader, assemblyName) == 0)
               throw new ArgumentException("Invalid type name; Expected assembly name.");

            ConsumeWhitespace(reader, assemblyName);

            if (reader.Peek() == ',')
            {
               assemblyName.Append((char)reader.Read());
               ConsumeWhitespace(reader, assemblyName);
               ConsumeAssemblyNameProperties(reader, assemblyName);
            }

            return new AssemblyName(assemblyName.ToString());
         }

         private static void ConsumeAssemblyNameProperties(CharReader reader, StringBuilder assemblyName)
         {
            ConsumeWhitespace(reader, assemblyName);
            while (ConsumeAssemblyNamePropertyName(reader, assemblyName))
            {
               ConsumeWhitespace(reader, assemblyName);
               if (reader.Peek() != '=')
                  throw new ArgumentException($"Invalid type name; Missing value for assembly name property.");
               assemblyName.Append((char)reader.Read()); // Consume the equal sign.
               ConsumeAssemblyNamePropertyValue(reader, assemblyName);
               ConsumeWhitespace(reader, assemblyName);
               if (reader.Peek() != ',')
                  break;

               // Consume the comma.
               assemblyName.Append((char)reader.Read());
            }
         }

         private static void ConsumeAssemblyNamePropertyValue(CharReader reader, StringBuilder target)
         {
            if (reader.Peek() == '\"')
               ConsumeQuotedValue(reader, target);
            else
               ConsumeUnquotedValue(reader, target);
         }

         private static void ConsumeQuotedValue(CharReader reader, StringBuilder target)
         {
            target.Append((char)reader.Read()); // Leading quote

            int ch;
            while ((ch = reader.Peek()) != -1)
            {
               target.Append((char)reader.Read());
               if (ch == '\"')
                  break;
            }

            if (ch != '\"')
               throw new ArgumentException("Invalid type name; Missing closing quote in assembly name property value.");
         }

         private static void ConsumeUnquotedValue(CharReader reader, StringBuilder target)
         {
            int ch;
            while ((ch = reader.Peek()) != -1 && ch != ',' && ch != ']' && !Char.IsWhiteSpace((char)ch))
            {
               target.Append((char)reader.Read());
            }
         }

         private static bool ConsumeAssemblyNamePropertyName(CharReader reader, StringBuilder target)
         {
            int initialLength = target.Length;
            ConsumeWhitespace(reader, target);
            int ch;
            while ((ch = reader.Peek()) != -1 && ch != '=' && !Char.IsWhiteSpace((char)ch) && ch != ',')
            {
               target.Append((char)reader.Read());
            }

            return target.Length > initialLength;
         }

         private static void ConsumeWhitespace(CharReader reader, StringBuilder target)
         {
            int ch;
            while ((ch = reader.Peek()) != -1 && Char.IsWhiteSpace((char)ch))
            {
               target.Append((char)reader.Read());
            }
         }

         private static int ConsumeAssemblyName(CharReader reader, StringBuilder target)
         {
            int pos = reader.Position;
            int ch;
            while ((ch = reader.Peek()) != -1 && ch != ',' && ch != ']')
            {
               target.Append((char)reader.Read());
            }
            return reader.Position - pos;
         }

         private static void SkipWhitespace(CharReader reader)
         {
            int ch;
            while ((ch = reader.Peek()) != -1 && Char.IsWhiteSpace((char)ch))
            {
               reader.Read();
            }
         }

         public static TypeName Parse(string s)
         {
            CharReader reader = new CharReader(s);
            return Parse(reader, true);
         }

         private static IList<TypeName> ParseGenericArguments(CharReader reader)
         {
            List<TypeName> args = new List<TypeName>();
            if (reader.Peek() == '[')
            {
               do
               {
                  reader.Read();
                  bool fullyQualified = false;
                  if (reader.Peek() == '[')
                  {
                     fullyQualified = true;
                     reader.Read();
                  }

                  args.Add(Parse(reader, fullyQualified));
                     

                  if (fullyQualified == true)
                  {
                     if (reader.Peek() == ']')
                        reader.Read();
                     else
                        throw new TypeNameParserException($"Invalid type name \"{reader.Data}\"; Unexpected character '{(char)reader.Peek()}' at position {reader.Position}; expected ']'.");
                  }
               }
               while (reader.Peek() == ',');

               reader.Read(); // Consume closing ']'
            }
            return args;
         }

         private static IReadOnlyList<TypeSpecifier> ParseRefPtrArrSpec(CharReader reader)
         {
            List<TypeSpecifier> specifiers = null;
            int ch;
            while ((ch = reader.Peek()) != -1)
            {
               TypeSpecifier specifier;

               switch (ch)
               {
                  case '[':
                     int la1 = reader.Peek(1);
                     if (la1 == '*' || la1 == ']' || la1 == ',')
                        specifier = ParseArraySpecifier(reader);
                     else
                        return specifiers;
                     break;
                  case '*':
                     specifier = TypeSpecifier.Pointer;
                     reader.Read();
                     break;

                  case '&':
                     specifier = TypeSpecifier.Reference;
                     reader.Read();
                     break;

                  case ',':
                  case ']':
                     return specifiers;
                  default:
                     throw new TypeNameParserException($"Invalid type name \"{reader.Data}\"; Unexpected character '{(char)ch}' at position {reader.Position}; one of '[', '*', '&', ',', ']'.");
               }

               if (specifiers == null)
                  specifiers = new List<TypeSpecifier>();

               specifiers.Add(specifier);
            }

            return specifiers;
         }

         private static TypeSpecifier ParseArraySpecifier(CharReader reader)
         {
            System.Diagnostics.Debug.Assert(reader.Peek() == '[');
            int rank = 1;

            // Consume the leading '['
            reader.Read();

            while (true)
            {
               int ch = reader.Read();
               switch (ch)
               {
                  case ',':
                     rank++;
                     break;
                  case ']':
                     return TypeSpecifier.Array(rank);
                  case '*':
                     int la1 = reader.Peek();
                     if (la1 != ',' && la1 != ']')                      
                        throw new TypeNameParserException($"Invalid type name \"{reader.Data}\"; Unexpected character '{(la1 == -1 ? "EOS" : ((char)la1).ToString())}' at position {reader.Position}; expected one of ',', ']'.");
                     break;
                  default:
                     throw new TypeNameParserException($"Invalid type name \"{reader.Data}\"; Unexpected character '{(char)ch}' at position {reader.Position}; one of ',', ']', '*'.");
               }
            }
         }

         // ID ('.' ID)* ('+' ID)*
         private static NamespaceTypeNameEntry ParseNamespaceTypeName(CharReader reader, bool allowTrailingCharacters)
         {
            List<string> nestedTypeName = new List<string>();
            StringBuilder namespaceBuilder = new StringBuilder();
            int lastDelimiterPos = -1;

            // Parse namespace including root type name, stop at nested types. 
            while (reader.HasMore && TryParseIdentifierInto(reader, namespaceBuilder))
            {
               if (reader.Peek() == '.')
               {
                  lastDelimiterPos = namespaceBuilder.Length;
                  namespaceBuilder.Append('.');
                  reader.Read();
               }
               else
               {
                  break;
               }
            }

            // Verify that we actually parsed something.
            if (namespaceBuilder.Length == 0)
               throw new TypeNameParserException($"Failed to parse type name from \"{reader.Data}\"; Expected NamespaceTypeName, but none found.");

            // The type name is the identifier following the last dot. Extract that from the namespaceBuilder.
            nestedTypeName.Add(namespaceBuilder.ToString(lastDelimiterPos + 1, namespaceBuilder.Length - lastDelimiterPos - 1));
            namespaceBuilder.Length = lastDelimiterPos == -1 ? 0 : lastDelimiterPos;


            // Now parse any NestedTypeNames
            while (reader.Peek() == '+')
            {
               // Consume the +
               reader.Read();

               nestedTypeName.Add(ParseIdentifier(reader));
            }

            if (!allowTrailingCharacters && reader.HasMore)
               throw new TypeNameParserException($"Invalid type name \"{reader.Data}\"; Unexpected character '{(char)reader.Peek()}' at position {reader.Position}; expected end-of-string.");

            return new NamespaceTypeNameEntry(namespaceBuilder.ToString(), nestedTypeName);
         }

         private static IReadOnlyList<string> ParseNestedTypeName(CharReader reader)
         {
            List<string> result = new List<string>();

            result.Add(ParseIdentifier(reader));

            while (reader.Peek() == '+')
            {
               result.Add(ParseIdentifier(reader));
            }

            return result;
         }

         private static string ParseIdentifier(CharReader reader)
         {
            StringBuilder sb = new StringBuilder();
            if (!TryParseIdentifierInto(reader, sb))
               throw new TypeNameParserException($"Invalid type name; Expected IDENTIFIER at position {reader.Position}.");

            return sb.ToString();
         }

         private static bool TryParseIdentifierInto(CharReader reader, StringBuilder target)
         {
            int startPos = reader.Position;
            int ch;
            while ((ch = reader.Peek()) != -1)
            {
               if (ch != '\\' && IsSpecialCharacter((char)ch))
                  break;

               reader.Read();

               if (ch == '\\' && reader.HasMore)
               {
                  target.Append('\\');
                  ch = reader.Read();
               }

               target.Append((char)ch);
            }

            return reader.Position > startPos;
         }

         private static bool IsSpecialCharacter(char ch)
         {
            switch (ch)
            {
               case ',':
               case '+':
               case '&':
               case '*':
               case '[':
               case ']':
               case '.':
               case '\\':
                  return true;
               default:
                  return false;
            }
         }

         public sealed class CharReader
         {
            public CharReader(string data)
            {
               Data = data;
            }

            public int Position { get; private set; }

            public bool HasMore => Peek() != -1;

            public int Peek()
            {
               if (Position < Data.Length)
                  return Data[Position];

               return -1;
            }

            public int Peek(int offset)
            {
               int absoluteOffset = Position + offset;
               if (absoluteOffset < Data.Length)
                  return Data[absoluteOffset];
               return -1;
            }

            public int Read()
            {
               if (Position < Data.Length)
                  return Data[Position++];

               return -1;
            }

            public string Data { get; }
         }
      }
   }
}
