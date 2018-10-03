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

//[assembly: ConsoleApp1.SecondTest(22, typeof(ClassLibrary1.Class1.CL))]
//[assembly: ConsoleApp1.Test("Pelle", "Bosse", 1, 2, 3, 4, 5, 6, 7, 8, true, 'c', 1.1f, 2.2, "Hello", typeof(MyEnum), ConsoleApp1.MyEnum.Bepa, new string[] { "p", "l" })]
//[assembly: ConsoleApp1.Test(23, "Pelle")]
//[assembly: ConsoleApp1.Test(21, typeof(int), MyEnum.Cepa, StringComparison.CurrentCulture, Pelle.MyOtherENum.v1, ClassLibrary1.Class1.CL.v1, NamedOne = "PPP")]

namespace ConsoleApp1
{
   [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
   public class SecondTestAttribute : Attribute
   {
      public SecondTestAttribute(int apa, Type t)
      {
      }
   }

   public enum MyEnum : long
   {
      Apa,
      Bepa,
      Cepa
   }
}

namespace Alphaleonis.Reflection.Metadata
{

   public class BasicAssemblyInfo
   {
      public BasicAssemblyInfo(bool isAssembly, AssemblyName assemblyName, IReadOnlyList<AssemblyName> references, FrameworkName targetFramework)
      {
         IsAssembly = isAssembly;
         AssemblyName = assemblyName;
         AssemblyReferences = references ?? Array.Empty<AssemblyName>();
         TargetFramework = targetFramework;
      }

      public bool IsAssembly { get; }
      public AssemblyName AssemblyName { get; }
      public IReadOnlyList<AssemblyName> AssemblyReferences { get; }
      public FrameworkName TargetFramework { get; }

      //private static string GetAttributeTypeName(CustomAttribute attribute, MetadataReader reader)
      //{
      //   switch (attribute.Constructor.Kind)
      //   {
      //      case HandleKind.MethodDefinition:
      //         var definition = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
      //         var type = reader.GetTypeDefinition(definition.GetDeclaringType());
      //         return reader.GetString(type.Namespace) + "." + reader.GetString(type.Name);
      //      case HandleKind.MemberReference:
      //         var member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
      //         var typeRef = reader.GetTypeReference((TypeReferenceHandle)member.Parent);
      //         return reader.GetString(typeRef.Namespace) + "." + reader.GetString(typeRef.Name);
      //      default:
      //         throw new BadImageFormatException();
      //   }
      //}

      private static readonly KnownEnumType[] s_knownEnumTypes = new KnownEnumType[] {
                     new KnownEnumType(MetadataTypeName.FromFullName("ConsoleApp1.MyEnum"), PrimitiveTypeCode.Int32),
                     new KnownEnumType(MetadataTypeName.FromFullName("System.StringComparison"), PrimitiveTypeCode.Int32),
                     new KnownEnumType(MetadataTypeName.FromFullName("MyOtherEnum"), PrimitiveTypeCode.Int32),
                     new KnownEnumType(MetadataTypeName.FromFullName("CL"), PrimitiveTypeCode.Int32)
                  };

      private static class KnownAttributeSignatures
      {
         public static readonly IReadOnlyList<TypeDescription> String = new[] { TypeDescription.Primitive(PrimitiveTypeCode.String) };
      }

      private static class KnownAttributes
      {
         public static readonly AttributeDescription TargetFrameworkAttribute = new AttributeDescription("System.Runtime.Versioning.TargetFrameworkAttribute", KnownAttributeSignatures.String);
      }

      private static CustomAttributeValue<TypeDescription>? FindAssemblyAttribute(MetadataReader reader, AttributeDescription attributeDescription)
      {
         foreach (CustomAttribute attribute in reader.CustomAttributes.Select(reader.GetCustomAttribute))
         {
            var actualName = MetadataTypeName.GetMetadataTypeName(reader, attribute);
            if (actualName.Equals(attributeDescription.TypeName))
            {

               var attributeData = attribute.DecodeValue(new MyProvider(s_knownEnumTypes));

               if (attributeDescription.Signatures.Any(signature => signature.SequenceEqual(attributeData.FixedArguments.Select(arg => arg.Type))))
               {
                  return attributeData;
               }
            }
         }

         return null;
      }

