using System;
using System.Linq;
using System.Reflection.Metadata;

namespace Alphaleonis.Reflection.Metadata
{
   internal class PrimitiveTypeDescription : TypeDescription
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
}
