
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Alphaleonis.Reflection.Metadata.Tests
{
   public partial class TypeIdentifierTests
   {
      [Fact]
      public void Parse_NullArgument_ThrowsArgumentNullException()
      {         
         Assert.Throws<ArgumentNullException>(() => TypeIdentifier.Parse(null));
      }

      [Fact]
      private void Parse_EmptyArgument_ThrowsArgumentException()
      {
         Assert.Throws<ArgumentException>(() => TypeIdentifier.Parse(""));
      }

      [Fact]
      private void Parse_SimpleTypeName_Succeeds()
      {
         Assert.NotNull(TypeIdentifier.Parse("Simple"));
      }

      [Fact]
      private void Parse_SimpleTypeWithNamespace_Succeeds()
      {
         Assert.NotNull(TypeIdentifier.Parse("System.String"));
      }

      [Fact]
      private void Parse_NestedTypeWithNamespace_Succeeds()
      {
         Assert.NotNull(TypeIdentifier.Parse("System.String+Nested+Type"));
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void Name_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {         
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.Name, sut.Name);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void Namespace_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.Namespace, sut.Namespace);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void AssemblyName_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.AssemblyName, sut.AssemblyName?.FullName);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void AssemblyQualifiedName_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.AssemblyQualifiedName, sut.AssemblyQualifiedName);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void FullName_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.FullName, sut.FullName);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void IsArray_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.IsArray, sut.IsArray);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void IsPointer_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.IsPointer, sut.IsPointer);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void IsReference_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.IsReference, sut.IsReference);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void NamespaceTypeName_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.Equal(data.NamespaceTypeName, sut.NamespaceTypeName);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void TypeSpecifiers_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);         
         Assert.Equal(data.TypeSpecifiers, sut.TypeSpecifiers);
      }

      [Theory]
      [MemberData(nameof(GetTestData))]
      private void GenericArguments_VariousTypeNames_ReturnsExpectedValue(string input, ExpectedData data)
      {
         var sut = TypeIdentifier.Parse(input);
         Assert.NotNull(sut.GenericArguments);
         Assert.Equal(data.GenericArguments, sut.GenericArguments.Select(arg => new ExpectedData(arg)));
      }

      [Fact]
      private void GetElementType_NonArrayPtrOrRef_ReturnsNull()
      {
         var sut = TypeIdentifier.Parse("System.Int32");
         Assert.Null(sut.GetElementType());
      }

      [Fact]
      private void GetElementType_SimpleArray_ReturnsNonArray()
      {
         var sut = TypeIdentifier.Parse("System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
         var actual = sut.GetElementType();
         Assert.NotNull(actual);
         Assert.Equal(SystemInt, new ExpectedData(actual));
      }

      [Fact]
      private void GetElementType_MultiArray_ReturnsSingleArray()
      {
         var sut = TypeIdentifier.Parse("System.Int32[][,], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
         var actual = sut.GetElementType();
         Assert.NotNull(actual);
         Assert.Equal(SystemIntArray, new ExpectedData(actual));
      }
   }
}
