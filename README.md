![Licensed under the MIT License](https://img.shields.io/github/license/teo-tsirpanis/Covarsky.svg)
[![NuGet](https://img.shields.io/nuget/v/Covarsky.svg)][nuget]
[![Build Status](https://img.shields.io/appveyor/ci/teo-tsirpanis/Covarsky/master.svg)](https://ci.appveyor.com/project/teo-tsirpanis/covarsky)

# Covarsky

Covarsky is a tool that brings co(ntra)variant types to F# or any other .ΝΕΤ language that does not natively support them. Powered by [Sigourney], it runs an MSBuild task that modifies assemblies after compilation.

## How to use

1. Add the [`Covarsky`][nuget] NuGet package to your project.

2. Create a new source file with two (or one, if you only need one kind of variance) attributes like that (F# example shown):

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

3. You are good to go! Let's see an example.

``` fsharp
type MyType<[<CovariantOut>] 'T> =
    abstract GetObject: unit -> 'T

// [...]

let cast (x: MyType<string>): MyType<obj> =
    unbox x
```

As you have seen, we have to perform manual type casts using `unbox` because F# does not recognize that `MyType`'s `'T` is covariant.

## Customizing Covarsky

### Using custom attribute names

If for any reason you want to customize the attribute names Covarsky will recognize, you can do it by specifying it in your project file:

``` xml
<PropertyGroup>
    <CustomInAttributeName>MyLibrary.MakeItContravariantPleaseAttribute</CustomInAttributeName>
    <CustomOutAttributeName>MyLibrary.MakeItContravariantPleaseAttribute</CustomOutAttributeName>
</PropertyGroup>
```

As you see, fully qualified names are accepted. The attribute classes however still have to be internal and belong to the same assembly.

### Disabling Covarsky

Since version 1.3.0 Covarsky is enabled by default when you install the package. If for some reason you want to disable it you can do it with the following property:

``` xml
<PropertyGroup>
    <CovarskyEnable>false</CovarskyEnable>
</PropertyGroup>
```

## Caveats

* After Covarsky's execution, the two attributes will _not_ be removed.

* These two attributes will be ignored if used anywhere but in the generic parameters of an interface or a delegate.

* Using these two attributes in a generic parameter that is already co(ntra)variant will raise a warning but will be ignored as well.

* Using both attributes on the same time will raise an error and fail the build (unless something above hasn't already happened).

* _Using the attributes in any other wrong way (such as a contravariant interface with a method that returns the generic type) __will not be checked by Covarsky and may break your assembly.___

## Maintainer(s)

- [@teo-tsirpanis](https://github.com/teo-tsirpanis)

[nuget]: https://nuget.org/packages/Covarsky
[sigourney]: https://github.com/teo-tsirpanis/Sigourney
