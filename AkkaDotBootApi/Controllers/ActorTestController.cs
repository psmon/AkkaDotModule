using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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

        public ActorTestController(ILogger<ActorTestController> logger)
        {
            _logger = logger;
            throttleWork = AkkaLoad.ActorSelect("throttleWork");
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
    }
}
