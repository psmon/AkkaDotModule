# AkkaDotModule

프로젝트명 : 아카닷부트

닷넷 환경에서 AKKA의 모듈을 공통화하고

AKKA를 잘 모르더라도, 유용한 메시지 큐처리를 다양한 프로젝트에서 심플하게

사용할수 있게 하는것이 목표입니다. 버전업이 될때마다 유용한 커스텀 액터모델을 제공하게되며

메시지 큐를 활용한 후처리 시스템을 설계하고자 할때 도움이 되었으면 좋겠습니다.


# 제공기능

- 0.0.8 : 조절기([Usage](TestAkkaDotModule/TestActors/ThrottleWorkTest.cs)) 추가, 메시지를 대량인입하고 조절기에서 안전한 속도제어가 필요할때 사용


# 모듈 테스트

여기서 작성되는 모듈은 버전업시 유닛테스트를 자동 수행하고 있습니다.

유닛테스트는 사용법을 스스로 설명하고 검증하기때문에 별도의 사용 메뉴얼이 필요하지 않습니다.

    dotnet test TestAkkaDotModule

Visual Studio 테스트 탐색기에서 검증결과 확인가능합니다.

![](Doc/ThrottleWork01.png)



# 모듈 사용방법

Private Github Nuget을 학습하고자 활용하고 있으며 Nuget을 읽어오는 권한은 Open하였습니다.

Nuget 셋팅없이, 프로젝트 참조로도 사용가능하며, 본 소스는 마음것 수정하여 활용가능합니다.

    Nuget Read Token : 9d5defa3db7ec456b0bad0b273e720ff33860396

## nuget.config


'''
    
    //nuget.config로 저장하여 사용할 동일프로젝트 루트에 위치
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <packageSources>
		    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
            <add key="psmon.github" value="https://nuget.pkg.github.com/psmon/index.json" />
        </packageSources>
        <packageSourceCredentials>
		    <psmon.github>
                <add key="Username" value="psmon" />
                <add key="ClearTextPassword" value="9d5defa3db7ec456b0bad0b273e720ff33860396" />
        </psmon.github>
        </packageSourceCredentials>
    </configuration>


    //프로젝트내 Nuget참조 추가
    <ItemGroup>
        <PackageReference Include="AkkaDotModule.Webnori" Version="0.0.8" />
    </ItemGroup>

'''


## 추가 참고자료
 - http://wiki.webnori.com/display/AKKA : AKKA의 전반적인 컨셉
 - http://wiki.webnori.com/display/webfr/AKKA+Setting+For+NetCore : NetCoreAPI에 AKKA 탑재
 - http://wiki.webnori.com/display/webfr/GitHub+Action+With+Nuget+Package : Github에서 개인 Nuget Package 활용법 


