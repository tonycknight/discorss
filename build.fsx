#r "paket:
nuget Microsoft.Build 17.3.2
nuget Microsoft.Build.Framework 17.3.2
nuget Microsoft.Build.Tasks.Core 17.3.2
nuget Microsoft.Build.Utilities.Core 17.3.2
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MSBuild
nuget Fake.BuildServer.GitHubActions
nuget Fake.Core.Target //"
#if !FAKE
#load "./.fake/fakebuild.fsx/intellisense.fsx"
#endif

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.SystemHelper

let packageDir = "./package"
let publishDir = "./publish"
let benchmarksDir = "./BenchmarkDotNet.Artifacts"
let mainSolution = "./Discorss.sln"


let ghVersionNumber =
    (match Fake.BuildServer.GitHubActions.Environment.CI false with
     | true -> Fake.Core.Environment.environVar "build-version-number" |> Some
     | _ -> None)

let commitSha = Fake.BuildServer.GitHubActions.Environment.Sha

let versionSuffix =
    match Fake.BuildServer.GitHubActions.Environment.Ref with
    | null
    | "refs/heads/main" -> ""
    | _ -> "-preview"

let version =
    (match ghVersionNumber with
     | Some vsn -> sprintf "%s%s" vsn versionSuffix
     | None -> "0.0.0")

let infoVersion =
    match commitSha with
    | null -> sprintf "%s" version
    | sha -> sprintf "%s.%s" version sha

let assemblyInfoParams (buildParams) =
    [ ("Version", version); ("AssemblyInformationalVersion", infoVersion) ]
    |> List.append buildParams

let codeCoverageParams (buildParams) =
    [ ("CollectCoverage", "true")
      ("CoverletOutput", "./TestResults/coverage.info")
      ("CoverletOutputFormat", "cobertura") ]
    |> List.append buildParams

let buildOptions =
    fun (opts: DotNet.BuildOptions) ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Release
            MSBuildParams =
                { opts.MSBuildParams with
                    Properties = assemblyInfoParams opts.MSBuildParams.Properties
                    WarnAsError = Some [ "*" ] } }

let testOptions (opts: DotNet.TestOptions) =
    let properties = codeCoverageParams opts.MSBuildParams.Properties

    { opts with
        NoBuild = false
        Configuration = DotNet.BuildConfiguration.Debug // Temporary, to ensure Coverlet can find otherwise optimised-out code
        Logger = Some "trx;LogFileName=test_results.trx"
        Filter = Some "OS!=Windows"
        MSBuildParams =
            { opts.MSBuildParams with
                Properties = properties } }

let publishByRuntimeOptions =
    fun (runtime: string) (opts: DotNet.PublishOptions) ->
        let props = ("AssemblyFileVersion", version) :: opts.MSBuildParams.Properties

        { opts with
            Configuration = DotNet.BuildConfiguration.Release
            MSBuildParams =
                { opts.MSBuildParams with
                    DisableInternalBinLog = true

                    Properties = props }
            SelfContained = Some true
            Runtime = Some runtime }

let publishProjects = !! "src/**/Discorss.*.Service.fsproj" |> List.ofSeq

let publishAndCopy runtime =
    publishProjects
    |> Seq.iter (fun p -> p |> DotNet.publish (publishByRuntimeOptions runtime))

    publishProjects
    |> Seq.iter (fun p ->
        let subdir = sprintf "bin/Release/net6.0/%s/publish" runtime
        let dir = Path.getDirectory p
        let name = System.IO.Path.GetFileNameWithoutExtension(p)

        let sourceDir = System.IO.Path.Combine(dir, subdir)
        let targetDir = sprintf @"./%s/%s/%s" publishDir runtime (name.ToLower())

        name |> sprintf "name: %s" |> Fake.Core.Trace.log
        sourceDir |> sprintf "sourceDir: %s" |> Fake.Core.Trace.log
        targetDir |> sprintf "targetDir: %s" |> Fake.Core.Trace.log

        Shell.copyDir targetDir sourceDir (fun _ -> true))

Target.create "Clean" (fun _ ->
    !! "src/**/bin" ++ "src/**/obj" |> Shell.cleanDirs

    !! "test/**/bin"
    ++ "test/**/obj"
    ++ "test/**/TestResults"
    ++ packageDir
    ++ publishDir
    |> Shell.cleanDirs

    !!benchmarksDir |> Shell.cleanDirs)

Target.create "Restore" (fun _ -> !!mainSolution |> Seq.iter (DotNet.restore id))

Target.create "Build" (fun _ -> !!mainSolution |> Seq.iter (DotNet.build buildOptions))

Target.create "Publish services" (fun _ -> publishAndCopy "win-x64")

Target.create "Unit Tests" (fun _ -> !! "test/**/*.Test.Unit.fsproj" |> Seq.iter (DotNet.test testOptions))

Target.create "Generate code coverage reports" (fun _ ->
    let args =
        sprintf
            @"-reports:""./test/**/coverage.info"" -targetdir:""./%s/codecoverage"" -reporttypes:""Html"""
            publishDir

    let result = DotNet.exec id "reportgenerator" args

    if not result.OK then
        failwithf "reportgenerator failed!")

Target.create "Consolidate code coverage" (fun _ ->
    let args =
        sprintf
            @"-reports:""./test/**/coverage.info"" -targetdir:""./%s/codecoverage"" -reporttypes:""Cobertura"""
            publishDir

    let result = DotNet.exec id "reportgenerator" args

    if not result.OK then
        failwithf "reportgenerator failed!")

Target.create "Check Style Rules" (fun _ ->
    let args = "./src/ ./test/ --recurse --check"
    let result = DotNet.exec id "fantomas" args

    if result.OK then
        Trace.log "No files need formatting"
    elif result.ExitCode = 99 then
        failwith "Some files need formatting, run build with `Apply Style Rules` to resolve this."
    else
        failwithf "Errors while checking formatting: %A" result.Errors)

Target.create "Apply Style Rules" (fun _ ->
    let args = "./src/ ./test/ --recurse"
    let result = DotNet.exec id "fantomas" args

    if result.OK then
        Trace.log "No files need formatting"
    else
        failwithf "Errors while applying formatting: %A" result.Errors)

Target.create "Benchmarks" (fun _ ->
    let args = "-f * "

    let result =
        DotNet.exec id "test/Discorss.Test.Benchmarks/bin/Release/net6.0/Discorss.Test.Benchmarks.dll" args

    if not result.OK then
        failwithf "Benchmarks failed!")

Target.create "All" ignore

"Clean"
==> "Restore"
==> "Check Style Rules"
==> "Build"
==> "Unit Tests"
==> "Generate code coverage reports"
==> "Consolidate code coverage"

"Clean" ==> "Restore" ==> "Build" ==> "Publish services"

"Clean" ==> "Restore" ==> "Build" ==> "Benchmarks"

"Benchmarks" ==> "All"

"Consolidate code coverage" ==> "All"

Target.runOrDefault "All"
