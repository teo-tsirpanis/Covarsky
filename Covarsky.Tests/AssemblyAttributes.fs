// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace global

open System

[<AttributeUsage(AttributeTargets.GenericParameter)>]
type internal CovariantOutAttribute() =
    inherit Attribute()

[<AttributeUsage(AttributeTargets.GenericParameter)>]
type internal ContravariantInAttribute() =
    inherit Attribute()
