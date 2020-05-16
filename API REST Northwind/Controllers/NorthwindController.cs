using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Web.Http.Cors;

namespace API_REST_Northwind.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    [RoutePrefix("Northwind/v1")]
    public class NorthwindController : ApiController
    {
        [HttpGet]
        [Route("Test")]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(
                HttpStatusCode.OK,
                "Test exitoso");
        }

        [HttpPost]
        [Route("TestPost")]
        public HttpResponseMessage TestPost([FromBody]example s)
        {
            return Request.CreateResponse(
               HttpStatusCode.OK,
               $"Test exitoso {s.palabra}");
        }
    }

    public class example
    {
        public int num { get; set; }
        public string palabra { get; set; }
    }
}
