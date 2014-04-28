#if INTERACTIVE
#I "../../packages/FSharp.Data.2.0.5/lib/net40"
#I "../../bin"
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
let ``Can authenticate with Twitter using AppOnly mode`` () =
  let key = "mKQL29XNemjQbLlQ8t0pBg"
  let secret = "T27HLDve1lumQykBUgYAbcEkbDrjBe6gwbu0gqi4saM"
  let twitter = Twitter.AuthenticateAppOnly(key, secret)
  Assert.AreSame(1, 2)