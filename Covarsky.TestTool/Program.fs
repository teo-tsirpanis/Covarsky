// This is a command-line tool that tests Covarsky and allows it to be debugged.

open System.IO
open Argu
open Covarsky
open Serilog
open Sigourney

type Arguments =
    | [<ExactlyOnce; MainCommand>] AssemblyFile of string
    | [<Unique>] CustomInAttributeName of string
    | [<Unique>] CustomOutAttributeName of string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | AssemblyFile _ -> "The assembly file to process"
            | CustomInAttributeName _ -> "A custom name for the attribute that marks contravariant generic parameters"
            | CustomOutAttributeName _ -> "A custom name for the attribute that marks covariant generic parameters"

[<EntryPoint>]
let main argv =
    let argParser = ArgumentParser.Create()

    let parseResult = argParser.Parse(argv)

    let assemblyFile = parseResult.GetResult AssemblyFile
    let intermediateDirectory = Path.GetDirectoryName assemblyFile

    let customInAttributeName =
        parseResult.TryGetResult CustomInAttributeName
        |> Option.toObj

    let customOutAttributeName =
        parseResult.TryGetResult CustomOutAttributeName
        |> Option.toObj

    let logger =
        LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger()

    let config = WeaverConfig()
    config.IntermediateDirectory <- intermediateDirectory
    Weaver.Weave(assemblyFile, Path.ChangeExtension(assemblyFile, ".processed.dll"),
         (fun asm -> AssemblyRewriter.DoWeave(asm, logger, customOutAttributeName, customInAttributeName)),
         logger, config, "Covarsky")
    0
