﻿using Linked_Data_Server.Models;
using Linked_Data_Server.Models.Entities;
using Linked_Data_Server.Models.Services;
using Linked_Data_Server.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;

namespace Linked_Data_Server.Controllers
{

    public class AutocompleteController : Controller
    {
        private readonly static ConfigService mConfigService = new ConfigService();
        private readonly static Config_Linked_Data_Server mLinked_Data_Server_Config = LoadLinked_Data_Server_Config();
        private readonly ILogger<HomeController> _logger;

        public AutocompleteController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index(string q)
        {
            List<KeyValuePair<string, string>> response = new List<KeyValuePair<string, string>>();
            string consulta = @$"   select distinct ?s ?o where 
                                    {{
                                        ?s ?p ?o.
                                        FILTER(?p in (<{string.Join(">,<", mLinked_Data_Server_Config.PropsTitle)}>) AND (lcase(?o) like'{q.ToLower()}*' OR lcase(?o) like'* {q.ToLower()}*'))
                                    }}limit 10";
            SparqlObject sparqlObject = SparqlUtility.SelectData(mConfigService.GetSparqlEndpoint(), mConfigService.GetSparqlGraph(), consulta, mConfigService.GetSparqlQueryParam());
            foreach (Dictionary<string, SparqlObject.Data> row in sparqlObject.results.bindings)
            {
                response.Add(new KeyValuePair<string, string>(row["o"].value, row["s"].value));
            }
            return Json(response);
        }

        /// <summary>
        /// Cargamos las configuraciones 
        /// </summary>
        /// <returns></returns>
        private static Config_Linked_Data_Server LoadLinked_Data_Server_Config()
        {
            return JsonConvert.DeserializeObject<Config_Linked_Data_Server>(System.IO.File.ReadAllText("Config/Linked_Data_Server_Config.json"));
        }
    }
}
