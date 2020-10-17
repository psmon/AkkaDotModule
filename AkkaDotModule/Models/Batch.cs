using Akka.Actor;
using System;
using System.Collections.Immutable;

namespace AkkaDotModule.Models
{
    public class BatchData
    {
        public object Data { get; set; }
    }

    public class BatchList
    {
        public BatchList(ImmutableList<object> obj)
        {
            Obj = obj;
        }

        public ImmutableList<object> Obj { get; }
    }

    public class Queue
    {
        public Queue(object obj)
        {
            Obj = obj;
        }

        public Object Obj { get; }
    }

    public class Flush { }

    // send events
    public class Batch
    {
        public Batch(ImmutableList<object> obj)
        {
            Obj = obj;
        }

        public ImmutableList<object> Obj { get; }
    }

    public enum State
    {
        Idle,
        Active
    }

    // data
    public interface IData { }

    public class Uninitialized : IData
    {
        public static Uninitialized Instance = new Uninitialized();

        private Uninitialized() { }
    }

    public class BaseMessage
    {
    }

    public class Todo : BaseMessage, IData
    {
        public Todo(IActorRef target, ImmutableList<object> queue)
        {
            Target = target;
            Queue = queue;
        }

        public IActorRef Target { get; }

        public ImmutableList<object> Queue { get; }

        public Todo Copy(ImmutableList<object> queue)
        {
            return new Todo(Target, queue);
        }
    }

}
