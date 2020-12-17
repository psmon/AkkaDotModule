using System;
using System.IO;
using Akka.TestKit.Xunit2;
using Xunit.Abstractions;

namespace AkkaNetCoreTest
{
    /// <summary>
    /// 설명 : XUnit+TestKit 을 활용할수 있는 추상 객체로 상속받아 사용
    /// 목적 : Xunit 테스트툴에 Console.Write 출력가능 / Akka의 옵션적용 가능
    /// </summary>
    public abstract class TestKitXunit : TestKit
    {
        private readonly ITestOutputHelper _output;
        private readonly TextWriter _originalOut;
        private readonly TextWriter _textWriter;

        static public string akkaConfig = @"
akka.loglevel = DEBUG

my-custom-mailbox {
    mailbox-type : ""AkkaDotModule.Models.PriorityMessageMailbox, AkkaDotModule""
}

akka.persistence.max-concurrent-recoveries = 50 #복구 최고 개수
akka.actor.default-mailbox.stash-capacity = 10000
akka.persistence.internal-stash-overflow-strategy = ""akka.persistence.ThrowExceptionConfigurator""

actor.deployment {
    /mymailbox {
        mailbox = my-custom-mailbox
    }
}

# 기본 스레드 풀 사용 - 동시처리 10
fast-dispatcher {
	type = Dispatcher
	throughput = 10
}

# 기본 스레드 풀 사용 - 동시처리 1
slow-dispatcher {
	type = Dispatcher
	throughput = 1
}

# TPL 사용 - https://docs.microsoft.com/ko-kr/dotnet/standard/parallel-programming/task-parallel-library-tpl
custom-task-dispatcher {
	type = TaskDispatcher
	throughput = 10
}

# ForkJoin : 분할정복 알고리즘(큰 Task를 작은단위로 쪼개고 취합하는방식)
custom-fork-join-dispatcher1 {
	type = ForkJoinDispatcher
	throughput = 100
	dedicated-thread-pool {
		thread-count = 1
		deadlock-timeout = 3s
		threadtype = background
	}
}

custom-fork-join-dispatcher2 {
	type = ForkJoinDispatcher
	throughput = 100
	dedicated-thread-pool {
		thread-count = 2
		deadlock-timeout = 3s
		threadtype = background
	}
}
";
        public TestKitXunit(ITestOutputHelper output) : base(akkaConfig)
        {
            _output = output;
            _originalOut = Console.Out;
            _textWriter = new StringWriter();
            Console.SetOut(_textWriter);         
        }

        protected override void Dispose(bool disposing)
        {            
            _output.WriteLine(_textWriter.ToString());
            Console.SetOut(_originalOut);
            base.Dispose(disposing);
        }
    }
}
