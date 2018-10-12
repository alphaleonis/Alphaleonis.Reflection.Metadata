# Alphaleonis.Reflection.Metadata

@Alphaleonis.Reflection.Metadata is a small utility library containing classes and methods for working with 
.NET assemblies and their metadata.


### [TypeIdentifier](xref:Alphaleonis.Reflection.Metadata.TypeIdentifier)

This is a class that can be used to parse, deconstruct and modify a .NET type name. 

### [AssemblyInfo](xref:Alphaleonis.Reflection.Metadata.AssemblyInfo)

Class providing some basic information about an assembly file, such as it's name, references, target
framework version and more, without the need to load it into the appdomain with the `Assembly.ReflectionOnlyLoad` api.