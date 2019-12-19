﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrisFactory.Models.ConfigEntities
{
    public class UrlStructure
    {
        public string Name { get; set; }
        public IList<Component> Components { get; set; }
    }
}