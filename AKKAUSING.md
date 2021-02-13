## HelloActor

액터의 기본 사용법을 익힐수 있으며 , HelloActor를 통해 기본적인 액터 메시징 처리를 학습합니다.

    // HelloActor 기본액터생성
    AkkaLoad.RegisterActor("helloActor" /*AkkaLoad가 인식하는 유니크명*/,
        actorSystem.ActorOf(Props.Create(() => new HelloActor("webnori")), "helloActor" /*AKKA가 인식하는 Path명*/
    ));

    // 액터의 참조를 얻는방법
    // 액터의 참조가 생성후 변경되지 않으면,AkkaLoad에서 액터 참조를 얻을수 있으며
    // 액터의 참조가 동적으로 변경하면 ActorSelection에의해 주소참조를 얻는것이 유리하다.
    var helloActor = actorSystem.ActorSelection("user/helloActor");
    var helloActor2 = AkkaLoad.ActorSelect("helloActor");

    helloActor.Tell("hello");
    helloActor2.Tell("hello");



## ThrottleWork

Throttle(조절기)는 메시징의 흐름제어를 할때 유용하게 사용할수 있습니다.

    int timeSec = 1;        //처리되는주기
    int elemntPerMax = 5;   //처리되는 주기방 처리할 최대개수            

    var throttleWork = Sys.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)));
    throttleWork.Tell(new SetTarget(probe));

    int totalBatchCount = 30;   //총 테스트 개수
            
    var batchList = new BatchList(batchDatas.ToImmutableList());

    // 데이터를 한꺼번에 큐에 넣는다.
    throttleWork.Tell(batchList);

    지정된 작업자(probe)에서 설정된 속력으로 안정적으로 처리됨

## PriorityMessageMailbox

PriorityMessage(우선순위메시지)는 동시에 발생하는 메시지에대한, 전송 우선순위를 조절할수 있습니다.

    // priority 우선순위높음 = 낮은순자를 위미...
    PriorityMessage msg1 = new PriorityMessage("test1", 5);
    PriorityMessage msg2 = new PriorityMessage("test2", 4);
    PriorityMessage msg3 = new PriorityMessage("test3", 3);
    PriorityMessage msg4 = new PriorityMessage("test4", 2);
    PriorityMessage msg5 = new PriorityMessage("test5", 1);
    mailBoxActor.Tell(msg1);
    mailBoxActor.Tell(msg2);
    mailBoxActor.Tell(msg3);
    mailBoxActor.Tell(msg4);
    mailBoxActor.Tell(msg5);

## KAfka Stream

액터시스템을 이용하여 Kafka를 더 심플하고 강력하게 사용가능합니다.

    // KAFKA 셋팅
    // 각 System은 싱글톤이기때문에 DI를 통해 Controller에서 참조획득가능
    var consumerSystem = app.ApplicationServices.GetService<ConsumerSystem>();
    var producerSystem = app.ApplicationServices.GetService<ProducerSystem>();

    //소비자 : 복수개의 소비자 생성가능
    consumerSystem.Start(new ConsumerAkkaOption()
    {
        KafkaGroupId = "testGroup",
        KafkaUrl = "kafka:9092",
        RelayActor = null,          //소비되는 메시지가 지정 액터로 전달되기때문에,처리기는 액터로 구현
        Topics = "akka100"
    });

    //생산자 : 복수개의 생산자 생성가능
    producerSystem.Start(new ProducerAkkaOption()
    {
        KafkaUrl = "kafka:9092",
        ProducerName = "producer1"
    });

    List<string> messages = new List<string>();
    //보너스 : 생산의 속도를 조절할수 있습니다.
    int tps = 10;
    producerSystem.SinkMessage("producer1", "akka100", messages, tps);

## AKKA 모니터링을 위한 DatadogAgent

- https://docs.datadoghq.com/agent/docker/?tab=standard
- https://docs.datadoghq.com/agent/basic_agent_usage/windows/?tab=gui




