

# Private Nuget

Github Nuget빌드를 구축하고자 할시 참고 하세요..

## 개인 Nuget 등록(최초1회)

    dotnet nuget add source https://nuget.pkg.github.com/psmon/index.json -n psmon.github -u ${{ secrets.NUGET_USER }} -p ${{ secrets.NUGET_TOKEN }} --store-password-in-clear-text


## Build And Push

    dotnet restore AkkaDotModule/AkkaDotModule.csproj

    dotnet build AkkaDotModule/AkkaDotModule.csproj --configuration Release --no-restore

    dotnet pack --configuration Release

    dotnet nuget push "AkkaDotModule/bin/Release/AkkaDotModule.Webnori.0.0.5.nupkg" --source psmon.github


## Nuget 명령

    dotnet list package

    dotnet nuget list source

    dotnet nuget remove source psmon.github