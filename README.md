![Licensed under the MIT License](https://img.shields.io/github/license/teo-tsirpanis/Covarsky.svg)
[![NuGet](https://img.shields.io/nuget/v/Covarsky.svg)](https://nuget.org/packages/Covarsky)

# Covarsky

Covarsky is a tool that brings co(ntra)variant types to F# (or any other language that does not support them). It runs an MSBuild task that modifies assemblies after compilation.

## How to install

1. Add the [`Covarsky`](https://nuget.org/packages/Covarsky) NuGet package to your project.

2. Create a new source file with two attributes like that (F# example shown):

``` fsharp
namespace global

open System

[<AttributeUsage(AttributeTargets.GenericParameter)>]
type internal CovariantOutAttribute() =
    inherit Attribute()

[<AttributeUsage(AttributeTargets.GenericParameter)>]
type internal ContravariantInAttribute() =
    inherit Attribute()
```

> __Warning:__ The attributes must be declared in the global namespace, and must be internal.

3. Add the following line inside a `PropertyGroup` in your project file:

``` xml
<EnableCovarsky>true</EnableCovarsky>
```

4. You are good to go! Let's see an example.

``` fsharp
type MyType<[<CovariantOut>] 'T> =
    abstract GetObject: unit -> 'T

// [...]

let cast (x: MyType<string>): MyType<obj> =
    unbox x
```

## Notes

These two attributes will be ignored if used anywhere but in the generic parameters of an interface or a delegate.

Using these two attributes in a generic parameter that is already co(ntra)variant will raise a warning but will be ignored as well.

Using both attributes on the same time will raise an error and fail the build (unless something above hasn't already happened).

> __DISCLAIMER:__ Using the attributes in any other wrong way (such as a covariant interface with a method that returns the generic type) will not be checked by Covarsky and may break your assembly.

## Maintainer(s)

- [@teo-tsirpanis](https://github.com/teo-tsirpanis)
