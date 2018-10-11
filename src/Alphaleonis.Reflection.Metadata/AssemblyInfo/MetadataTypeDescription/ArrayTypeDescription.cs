using System;
using System.Linq;

namespace Alphaleonis.Reflection.Metadata
{
   internal class ArrayTypeDescription : TypeDescription
   {
      public ArrayTypeDescription(TypeDescription elementType)
      {
         ElementType = elementType;
      }

      public override TypeDescriptionKind Kind => TypeDescriptionKind.SZArray;

      public TypeDescription ElementType { get; }

      public override bool Equals(TypeDescription other)
      {
         return other is ArrayTypeDescription atd && ElementType.Equals(atd.ElementType);
      }

      public override int GetHashCode()
      {
         return 11 + ElementType.GetHashCode();
      }
   }
}
