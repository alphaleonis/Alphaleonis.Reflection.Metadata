using System;
using System.Linq;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   internal class TypeHandleTypeDescription : TypeDescription
   {
      public TypeHandleTypeDescription(TypeIdentifier typeName)
      {
         TypeName = typeName;
      }

      public TypeHandleTypeDescription(MetadataReader reader, EntityHandle handle)
      {
         TypeName = MetadataTypeIdentifier.GetFrom(reader, handle);
      }

      public override TypeDescriptionKind Kind => TypeDescriptionKind.TypeReference;

      public TypeIdentifier TypeName { get; }

      public override string ToString()
      {
         return $"NamedType \"{TypeName.FullName}\"";
      }

      public override bool Equals(TypeDescription other)
      {
         return other is TypeHandleTypeDescription thd && thd.TypeName.Equals(TypeName);
      }

      public override int GetHashCode()
      {
         return TypeName.GetHashCode();
      }
   }
}
