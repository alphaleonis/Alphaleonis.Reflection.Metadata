using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   public class MetadataTypeName : IEquatable<MetadataTypeName>
   {
      private MetadataTypeName(string typeName, string namespaceName)
      {
         TypeName = typeName;
         NamespaceName = namespaceName ?? String.Empty;
      }

      public static MetadataTypeName GetMetadataTypeName(MetadataReader reader, EntityHandle handle)
      {
         if (handle.Kind == HandleKind.TypeDefinition)
         {
            return GetMetadataTypeName(reader, (TypeDefinitionHandle)handle);
         }
         else if (handle.Kind == HandleKind.TypeReference)
         {
            return GetMetadataTypeName(reader, (TypeReferenceHandle)handle);
         }
         else if (handle.Kind == HandleKind.CustomAttribute)
         {
            return GetMetadataTypeName(reader, (CustomAttributeHandle)handle);
         }
         else
         {
            throw new NotSupportedException();
         }
      }

      public static MetadataTypeName GetMetadataTypeName(MetadataReader reader, TypeDefinitionHandle handle)
      {
         return GetMetadataTypeName(reader, reader.GetTypeDefinition(handle));
      }

      public static MetadataTypeName GetMetadataTypeName(MetadataReader reader, TypeReferenceHandle handle)
      {
         return GetMetadataTypeName(reader, reader.GetTypeReference(handle));
      }

      public static MetadataTypeName GetMetadataTypeName(MetadataReader reader, TypeDefinition definition)
      {
         var name = reader.GetAssemblyDefinition().GetAssemblyName();
         return new MetadataTypeName(reader.GetString(definition.Name), reader.GetString(definition.Namespace));
      }

      public static MetadataTypeName GetMetadataTypeName(MetadataReader reader, CustomAttributeHandle attribute)
      {
         return GetMetadataTypeName(reader, reader.GetCustomAttribute(attribute));
      }

      public static MetadataTypeName GetMetadataTypeName(MetadataReader reader, CustomAttribute attribute)
      {
         switch (attribute.Constructor.Kind)
         {
            case HandleKind.MethodDefinition:
               var definition = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
               return GetMetadataTypeName(reader, definition.GetDeclaringType());
            case HandleKind.MemberReference:
               var member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
               return GetMetadataTypeName(reader, member.Parent);
            default:
               throw new BadImageFormatException();
         }
      }
      public static MetadataTypeName GetMetadataTypeName(MetadataReader reader, TypeReference reference)
      {
         if (reference.ResolutionScope.Kind == HandleKind.AssemblyReference)
         {
            var asmRef = reader.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope);
            var asmName = asmRef.GetAssemblyName();
         }

         return new MetadataTypeName(reader.GetString(reference.Name), reader.GetString(reference.Namespace));
      }

      public string FullName
      {
         get
         {
            if (NamespaceName.Length > 0)
               return NamespaceName + "." + TypeName;

            return TypeName;
         }
      }

      public string TypeName { get; }

      public string NamespaceName { get; }

      public static MetadataTypeName FromNamespaceAndTypeName(string namespaceName, string typeName)
      {
         return new MetadataTypeName(typeName, namespaceName);
      }

      public static MetadataTypeName FromFullName(string qualifiedName)
      {
         TypeIdentifier identifier = new TypeIdentifier(qualifiedName);
         var splitName = SplitQualifiedName(qualifiedName);
         throw new NotImplementedException();
         // TODO PP (2018-09-30): Implement!
         //return new MetadataTypeName(identifier.);
      }

      private static (string typeName, string namespaceName) SplitQualifiedName(string qualifiedName)
      {
         var lastDelimiter = -1;
         for (int i = 0; i < qualifiedName.Length; i++)
         {
            switch (qualifiedName[i])
            {
               case '.':
                  if (i == 0 || lastDelimiter < i - 1)
                  {
                     lastDelimiter = i;
                  }
                  break;
               case ',':
                  qualifiedName = qualifiedName.Substring(0, i);
                  break;
            }
         }

         if (lastDelimiter < 0)
            return (qualifiedName, String.Empty);

         return (qualifiedName.Substring(lastDelimiter + 1), qualifiedName.Substring(0, lastDelimiter));
      }

      public bool Equals(MetadataTypeName other)
      {
         return FullName.Equals(other.FullName);
      }

      public override bool Equals(object obj)
      {
         if (obj is MetadataTypeName other)
            return Equals(other);

         return false;
      }

      public override int GetHashCode()
      {
         return FullName.GetHashCode();
      }

      public override string ToString()
      {
         return FullName;
      }
   }
}
