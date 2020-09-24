﻿// Copyright (c) UTE GNOSS - UNIVERSIDAD DE DEUSTO
// Licenciado bajo la licencia GPL 3. Ver https://www.gnu.org/licenses/gpl-3.0.html
// Proyecto Hércules ASIO Backend SGI. Ver https://www.um.es/web/hercules/proyectos/asio
using API_DISCOVER.Utility;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Writing;

namespace API_DISCOVER.Models.Entities
{
    /// <summary>
    /// Resultado despues de aplicar a un RDF el descubrimiento
    /// </summary>
    public class DiscoverResult
    {
        /// <summary>
        /// Constructor del resultado del descubirmiento
        /// </summary>
        /// <param name="pDataGraph">Grafo con los datos</param>
        /// <param name="pDataInferenceGraph">Grafo con los datos (con inferencia)</param>
        /// <param name="pOntologyGraph">Grafo con la ontología</param>
        /// <param name="pDiscoveredEntitiesWithSubject">Entidades descubiertas con los sujetos</param>
        /// <param name="pDiscoveredEntitiesWithId">Entidades descubiertas con los identificadores</param>
        /// <param name="pDiscoveredEntitiesWithDataBase">Entidades descubiertas con la BBDD</param>
        /// <param name="pDiscoveredEntitiesProbability">Probabilidades de descubriiento</param>
        /// <param name="pSecondsProcessed">Tiempo (en segundos) en procesar el descubrimiento</param>
        /// <param name="pOrcidIntegration">Datos obtendidos con la integración con ORCID</param>
        public DiscoverResult(RohGraph pDataGraph,RohGraph pDataInferenceGraph, RohGraph pOntologyGraph, HashSet<string> pDiscoveredEntitiesWithSubject, Dictionary<string, string> pDiscoveredEntitiesWithId, Dictionary<string, KeyValuePair<string, float>> pDiscoveredEntitiesWithDataBase, Dictionary<string, Dictionary<string, float>> pDiscoveredEntitiesProbability,double pSecondsProcessed,Dictionary<string,Dictionary<string,string>> pOrcidIntegration)
        {
            dataGraph = pDataGraph;
            dataInferenceGraph = pDataInferenceGraph;
            ontologyGraph = pOntologyGraph;
            discoveredEntitiesWithSubject = pDiscoveredEntitiesWithSubject;
            discoveredEntitiesWithId = pDiscoveredEntitiesWithId;
            discoveredEntitiesWithDataBase = pDiscoveredEntitiesWithDataBase;
            discoveredEntitiesProbability = pDiscoveredEntitiesProbability;
            secondsProcessed = pSecondsProcessed;
            orcidIntegration = pOrcidIntegration;
        }

        /// <summary>
        /// Grafo con los datos
        /// </summary>
        public RohGraph dataGraph { get; set; }

        /// <summary>
        /// Grafo con los datos (con inferencia)
        /// </summary>
        public RohGraph dataInferenceGraph { get; set; }

        /// <summary>
        /// Grafo con la ontología
        /// </summary>
        public RohGraph ontologyGraph { get; set; }

        /// <summary>
        /// Entidades descubiertas con los sujetos
        /// </summary>
        public HashSet<string> discoveredEntitiesWithSubject { get; }

        /// <summary>
        /// Entidades descubiertas con los identificadores
        /// </summary>
        public Dictionary<string, string> discoveredEntitiesWithId { get; }

        /// <summary>
        /// Entidades descubiertas con la BBDD
        /// </summary>
        public Dictionary<string, KeyValuePair<string, float>> discoveredEntitiesWithDataBase { get; }

        /// <summary>
        /// Probabilidades de descubriiento
        /// </summary>
        public Dictionary<string, Dictionary<string, float>> discoveredEntitiesProbability { get; }

        /// <summary>
        /// Tiempo (en segundos) en procesar el descubrimiento
        /// </summary>
        public double secondsProcessed { get; }

        /// <summary>
        /// Datos obtendidos con la integración con ORCID
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> orcidIntegration { get; }
        

        /// <summary>
        /// Obtiene la lista de triples del dataGraph
        /// </summary>
        /// <returns></returns>
        public List<string> GetDataGraphTriples()
        {
            return SparqlUtility.GetTriplesFromGraph(dataGraph);
        }

        /// <summary>
        /// Obtiene el RDF del dataGraph
        /// </summary>
        /// <returns></returns>
        public string GetDataGraphRDF()
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            RdfXmlWriter rdfXmlWriter = new RdfXmlWriter();
            rdfXmlWriter.Save(dataGraph, sw);
            return sw.ToString();
        }

    }
}