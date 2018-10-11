using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.Versioning;
using System.Text;

namespace Alphaleonis.Reflection.Metadata
{
   /// <summary>Represents basic information about a PE file representing an assembly.</summary>
   /// <remarks>
   /// The information provided by this class is usually retrieved using
   /// <see cref="Assembly.ReflectionOnlyLoad(string)"/> and then examining the returned
   /// <see cref="Assembly"/> object. This however has the drawback that it is not possible to unload the assembly without performing 
   /// the operation in a separate <see cref="AppDomain"/>.
   ///                        
   /// This class retrieves basic information about the assembly only, via the <see cref="MetadataReader"/> provided in <see cref="System.Reflection.Metadata"/>, 
   /// so the targetted assembly is not loaded into the AppDomain as is the case with <see cref="O:Assembly.Load"/> and <see cref="O:Assembly.ReflectionOnlyLoad"/> methods.      
   /// </remarks>
   public sealed class AssemblyInfo
   {
      private static readonly KnownEnumType[] s_knownEnumTypes = new KnownEnumType[] { };      

      private static class KnownAttributeSignatures
      {
         public static readonly IReadOnlyList<TypeDescription> String = new[] { TypeDescription.Primitive(PrimitiveTypeCode.String) };
      }

      private static class KnownAttributes
      {
         public static readonly AttributeDescription TargetFrameworkAttribute = new AttributeDescription("System.Runtime.Versioning.TargetFrameworkAttribute", KnownAttributeSignatures.String);
      }
      
      private AssemblyInfo(bool isAssembly, AssemblyName assemblyName, IReadOnlyList<AssemblyName> references, FrameworkName targetFramework, string metadataVersion, bool isExe, bool isDll, bool isConsoleApplication)
      {
         IsAssembly = isAssembly;
         AssemblyName = assemblyName;
         AssemblyReferences = references ?? new AssemblyName[0];
         TargetFramework = targetFramework;
         MetadataVersion = metadataVersion;
         IsExe = isExe;
         IsDll = isDll;
         IsConsoleApplication = isConsoleApplication;
      }

      /// <summary>Gets a value indicating whether the parsed file was indeed an assembly or not.</summary>
      /// <value><see langword="true"/> if the file was a valid assembly or <see langword="false"/> otherwise.</value>
      public bool IsAssembly { get; }

      /// <summary>Gets the name of the assembly.</summary>
      /// <value>The name of the assembly or <see langword="null"/> if the file was not an assembly.</value>
      public AssemblyName AssemblyName { get; }

      /// <summary>Gets references of the assembly.</summary>
      /// <value>The assembly references.</value>
      public IReadOnlyList<AssemblyName> AssemblyReferences { get; }

      /// <summary>Gets target framework as provided by the <see cref="System.Runtime.Versioning.TargetFrameworkAttribute"/>.</summary>
      /// <value>The target framework for which the assembly was built, or <see langword="null"/> if the information was not available.</value>
      /// <remarks>
      /// This is only available if the assembly was build against .NET 4.0 or later. In previous versions of the .NET framework, this attribute was
      /// not available, and the correct target framework cannot be determined by inspecting the assembly metadata.
      /// </remarks>
      public FrameworkName TargetFramework { get; }

      /// <summary>Gets the metadata version as specified in the assembly metadata.</summary>
      /// <value>The metadata version from the assembly metadata.</value>
      public string MetadataVersion { get; }

      /// <summary>Gets a value indicating whether the PE file is an executable.</summary>
      /// <value><see langword="true"/> if the PE file is an executable, <see langword="false"/> if not.</value>
      public bool IsExe { get; }

      /// <summary>Gets a value indicating whether the PE file is a DLL.</summary>
      /// <value><see langword="true"/> if the PE file is a DLL, <see langword="false"/> if not.</value>
      public bool IsDll { get; }

      /// <summary>Gets a value indicating whether the PE file is a console application.</summary>
      /// <value><see langword="true"/> if the PE file is a console application, <see langword="false"/> if not.</value>
      public bool IsConsoleApplication { get; }

      /// <summary>Gets assembly information from the specified file.</summary>
      /// <remarks>
      /// This method does not throw an exception if the file is not an assembly. Instead the properties
      /// of the returned object must be inspected to determine whether it was a valid PE file and/or
      /// assembly.
      /// </remarks>
      /// <param name="fileName">The full path to the assembly file to retrieve metadata information
      /// from.</param>
      /// <returns>The assembly information read.</returns>
      public static AssemblyInfo GetAssemblyInfo(string fileName)
      {
         using (FileStream stream = File.OpenRead(fileName))
         {
            return GetAssemblyInfo(stream, fileName);
         }
      }

