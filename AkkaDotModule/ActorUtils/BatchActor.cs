using Akka.Actor;
using Akka.Event;
using AkkaDotModule.Models;
using System;
using System.Collections.Immutable;

namespace AkkaDotModule.ActorUtils
{
    public class BatchActor : FSM<State, IData>
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();
        private int CollectSec;

        public BatchActor(int _CollectSec)
        {
            CollectSec = _CollectSec;

            StartWith(State.Idle, Uninitialized.Instance);

            When(State.Idle, state =>
            {
                if (state.FsmEvent is SetTarget target && state.StateData is Uninitialized)
                {
                    return Stay().Using(new Todo(target.Ref, ImmutableList<object>.Empty));
                }

                return null;
            });

            When(State.Active, state =>
            {
                if ((state.FsmEvent is Flush || state.FsmEvent is StateTimeout)
                    && state.StateData is Todo t)
                {
                    return GoTo(State.Idle).Using(t.Copy(ImmutableList<object>.Empty));
                }

                return null;
            }, TimeSpan.FromSeconds(CollectSec));

            WhenUnhandled(state =>
            {
                if (state.FsmEvent is Queue q && state.StateData is Todo t)
                {
                    return GoTo(State.Active).Using(t.Copy(t.Queue.Add(q.Obj)));
                }
                else
                {
                    logger.Warning($"Received unhandled request {state.FsmEvent} in state {StateName}/{state.StateData}");
                    return Stay();
                }
            });

            OnTransition((initialState, nextState) =>
            {
                if (initialState == State.Active && nextState == State.Idle)
                {
                    if (StateData is Todo todo)
                    {
                        todo.Target.Tell(new Batch(todo.Queue));
                    }
                    else
                    {
                        // nothing to do
                    }
                }
            });

            Initialize();
        }
    }
}
