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