      public static BasicAssemblyInfo GetBasicAssemblyInfo(string fileName)
      {
         using (FileStream stream = File.OpenRead(fileName))
         using (PEReader peReader = new PEReader(stream))
         {
            if (!peReader.HasMetadata)
               throw new NotSupportedException($"The file \"{fileName}\" has no managed assembly metadata associated with it.");

            MetadataReader metadata = peReader.GetMetadataReader();

            AssemblyDefinition assemblyDefinition = metadata.GetAssemblyDefinition();

            List<AssemblyName> references = new List<AssemblyName>();
            foreach (var reference in metadata.AssemblyReferences.Select(metadata.GetAssemblyReference))
            {
               references.Add(reference.GetAssemblyName());
            }

            var attr = FindAssemblyAttribute(metadata, KnownAttributes.TargetFrameworkAttribute);
            FrameworkName fnn = null;
            //foreach (CustomAttribute attr in metadata.CustomAttributes.Select(metadata.GetCustomAttribute))
            //{
            //   string attrTypeName = GetAttributeTypeName(attr, metadata);
            //   //Console.WriteLine(attrTypeName);
            //   //TypeReferenceHandle attrTypeHandle;
            //   //string attributeTypeName;
            //   if (attrTypeName.EndsWith("TestAttribute"))
            //   {
            //      CustomAttributeValue<TypeDescription> attrData = attr.DecodeValue(new MyProvider(new KnownEnumType[] {
            //         new KnownEnumType(MetadataTypeName.FromFullName("ConsoleApp1.MyEnum"), PrimitiveTypeCode.Int32),
            //         new KnownEnumType(MetadataTypeName.FromFullName("System.StringComparison"), PrimitiveTypeCode.Int32),
            //         new KnownEnumType(MetadataTypeName.FromFullName("MyOtherEnum"), PrimitiveTypeCode.Int32),
            //         new KnownEnumType(MetadataTypeName.FromFullName("CL"), PrimitiveTypeCode.Int32)
            //      }));
            //      MethodDefinition methodDef = metadata.GetMethodDefinition((MethodDefinitionHandle)attr.Constructor);
            //      var sig = methodDef.DecodeSignature(new SigDec(), null);

            //      //CustomAttributeValue<AttributeParameterTypeInfo> something = attr.DecodeValue(new AttributeParameterValueTypeProvider());
            //   }


            //   //if (attr.Constructor.Kind == HandleKind.MemberReference)
            //   //{
            //   //   var type = metadata.GetTypeReference((TypeReferenceHandle)metadata.GetMemberReference((MemberReferenceHandle)attr.Constructor).Parent);
            //   //   attributeTypeName = metadata.GetString(type.Name);
            //   //}
            //   //else if (attr.Constructor.Kind == HandleKind.MethodDefinition)
            //   //{
            //   //   //MethodDefinition ctor = metadata.GetMethodDefinition((MethodDefinitionHandle)attr.Constructor);
            //   //   //var type = ctor.GetDeclaringType();
            //   //   //attributeTypeName = metadata.GetString(metadata.GetTypeDefinition(type).Name);
            //   //   //var sig = ctor.DecodeSignature(new AttributeParameterValueTypeProvider(metadata, attr.Value), null);
            //   //}
            //   //else
            //   //{
            //   //   throw new NotSupportedException();
            //   //}

            //   //var attrType = metadata.GetTypeReference((TypeReferenceHandle)ctor.Parent);
            //   //string attrTypeName = metadata.GetString(attrType.Name);
            //   //string attrTypeName = attributeTypeName;
            //   //Console.WriteLine(attrTypeName);
            //   //if (attrTypeName == "TestAttribute")
            //   //{
            //   //   var provider = new StringParameterValueTypeProvider(metadata, attr.Value);
            //   //   //MethodSignature<string> signature = ctor.DecodeMethodSignature(provider, null);
            //   //}

            //   //if (attrTypeName == "TargetFrameworkAttribute" && metadata.GetString(attrType.Namespace) == "System.Runtime.Versioning")
            //   //{                  
            //   //   var provider = new StringParameterValueTypeProvider(metadata, attr.Value);
            //   //   MethodSignature<string> signature = ctor.DecodeMethodSignature(provider, null);
            //   //   fnn = new FrameworkName(signature.ParameterTypes[0]);
            //   //   Console.WriteLine(fnn.Version);
            //   //}
            //}

            return new BasicAssemblyInfo(metadata.IsAssembly, assemblyDefinition.GetAssemblyName(), references, fnn);
         }
      }




