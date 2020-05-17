using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Web.Http.Cors;

namespace API_REST_Northwind.Controllers
{

    public class example
    {
        public int num { get; set; }
        public string palabra { get; set; }
    }

    [EnableCors(origins: "*", headers: "*", methods: "*")]
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

        [HttpPost]
        [Route("GetTop5Pie")]
        public HttpResponseMessage GetTop5YearMonth([FromBody] string[] parameter)
        {
            switch (parameter[0])
            {
                case "Cliente":
                    parameter[0] = "[Dim Cliente].[Dim Cliente Nombre].CHILDREN";
                    break;
                case "Producto":
                    parameter[0] = "[Dim Producto].[Dim Producto Nombre].CHILDREN";
                    break;
                case "Empleado":
                    parameter[0] = "[Dim Empleado].[Dim Empleado Nombre].CHILDREN";
                    break;
                default:
                    parameter[0] = "[Dim Cliente].[Dim Cliente Nombre].CHILDREN";
                    break;
            }
            string MDX_QUERY = string.Empty;

            if (parameter[1] == "" && parameter[2] == "")
            {

                MDX_QUERY = @"
                    SELECT
	                    NON EMPTY
	                    {
		                    [Measures].[Hec Ventas Ventas]
	                    }
	                    ON COLUMNS,
	                    NON EMPTY
	                    {
		                    HEAD(
			                    NONEMPTY
			                    (
				                    ORDER
				                    (
					                    " + parameter[0] + @",
					                    [Measures].[Hec Ventas Ventas],
					                    DESC
				                    )
			                    )
			                    ,5
		                    )
	                    }
	                    ON ROWS
	                    FROM
	                    [DWH Northwind]
                    ";
            }
            else if (parameter[1] != "" && parameter[2] == "")
            {
                MDX_QUERY = @"
                    SELECT
                        NON EMPTY
                        {
		
			                    [Dim Tiempo].[Anio].[" + parameter[1] + @"]
		
	                    }
                        ON COLUMNS,
                        NON EMPTY
                        {	
		                    HEAD(
			                    ORDER(
					                    "+parameter[0]+@",
					                    [Measures].[Hec Ventas Ventas],
					                    DESC
				                    ),5
		                    )
                        }
                        ON ROWS
	                    FROM
	                    [DWH Northwind]
	                    where
	                    [Measures].[Hec Ventas Ventas]
                    ";
            }
            else
            {
                MDX_QUERY = @"
                    SELECT
                        NON EMPTY
                        {

                            ([Dim Tiempo].[Anio].[" + parameter[1] + @"], 
		                    [Dim Tiempo].[Numero Mes].[" + parameter[2] + @"])	            
	                    }
                        ON COLUMNS,
                        NON EMPTY
                        {	
		                    HEAD(
			                    ORDER("
                                    + parameter[0] + @",
				                    [Measures].[Hec Ventas Ventas],
				                    DESC
			                    ),5
		                    )
                        }
                        ON ROWS
	                    FROM
	                    [DWH Northwind]
	                    where
	                    [Measures].[Hec Ventas Ventas]
                    ";
            }
            Debug.Write(parameter);

            Debug.Write(MDX_QUERY);

            List<string> clients = new List<string>();
            List<decimal> sales = new List<decimal>();
            dynamic result = new
            {
                Label = clients,
                SingleDataSet = sales
            };

            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDX_QUERY, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {

                            clients.Add(dr.GetString(0));
                            sales.Add(Math.Round(dr.GetDecimal(1)));
                        }
                        dr.Close();
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, (object)result);

        }

        [HttpGet]
        [Route("GetDimensions")]
        public HttpResponseMessage GetDimensions()
        {
            List<dynamic> result = new List<dynamic>();
            result.Add(new
            {
                value = 1,
                label = "Cliente"
            });
            result.Add(new
            {
                value = 2,
                label = "Empleado"
            });
            result.Add(new
            {
                value = 3,
                label = "Producto"
            });
            return Request.CreateResponse(HttpStatusCode.OK, (object)result);
        }

        [HttpPost]
        [Route("GetDimensionsItems")]
        public HttpResponseMessage GetDimensionsItems([FromBody] string[] parameter)
        {
            switch (parameter[0])
            {
                case "Cliente":
                    parameter[0] = "[Dim Cliente].[Dim Cliente Nombre].CHILDREN";
                    break;
                case "Producto":
                    parameter[0] = "[Dim Producto].[Dim Producto Nombre].CHILDREN";
                    break;
                case "Empleado":
                    parameter[0] = "[Dim Empleado].[Dim Empleado Nombre].CHILDREN";
                    break;
                default:
                    parameter[0] = "[Dim Cliente].[Dim Cliente Nombre].CHILDREN";
                    break;
            }



            string MDX_QUERY = string.Empty;
            if (parameter[1] == "" && parameter[2] == "")
            {
                MDX_QUERY = @"
                SELECT
			        [Measures].[Hec Ventas Ventas]
		            ON COLUMNS,
                    HEAD(
			            ORDER
			            (        
				            " + parameter[0] + @",
				            [Measures].[Hec Ventas Ventas],
				            BDESC
			            ),5
                    )
		        ON ROWS
		        FROM
		        [DWH Northwind]";
            }
            else if (parameter[1] != "" && parameter[2] == "")
            {
                MDX_QUERY = @"
                    SELECT
			                    [Dim Tiempo].[Anio].[" + parameter[1] + @"]
                        ON COLUMNS,
		                    HEAD(
			                    ORDER(
					                    " + parameter[0] + @",
					                    [Measures].[Hec Ventas Ventas],
					                    DESC
				                    ),5
		                    )
                        ON ROWS
	                    FROM
	                    [DWH Northwind]
	                    where
	                    [Measures].[Hec Ventas Ventas]
                    ";
            }
            else
            {
                MDX_QUERY = @"
                    SELECT                      
                            ([Dim Tiempo].[Anio].[" + parameter[1] + @"], 
		                    [Dim Tiempo].[Numero Mes].[" + parameter[2] + @"])	            
                        ON COLUMNS,
		                    HEAD(
			                    ORDER("
                                   + parameter[0] + @",
				                    [Measures].[Hec Ventas Ventas],
				                    DESC
			                    ),5
		                    )
                        ON ROWS
	                    FROM
	                    [DWH Northwind]
	                    where
	                    [Measures].[Hec Ventas Ventas]
                    ";
            }
               

            List<dynamic> result = new List<dynamic>();

            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDX_QUERY, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        int i = 1;
                        while (dr.Read())
                        {
                            result.Add(new
                            {
                                value = i,
                                label = dr.GetString(0)                               
                            });
                            i++;
                        }
                        dr.Close();
                        result = result.OrderBy(o => o.label).ToList();
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, (object)result);
        }

        [HttpGet]
        [Route("GetDimensionYears")]
        public HttpResponseMessage GetDimensionYears()
        {
            string MDX_QUERY = string.Empty;
            MDX_QUERY = @"
            SELECT
			    [Measures].[Hec Ventas Ventas]
		        ON COLUMNS,
			    ORDER
			    (
				    [Dim Tiempo].[Anio].CHILDREN,
				    [Measures].[Hec Ventas Ventas],
				    BDESC
			    )
		    ON ROWS
		    FROM
		    [DWH Northwind]";

            List<dynamic> result = new List<dynamic>();
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDX_QUERY, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        int i = 1;
                        while (dr.Read())
                        {
                            result.Add(new
                            {
                                value = i,
                                label = dr.GetString(0)
                            });                         
                            i++;
                        }
                        result = result.OrderBy(o => o.label).ToList();
                        dr.Close();
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, (object)result);
        }

        [HttpGet]
        [Route("GetDimensionYearsMonths")]
        public HttpResponseMessage GetDimensionYearsMonths()
        {
            //      string MDX_QUERY = string.Empty;
            //      MDX_QUERY = @"
            //      SELECT
            // [Measures].[Hec Ventas Ventas]
            //    ON COLUMNS,
            // ORDER
            // (
            //  [Dim Tiempo].[Mes Ingles].CHILDREN,
            //  [Measures].[Hec Ventas Ventas],
            //  BDESC
            // )
            //ON ROWS
            //FROM
            //[DWH Northwind]";

            //      List<string> labels = new List<string>();
            //      List<int> values = new List<int>();
            //      dynamic result = new
            //      {
            //          value = values,
            //          label = labels
            //      };

            //      using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            //      {
            //          cnn.Open();
            //          using (AdomdCommand cmd = new AdomdCommand(MDX_QUERY, cnn))
            //          {
            //              using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            //              {
            //                  int i = 1;
            //                  while (dr.Read())
            //                  {
            //                      values.Add(i);
            //                      labels.Add(dr.GetString(0).Substring(0,3));
            //                      i++;
            //                  }
            //                  labels.Sort();
            //                  dr.Close();
            //              }
            //          }
            //      }
            List<dynamic> result = new List<dynamic>();
            result.Add(new
            {
                value = 1,
                label = "Jan"
            });
            result.Add(new
            {
                value = 2,
                label = "Feb"
            });
            result.Add(new
            {
                value = 3,
                label = "Mar"
            });
            result.Add(new
            {
                value = 4,
                label = "Apr"
            });
            result.Add(new
            {
                value = 5,
                label = "May"
            });
            result.Add(new
            {
                value = 6,
                label = "Jun"
            });
            result.Add(new
            {
                value = 7,
                label = "Jul"
            });
            result.Add(new
            {
                value = 8,
                label = "Aug"
            });
            result.Add(new
            {
                value = 9,
                label = "Sep"
            });
            result.Add(new
            {
                value = 10,
                label = "Oct"
            });
            result.Add(new
            {
                value = 11,
                label = "Nov"
            });
            result.Add(new
            {
                value = 12,
                label = "Dic"
            });
            return Request.CreateResponse(HttpStatusCode.OK, (object)result);
        }
    }
}
