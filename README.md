# AkkaDotModule

## 저장소 : https://github.com/psmon/AkkaDotModule

프로젝트명 : 아카닷모듈

닷넷 환경에서 AKKA(https://getakka.net/)의 모듈을 안정적으로 공통화하고 AKKA.NET을 학습할수 있는 환경을 제공하여

닷넷코어에서 유용한 메시지 큐처리를 다양한 프로젝트에서 심플하게 사용할수 있게 하는것이 목표입니다.

버전업이 될때마다 유용한 커스텀 액터모델을 제공및 설명하며, 액터와 API에서 사용 샘플을 동시에 추가합니다.

AKKA의 버전업에 항상 대응하는것이아닌, 유닛테스트를 통해 안정성을 검증하고 다양한 메시지처리기능을

안정적으로 사용하는 것에 목적이 있습니다.

# AppLayOut

- AkkaDotBootApi : 닷넷코어 API에서 AKKA를 활용하는 샘플 API입니다.
- AkkaBlazorApp : Blazor에서 액터모델을 활용하는 샘플 웹입니다. 
- AkkaDotModule : AKKA.NET 모듈이 패키징되어 있으며, 닷넷코어를 위해 유틸리티화 되어있습니다.
- TestAkkaModule : AkkaDotModule을 테스트하기위한 유닛테스트
- Doc : Akka를 설명하기위한 여러가지 문서들


# 지원기능

- HelloActor : AKKA 입문시 처음봐야할 기본 액터 다루기 (메시지큐를 내장하기)
- ThrottleWork : 스트림의 속도 제어가 필요할때 밸브의 잠금기능을 활용
- PriorityMessageMailbox : 동시에 발생하는 메시지의 우선순위 조절이 필요한경우
- ConsumerSystem : Kafka의 Consumer를 심플하고 강력하게 사용
- ProducerSystem : Kafka의 Producer를 심플하고 강력하게 사용

[More Detail](AKKAUSING.md)

# 주요 릴리즈 노트
- 1.1.1 : Signalr Stream 지원 - http://wiki.webnori.com/display/webfr/SignalR+with+AKKA+Stream
- 1.1.0 : Blazor With AKKA - http://wiki.webnori.com/display/webfr/Blazor+With+AKKA
- 1.0.9 : Kafka ConsumerActor 추가 (목적:Kafka SSL모드지원) - [사용법](AkkaDotBootApi/Test/TestAkka.cs)
- 1.0.8 : Kafka ProducerActor 추가 (목적:Kafka SSL모드지원) - [Link](http://wiki.webnori.com/display/webfr/Auzere+EventHub%28KAFKA%29+With+Actor)
- 1.0.7 : 실시간 배치처리기([BatchActor](TestAkkaDotModule/TestActors/BatchActorTest.cs)) 추가
- 1.0.6 : Kafka 도커 인프라추가및, TestAPI 샘플 추가
- 1.0.5 : Kafka Stream 지원 : 액터시스템을 이용하여 Kafka를 더 심플하고 강력하게 사용가능합니다.
- 1.0.4 : AKKA 1.4.7 버전사용
- 1.0.3 : 메시지 우선순위([PriorityMessageMailbox](TestAkkaDotModule/TestActors/PriorityMessageMailboxTest.cs)) 처리기 추가
- 1.0.1 : 기본 액터([TestActors](TestAkkaDotModule/TestActors/HelloActorTest.cs)) 사용법추가
- 1.0.0 : Nuget에서 AkkaDotModule.Webnori 로 검색하여 설치가능 - 공식 Oepn
- 0.0.9 : DotNetAPP에서 AkkaDotModule을 쉽게사용하기위한 AkkaLoad 를 추가
- 0.0.8 : 조절기([ThrottleWork](TestAkkaDotModule/TestActors/ThrottleWorkTest.cs)) 추가, 메시지를 대량인입하고 조절기에서 안전한 속도제어가 필요할때 사용

# Nuget 경로

Link : https://www.nuget.org/packages/AkkaDotModule.Webnori/

    dotnet add package AkkaDotModule.Webnori --version x.x.x


# 닷넷어플리케이션 탑재

##  akka.conf / akka.kafka.conf 추가

자세한 옵션설정은 프로젝트내 conf 참고

    //akka.conf
    akka {
        loggers = ["Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog"]
        loglevel = debug
    }

    //akka.kafka.conf
    akka.kafka.committer { 
      max-batch = 1000  
      max-interval = 10s  
      parallelism = 100
      delivery = WaitForAck
    }

## Startup.cs 수정

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Akka 셋팅
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var akkaConfig = AkkaLoad.Load(envName, Configuration);
        actorSystem = ActorSystem.Create("AkkaDotBootSystem", akkaConfig);            
        services.AddAkka(actorSystem);
    ...................
    }    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationLifetime lifetime)
    {
        lifetime.ApplicationStarted.Register(() =>
        {
            // 밸브 Work : 초당 작업량을 조절                
            int timeSec = 1;
            int elemntPerSec = 5;
            var throttleWork = AkkaLoad.RegisterActor("throttleWork", 
                actorSystem.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)), "throttleWork"));

            // 실제 Work : 밸브에 방출되는 Task를 개별로 처리
            var worker = AkkaLoad.RegisterActor("worker", actorSystem.ActorOf(Props.Create<WorkActor>(), "worker"));
            // 배브의 작업자를 지정
            throttleWork.Tell(new SetTarget(worker));
        });
    }

## 사용
    
    // DI를 지원할수도 있으나, 유니크한 네이밍으로 스레드 세이프한 경량화된 액터를 선택할수 있습니다.
    IActorRef throttleWork = AkkaLoad.ActorSelect("throttleWork");
    var batchList = new BatchList(batchDatas.ToImmutableList());
    throttleWork.Tell(batchList);
    
    // ACTOR 구성은 TopLevel Architeture로 DI와 어울리지 않으며, 사실상 필요없습니다.
    //DI연동시 다음을 참고 DI를 완벽하게 이해하고, 더 이점이 있을때 사용할것을 권장합니다.     
    참고 url : https://getakka.net/articles/actors/di-core.html
            

## 추가 참고자료
 - https://getakka.net/ : Akka.net Origin - 본 저장소는 Akka.net(한글)입문을 도와주며 AKKA의 고급 학습은 원문사이트에서 하는것을 권장합니다. 
 - http://wiki.webnori.com/display/AKKA : AKKA의 전반적인 컨셉 (JAVA포함)
 - http://wiki.webnori.com/display/webfr/.NET+Core+With+Akka : NetCoreAPI에서 활용 (NET core 전용)
 - http://wiki.webnori.com/display/webfr/Kafka+with+Stream : Kafka 활용하기
 - https://getakka.net/articles/intro/tutorial-1.html : Top레벨 아키텍쳐 : 액터접근은 DI를 사용하지 않아도 충분합니다.
 - https://github.com/Azure/azure-event-hubs-for-kafka : Azure EventHub에 Kafka연결하기
 - https://github.com/confluentinc/confluent-kafka-dotnet : 닷넷 코어로 Kafka연결하기

## 기술지원

이 모듈 사용을 포함 akka에 대한 기술문의 언제든 환영입니다.

Akka FaceBook Link : https://www.facebook.com/groups/akkalabs

Emal : psmon@live.co.kr
