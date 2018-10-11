using System;
using System.Linq;

namespace Alphaleonis.Reflection.Metadata
{
   internal class SystemTypeDescription : TypeDescription
   {
      public static SystemTypeDescription Instance { get; } = new SystemTypeDescription();

      public override TypeDescriptionKind Kind => TypeDescriptionKind.SystemType;

      public override bool Equals(TypeDescription other)
      {
         return other is SystemTypeDescription;
      }

      public override int GetHashCode()
      {
         return 0;
      }
   }
}
