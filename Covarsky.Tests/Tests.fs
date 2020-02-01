// Copyright (c) 2020 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module Covarsky.Tests

open Xunit

type General = obj
type Specific = string

type DContrav<[<ContravariantIn>] 'T> = delegate of 'T -> unit
type DCov<[<CovariantOut>] 'T> = delegate of unit -> 'T
type IContrav<[<ContravariantIn>] 'T> = interface end
type ICov<[<CovariantOut>] 'T> = interface end

[<Fact>]
let ``Covariant delegate`` () =
    let d = DCov<Specific>(fun _ -> null)
    let dCast = unbox<DCov<General>> d
    Assert.Equal(d :> obj, dCast)

[<Fact>]
let ``Contravariant delegate`` () =
    let d = DContrav<General>(ignore)
    let d = unbox<DContrav<Specific>> d
    Assert.Equal(d :> obj, d)

[<Fact>]
let ``Contravariant interface`` () =
    let i = {new IContrav<General>}
    let iCasted = unbox<IContrav<Specific>> i
    Assert.Equal(i :> obj, iCasted)

[<Fact>]
let ``Covariant interface`` () =
    let i = {new ICov<Specific>}
    let iCasted = unbox<ICov<General>> i
    Assert.Equal(i :> obj, iCasted)