      /// <summary>Gets assembly information from the specified assembly.</summary>
      /// <remarks>
      /// This method does not throw an exception if the file is not an assembly. Instead the properties
      /// of the returned object must be inspected to determine whether it was a valid PE file and/or
      /// assembly.
      /// </remarks>
      /// <param name="rawAssembly">A byte array that is a COFF-based image containing an emitted assembly.</param>
      /// <returns>The assembly information read.</returns>
      public static AssemblyInfo GetAssemblyInfo(byte[] rawAssembly)
      {
         using (MemoryStream stream = new MemoryStream(rawAssembly, false))
         {
            return GetAssemblyInfo(rawAssembly);
         }
      }

      /// <summary>Gets assembly information from the specified assembly.</summary>
      /// <remarks>
      /// This method does not throw an exception if the file is not an assembly. Instead the properties
      /// of the returned object must be inspected to determine whether it was a valid PE file and/or
      /// assembly.
      /// </remarks>
      /// <param name="assemblyStream">A stream from which the assembly metadata will be read.</param>
      /// <returns>The assembly information read.</returns>
      private static AssemblyInfo GetAssemblyInfo(Stream assemblyStream)
      {
         return GetAssemblyInfo(assemblyStream, null);
      }

      private static AssemblyInfo GetAssemblyInfo(Stream assemblyStream, string assemblyLocation)
      {
         try
         {
            using (PEReader peReader = new PEReader(assemblyStream))
            {
               bool isExe = peReader.PEHeaders?.IsExe ?? false;
               bool isDll = peReader.PEHeaders?.IsDll ?? false;
               bool isConsoleApplication = peReader.PEHeaders?.IsConsoleApplication ?? false;

               if (!peReader.HasMetadata)
                  return new AssemblyInfo(false, null, null, null, null, isExe, isDll, isConsoleApplication);

               MetadataReader metadata = peReader.GetMetadataReader();


               List<AssemblyName> references = new List<AssemblyName>();
               foreach (var reference in metadata.AssemblyReferences.Select(metadata.GetAssemblyReference))
               {
                  AssemblyName referenceAssemblyName = metadata.ParseAssemblyName(reference);
                  references.Add(referenceAssemblyName);
               }

               var attr = metadata.FindAssemblyAttribute(KnownAttributes.TargetFrameworkAttribute, s_knownEnumTypes);
               FrameworkName fnn = null;
               if (attr != null)
               {
                  var fnns = attr.Value.FixedArguments[0].Value as string;
                  fnn = new FrameworkName(fnns);
               }

               AssemblyDefinition assemblyDefinition = metadata.GetAssemblyDefinition();
               AssemblyName assemblyName = metadata.ParseAssemblyName(assemblyDefinition);
               if (assemblyLocation != null)
               {
                  assemblyName.CodeBase = FilePathToFileUrl(assemblyLocation);
               }

               return new AssemblyInfo(metadata.IsAssembly, assemblyName, references, fnn, metadata.MetadataVersion, isExe, isDll, isConsoleApplication);
            }
         }
         catch (BadImageFormatException)
         {
            return new AssemblyInfo(false, null, null, null, null, false, false, false);
         }         
      }

      private static string FilePathToFileUrl(string filePath)
      {
         StringBuilder uri = new StringBuilder();
         foreach (char v in filePath)
         {
            if ((v >= 'a' && v <= 'z') || (v >= 'A' && v <= 'Z') || (v >= '0' && v <= '9') ||
              v == '+' || v == '/' || v == ':' || v == '.' || v == '-' || v == '_' || v == '~' ||
              v > '\xFF')
            {
               uri.Append(v);
            }
            else if (v == Path.DirectorySeparatorChar || v == Path.AltDirectorySeparatorChar)
            {
               uri.Append('/');
            }
            else
            {
               uri.Append(String.Format("%{0:X2}", (int)v));
            }
         }
         if (uri.Length >= 2 && uri[0] == '/' && uri[1] == '/') // UNC path
            uri.Insert(0, "file:");
         else
            uri.Insert(0, "file:///");
         return uri.ToString();
      }



   }
}
