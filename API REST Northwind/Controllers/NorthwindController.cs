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
    [RoutePrefix("Northwind")]
    public class NorthwindController : ApiController
    {
        [HttpGet]
        [Route("Test")]
        public HttpResponseMessage Testing()
        {
            return Request.CreateResponse(
                HttpStatusCode.OK,
                "Test exitoso");
        }
    }
}
