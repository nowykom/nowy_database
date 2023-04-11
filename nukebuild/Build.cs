using System;
using System.IO;
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

    string NuGetOrgSource => "https://api.nuget.org/v3/index.json";
    [Parameter] [Secret] string NuGetOrgApiKey;

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

    private AbsolutePath HomeDirectory => (AbsolutePath)(
        ( Environment.OSVersion.Platform == PlatformID.Unix ||
          Environment.OSVersion.Platform == PlatformID.MacOSX )
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%")
    );

    Target ClearCache => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNet($"nuget locals http-cache --clear");
            DotNetTasks.DotNet($"nuget locals plugins-cache --clear");
            DotNetTasks.DotNet($"nuget locals temp --clear");
            DeleteDirectory(HomeDirectory / ".dotnet" / "toolResolverCache");
        });

    Target Upgrade => _ => _
        .Before(Restore)
        .DependsOn(ClearCache)
        .Executes(() =>
        {
            DotNetTasks.DotNetToolRestore(
                _ => _.EnableNoCache()
            );

            DotNetTasks.DotNet($"outdated --upgrade --no-restore {Solution.Path}");
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/node_modules").ForEach(DeleteDirectory);
            SourceDirectory.GlobFiles("**/*.stamp").ForEach(DeleteFile);
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
                "Nowy.Auth.Client",
                "Nowy.Auth.ClientWasm",
                "Nowy.Auth.Common",
                "Nowy.Auth.Contract",
                "Nowy.Auth.Server",
                "Nowy.Database.Client",
                "Nowy.Database.Contract",
                "Nowy.Database.Common",
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

            if (!string.IsNullOrEmpty(NuGetOrgApiKey))
            {
                foreach (AbsolutePath file in ArtifactsDirectory.GlobFiles("*.nupkg"))
                {
                    DotNetTasks.DotNetNuGetPush(
                        _ => _
                            .SetSource(NuGetOrgSource)
                            .SetApiKey(NuGetOrgApiKey)
                            .EnableSkipDuplicate()
                            .SetTargetPath(file)
                            .SetProcessWorkingDirectory(ArtifactsDirectory)
                    );
                }
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
