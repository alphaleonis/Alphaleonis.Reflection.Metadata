
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

      [Fact]
      private void Set_AssemblyName_SetToNull_AssemblyNameRemoved()
      {         
         var sut = TypeIdentifier.Parse("System.Linq.IGrouping`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
         sut.AssemblyName = null;
         Assert.Equal("System.Linq.IGrouping`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][]", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Set_AssemblyName_SetToNewValue_AssemblyQualifiedNameIsUpdated()
      {
         var sut = TypeIdentifier.Parse("System.Linq.IGrouping`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
         sut.AssemblyName = new AssemblyName("MyAssembly, Version=1.0.0.1, Culture=neutral, PublicKeyToken=null");
         Assert.Equal("System.Linq.IGrouping`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][], MyAssembly, Version=1.0.0.1, Culture=neutral, PublicKeyToken=null", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Remove_GenericArgument_AssemblyQualifiedNameIsUpdated()
      {
         var sut = TypeIdentifier.Parse("System.Linq.IGrouping`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
         sut.GenericArguments.RemoveAt(0);
         Assert.Equal("System.Linq.IGrouping`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Set_Name_NestedType_AssemblyQualifiedNameIsCorrectlyUpdated()
      {
         var sut = TypeIdentifier.Parse("ConsoleApp10.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
         sut.Name = "Foo";
         Assert.Equal("ConsoleApp10.Root+Nested+Foo[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Set_FullName_NestedType_AssemblyQualifiedNameIsCorrectlyUpdated()
      {
         var sut = TypeIdentifier.Parse("ConsoleApp10.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
         sut.FullName = "System.Foo[]";
         Assert.Equal("System.Foo[], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Set_FullName_TypeWithoutGenericArguments_GenericArgumentsIsCorrectlyUpdated()
      {
         var sut = TypeIdentifier.Parse("ConsoleApp10.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
         sut.FullName = "System.Foo[]";
         Assert.Empty(sut.GenericArguments);
      }

      [Fact]
      private void Set_FullName_TypeWithGenericArguments_GenericArgumentsIsCorrectlyUpdated()
      {
         var sut = TypeIdentifier.Parse("ConsoleApp10.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
         sut.FullName = "System.Foo[System.String,System.Int32]";
         Assert.Equal(2, sut.GenericArguments.Count);
         Assert.Equal("System.String", sut.GenericArguments[0].FullName);
         Assert.Equal("System.Int32", sut.GenericArguments[1].FullName);
      }

      [Fact]
      private void Set_Namespace_ToNull_AssemblyQualifiedNameIsCorrect()
      {
         var sut = TypeIdentifier.Parse("ConsoleApp10.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
         sut.Namespace = null;
         Assert.Equal("Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Set_Namespace_ToNonNull_AssemblyQualifiedNameIsCorrect()
      {
         var sut = TypeIdentifier.Parse("ConsoleApp10.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
         sut.Namespace = "Foo.Bar";
         Assert.Equal("Foo.Bar.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Set_NamespaceTypeName_NestedType_AssemblyQualifiedNameIsCorrectlyUpdated()
      {
         var sut = TypeIdentifier.Parse("ConsoleApp10.Root+Nested+Nested2[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
         sut.NamespaceTypeName = "Foo.Bar.Type+Nested";
         Assert.Equal("Foo.Bar.Type+Nested[System.Int32][], ConsoleApp10, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", sut.AssemblyQualifiedName);
      }

      [Fact]
      private void Set_NamespaceTypeName_Null_ThrowsArgumentNullException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentNullException>(() => sut.NamespaceTypeName = null);
      }

      [Fact]
      private void Set_NamespaceTypeName_Empty_ThrowsArgumentException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentException>(() => sut.NamespaceTypeName = "");
      }

      [Fact]
      private void Set_Name_Null_ThrowsArgumentNullException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentNullException>(() => sut.Name = null);
      }

      [Fact]
      private void Set_Name_Empty_ThrowsArgumentException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentException>(() => sut.Name = "");
      }

      [Fact]
      private void Set_FullName_Null_ThrowsArgumentNullException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentNullException>(() => sut.FullName = null);
      }

      [Fact]
      private void Set_FullName_Empty_ThrowsArgumentException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentException>(() => sut.FullName = "");
      }

      [Fact]
      private void Set_AQN_Null_ThrowsArgumentNullException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentNullException>(() => sut.AssemblyQualifiedName = null);
      }

      [Fact]
      private void Set_AQN_Empty_ThrowsArgumentException()
      {
         var sut = TypeIdentifier.Parse(typeof(int).AssemblyQualifiedName);
         Assert.Throws<ArgumentException>(() => sut.AssemblyQualifiedName = "");
      }
   }
}
