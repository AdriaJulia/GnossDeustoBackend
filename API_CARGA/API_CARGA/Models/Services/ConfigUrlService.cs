﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace API_CARGA.Models.Services
{
    public class ConfigUrlService
    {
        public IConfigurationRoot Configuration { get; set; }
        public string Url { get; set; }
        public string GetUrl()
        {
            if (string.IsNullOrEmpty(Url))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
                var connectionString = Configuration["ConfigUrl"];
                Url = connectionString;
            }
            return Url;
        }
    }
}