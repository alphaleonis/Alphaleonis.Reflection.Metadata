using System;
using System.Collections.Generic;
using System.Linq;

namespace Alphaleonis.Reflection.Metadata
{
   internal struct AttributeDescription
   {
      public AttributeDescription(string fullTypeName, params IReadOnlyList<TypeDescription>[] signatures)
         : this(MetadataTypeIdentifier.GetFrom(fullTypeName), signatures)
      {
      }

      public AttributeDescription(TypeIdentifier typeName, params IReadOnlyList<TypeDescription>[] signatures)
      {
         TypeName = typeName;
         Signatures = signatures;
      }

      public TypeIdentifier TypeName { get; }

      public IReadOnlyList<IReadOnlyList<TypeDescription>> Signatures { get; }
   }
}
