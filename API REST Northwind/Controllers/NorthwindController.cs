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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("Northwind/v1")]
    public class NorthwindController : ApiController
    {
        //Pie Graphic
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

        //Fill Combobox
        [HttpGet]
        [Route("GetDimensions")]
        public HttpResponseMessage GetDimensions()
        {
            List<string> result = new List<string>();
            result.Add("Cliente");
            result.Add("Empleado");
            result.Add("Producto");

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
                    NON EMPTY
                    {
			            [Measures].[Hec Ventas Ventas]
                    }
		            ON COLUMNS,
                    NON EMPTY
                    {
                        HEAD(
			                ORDER
			                (        
				                " + parameter[0] + @",
				                [Measures].[Hec Ventas Ventas],
				                DESC
			                ),5
                        )
                    }
		        ON ROWS
		        FROM
		        [DWH Northwind]";
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
					                    " + parameter[0] + @",
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
	                    WHERE
                        [measures].[hec ventas ventas]
                    ";
            }
               

            List<string> result = new List<string>();

            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDX_QUERY, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            Debug.Write(dr);
                            if  (dr.GetValue(1) == null)
                                break;
                            else
                                result.Add(dr.GetString(0));
                        }
                        dr.Close();
                        result = result.OrderBy(o => o).ToList();
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

            List<string> result = new List<string>();
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDX_QUERY, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            result.Add(dr.GetString(0));                         
                        }
                        result = result.OrderBy(o => o).ToList();
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
            List<string> result = new List<string>();

            result.Add("Jan");
            result.Add("Feb");
            result.Add("Mar");
            result.Add("Apr");
            result.Add("May");
            result.Add("Jun");
            result.Add("Jul");
            result.Add("Aug");
            result.Add("Sep");
            result.Add("Oct");
            result.Add("Nov");
            result.Add("Dec");

            return Request.CreateResponse(HttpStatusCode.OK, (object)result);
        }

        //Bar Graphic
        //
        [HttpPost]
        [Route("GetHistogram")]
        public HttpResponseMessage GetHistogram([FromBody]List<string[]> parameter)
        {
            string[] parameters = parameter[0].ToArray();
            string[] items = parameter[1].ToArray();

            switch (parameters[0])
            {
                case "Cliente":
                    parameters[0] = "[Dim Cliente].[Dim Cliente Nombre].CHILDREN";
                    break;
                case "Producto":
                    parameters[0] = "[Dim Producto].[Dim Producto Nombre].CHILDREN";
                    break;
                case "Empleado":
                    parameters[0] = "[Dim Empleado].[Dim Empleado Nombre].CHILDREN";
                    break;
                default:
                    parameters[0] = "[Dim Cliente].[Dim Cliente Nombre].CHILDREN";
                    break;
            }

            string MDX_QUERY = string.Empty;
            string validate = string.Empty;
            if (items.Count() <= 0)
            {
                if (parameters[1] == "" && parameters[2] == "")
                {
                    MDX_QUERY = @"
                    SELECT
                        NON EMPTY
                        {
		
			                    [Dim Tiempo].[Anio].[1996],
			                    [Dim Tiempo].[Anio].[1997],
			                    [Dim Tiempo].[Anio].[1998]
	                    }
                        ON COLUMNS,
                        NON EMPTY
                        {	
			                    ORDER(
					                    " + parameters[0] + @",
					                    [Measures].[Hec Ventas Ventas],
					                    DESC
		                    )
                        }
                        ON ROWS
	                    FROM
	                    [DWH Northwind]
	                    where
	                    [Measures].[Hec Ventas Ventas]
                    ";

                    validate = "añossinitems";
                }
                else if (parameters[1] != "" && parameters[2] == "")
                {
                    MDX_QUERY = @"
                    SELECT
                        NON EMPTY
                        {
		
			                    [Dim Tiempo].[Anio].[" + parameters[1] + @"]
	                    }
                        ON COLUMNS,
                        NON EMPTY
                        {	
			                    ORDER(
					                     " + parameters[0] + @",
					                    [Measures].[Hec Ventas Ventas],
					                    DESC
		                    )
                        }
                        ON ROWS
	                    FROM
	                    [DWH Northwind]
	                    where
	                    [Measures].[Hec Ventas Ventas]
                    ";
                    validate = "añosinitems";
                }
                else if (parameters[1] != "" && parameters[2] != "")
                {
                    MDX_QUERY = @"
                    SELECT
                        NON EMPTY
                        {
		
			                    ([Dim Tiempo].[Anio].[" + parameters[1] + @"],
                                [Dim Tiempo].[Numero Mes].[" + parameters[2] + @"])
	                    }
                        ON COLUMNS,
                        NON EMPTY
                        {	
			                    ORDER(
					                     " + parameters[0] + @",
					                    [Measures].[Hec Ventas Ventas],
					                    DESC
		                    )
                        }
                        ON ROWS
	                    FROM
	                    [DWH Northwind]
	                    where
	                    [Measures].[Hec Ventas Ventas]
                    ";
                    validate = "añomessinitems";
                }
            }
            else
            {
                string items2 = string.Empty;
                switch (parameters[0])
                {
                    case "[Dim Cliente].[Dim Cliente Nombre].CHILDREN":
                        parameters[0] = "[Dim Cliente].[Dim Cliente Nombre].[";
                        break;
                    case "[Dim Producto].[Dim Producto Nombre].CHILDREN":
                        parameters[0] = "[Dim Producto].[Dim Producto Nombre].[";
                        break;
                    case "[Dim Empleado].[Dim Empleado Nombre].CHILDREN":
                        parameters[0] = "[Dim Empleado].[Dim Empleado Nombre].[";
                        break;
                    default:
                        parameters[0] = "[Dim Cliente].[Dim Cliente Nombre].[";
                        break;
                }
                for (int i = 0; i < items.Count(); i++)
                {
                    items2 = items2 + parameters[0] + items[i] + "],";
                }

                items2 = items2.TrimEnd(',');

                if (parameters[1] == "" && parameters[2] == "")
                {
                    MDX_QUERY = @"
                    WITH
                        SET [Items] AS 
                        {
	                        " + items2 + @"
                        }
                        SELECT
                            NON EMPTY
                            {
		
			                        [Dim Tiempo].[Anio].[1996],
			                        [Dim Tiempo].[Anio].[1997],
			                        [Dim Tiempo].[Anio].[1998]
	                        }
                            ON COLUMNS,
                            NON EMPTY
                            {	
			                        ORDER(
					                        [Items],
					                        [Measures].[Hec Ventas Ventas],
					                        DESC
		                        )
                            }
                            ON ROWS
	                        FROM
	                        [DWH Northwind]
	                        where
	                        [Measures].[Hec Ventas Ventas]
                    ";
                    validate = "añosconitems";
                }
                else if (parameters[1] != "" && parameters[2] == "")
                {
                    MDX_QUERY = @"
                    WITH
                        SET [Items] AS 
                        {
	                        " + items2 + @"
                        }
                        SELECT
                            NON EMPTY
                            {
		
			                        [Dim Tiempo].[Anio].[" + parameters[1] + @"]
	                        }
                            ON COLUMNS,
                            NON EMPTY
                            {	
			                        ORDER(
					                        [Items],
					                        [Measures].[Hec Ventas Ventas],
					                        DESC
		                        )
                            }
                            ON ROWS
	                        FROM
	                        [DWH Northwind]
	                        where
	                        [Measures].[Hec Ventas Ventas]
                    ";
                    validate = "añoconitems";
                }
                else if (parameters[1] != "" && parameters[2] != "")
                {
                    MDX_QUERY = @"
                    WITH
                        SET [Items] AS 
                        {
	                        " + items2 + @"
                        }
                        SELECT
                            NON EMPTY
                            {
		
			                        ([Dim Tiempo].[Anio].[" + parameters[1] + @"],
			                        [Dim Tiempo].[Numero Mes].[" + parameters[2] + @"])
	                        }
                            ON COLUMNS,
                            NON EMPTY
                            {	
			                        ORDER(
					                        [Items],
					                        [Measures].[Hec Ventas Ventas],
					                        DESC
		                        )
                            }
                            ON ROWS
	                        FROM
	                        [DWH Northwind]
	                        where
	                        [Measures].[Hec Ventas Ventas]
                    ";
                    validate = "añomesconitems";
                }
            }
            List<decimal> data1 = new List<decimal>();
            List<decimal> data2 = new List<decimal>();
            List<decimal> data3 = new List<decimal>();

            List<string> labels = new List<string>();
            string label = "";
           

            Debug.Write(MDX_QUERY);
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDX_QUERY, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        
                        int i = 0;
                        while (dr.Read())
                        {
                            if(validate == "añossinitems" || validate == "añosconitems")
                            {
                                if(dr.GetValue(1) != null)
                                {
                                    data1.Add(dr.GetDecimal(1));
                                    label = "1996";
                                }
                                else
                                {
                                    data1.Add(0);
                                    label = "1996";
                                }

                                if (dr.GetValue(2) != null)
                                {
                                    data2.Add(dr.GetDecimal(2));
                                    label = "1996";
                                }
                                else
                                {
                                    data2.Add(0);
                                    label = "1996";
                                }

                                if (dr.GetValue(3) != null)
                                {
                                    data3.Add(dr.GetDecimal(3));
                                    label = "1996";
                                }
                                else
                                {
                                    data3.Add(0);
                                    label = "1996";
                                }

                                labels.Add(dr.GetString(0));
                            }
                            else if(validate == "añosinitems" || validate == "añoconitems")
                            {
                                if(dr.GetValue(1) != null)
                                {
                                    data1.Add(dr.GetDecimal(1));
                                    label = parameters[1];
                                }
                                else
                                {
                                    data1.Add(0);
                                    label = parameters[1];
                                }

                                labels.Add(dr.GetString(0));
                            }
                            else if(validate == "añomessinitems" || validate == "añomesconitems")
                            {
                                if (dr.GetValue(1) != null)
                                {
                                    data1.Add(dr.GetDecimal(1));
                                    label = parameters[2];
                                }
                                else
                                {
                                    data1.Add(0);
                                    label = parameters[2];
                                }

                                labels.Add(dr.GetString(0));
                            }
                        }
                        dr.Close();
                    }
                }
            }

            List<dynamic> barChartData = new List<dynamic>();

            if (validate == "añossinitems" || validate == "añosconitems")
            {
                barChartData.Add(new
                {
                    data = data1.ToArray(),
                    label = "1996"
                });
                barChartData.Add(new
                {
                    data = data2.ToArray(),
                    label = "1997"
                });
                barChartData.Add(new
                {
                    data = data3.ToArray(),
                    label = "1996"
                });
            }
            else if(validate == "añosinitems" || validate == "añoconitems")
            {
                barChartData.Add(new
                {
                    data = data1.ToArray(),
                    label = parameters[1]
                });
            }
            else if (validate == "añomessinitems" || validate == "añomesconitems")
            {
                barChartData.Add(new
                {
                    data = data1.ToArray(),
                    label = parameters[2]
                });
            }


            dynamic result = new
            {
                barChartData,
                barChartLabels = labels.ToArray()
            };


            return Request.CreateResponse(HttpStatusCode.OK, (object)result);
        }

    }
}
