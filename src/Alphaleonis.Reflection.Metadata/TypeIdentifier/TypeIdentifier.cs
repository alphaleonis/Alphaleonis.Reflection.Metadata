using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alphaleonis.Reflection.Metadata
{
   /// <summary>Describes the unique identity of a <see cref="System.Type" /> in full.</summary>
   [Serializable]
   public class TypeIdentifier
   {
      #region Private Fields

      private List<string> m_nestedTypeName;

      #endregion

      #region Construction 

      private TypeIdentifier(string namespaceName, List<string> nestedTypeName, IList<TypeSpecifier> typeSpecifiers, IList<TypeIdentifier> genericArguments, AssemblyName assemblyName)
      {
         TypeSpecifiers = typeSpecifiers ?? new List<TypeSpecifier>();
         Namespace = namespaceName;
         m_nestedTypeName = nestedTypeName;
         GenericArguments = genericArguments;
         AssemblyName = assemblyName;
      }

      private TypeIdentifier(TypeIdentifier other)
      {
         TypeSpecifiers = new List<TypeSpecifier>(other.TypeSpecifiers);
         GenericArguments = CloneGenericArguments(other.GenericArguments);
         AssemblyName = other.AssemblyName;
         Namespace = other.Namespace;
         m_nestedTypeName = new List<string>(other.m_nestedTypeName);

      }

      private static IList<TypeIdentifier> CloneGenericArguments(IList<TypeIdentifier> genericArguments)
      {
         return new List<TypeIdentifier>(genericArguments.Select(arg => new TypeIdentifier(arg)));
      }

      #endregion

      #region Properties

      /// <summary>Gets or sets the assembly name in which this type resides. This may be <see langword="null"/> if no 
      ///          assembly name was provided.</summary>
      public AssemblyName AssemblyName { get; set; }

      /// <summary>Gets or sets the specifiers indicating whether this type is an array, pointer or reference type.</summary>
      public IList<TypeSpecifier> TypeSpecifiers { get; private set; }

      /// <summary>Gets or sets the namespace of the type. May be <see langword="null"/> if the type is not contained within a namespace.</summary>      
      public string Namespace { get; set; }

      /// <summary>
      /// Gets or sets the namespace and type name, without any array/pointer/reference specifiers or
      /// generic arguments. For example the
      /// <see cref="NamespaceTypeName" /> of the type
      /// <c>System.Collections.Generic.Dictionary`2+KeyCollection[System.String,System.Int32][,]</c>
      /// is <c>System.Collections.Generic.Dictionary`2+KeyCollection</c>.
      /// </summary>
      /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
      /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
      /// illegal values.</exception>
      /// <value>The name of the namespace type.</value>
      public string NamespaceTypeName
      {
         get
         {
            StringBuilder result = new StringBuilder();
            if (!String.IsNullOrEmpty(Namespace))
            {
               result.Append(Namespace);
               result.Append('.');
            }

            result.Append(String.Join("+", m_nestedTypeName));
            return result.ToString();
         }

         set
         {
            if (value == null)
               throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
               throw new ArgumentException($"{nameof(value)} is empty.", nameof(value));

            (Namespace, m_nestedTypeName) = ParseNamespaceTypeName(new CharReader(value), false);
         }
      }

      /// <summary>Gets or sets the full assembly qualified name of the type.</summary>
      /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
      /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
      /// illegal values.</exception>
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
            if (value == null)
               throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
               throw new ArgumentException($"{nameof(value)} is empty.", nameof(value));

            (Namespace, m_nestedTypeName, GenericArguments, TypeSpecifiers, AssemblyName) = ParseAssemblyQualifiedName(new CharReader(value), false);
         }
      }

      /// <summary>Gets a list containing the generic arguments of this type, or an empty list if no generic arguments are available.</summary>      
      public IList<TypeIdentifier> GenericArguments { get; private set; }

      /// <summary>
      /// Gets or sets the full name of the type. This is equivalent to
      /// <see cref="System.Type.FullName"/>.
      /// </summary>
      /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
      /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
      /// illegal values.</exception>
      public string FullName
      {
         get
         {
            StringBuilder result = new StringBuilder();
            BuildTypeFullName(result);
            return result.ToString();
         }

         set
         {
            if (value == null)
               throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
               throw new ArgumentException($"{nameof(value)} is empty.", nameof(value));

            (Namespace, m_nestedTypeName, GenericArguments, TypeSpecifiers) = ParseFullName(new CharReader(value), false);
         }
      }

      /// <summary>
      /// Gets or sets the simple name of the type <b>without</b> any array/pointer/byref specs.
      /// </summary>
      /// <remarks>
      /// This is different from <see cref="MemberInfo.Name"/> that does include the array/pointer/byRef
      /// specifiers.
      /// </remarks>
      /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
      /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
      /// illegal values.</exception>
      /// <value>The name.</value>
      public string Name
      {
         get
         {
            return m_nestedTypeName.LastOrDefault();
         }

         set
         {
            if (value == null)
               throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
               throw new ArgumentException($"{nameof(value)} is empty.", nameof(value));

            m_nestedTypeName[m_nestedTypeName.Count - 1] = value;
         }
      }

      /// <summary>Returns true if this type is an array.</summary>
      public bool IsArray => TypeSpecifiers.Count > 0 && TypeSpecifiers[TypeSpecifiers.Count - 1].Kind == TypeSpecifierKind.Array;

      /// <summary>Returns true if this type is a pointer.</summary>
      public bool IsPointer => TypeSpecifiers.Count > 0 && TypeSpecifiers[TypeSpecifiers.Count - 1].Kind == TypeSpecifierKind.Pointer;

      /// <summary>Returns true if this type is a reference.</summary>
      public bool IsReference => TypeSpecifiers.Count > 0 && TypeSpecifiers[TypeSpecifiers.Count - 1].Kind == TypeSpecifierKind.Reference;

      #endregion

      #region Public Methods

      /// <summary>Gets element type of this type if this type is an array, pointer or reference. Returns <see langword="null"/> otherwise.</summary>
      public TypeIdentifier GetElementType()
      {
         if (TypeSpecifiers.Count == 0)
            return null;

         return new TypeIdentifier(Namespace, new List<string>(m_nestedTypeName), TypeSpecifiers.Take(TypeSpecifiers.Count - 1).ToList(), CloneGenericArguments(GenericArguments), AssemblyName);
      }

      /// <summary>Gets declaring type of this type if this type is a nested type, or returns <see langword="null"/> otherwise.</summary>
      public TypeIdentifier GetDeclaringType()
      {
         if (m_nestedTypeName.Count <= 1)
            return null;

         return new TypeIdentifier(Namespace, m_nestedTypeName.GetRange(0, m_nestedTypeName.Count - 1), TypeSpecifiers, CloneGenericArguments(GenericArguments), AssemblyName);
      }

      #endregion

      #region Static Methods

      /// <summary>Parses a type name. This may be an assembly qualified name, a full type name or a simple type name.</summary>
      /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
      /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
      /// illegal values.</exception>
      /// <param name="typeName">The type name to parse</param>
      /// <returns>A new TypeIdentifier representing the specified type.</returns>
      public static TypeIdentifier Parse(string typeName)
      {
         if (typeName == null)
            throw new ArgumentNullException(nameof(typeName));

         if (typeName.Length == 0)
            throw new ArgumentException($"{nameof(typeName)} must not be empty.", nameof(typeName));


         return ParseTypeIdentifier(new CharReader(typeName), true);
      }

      #endregion

      #region Internal Parsing

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

               if (GenericArguments[i].AssemblyName != null)
                  result.Append('[');
               result.Append(GenericArguments[i].AssemblyQualifiedName);
               if (GenericArguments[i].AssemblyName != null)
                  result.Append(']');
            }


            result.Append(']');
         }

         if (TypeSpecifiers != null)
         {
            foreach (var specifier in TypeSpecifiers)
            {
               result.Append(specifier.ToString());
            }
         }
      }

      private static TypeIdentifier ParseTypeIdentifier(CharReader reader, bool fullyQualified)
      {
         if (fullyQualified)
         {
            var aqn = ParseAssemblyQualifiedName(reader, true);
            return new TypeIdentifier(aqn.Namespace, aqn.NestedTypeName, aqn.Specifiers, aqn.GenericArguments, aqn.AssemblyName);
         }
         else
         {
            var fn = ParseFullName(reader, true);
            return new TypeIdentifier(fn.Namespace, fn.NestedTypeName, fn.Specifiers, fn.GenericArguments, null);
         }
      }

      private static (string Namespace, List<string> NestedTypeName, IList<TypeIdentifier> GenericArguments, IList<TypeSpecifier> Specifiers) ParseFullName(CharReader reader, bool allowTrailingCharacters)
      {
         var namespaceTypeName = ParseNamespaceTypeName(reader, true);

         IList<TypeIdentifier> genericArguments;
         int la1 = reader.Peek(1);
         if (reader.Peek() == '[' && la1 != ',' && la1 != '*' && la1 != ']')
            genericArguments = ParseGenericArguments(reader);
         else
            genericArguments = new List<TypeIdentifier>();

         IList<TypeSpecifier> spec = ParseRefPtrArrSpec(reader);

         if (!allowTrailingCharacters && reader.HasMore)
            throw new TypeNameParserException($"Invalid type name \"{reader.Data}\"; Unexpected character '{(char)reader.Peek()}' at position {reader.Position}; expected end-of-string.");

         return (namespaceTypeName.Namespace, namespaceTypeName.NestedTypeName, genericArguments, spec);
      }

      private static (string Namespace, List<string> NestedTypeName, IList<TypeIdentifier> GenericArguments, IList<TypeSpecifier> Specifiers, AssemblyName AssemblyName) ParseAssemblyQualifiedName(CharReader reader, bool allowTrailingCharacters)
      {
         var fullName = ParseFullName(reader, true);

         AssemblyName assemblyName = null;
         if (reader.Peek() == ',')
         {
            reader.Read();
            SkipWhitespace(reader);
            assemblyName = ParseAssemblyName(reader);
         }

         if (!allowTrailingCharacters && reader.HasMore)
            throw new TypeNameParserException($"Invalid type name \"{reader.Data}\"; Unexpected character '{(char)reader.Peek()}' at position {reader.Position}; expected end-of-string.");

         return (fullName.Namespace, fullName.NestedTypeName, fullName.GenericArguments, fullName.Specifiers, assemblyName);
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

      private static IList<TypeIdentifier> ParseGenericArguments(CharReader reader)
      {
         List<TypeIdentifier> args = new List<TypeIdentifier>();
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

               args.Add(ParseTypeIdentifier(reader, fullyQualified));


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

      private static IList<TypeSpecifier> ParseRefPtrArrSpec(CharReader reader)
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
      private static (string Namespace, List<string> NestedTypeName) ParseNamespaceTypeName(CharReader reader, bool allowTrailingCharacters)
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

         return ((namespaceBuilder.Length == 0) ? null : namespaceBuilder.ToString(), nestedTypeName);
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

      #endregion

      #region Nested Types

      private sealed class CharReader
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

      #endregion
   }
}
