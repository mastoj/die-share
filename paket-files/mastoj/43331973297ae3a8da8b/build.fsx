#r "packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FAKE/tools/FakeLib.dll"

#load "paket-files/mastoj/43331973297ae3a8da8b/targets.fsx"

open Fake
open Targets

RunTargetOrDefault "run"
