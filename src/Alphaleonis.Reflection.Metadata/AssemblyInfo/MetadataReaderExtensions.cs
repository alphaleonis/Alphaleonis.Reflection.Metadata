using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   /// <summary>Provides some useful extension methods to <see cref="MetadataReader"/>.</summary>
   public static class MetadataReaderExtensions
   {
      internal static CustomAttributeValue<TypeDescription>? FindAssemblyAttribute(this MetadataReader reader, AttributeDescription attributeDescription, IEnumerable<KnownEnumType> knownEnumTypes)
      {
         foreach (CustomAttribute attribute in reader.CustomAttributes.Select(reader.GetCustomAttribute))
         {
            var actualName = MetadataTypeIdentifier.GetFrom(reader, attribute);
            if (actualName.FullName.Equals(attributeDescription.TypeName.FullName))
            {
               var attributeData = attribute.DecodeValue(new TypeDescriptionCustomAttributeProvider(knownEnumTypes));

               if (attributeDescription.Signatures.Any(signature => signature.SequenceEqual(attributeData.FixedArguments.Select(arg => arg.Type))))
               {
                  return attributeData;
               }
            }
         }

         return null;
      }

      /// <summary>
      /// Gets the <see cref="AssemblyName"/> of the given <see cref="AssemblyDefinition"/>.
      /// </summary>
      /// <remarks>
      /// This is similar to the `AssemblyDefinition.GetAssemblyName` from `System.Reflection.Metadata`, with the difference that
      /// this method will give set culture to `neutral` and the public key token to `null` instead of
      /// leaving them empty.
      /// </remarks>
      /// <param name="metadataReader">The <see cref="MetadataReader"/> to act on.</param>
      /// <param name="assemblyDefinition">The assembly definition.</param>
      /// <returns>An AssemblyName representing the specified <paramref name="assemblyDefinition"/>.</returns>
      public static AssemblyName ParseAssemblyName(this MetadataReader metadataReader, AssemblyDefinition assemblyDefinition)
      {
         return ParseAssemblyName(metadataReader, assemblyDefinition.Name, assemblyDefinition.Culture, assemblyDefinition.PublicKey, assemblyDefinition.Version, assemblyDefinition.HashAlgorithm, assemblyDefinition.Flags);
      }

      /// <summary>
      /// Gets the <see cref="AssemblyName"/> of the given <see cref="AssemblyReference"/>.
      /// </summary>
      /// <remarks>
      /// This is similar to the `AssemblyDefinition.GetAssemblyName` from `System.Reflection.Metadata`, with the difference that
      /// this method will give set culture to `neutral` and the public key token to `null` instead of
      /// leaving them empty.
      /// </remarks>
      /// <param name="metadataReader">The <see cref="MetadataReader"/> to act on.</param>
      /// <param name="assemblyReference">The assembly reference.</param>
      /// <returns>An AssemblyName representing the specified <paramref name="assemblyReference"/>.</returns>
      public static AssemblyName ParseAssemblyName(this MetadataReader metadataReader, AssemblyReference assemblyReference)
      {
         return ParseAssemblyName(metadataReader, assemblyReference.Name, assemblyReference.Culture, assemblyReference.PublicKeyOrToken, assemblyReference.Version, AssemblyHashAlgorithm.None, assemblyReference.Flags);
      }

      private static AssemblyName ParseAssemblyName(this MetadataReader metadataReader, StringHandle nameHandle, StringHandle cultureHandle, BlobHandle publicKeyTokenHandle, Version version, AssemblyHashAlgorithm assemblyHashAlgorithm, AssemblyFlags flags)
      {
         string culture = cultureHandle.IsNil ? "" : metadataReader.GetString(cultureHandle);
         string name = metadataReader.GetString(nameHandle);

         AssemblyName assemblyName = new AssemblyName(name);
         assemblyName.Version = version;
#if net452
         assemblyName.CultureInfo = CultureInfo.CreateSpecificCulture(culture);
#else
         assemblyName.CultureName = culture;
#endif
         assemblyName.HashAlgorithm = (System.Configuration.Assemblies.AssemblyHashAlgorithm)assemblyHashAlgorithm;
         assemblyName.Flags = GetAssemblyNameFlags(flags);
         assemblyName.ContentType = GetContentTypeFromAssemblyFlags(flags);
         ReadPublicKeyToken(metadataReader, assemblyName, publicKeyTokenHandle, flags);

         return assemblyName;
      }

      private static AssemblyNameFlags GetAssemblyNameFlags(AssemblyFlags flags)
      {
         AssemblyNameFlags assemblyNameFlags = AssemblyNameFlags.None;

         if ((flags & AssemblyFlags.PublicKey) != 0)
            assemblyNameFlags |= AssemblyNameFlags.PublicKey;

         if ((flags & AssemblyFlags.Retargetable) != 0)
            assemblyNameFlags |= AssemblyNameFlags.Retargetable;

         if ((flags & AssemblyFlags.EnableJitCompileTracking) != 0)
            assemblyNameFlags |= AssemblyNameFlags.EnableJITcompileTracking;

         if ((flags & AssemblyFlags.DisableJitCompileOptimizer) != 0)
            assemblyNameFlags |= AssemblyNameFlags.EnableJITcompileOptimizer;

         return assemblyNameFlags;
      }

      private static AssemblyContentType GetContentTypeFromAssemblyFlags(AssemblyFlags flags)
      {
         return (AssemblyContentType)(((int)flags & (int)AssemblyFlags.ContentTypeMask) >> 9);
      }

      private static void ReadPublicKeyToken(MetadataReader metadataReader, AssemblyName targetName, BlobHandle publicKeyTokenHandle, AssemblyFlags flags)
      {
         if (publicKeyTokenHandle.IsNil)
         {
            targetName.SetPublicKeyToken(new byte[0]);
         }
         else
         {
            byte[] publicKeyOrToken = metadataReader.GetBlobBytes(publicKeyTokenHandle);
            bool hasPublicKey = (flags & AssemblyFlags.PublicKey) != 0;
            if (hasPublicKey)
            {
               targetName.SetPublicKey(publicKeyOrToken);
            }
            else
            {
               targetName.SetPublicKeyToken(publicKeyOrToken);
            }
         }
      }
   }
}
