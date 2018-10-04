using System;
using System.Linq;
using System.Text;

namespace Alphaleonis.Reflection.Metadata
{
   public struct TypeSpecifier : IEquatable<TypeSpecifier>
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

      public bool Equals(TypeSpecifier other)
      {
         return Kind.Equals(other.Kind) && ArrayRank.Equals(other.ArrayRank);
      }

      public override bool Equals(object obj)
      {
         return obj is TypeSpecifier other && Equals(other);
      }

      public override int GetHashCode()
      {
         int hashCode = 29;
         hashCode = hashCode * 31 + Kind.GetHashCode();
         hashCode = hashCode * 31 + ArrayRank.GetHashCode();
         return hashCode;
      }
   }
}
