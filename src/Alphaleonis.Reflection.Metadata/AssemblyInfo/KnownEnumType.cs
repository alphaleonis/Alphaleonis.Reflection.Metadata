using System;
using System.Linq;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   internal class KnownEnumType
   {
      public KnownEnumType(TypeIdentifier typeName, PrimitiveTypeCode underlyingEnumType)
      {
         TypeName = typeName;
         UnderlyingEnumType = underlyingEnumType;
      }

      public TypeIdentifier TypeName { get; }
      public PrimitiveTypeCode UnderlyingEnumType { get; }
   }
}
