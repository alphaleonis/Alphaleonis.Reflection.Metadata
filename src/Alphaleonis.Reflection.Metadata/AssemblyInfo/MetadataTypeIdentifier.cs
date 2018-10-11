using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   internal static class MetadataTypeIdentifier
   {
      public static TypeIdentifier GetFrom(MetadataReader reader, EntityHandle handle)
      {
         if (handle.Kind == HandleKind.TypeDefinition)
         {
            return GetFrom(reader, (TypeDefinitionHandle)handle);
         }
         else if (handle.Kind == HandleKind.TypeReference)
         {
            return GetFrom(reader, (TypeReferenceHandle)handle);
         }
         else if (handle.Kind == HandleKind.CustomAttribute)
         {
            return GetFrom(reader, (CustomAttributeHandle)handle);
         }
         else
         {
            throw new NotSupportedException();
         }
      }

      public static TypeIdentifier GetFrom(MetadataReader reader, TypeDefinitionHandle handle)
      {
         return GetFrom(reader, reader.GetTypeDefinition(handle));
      }

      public static TypeIdentifier GetFrom(MetadataReader reader, TypeReferenceHandle handle)
      {
         return GetFrom(reader, reader.GetTypeReference(handle));
      }

      public static TypeIdentifier GetFrom(MetadataReader reader, TypeDefinition definition)
      {         
         var assemblyName = reader.ParseAssemblyName(reader.GetAssemblyDefinition());
         return new TypeIdentifier(reader.GetString(definition.Name), reader.GetString(definition.Namespace), assemblyName);
      }

      public static TypeIdentifier GetFrom(MetadataReader reader, CustomAttributeHandle attribute)
      {
         return GetFrom(reader, reader.GetCustomAttribute(attribute));
      }

      public static TypeIdentifier GetFrom(MetadataReader reader, CustomAttribute attribute)
      {
         switch (attribute.Constructor.Kind)
         {
            case HandleKind.MethodDefinition:
               var definition = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
               return GetFrom(reader, definition.GetDeclaringType());
            case HandleKind.MemberReference:
               var member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
               return GetFrom(reader, member.Parent);
            default:
               throw new BadImageFormatException();
         }
      }

      public static TypeIdentifier GetFrom(MetadataReader reader, TypeReference reference)
      {
         AssemblyName assemblyName = null;
         if (reference.ResolutionScope.Kind == HandleKind.AssemblyReference)
         {
            var asmRef = reader.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope);
            assemblyName = reader.ParseAssemblyName(asmRef);
         }

         return new TypeIdentifier(reader.GetString(reference.Name), reader.GetString(reference.Namespace), assemblyName);
      }

      public static TypeIdentifier GetFrom(string namespaceName, string typeName)
      {
         return new TypeIdentifier(typeName, namespaceName, null);
      }

      public static TypeIdentifier GetFrom(string qualifiedName)
      {
         return TypeIdentifier.Parse(qualifiedName);
      }

   }
}
