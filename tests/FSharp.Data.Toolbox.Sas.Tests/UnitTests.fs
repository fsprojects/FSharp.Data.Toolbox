#if INTERACTIVE
#I "../../packages/FSharp.Data.2.0.7/lib/net40"
#I "../../bin"
#r "FSharp.Data.Toolbox.Sas.dll"
open FSharp.Data.Toolbox.SasFile
#else
module FSharp.Data.Toolbox.Sas.UnitTests
#endif

open FSharp.Data
open FSharp.Data.Toolbox.Sas

open NUnit.Framework

open System

[<TestFixture>]
type ``Unit tests`` () = 
    [<Test>]
    member x.``Adding numbers``() =
        let a, b = Number 2., Number 4.
        Assert.AreEqual(a + b, Number 6.)
    [<Test>]
    member x.``Adding number to a float``() =
        let a, b = 2., Number 4.
        Assert.AreEqual(a + b, Number 6.)
    [<Test>]
    member x.``Adding a float to number``() =
        let a, b = Number 2., 4.
        Assert.AreEqual(a + b, Number 6.)
    [<Test>]
    member x.``Adding an int to number``() =
        let a, b = Number 4., 5
        Assert.AreEqual(a + b, Number 9.)
    [<Test>]
    member x.``Adding empty value to number``() =
        let a, b = Empty, Number 4.
        Assert.AreEqual(a + b, Number 4.)
    [<Test>]
    member x.``Adding number to empty value``() =
        let a, b = Number 2., Empty
        Assert.AreEqual(a + b, Number 2.)
    [<Test>]
    member x.``Adding a float to character``() =
        let a, b = 2., Character "hello"
        Assert.AreEqual(a + b, Character "2hello" )
    [<Test>]
    member x.``Adding character to a float ``() =
        let a, b = 2., Character "hello"
        Assert.AreEqual(b + a, Character "hello2" )
    [<Test>]
    [<ExpectedException("System.InvalidOperationException")>]
    member x.``Adding a number to date value throws exception``() =
        let a, b = Number 2., Date <| DateTime.Now
        a + b |> ignore 

    [<Test>]
    member x.``Substract positive numbers``() =
        let a, b = Number 7., Number 5.
        Assert.AreEqual(a - b, Number 2. )
    [<Test>]
    member x.``Substract negative numbers``() =
        let a, b = Number -7., Number -5.
        Assert.AreEqual(a - b, Number -2. )
    [<Test>]
    member x.``Substract number and a float``() =
        let a, b = Number -7., 5.
        Assert.AreEqual(a - b, Number -12. )
    [<Test>]
    member x.``Substract a float and number``() =
        let a, b = 6., Number 5.
        Assert.AreEqual(a - b, Number 1. )
    [<Test>]
    [<ExpectedException("System.InvalidOperationException")>]
    member x.``Substracting from a character throws exception``() =
        let a, b = Character "hi", Number 1.
        a - b |> ignore 

    [<Test>]
    member x.``Multiply positive numbers``() =
        let a, b = Number 3., Number 5.
        Assert.AreEqual(a * b, Number 15. )
    [<Test>]
    member x.``Multiply negative numbers``() =
        let a, b = Number -7., Number -5.
        Assert.AreEqual(a * b, Number 35. )
    [<Test>]
    member x.``Multiply number and a float``() =
        let a, b = Number -7., 5.
        Assert.AreEqual(a * b, Number -35. )
    [<Test>]
    member x.``Multiply a float and number``() =
        let a, b = 6., Number 5.
        Assert.AreEqual(a * b, Number 30. )
    [<Test>]
    member x.``Multiply character and number``() =
        let a, b = Character "F# ", Number 5.
        Assert.AreEqual(a * b, Character "F# F# F# F# F# ")
    [<Test>]
    [<ExpectedException("System.InvalidOperationException")>]
    member x.``Dividing a character throws exception``() =
        let a, b = Character "hi", Number 1.
        a / b |> ignore 
    [<Test>]
    member x.``Divide positive numbers``() =
        let a, b = Number 15., Number 3.
        Assert.AreEqual(a / b, Number 5. )
    [<Test>]
    member x.``Divide rational numbers``() =
        let a, b = Number 10., Number 3.
        Assert.AreEqual(a / b, Number (10. / 3.) )
    [<Test>]
    member x.``Divide number by int``() =
        let a, b = Number 12., 3
        Assert.AreEqual(a / b, Number 4. )
    [<Test>]
    [<ExpectedException("System.DivideByZeroException")>]
    member x.``Divide by empty``() =
        let a, b = Number 10., Empty
        a / b |> ignore
    [<Test>]
    [<ExpectedException("System.DivideByZeroException")>]
    member x.``Divide by zero``() =
        let a, b = Number 10., 0
        a / b |> ignore
    [<Test>]
    [<ExpectedException("System.InvalidOperationException")>]
    member x.``Divide character and number``() =
        let a, b = Character "F# ", Number 5.
        a / b |> ignore
