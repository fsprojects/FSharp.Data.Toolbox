#if INTERACTIVE
#I "../../packages/FSharp.Data.2.0.7/lib/net40"
#I "../../bin"
#I "../../packages/NUnit.2.6.3/lib"
#r "nunit.framework.dll"
#r @"FSharp.Data.dll"
#r "FSharp.Data.Toolbox.Twitter.dll"
open FSharp.Data.Toolbox.Twitter
#else
module FSharp.ProjectScaffold.Tests
#endif

open FSharp.Data
open FSharp.Data.Toolbox.Twitter
open NUnit.Framework

[<Test>]
let ``Can authenticate with Twitter using AppOnly mode and search for F# org twitter`` () =
  let key = "mKQL29XNemjQbLlQ8t0pBg"
  let secret = "T27HLDve1lumQykBUgYAbcEkbDrjBe6gwbu0gqi4saM"
  let twitter = Twitter.AuthenticateAppOnly(key, secret)
  let actual = twitter.Users.Lookup ["fsharporg"] |> Seq.head
  Assert.IsTrue(actual.Id = 880772426L)