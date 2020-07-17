﻿// Copyright (c) UTE GNOSS - UNIVERSIDAD DE DEUSTO
// Licenciado bajo la licencia GPL 3. Ver https://www.gnu.org/licenses/gpl-3.0.html
// Proyecto Hércules ASIO Backend SGI. Ver https://www.um.es/web/hercules/proyectos/asio
// Clase para obtener la configuración necesaria para el uso de Sparql
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.IO;

namespace API_CARGA.Models.Services
{
    ///<summary>
    ///Clase para obtener la configuración necesaria para el uso de Sparql
    ///</summary>
    public class ConfigSparql
    {
        public IConfigurationRoot Configuration { get; set; }
        private string Graph { get; set; }
        public string Endpoint { get; set; }
        private string QueryParam { get; set; }
        private string GraphRoh { get; set; }
        private string GraphRohes { get; set; }
        private string GraphRohum { get; set; }
        ///<summary>
        ///Obtiene el gráfo configurado en Sparql:Graph del fichero appsettings.json
        ///</summary>
        public string GetGraph()
        {
            if (string.IsNullOrEmpty(Graph))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Graph"))
                {
                    Graph = environmentVariables["Graph"] as string;
                }
                else
                {
                    Graph = Configuration["Sparql:Graph"];
                }
                
            }
            return Graph;
        }

        ///<summary>
        ///Obtiene el gráfo configurado en Sparql:GraphRoh del fichero appsettings.json
        ///</summary>
        public string GetGraphRoh()
        {
            if (string.IsNullOrEmpty(GraphRoh))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("GraphRoh"))
                {
                    GraphRoh = environmentVariables["GraphRoh"] as string;
                }
                else
                {
                    GraphRoh = Configuration["Sparql:GraphRoh"];
                }

            }
            return GraphRoh;
        }

        ///<summary>
        ///Obtiene el gráfo configurado en Sparql:GraphRohes del fichero appsettings.json
        ///</summary>
        public string GetGraphRohes()
        {
            if (string.IsNullOrEmpty(GraphRohes))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("GraphRohes"))
                {
                    GraphRohes = environmentVariables["GraphRohes"] as string;
                }
                else
                {
                    GraphRohes = Configuration["Sparql:GraphRohes"];
                }

            }
            return GraphRohes;
        }

        ///<summary>
        ///Obtiene el gráfo configurado en Sparql:GraphRohes del fichero appsettings.json
        ///</summary>
        public string GetGraphRohum()
        {
            if (string.IsNullOrEmpty(GraphRohum))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("GraphRohum"))
                {
                    GraphRohum = environmentVariables["GraphRohum"] as string;
                }
                else
                {
                    GraphRohum = Configuration["Sparql:GraphRohum"];
                }

            }
            return GraphRohum;
        }

        ///<summary>
        ///Obtiene el endpoint configurado en Sparql:Endpoint del fichero appsettings.json
        ///</summary>
        public string GetEndpoint()
        {
            if (Endpoint==null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Endpoint"))
                {
                    Endpoint = environmentVariables["Endpoint"] as string;
                }
                else
                {
                    Endpoint = Configuration["Sparql:Endpoint"];
                }
            }
            return Endpoint;
        }

        ///<summary>
        ///Obtiene el parametro de query configurado en Sparql:QueryParam del fichero appsettings.json
        ///</summary>
        public string GetQueryParam()
        {
            if (string.IsNullOrEmpty(QueryParam))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("QueryParam"))
                {
                    QueryParam = environmentVariables["QueryParam"] as string;
                }
                else
                {
                    QueryParam = Configuration["Sparql:QueryParam"];
                }

            }
            return QueryParam;
        }
    }
}
