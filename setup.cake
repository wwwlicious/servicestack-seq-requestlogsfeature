#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context, 
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Servicestack.Seq.Requestlogsfeature",
                            repositoryOwner: "wwwlicious",
                            repositoryName: "servicestack-seq-requestlogsfeature",
							              shouldRunCodecov: false,
                            shouldPostToGitter: false,
                            shouldPostToMicrosoftTeams: false,
                            shouldPostToSlack: false,
                            shouldPostToTwitter: false,
                            shouldRunDupFinder: false,
                            shouldGenerateDocumentation: false, // until wyam oin recipe is fixed
                            appVeyorAccountName: "wwwlicious");

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);

Build.RunDotNetCore();
