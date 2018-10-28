# commons-poc

This repository illustrates one of the ways how to share common code.

The straightforward way to approach that is to have a single library.
However, such libraries tend to grow and become unmanageable. Over time 
too many dependencies are added. When some project needs to use such 
library, it has to get all unrelated dependencies as well. 
It becomes messy especially when the project has already some of that
dependencies but of different versions. 

Another approach would be to copy separate files from a central repository.
This is cumbersome both to copy and then maintain copied code. Especially 
that tests should be copied as well. 

Another approach would be to have many separate libraries and distribute 
them as NuGet packages. They will have minimum dependencies and will be easily versioned. It might look tedious but that can be automated. 
This repository shows how NuGet packages can be created and published 
automatically. 

To add a new shared piece of code:
1. Create a new directory in `src` e.g. `Library1`
2. `cd src\Library1`
3. Create a new library: `dotnet new classlib`
4. Add at least these two properties to `Library1.csproj` e.g.:
```
<PropertyGroup>
    <PackageId>MyCompany.Library1</PackageId>
    <PackageVersion>1.0.2</PackageVersion>
</PropertyGroup>
```
Dependencies are included to the NuGet spec automatically.
4. Publish a package:
```
cd ../../
./build.sh --target=Publish
```

That's it!

Note, you need to specify your MyGet feed and API key in `build.cake`.

Package will be published to your MyGet feed if this package 
with this version hasn't been yet published. 

To publish a new version of your library:
1. Make changes to your code
2. Bump the version in `Library1.csproj`
3. Publish a package:
```
./build.sh --target=Publish
```

Building, testing and publishing can be automated with CI e.g. TeamCity. Then just `git commit` and `git push` to publish your shared code. 