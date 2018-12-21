using Animus.ANN.Core;
using Animus.ANN.Types;
using Animus.Tensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SchedulingAPI.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        public object Hello() => "Hello";

        [HttpGet]
        public object Check()
        {
            return typeof(FeedForwardNet<Vector>).Name;
        }
    }
}
