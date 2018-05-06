#addin nuget:?package=Cake.Incubator&version=2.0.2
#addin nuget:?package=Cake.AppVeyor&version=3.0.0

#tool nuget:?package=GitVersion.CommandLine&version=4.0.0-beta0012
#tool nuget:?package=xunit.runner.console&version=2.3.1
#tool nuget:?package=gitreleasemanager&version=0.7.0


///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var nuGetApiKeyVariable = EnvironmentVariable("NUGET_API_KEY");
var nuGetSourceUrlVariable = EnvironmentVariable("NUGET_SOURCE");
var gitHubUserName = EnvironmentVariable("GITHUB_USERNAME");
var gitHubPassword = EnvironmentVariable("GITHUB_PASSWORD");
var configuration = Argument("configuration", "Debug");
var target = Argument("target", "Default");
var prepareLocalRelease = Argument("prepareLocalRelease", false);
var repositoryOwner = "wwwlicious";
var repositoryName = "servicestack-seq-requestlogsfeature";
var publishingError = false;

// Directories
var buildDirectoryPath = "./.artifacts";
var testResultsDirectory = buildDirectoryPath + "/TestResults";
var nuGetPackagesOutputDirectory = buildDirectoryPath + "/nuget";


// Files
var buildLogFilePath = ((DirectoryPath)buildDirectoryPath).CombineWithFilePath("MsBuild.binlog");
var solutionFile = "./src/ServiceStack.Seq.RequestLogsFeature.sln";

// versioning 
var gvSettings = new GitVersionSettings();
if(BuildSystem.IsRunningOnAppVeyor) gvSettings.OutputType = GitVersionOutput.BuildServer;
var gitVersion = GitVersion(gvSettings);
var version = gitVersion.MajorMinorPatch;
var milestone = version;
var semVersion = gitVersion.SemVer;
var informationalVersion = gitVersion.InformationalVersion;
var fullSemVersion = gitVersion.FullSemVer;
var milestoneReleaseNotesFilePath = ((DirectoryPath)buildDirectoryPath).CombineWithFilePath($"{milestone}.md");

var isLocalBuild = BuildSystem.IsLocalBuild;
var isMasterBranch = gitVersion.BranchName.EqualsIgnoreCase("master");
var isDevelopBranch = gitVersion.BranchName.EqualsIgnoreCase("develop");
var isReleaseBranch = gitVersion.BranchName.StartsWith("release", StringComparison.OrdinalIgnoreCase);
var isHotFixBranch = gitVersion.BranchName.StartsWith("hotfix", StringComparison.OrdinalIgnoreCase);
var isPullRequest = BuildSystem.IsRunningOnAppVeyor && BuildSystem.AppVeyor.Environment.PullRequest.IsPullRequest;
var isTagged = BuildSystem.IsRunningOnAppVeyor && (BuildSystem.AppVeyor.Environment.Repository.Tag.IsTag &&
            !string.IsNullOrWhiteSpace(BuildSystem.AppVeyor.Environment.Repository.Tag.Name));
var shouldPublishRelease = (!isLocalBuild &&
                                !isPullRequest &&
                                (isMasterBranch || isReleaseBranch || isHotFixBranch) && isTagged);


// parse projects
var solution = ParseSolution(solutionFile);
var projects = solution.GetProjects().Select(x => ParseProject(x.Path, configuration)).ToArray();

Information(gitVersion.Dump());
Information(projects.Dump());

