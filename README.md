# AkkaDotModule

닷넷 환경에서 AKKA의 모듈을 쉽게 사용하는 모듈
프라이빗 누겟을 사용하는 방법도 포함되어 있습니다.



# 모듈 사용방법

여기서 사용된 Nuget을 읽어오는 권한은 Open하였습니다.

    Nuget Read Token : 9d5defa3db7ec456b0bad0b273e720ff33860396



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

## 추가 참고자료
 - http://wiki.webnori.com/display/webfr/GitHub+Action+With+Nuget+Package : Github에서 개인 Nuget Package 활용법
 - 