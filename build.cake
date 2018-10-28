#addin nuget:?package=Cake.NuGet&version=0.30.0
#addin "nuget:?package=Cake.Incubator&version=3.0.0"

var target = Argument("target", "Default");
var libraryProjectFiles = GetFiles("./src/*/*.csproj");
var unitTestProjectFiles = GetFiles("./test/*.UnitTests/*.csproj");
var allProjectFiles = libraryProjectFiles.Concat(unitTestProjectFiles);

Task("Default")
    .IsDependentOn("Build");

Task("Build")
    .DoesForEach(allProjectFiles, projectFile =>
{
    Information("Building project: {0}", projectFile);
    DotNetCoreBuild(projectFile.ToString());
});

Task("UnitTest")
    .IsDependentOn("Build")
    .DoesForEach(unitTestProjectFiles, projectFile =>
{
    Information("Running unit tests: {0}", projectFile);
    DotNetCoreTest(projectFile.ToString());
});

//TODO Define a task for integration tests

Task("Publish")
    .DoesForEach(libraryProjectFiles, projectFile =>
{
    Information("Publishing library project: {0}", projectFile);

    DirectoryPath projectDir = projectFile.GetDirectory();
    
    var project = ParseProject(projectFile, configuration: "Release");
    DirectoryPath projectOutDir = project.OutputPath;
    Information("Project output path: {0}", project.OutputPath);

    Information("Building project with Release configuration");
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = "Release",
            OutputDirectory = projectOutDir
        };

        DotNetCoreBuild(projectFile.ToString(), settings);
    }
    
    DirectoryPath packageOutDir = projectDir.Combine(new DirectoryPath("nuget"));
    Information("Package output directory: {0}", packageOutDir);

    var packageId = XmlPeek(projectFile, "/Project/PropertyGroup/PackageId");
    Information("Package ID: " + packageId);
    if (string.IsNullOrEmpty(packageId))
    {
        throw new InvalidOperationException("Package ID is empty");
    }

    var packageVersion = XmlPeek(projectFile, "/Project/PropertyGroup/PackageVersion");
    Information("Package version: " + packageVersion);
    if (string.IsNullOrEmpty(packageVersion))
    {
        throw new InvalidOperationException("Package version is empty");
    }

    Information("Check if this version was already published");
    {
        var settings = new NuGetListSettings 
        {
            AllVersions = true,
            Prerelease = true,
            Source = new string[] { "https://www.myget.org/F/mkuz/api/v3/index.json" }
        };

        var foundPackages = NuGetList("PackageId:" + packageId, settings);

        foreach(var foundPackage in foundPackages)
        {
            Information("Found version {0}", foundPackage.Version);
            if (foundPackage.Version == packageVersion)
            {
                Information("Version {0} has been already published", foundPackage.Version);
                return;
            }
        }

        Information("Version {0} was not found. Proceed to publishing. ", packageVersion);
    }
    
    Information("Creating NuGet package");
    {
        var settings = new DotNetCorePackSettings 
        {
            Configuration = "Release",
            IncludeSource = true,
            IncludeSymbols = false,
            OutputDirectory = packageOutDir
        };

        DotNetCorePack(projectFile.ToString(), settings);
    }

    Information("Pushing NuGet package");
    {
        var packageFileName = $"{packageId}.{packageVersion}.nupkg";
        var packageFile = packageOutDir.GetFilePath(new FilePath(packageFileName));
        Information("Package file: {0}", packageFile);

        var settings = new DotNetCoreNuGetPushSettings 
        {
            Source = "https://www.myget.org/F/mkuz/api/v2/package",
            ApiKey = "<api-key>"
        };

        DotNetCoreNuGetPush(packageFile.ToString(), settings);
    }

    Information("Project was published: {0}", projectFile);
});

RunTarget(target);