Task("Restore")
    .Does(() => {
    DotNetCoreRestore(solutionFile);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => {
        Information("Building {0}", solutionFile);

        var dotnetCoreMsBuildSettings = new DotNetCoreMSBuildSettings{
        };

        var releaseNotes = string.Empty;
        if(FileExists(milestoneReleaseNotesFilePath)){
            releaseNotes = string.Join("\n", ParseReleaseNotes(milestoneReleaseNotesFilePath).Notes);
        }
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            MSBuildSettings = dotnetCoreMsBuildSettings,
            ArgumentCustomization = args => args
                .Append("/p:Version={0}", semVersion)
                .Append("/p:AssemblyVersion={0}", version)
                .Append("/p:FileVersion={0}", version)
                .Append("/p:AssemblyInformationalVersion={0}", informationalVersion)
                .AppendQuoted("/p:PackageOutputPath={0}", MakeAbsolute((DirectoryPath)nuGetPackagesOutputDirectory).FullPath)
                .AppendQuoted("/p:PackageReleaseNotes={0}", releaseNotes)
                .Append("/bl:{0}", buildLogFilePath)
        };

        DotNetCoreBuild(solutionFile, settings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() => {

    foreach (var project in projects.Where(x => x.IsDotNetCliTestProject()))
    {
        foreach(var framework in project.TargetFrameworkVersions){
            var settings = new DotNetCoreTestSettings
            {
                Configuration = configuration,
                Framework = framework,
                NoBuild = true,
                NoRestore = true,
            };
            DotNetCoreTest(project.ProjectFilePath.FullPath, settings);
        }
    };
});

///////////////////////////////////////////////////////////////////////////////
// APPVEYOR
///////////////////////////////////////////////////////////////////////////////

Task("Print-AppVeyor-Environment-Variables")
    .WithCriteria(() => BuildSystem.IsRunningOnAppVeyor)
    .Does(() =>
{
    Information("CI: {0}", EnvironmentVariable("CI"));
    Information("APPVEYOR_API_URL: {0}", EnvironmentVariable("APPVEYOR_API_URL"));
    Information("APPVEYOR_PROJECT_ID: {0}", EnvironmentVariable("APPVEYOR_PROJECT_ID"));
    Information("APPVEYOR_PROJECT_NAME: {0}", EnvironmentVariable("APPVEYOR_PROJECT_NAME"));
    Information("APPVEYOR_PROJECT_SLUG: {0}", EnvironmentVariable("APPVEYOR_PROJECT_SLUG"));
    Information("APPVEYOR_BUILD_FOLDER: {0}", EnvironmentVariable("APPVEYOR_BUILD_FOLDER"));
    Information("APPVEYOR_BUILD_ID: {0}", EnvironmentVariable("APPVEYOR_BUILD_ID"));
    Information("APPVEYOR_BUILD_NUMBER: {0}", EnvironmentVariable("APPVEYOR_BUILD_NUMBER"));
    Information("APPVEYOR_BUILD_VERSION: {0}", EnvironmentVariable("APPVEYOR_BUILD_VERSION"));
    Information("APPVEYOR_PULL_REQUEST_NUMBER: {0}", EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER"));
    Information("APPVEYOR_PULL_REQUEST_TITLE: {0}", EnvironmentVariable("APPVEYOR_PULL_REQUEST_TITLE"));
    Information("APPVEYOR_JOB_ID: {0}", EnvironmentVariable("APPVEYOR_JOB_ID"));
    Information("APPVEYOR_REPO_PROVIDER: {0}", EnvironmentVariable("APPVEYOR_REPO_PROVIDER"));
    Information("APPVEYOR_REPO_SCM: {0}", EnvironmentVariable("APPVEYOR_REPO_SCM"));
    Information("APPVEYOR_REPO_NAME: {0}", EnvironmentVariable("APPVEYOR_REPO_NAME"));
    Information("APPVEYOR_REPO_BRANCH: {0}", EnvironmentVariable("APPVEYOR_REPO_BRANCH"));
    Information("APPVEYOR_REPO_TAG: {0}", EnvironmentVariable("APPVEYOR_REPO_TAG"));
    Information("APPVEYOR_REPO_TAG_NAME: {0}", EnvironmentVariable("APPVEYOR_REPO_TAG_NAME"));
    Information("APPVEYOR_REPO_COMMIT: {0}", EnvironmentVariable("APPVEYOR_REPO_COMMIT"));
    Information("APPVEYOR_REPO_COMMIT_AUTHOR: {0}", EnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR"));
    Information("APPVEYOR_REPO_COMMIT_TIMESTAMP: {0}", EnvironmentVariable("APPVEYOR_REPO_COMMIT_TIMESTAMP"));
    Information("APPVEYOR_SCHEDULED_BUILD: {0}", EnvironmentVariable("APPVEYOR_SCHEDULED_BUILD"));
    Information("APPVEYOR_FORCED_BUILD: {0}", EnvironmentVariable("APPVEYOR_FORCED_BUILD"));
    Information("APPVEYOR_RE_BUILD: {0}", EnvironmentVariable("APPVEYOR_RE_BUILD"));
    Information("PLATFORM: {0}", EnvironmentVariable("PLATFORM"));
    Information("CONFIGURATION: {0}", EnvironmentVariable("CONFIGURATION"));
});

Task("Upload-AppVeyor-Artifacts")
    .WithCriteria(() => BuildSystem.IsRunningOnAppVeyor)
    .WithCriteria(() => DirectoryExists(nuGetPackagesOutputDirectory))
    .Does(() =>
{
    foreach(var package in GetFiles(nuGetPackagesOutputDirectory + "/*"))
    {
        AppVeyor.UploadArtifact(package);
    }
});

///////////////////////////////////////////////////////////////////////////////
// NUGET
///////////////////////////////////////////////////////////////////////////////

Task("Publish-Nuget-Packages")
    .WithCriteria(() => shouldPublishRelease)
    .WithCriteria(() => DirectoryExists(nuGetPackagesOutputDirectory))
    .Does(() =>
{
    var nupkgFiles = GetFiles(nuGetPackagesOutputDirectory + "/**/*.nupkg");

    foreach(var nupkgFile in nupkgFiles)
    {
        // Push the package.
        NuGetPush(nupkgFile, new NuGetPushSettings {
            Source = nuGetSourceUrlVariable,
            ApiKey = nuGetApiKeyVariable
        });
    }
})
.OnError(exception =>
{
    Error(exception.Message);
    Information("Publish-Nuget-Packages Task failed, but continuing with next Task...");
    publishingError = true;
});

///////////////////////////////////////////////////////////////////////////////
// GITRELEASEMANAGER
///////////////////////////////////////////////////////////////////////////////

Task("Create-Release-Notes")
    .Does(() => {
        GitReleaseManagerCreate(gitHubUserName, gitHubPassword, repositoryOwner, repositoryName, 
        new GitReleaseManagerCreateSettings {
            Milestone         = milestone,
            Name              = milestone,
            Prerelease        = false,
            TargetCommitish   = "master"
        });
    });

Task("Export-Release-Notes")
    .Does(() => {
        GitReleaseManagerExport(gitHubUserName, gitHubPassword, repositoryOwner, repositoryName, milestoneReleaseNotesFilePath, new GitReleaseManagerExportSettings {
            TagName = milestone
        });
    })
    .OnError(exception => {
        Warning(exception.Message);
        Information("No git release found or invalid credentials");
        publishingError = true;
    });

Task("Publish-GitHub-Release")
    .WithCriteria(() => shouldPublishRelease)
    .Does(() => {
            // Concatenating FilePathCollections should make sure we get unique FilePaths
            foreach(var package in GetFiles(nuGetPackagesOutputDirectory + "/*"))
            {
                GitReleaseManagerAddAssets(gitHubUserName, gitHubPassword, repositoryOwner, repositoryName, milestone, package.ToString());
            }

            GitReleaseManagerClose(gitHubUserName, gitHubPassword, repositoryOwner, repositoryName, milestone);
})
.OnError(exception =>
{
    Error(exception.Message);
    Information("Publish-GitHub-Release Task failed, but continuing with next Task...");
    publishingError = true;
});



Task("Default")
    .IsDependentOn("Print-AppVeyor-Environment-Variables")
    .IsDependentOn("Export-Release-Notes")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("ReleaseNotes")
  .IsDependentOn("Create-Release-Notes");

Task("AppVeyor")
    .WithCriteria(shouldPublishRelease)
    .IsDependentOn("Print-AppVeyor-Environment-Variables")
    .IsDependentOn("Export-Release-Notes")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Upload-AppVeyor-Artifacts")    
    .IsDependentOn("Publish-Nuget-Packages")
    .IsDependentOn("Publish-GitHub-Release")
    .Finally(() =>
    {
        if(publishingError)
        {
            throw new Exception($"An error occurred during the publishing of {solutionFile}.  All publishing tasks have been attempted.");
        }
    });

RunTarget(target);
