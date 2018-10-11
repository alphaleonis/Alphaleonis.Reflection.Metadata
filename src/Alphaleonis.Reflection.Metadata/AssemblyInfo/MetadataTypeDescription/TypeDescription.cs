using System;
using System.Linq;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   internal abstract class TypeDescription : IEquatable<TypeDescription>
   {
      public abstract TypeDescriptionKind Kind { get; }

      public abstract bool Equals(TypeDescription other);

      public override bool Equals(object obj)
      {
         return Equals(obj as TypeDescription);
      }

      public abstract override int GetHashCode();

      public static PrimitiveTypeDescription Primitive(PrimitiveTypeCode typeCode)
      {
         return new PrimitiveTypeDescription(typeCode);
      }

      public static ArrayTypeDescription Array(TypeDescription elementType)
      {
         return new ArrayTypeDescription(elementType);
      }

      public static SystemTypeDescription SystemType()
      {
         return SystemTypeDescription.Instance;
      }

      public static TypeHandleTypeDescription TypeHandle(TypeIdentifier typeName)
      {
         return new TypeHandleTypeDescription(typeName);
      }
   }
}
