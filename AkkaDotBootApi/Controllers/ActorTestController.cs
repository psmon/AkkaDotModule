using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using AkkaDotModule.Config;
using AkkaDotModule.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AkkaDotBootApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActorTestController : ControllerBase
    {

        private readonly ILogger<ActorTestController> _logger;

        private readonly IActorRef throttleWork;

        private readonly ActorSystem _actorSystem;


        public ActorTestController(ILogger<ActorTestController> logger, ActorSystem actorSystem)
        {
            _logger = logger;
            throttleWork = AkkaLoad.ActorSelect("throttleWork");
            _actorSystem = actorSystem;
        }

        /// <summary>
        /// ThrottleWorkTest
        /// </summary>
        [HttpGet("ThrottleWorkTest")]
        public int ThrottleWorkTest()
        {
            List<object> batchDatas = new List<object>();
            int totalBatchCount = 100;
            //테스트 데이터 준비
            for (int i = 0; i < totalBatchCount; i++)
            {
                batchDatas.Add(new BatchData()
                {
                    Data = $"test-{i}"
                });
            }

            var batchList = new BatchList(batchDatas.ToImmutableList());

            throttleWork.Tell(batchList);

            return 1;
        }

        /// <summary>
        /// HelloActor-Tell (fire or hello)
        /// 특정 액터에게 메시지만 전송(fire and forget)
        /// </summary>
        [HttpGet("HelloActor-Tell")]
        public int HelloActor_Tell(string message)
        {
            var helloActor = _actorSystem.ActorSelection("user/helloActor");
            helloActor.Tell(message);
            return 1;
        }

        /// <summary>
        /// HelloActor-ASK (hello)
        /// 액터에게 메시지 전송후,결과값을 받을수 있음(ask)
        /// </summary>
        [HttpGet("HelloActor-Ask")]
        public string HelloActor_Ask()
        {
            var helloActor = _actorSystem.ActorSelection("user/helloActor");
            var result = helloActor.Ask("hello").Result as string;
            return result;
        }

    }
}
