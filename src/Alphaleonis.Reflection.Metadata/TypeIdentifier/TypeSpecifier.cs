using System;
using System.Linq;
using System.Text;

namespace Alphaleonis.Reflection.Metadata
{
   /// <summary>
   /// Description of a type specifier, that indicates whether a type is an array, pointer or
   /// reference.
   /// </summary>
   public struct TypeSpecifier : IEquatable<TypeSpecifier>
   {
      /// <summary>Indicates a reference specifier in a type name, i.e. '&amp;'.</summary>
      public static readonly TypeSpecifier Reference = new TypeSpecifier(TypeSpecifierKind.Reference, 0);

      /// <summary>Indicates a pointer specifier in a type name, i.e. '*'.</summary>
      public static readonly TypeSpecifier Pointer = new TypeSpecifier(TypeSpecifierKind.Pointer, 0);

      /// <summary>Indicates an array specifier in a type name, i.e. '[]', or '[,,]' etc.</summary>
      /// <param name="rank">The rank of the array. Must be greater than 0.</param>
      public static TypeSpecifier Array(int rank) => new TypeSpecifier(TypeSpecifierKind.Array, rank);

      private TypeSpecifier(TypeSpecifierKind kind, int arrayRank)
      {
         Kind = kind;
         ArrayRank = arrayRank;
      }

      /// <summary>Gets the type of this specifier.</summary>
      public TypeSpecifierKind Kind { get; }

      /// <summary>Gets the array rank if this indicates an array, or 0 otherwise.</summary>
      /// <value>The array rank.</value>
      public int ArrayRank { get; }

      /// <summary>Convert this object into a string representation.</summary>
      /// <returns>A string that represents this object.</returns>
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

      /// <summary>Tests if this TypeSpecifier is considered equal to another.</summary>
      /// <param name="other">The type specifier to compare to this object.</param>
      /// <returns>True if the objects are considered equal, false if they are not.</returns>
      public bool Equals(TypeSpecifier other)
      {
         return Kind.Equals(other.Kind) && ArrayRank.Equals(other.ArrayRank);
      }

      /// <summary>Tests if this object is considered equal to another.</summary>
      /// <param name="obj">The object to compare to this object.</param>
      /// <returns>True if the objects are considered equal, false if they are not.</returns>
      public override bool Equals(object obj)
      {
         return obj is TypeSpecifier other && Equals(other);
      }

      /// <summary>Calculates a hash code for this object.</summary>
      /// <returns>A hash code for this object.</returns>
      public override int GetHashCode()
      {
         int hashCode = 29;
         hashCode = hashCode * 31 + Kind.GetHashCode();
         hashCode = hashCode * 31 + ArrayRank.GetHashCode();
         return hashCode;
      }
   }
}
