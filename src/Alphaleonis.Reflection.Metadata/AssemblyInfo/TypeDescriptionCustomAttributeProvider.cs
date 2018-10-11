using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   internal class TypeDescriptionCustomAttributeProvider : ICustomAttributeTypeProvider<TypeDescription>
   {
      private Dictionary<TypeIdentifier, PrimitiveTypeCode> m_knownEnumTypes;

      public TypeDescriptionCustomAttributeProvider(IEnumerable<KnownEnumType> knownEnumTypes)
      {
         m_knownEnumTypes = knownEnumTypes.ToDictionary(t => t.TypeName, t => t.UnderlyingEnumType);
      }

      public TypeDescription GetPrimitiveType(PrimitiveTypeCode typeCode)
      {
         return new PrimitiveTypeDescription(typeCode);
      }

      public TypeDescription GetSystemType()
      {
         return SystemTypeDescription.Instance;
      }

      public TypeDescription GetSZArrayType(TypeDescription elementType)
      {
         return new ArrayTypeDescription(elementType);
      }

      public TypeDescription GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
      {
         var result = new TypeHandleTypeDescription(reader, handle);
         if (result.TypeName.FullName == "System.Type")
            return SystemTypeDescription.Instance;
         return result;
      }

      public TypeDescription GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
      {
         var result = new TypeHandleTypeDescription(reader, handle);
         if (result.TypeName.FullName == "System.Type")
            return SystemTypeDescription.Instance;
         return result;
      }

      public TypeDescription GetTypeFromSerializedName(string name)
      {
         var type = Type.GetType(name, false).GetTypeInfo();

         if (type != null)
         {
            var typeDesc = PrimitiveTypeDescription.TryGetFrom(type);
            if (typeDesc != null)
               return typeDesc;

            if (type.Equals(typeof(Type)))
               return SystemTypeDescription.Instance;
         }

         return new TypeHandleTypeDescription(MetadataTypeIdentifier.GetFrom(name));
      }

      public PrimitiveTypeCode GetUnderlyingEnumType(TypeDescription type)
      {
         if (type is TypeHandleTypeDescription typeHandleDescription && m_knownEnumTypes.TryGetValue(typeHandleDescription.TypeName, out var underlyingEnumType))
         {
            return underlyingEnumType;
         }

         throw new NotSupportedException($"Unknown enum type: {type}");
      }

      public bool IsSystemType(TypeDescription type)
      {
         return type.Kind == TypeDescriptionKind.SystemType ||
            type is TypeHandleTypeDescription typeHandleDesc && typeHandleDesc.TypeName.FullName.Equals("System.Type");
      }
   }
}
