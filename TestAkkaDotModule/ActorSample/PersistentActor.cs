using System;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Persistence;

namespace TestAkkaDotModule.ActorSample
{
    #region MessageData
    public class Shutdown { }

    //커멘드와 이벤트를 분리합니다. 커멘드는 이벤트를 발생시키는 명령이며
    //1 커멘드는 n개의 이벤트로 복제가 될수 있습니다.
    public class Cmd
    {
        public Cmd(string data)
        {
            Data = data;
        }

        public string Data { get; }
    }

    public class Evt
    {
        public Evt(string data)
        {
            Data = data;
        }

        public string Data { get; }
    }

    public class ExampleState
    {
        private readonly ImmutableList<string> _events;

        public ExampleState(ImmutableList<string> events)
        {
            _events = events;
        }

        public ExampleState() : this(ImmutableList.Create<string>())
        {
        }

        public ExampleState Updated(Evt evt)
        {
            return new ExampleState(_events.Add(evt.Data));
        }

        public ExampleState RemoveLastItem()
        {
            return new ExampleState(_events.RemoveAt(_events.Count - 1));
        }

        public int Size => _events.Count;

        public override string ToString()
        {
            return string.Join(", ", _events.Reverse());
        }

        public int CountInBasket()
        {
            int Count = 0;
            //이벤트를 재생하여 실제 장바구니에 담긴 상품수를 반환한다.
            foreach (var str in _events)
            {
                if (str.Contains("담음"))
                {
                    Count++;
                }
                else if (str.Contains("뺌"))
                {
                    Count--;
                }
            }
            return Count;
        }
    }

    #endregion

    #region Actor
    public class MyPersistentActor : UntypedPersistentActor
    {
        private ExampleState _state = new ExampleState();

        protected IActorRef probe;

        public MyPersistentActor(IActorRef probe)
        {
            this.probe = probe;
        }

        private void UpdateState(Evt evt)
        {
            _state = _state.Updated(evt);
        }

        private int NumEvents => _state.Size;

        public override Recovery Recovery => new Recovery(fromSnapshot: SnapshotSelectionCriteria.None);

        protected override void OnRecover(object message)
        {
            switch (message)
            {
                case Evt evt:
                    UpdateState(evt);
                    break;
                case SnapshotOffer snapshot when snapshot.Snapshot is ExampleState:
                    _state = (ExampleState)snapshot.Snapshot;
                    break;
            }
        }

        protected override void OnCommand(object message)
        {
            switch (message)
            {
                case Cmd cmd:
                    // Command는 두개의 이벤트로 복제될수 있습니다.
                    //Persist(new Evt($"{cmd.Data}-{NumEvents}"), UpdateState);

                    Persist(new Evt($"{cmd.Data}-{NumEvents + 1}"), evt =>
                    {
                        UpdateState(evt);
                        Context.System.EventStream.Publish(evt);
                    });
                    break;
                case "snap":
                    SaveSnapshot(_state);
                    break;
                case "print":
                    //모든 이벤트를 보여준다.
                    Console.WriteLine("Try print");
                    Console.WriteLine(_state);

                    //이벤트 재생시 사이즈를 알려준다.(이벤트 복구 검증용)
                    probe.Tell(_state.CountInBasket());
                    break;
                case Shutdown down:
                    probe.Tell("die");
                    Context.Stop(Self);
                    break;
            }
        }

        public override string PersistenceId { get; } = "sample-id-1";  //영속성을 위한 고유한 아이디값을 가집니다.
    }


    #endregion
}
