FORFILES /m *.nupkg /C "cmd /c nuget push @file -Source https://api.nuget.org/v3/index.json"
FORFILES /m *.nupkg /C "cmd /c del @file"