      //internal sealed class StringParameterValueTypeProvider : ISignatureTypeProvider<string, object>
      //{
      //   private readonly BlobReader valueReader;

      //   public StringParameterValueTypeProvider(MetadataReader reader, BlobHandle value)
      //   {
      //      Reader = reader;
      //      valueReader = reader.GetBlobReader(value);

      //      var prolog = valueReader.ReadUInt16();
      //      if (prolog != 1) throw new BadImageFormatException("Invalid custom attribute prolog.");
      //   }

      //   public MetadataReader Reader { get; }

      //   public string GetArrayType(string elementType, ArrayShape shape) => "";
      //   public string GetByReferenceType(string elementType) => "";
      //   public string GetFunctionPointerType(MethodSignature<string> signature) => "";
      //   public string GetGenericInstance(string genericType, ImmutableArray<string> typestrings) => "";
      //   public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) { throw new NotImplementedException(); }
      //   public string GetGenericMethodParameter(int index) => "";
      //   public string GetGenericMethodParameter(object genericContext, int index) { throw new NotImplementedException(); }
      //   public string GetGenericTypeParameter(int index) => "";
      //   public string GetGenericTypeParameter(object genericContext, int index) { throw new NotImplementedException(); }
      //   public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => "";
      //   public string GetPinnedType(string elementType) => "";
      //   public string GetPointerType(string elementType) => "";
      //   public string GetPrimitiveType(PrimitiveTypeCode typeCode)
      //   {
      //      if (typeCode == PrimitiveTypeCode.String) return valueReader.ReadSerializedString();
      //      return "";
      //   }
      //   public string GetSZArrayType(string elementType) => "";
      //   public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => "";
      //   public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => "";
      //   public string GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => "";
      //}
   }

   public enum TypeDescriptionKind
   {
      PrimitiveType,
      TypeReference,
      SystemType,
      SZArray
   }

   public class ArrayTypeDescription : TypeDescription
   {
      public ArrayTypeDescription(TypeDescription elementType)
      {
         ElementType = elementType;
      }

      public override TypeDescriptionKind Kind => TypeDescriptionKind.SZArray;

      public TypeDescription ElementType { get; }

      public override bool Equals(TypeDescription other)
      {
         return other is ArrayTypeDescription atd && ElementType.Equals(atd.ElementType);
      }

      public override int GetHashCode()
      {
         return 11 + ElementType.GetHashCode();
      }
   }

   public abstract class TypeDescription : IEquatable<TypeDescription>
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

      public static TypeHandleTypeDescription TypeHandle(MetadataTypeName typeName)
      {
         return new TypeHandleTypeDescription(typeName);
      }
   }

   public class PrimitiveTypeDescription : TypeDescription
   {
      public PrimitiveTypeDescription(PrimitiveTypeCode typeCode)
      {
         TypeCode = typeCode;
      }

      public static PrimitiveTypeDescription TryGetFrom(Type type)
      {
         if (type == null || !type.IsPrimitive)
            return null;

         return TryGetFrom(Type.GetTypeCode(type));
      }

