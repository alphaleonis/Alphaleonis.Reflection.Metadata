# Alphaleonis.Reflection.Metadata 
[![Build status](https://ci.appveyor.com/api/projects/status/88ajelp6mybljm84/branch/master?svg=true)](https://ci.appveyor.com/project/alphaleonis/alphaleonis-reflection-metadata/branch/master) [![Test status](https://img.shields.io/appveyor/tests/alphaleonis/alphaleonis-reflection-metadata/master.svg)](https://ci.appveyor.com/project/alphaleonis/alphaleonis-reflection-metadata/build/tests) [![GitHub](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://github.com/alphaleonis/Alphaleonis.Reflection.Metadata/blob/master/LICENSE)

**Alphaleonis.Reflection.Metadata** is a small utility library containing classes and methods for working with 
.NET assemblies and their metadata. 

[See the documentation](https://alphaleonis.github.io/Alphaleonis.Reflection.Metadata/) for more information.

### TypeIdentifier

This is a class that can be used to parse, deconstruct and modify a .NET type name. 

### AssemblyInfo

Class providing some basic information about an assembly file, such as it's name, references, target
framework version and more, without the need to load it into the appdomain with the `Assembly.ReflectionOnlyLoad` api.

### StrongNameKeyPairGenerator

Utility that can be used to create a strong name key pair file (.snk).
