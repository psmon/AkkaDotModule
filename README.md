# AkkaDotModule

프로젝트명 : 아카닷부트

닷넷 환경에서 AKKA의 모듈을 공통화하고

AKKA를 잘 모르더라도, 유용한 메시지 큐처리를 다양한 프로젝트에서 심플하게

사용할수 있게 하는것이 목표입니다. 버전업이 될때마다 유용한 커스텀 액터모델을 제공하게되며

메시지 큐를 활용한 후처리 시스템을 설계하고자 할때 도움이 되었으면 좋겠습니다.


# 주요 릴리즈 노트

- 0.0.9 : DotNetAPP에서 AkkaDotModule을 쉽게사용하기위한 AkkaLoad 를 추가
- 0.0.8 : 조절기([Usage](TestAkkaDotModule/TestActors/ThrottleWorkTest.cs)) 추가, 메시지를 대량인입하고 조절기에서 안전한 속도제어가 필요할때 사용


# AkkaDotBootApi

AkkaDotModule 를 DotNetCoare API에서 활용하는 샘플을 살펴볼수 있습니다.

# 모듈 테스트

여기서 작성되는 모듈은 버전업시 유닛테스트를 자동 수행하고 있습니다.

유닛테스트는 사용법을 스스로 설명하고 검증하기때문에 별도의 사용 메뉴얼이 필요하지 않습니다.

    dotnet test TestAkkaDotModule

Visual Studio 테스트 탐색기에서 검증결과 확인가능합니다.

![](Doc/ThrottleWork01.png)


# Nuget 경로

    https://www.nuget.org/packages/AkkaDotModule.Webnori/

    dotnet add package AkkaDotModule.Webnori --version x.x.x


## 추가 참고자료
 - http://wiki.webnori.com/display/AKKA : AKKA의 전반적인 컨셉
 - http://wiki.webnori.com/display/webfr/AKKA+Setting+For+NetCore : NetCoreAPI에 AKKA 탑재
 - http://wiki.webnori.com/display/webfr/GitHub+Action+With+Nuget+Package : Github에서 개인 Nuget Package 활용법 