      public static PrimitiveTypeDescription TryGetFrom(TypeCode typeCode)
      {
         switch (typeCode)
         {
            case System.TypeCode.Object:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Object);
            case System.TypeCode.Boolean:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Boolean);
            case System.TypeCode.Char:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Char);
            case System.TypeCode.SByte:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.SByte);
            case System.TypeCode.Byte:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Byte);
            case System.TypeCode.Int16:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Int16);
            case System.TypeCode.UInt16:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.UInt16);
            case System.TypeCode.Int32:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Int32);
            case System.TypeCode.UInt32:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.UInt32);
            case System.TypeCode.Int64:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Int64);
            case System.TypeCode.UInt64:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.UInt64);
            case System.TypeCode.Single:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Single);
            case System.TypeCode.Double:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.Double);
            case System.TypeCode.String:
               return new PrimitiveTypeDescription(PrimitiveTypeCode.String);
         }

         return null;
      }

      public override TypeDescriptionKind Kind => TypeDescriptionKind.PrimitiveType;

      public PrimitiveTypeCode TypeCode { get; }

      public Type AsType()
      {
         switch (TypeCode)
         {
            case PrimitiveTypeCode.Boolean:
               return typeof(bool);
            case PrimitiveTypeCode.Byte:
               return typeof(byte);
            case PrimitiveTypeCode.SByte:
               return typeof(sbyte);
            case PrimitiveTypeCode.Char:
               return typeof(char);
            case PrimitiveTypeCode.Int16:
               return typeof(Int16);
            case PrimitiveTypeCode.UInt16:
               return typeof(ushort);
            case PrimitiveTypeCode.Int32:
               return typeof(int);
            case PrimitiveTypeCode.UInt32:
               return typeof(uint);
            case PrimitiveTypeCode.Int64:
               return typeof(long);
            case PrimitiveTypeCode.UInt64:
               return typeof(ulong);
            case PrimitiveTypeCode.Single:
               return typeof(float);
            case PrimitiveTypeCode.Double:
               return typeof(double);
            case PrimitiveTypeCode.IntPtr:
               return typeof(IntPtr);
            case PrimitiveTypeCode.UIntPtr:
               return typeof(UIntPtr);
            case PrimitiveTypeCode.Object:
               return typeof(object);
            case PrimitiveTypeCode.String:
               return typeof(string);
            case PrimitiveTypeCode.Void:
               return typeof(void);
            case PrimitiveTypeCode.TypedReference:
            default:
               throw new NotSupportedException($"Unsupported primitive type code {TypeCode}");
         }
      }

      public override bool Equals(TypeDescription other)
      {
         if (other is PrimitiveTypeDescription otherPd)
         {
            return TypeCode.Equals(otherPd.TypeCode);
         }
         return false;
      }

      public override int GetHashCode()
      {
         return TypeCode.GetHashCode();
      }

      public override string ToString()
      {
         return $"Primitive {TypeCode}";
      }
   }

   public class SystemTypeDescription : TypeDescription
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

   public class TypeHandleTypeDescription : TypeDescription
   {
      public TypeHandleTypeDescription(MetadataTypeName typeName)
      {
         TypeName = typeName;
      }

      public TypeHandleTypeDescription(MetadataReader reader, EntityHandle handle)
      {
         TypeName = MetadataTypeName.GetMetadataTypeName(reader, handle);
      }

      public override TypeDescriptionKind Kind => TypeDescriptionKind.TypeReference;

      public MetadataTypeName TypeName { get; }

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


   public class KnownEnumType
   {
      public KnownEnumType(MetadataTypeName typeName, PrimitiveTypeCode underlyingEnumType)
      {
         TypeName = typeName;
         UnderlyingEnumType = underlyingEnumType;
      }

      public MetadataTypeName TypeName { get; }
      public PrimitiveTypeCode UnderlyingEnumType { get; }
   }

   public class MyProvider : ICustomAttributeTypeProvider<TypeDescription>
   {
      private Dictionary<MetadataTypeName, PrimitiveTypeCode> m_knownEnumTypes;

      public MyProvider(IEnumerable<KnownEnumType> knownEnumTypes)
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

         return new TypeHandleTypeDescription(MetadataTypeName.FromFullName(name));
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

   public struct AttributeDescription
   {
      public AttributeDescription(string fullTypeName, params IReadOnlyList<TypeDescription>[] signatures)
         : this(MetadataTypeName.FromFullName(fullTypeName), signatures)
      {
      }

      public AttributeDescription(MetadataTypeName typeName, params IReadOnlyList<TypeDescription>[] signatures)
      {
         TypeName = typeName;
         Signatures = signatures;
      }

      public MetadataTypeName TypeName { get; }

      public IReadOnlyList<IReadOnlyList<TypeDescription>> Signatures { get; }
   }

}
