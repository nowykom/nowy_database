using System;
using System.Linq;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    GitHubActions GitHubActions => GitHubActions.Instance;

    [GitRepository] readonly GitRepository GitRepository;
    [Solution] readonly Solution Solution;

    public static int Main() => Execute<Build>(x => x.Upgrade, x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "build" / "artifacts";

    static bool IsRunningOnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    string NuGetAzureDevOpsSource => "https://pkgs.dev.azure.com/schulz-dev/nowy/_packaging/Nowy/nuget/v3/index.json";
    string NuGetAzureDevOpsUser => "schulz-dev";
    [Parameter] [Secret] string NuGetAzureDevOpsPassword;

    protected override void OnBuildInitialized()
    {
        /*
        if (false)
        {
            VersionSuffix = !IsTaggedBuild ? $"preview-{DateTime.UtcNow:yyyyMMdd-HHmm}" : "";
        }

        VersionSuffix = "";

        if (IsLocalBuild)
        {
            VersionSuffix = $"dev-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        }

        Log.Information("BUILD SETUP");
        Log.Information("Configuration:\t{Configuration}", Configuration);
        Log.Information("Version suffix:\t{VersionSuffix}", VersionSuffix);
        Log.Information("Tagged build:\t{IsTaggedBuild}", IsTaggedBuild);
        */

        Log.Information("BUILD SETUP");
        Log.Information("Configuration:\t{Configuration}", Configuration);
    }

    Target Upgrade => _ => _
        .Before(Restore)
        .Executes(() =>
        {
// dotnet tool install --global dotnet-outdated-tool
            try
            {
                DotNetTasks.DotNetToolInstall(
                    _ => _
                        .SetPackageName("dotnet-outdated-tool")
                        .EnableGlobal()
                );
            }
            catch (Exception)
            {
                DotNetTasks.DotNetToolUpdate(
                    _ => _
                        .SetPackageName("dotnet-outdated-tool")
                        .EnableGlobal()
                );
            }


            DotNetTasks.DotNet($"outdated --upgrade {RootDirectory / "src"}");
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild));
        });

    Target CreateNugetPackages => _ => _
        .DependsOn(Compile)
        .Produces(ArtifactsDirectory / "*.*")
        .Requires(() => NuGetAzureDevOpsPassword)
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);

            string[] projects = new[]
            {
                "Nowy.Database.Client",
            };

            foreach (string project in projects)
            {
                DotNetTasks.DotNetPack(s => s
                    .SetProject(Solution.GetProject(project))
                    // .SetAssemblyVersion(TagVersion)
                    // .SetFileVersion(TagVersion)
                    // .SetInformationalVersion(TagVersion)
                    // .SetVersionSuffix(VersionSuffix)
                    .SetConfiguration(Configuration)
                    .SetOutputDirectory(ArtifactsDirectory)
                    .SetDeterministic(IsServerBuild)
                    .SetContinuousIntegrationBuild(IsServerBuild)
                );
            }

            try
            {
                DotNetTasks.DotNet($"new nugetconfig", ArtifactsDirectory);
            }
            catch (Exception ex)
            {
            }

            DotNetTasks.DotNetNuGetAddSource(
                _ => _
                    .SetName(nameof(NuGetAzureDevOpsSource))
                    .SetUsername(NuGetAzureDevOpsUser)
                    .SetPassword(NuGetAzureDevOpsPassword)
                    .SetSource(NuGetAzureDevOpsSource)
                    .EnableStorePasswordInClearText()
                    .SetProcessWorkingDirectory(ArtifactsDirectory)
            );

            foreach (AbsolutePath file in ArtifactsDirectory.GlobFiles("*.nupkg"))
            {
                DotNetTasks.DotNetNuGetPush(
                    _ => _
                        .SetSource(nameof(NuGetAzureDevOpsSource))
                        .SetApiKey(NuGetAzureDevOpsPassword)
                        .EnableSkipDuplicate()
                        .SetTargetPath(file)
                        .SetProcessWorkingDirectory(ArtifactsDirectory)
                );
            }
        });


    Target Pack => _ => _
        .DependsOn(Upgrade, CreateNugetPackages);

    Target CiGithubActionsLinux => _ => _
        .DependsOn(Upgrade, CreateNugetPackages);

    Target Deploy => _ => _
        .DependsOn(Upgrade, Compile)
        .Executes(() =>
        {
        });
}
