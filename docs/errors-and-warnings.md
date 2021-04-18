# Covarsky's errors and warnings

Starting with Covarsky 1.4.0, all errors and warnings it might raise have an MSBuild message code and are listed in this document. Log messages of lesser importance have no code and are not listed.

## Warning `COVARSKY0001`

### The 'EnableCovarsky' property is deprecated. Use 'CovarskyEnable' instead and set it to false only if you want to disable Covarsky.

Starting with Covarsky 1.3.0, Covarsky is enabled by default as long as the package is installed. To disable it, set the `CovarskyEnable` MSBuild property to `false`. The similarly named `EnableCovarsky` property was used by earlier versions of Covarsky, and this warning ensures it is no longer used.

## Warning `COVARSKY0002`

### It is not recommended to use Covarsky on a C# project. Instead, use the language's 'in' and 'out' keywords for the best support and type safety.

Covarsky was created to be used by language that don't natively support co(ntra)variance in generic parameters. C# supports it ever since it became available and therefore C# projects should use the language's facilities instead of Covarsky.

## Warning `COVARSKY0003`

### It is not recommended to use Covarsky on a Visual Basic project. Instead, use the language's 'In' and 'Out' keywords for the best support and type safety.

Similar to warning `COVARSKY0002`, but for Visual Basic.

## Error `COVARSKY0101`

### The names of Covarsky's attributes cannot be the same.

Covarsky supports customizing the names of the attributes that mark covariant and contravariant generic parameters. Obviously these two attribute names cannot be the same.

## Warning `COVARSKY0102`

### Custom attribute `{AttributeName}` was not found.

This warning is shown if an attribute with a custom name was not found in the assembly Covarsky processed.

It is not raised if an attribute with the default name (`CovariantOutAttribute` or `ContravariantInAttribute`) is not found.

## Warning `COVARSKY0103`

### Type `{TypeName}`'s parameter `{GenericParameterName}` is already variant and Covarsky will not change it.

Covarsky found an attribute on a generic type parameter that is already co(tra)variant. This parameter's variance will not be changed.

This warning will be raised even if the parameter's variance matches the attribute (for example `CovariantOut` being applied on an already covariant parameter).

This warning indicates either a bug with Covarsky (which takes measures not to process the same assembly twice), or that Covarsky is being used from a language that supports co(ntra)variance (and shouldn't use Covarsky anyway).

## Error `COVARSKY0104`

### Type `{TypeName}`'s parameter `{GenericParameterName}` cannot be declared as both covariant and contravariant.

This error is raised when a generic type parameter has both Covarsky's attributes applied to it. Obviously it cannot be both covariant and contravariant, and one of the two attributes must be removed to make the error disappear.

## Warning `COVARSKY0105`

### Attribute `{AttributeName}` will be ignored because it is public.

Covarsky requires its attributes to be internal and will ignore them if they are public. Simply making them internal will solve the problem.
